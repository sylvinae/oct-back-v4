using API.Db;
using API.Entities.Bundles;
using API.Entities.Invoice;
using API.Entities.Item;
using API.Entities.User;
using API.Models;
using API.Models.Invoice;
using API.Models.Item;
using API.Services.Invoice.Interfaces;
using API.Services.Item.Interfaces;
using API.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Invoice;

public class CreateInvoiceService(
    ILogger<CreateInvoiceService> log,
    Context db,
    IItemHistoryService ih,
    IValidator<CreateInvoiceModel> createValidator,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor) : ICreateInvoiceService
{
    public async Task<BulkFailure<CreateInvoiceModel>> CreateInvoice(CreateInvoiceModel invoice)
    {
        log.LogInformation("Create invoice called.");
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);

        var newInvoiceEntity = PropCopier.Copy(invoice, new InvoiceEntity
            { UserId = user!.Id, IsVoided = false });
        var newInvoiceItemsEntity = new List<InvoiceItemEntity>();
        var toAddHistory = new List<CreateItemHistoryModel>();
        var errors = new Dictionary<string, string>();

        var validationResult = await createValidator.ValidateAsync(invoice);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors) errors[error.PropertyName] = error.ErrorMessage;
            return new BulkFailure<CreateInvoiceModel> { Input = invoice, Errors = errors };
        }


        foreach (var item in invoice.InvoiceItems)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product == null)
            {
                errors[item.ProductId.ToString()] = "Product not found.";
                return new BulkFailure<CreateInvoiceModel> { Input = invoice, Errors = errors };
            }

            newInvoiceItemsEntity.Add(PropCopier.Copy(item,
                new InvoiceItemEntity { InvoiceId = newInvoiceEntity.Id, PurchasePrice = product.RetailPrice }
            ));

            bool result;
            if (product is ItemEntity itemEntity)
            {
                result = ProductMod(itemEntity, toAddHistory, item.QuantitySold);
                if (result || itemEntity.Stock >= item.QuantitySold) continue;
                errors[item.ProductId.ToString()] = "Not enough stock.";
                return new BulkFailure<CreateInvoiceModel> { Input = invoice, Errors = errors };
            }

            if (product is BundleEntity bundleEntity)
            {
                result = await BundleMod(bundleEntity, toAddHistory);
                if (result) continue;
                errors[item.ProductId.ToString()] = "Not enough stock in bundle items.";
                return new BulkFailure<CreateInvoiceModel> { Input = invoice, Errors = errors };
            }

            errors[item.ProductId.ToString()] = "Unknown product type.";
            return new BulkFailure<CreateInvoiceModel> { Input = invoice, Errors = errors };
        }

        newInvoiceEntity.InvoiceItems = newInvoiceItemsEntity;

        try
        {
            await db.Invoices.AddAsync(newInvoiceEntity);
            await ih.AddItemHistoryRange(toAddHistory);
            await db.SaveChangesAsync();

            return new BulkFailure<CreateInvoiceModel>
                { Input = invoice, Errors = new Dictionary<string, string>() };
        }
        catch (Exception ex)
        {
            log.LogError("Error saving invoice: {ex}", ex);
            errors["SaveInvoice"] = ex.Message;
            return new BulkFailure<CreateInvoiceModel> { Input = invoice, Errors = errors };
        }
    }


    private bool ProductMod(ItemEntity item, List<CreateItemHistoryModel> toAddHistory, int quantity = 0,
        int usesConsumed = 0)
    {
        try
        {
            if (quantity == 0 && usesConsumed == 0) return false;

            if (quantity > 0)
            {
                item.Stock -= quantity;
                item.IsLow = item.Stock <= item.LowThreshold;
            }
            else if (usesConsumed > 0)
            {
                ProcessUsesConsumption(item, ref usesConsumed);
            }

            if (item.Stock <= 0 && usesConsumed > 0)
                log.LogWarning(
                    "Insufficient stock to consume all requested uses for item {itemId}. Remaining uses: {remainingUses}.",
                    item.Id, usesConsumed);

            var newHash = Cryptics.ComputeHash(item);
            item.Hash = newHash;

            toAddHistory.Add(
                PropCopier.Copy(item,
                    new CreateItemHistoryModel { ItemId = item.Id, Action = Actions.Purchased.ToString() }));

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error modding item {x}. Exception: {ex}", item.Id, ex);
            return false;
        }
    }

    private static void ProcessUsesConsumption(ItemEntity item, ref int usesConsumed)
    {
        while (usesConsumed > 0 && item.Stock > 0)
        {
            if (item.UsesLeft == 0)
            {
                if (item.Stock > 0)
                {
                    item.Stock--;
                    item.IsLow = item.Stock <= item.LowThreshold;
                }

                item.UsesLeft = item.UsesMax;
            }

            if (item.UsesLeft > 0)
            {
                var usesToDeduct = Math.Min(usesConsumed, item.UsesLeft.GetValueOrDefault());
                item.UsesLeft -= usesToDeduct;
                usesConsumed -= usesToDeduct;
            }

            if (item.UsesLeft != 0 || item.Stock <= 0) continue;
            item.Stock--;
            item.IsLow = item.Stock <= item.LowThreshold;
        }
    }


    private async Task<bool> BundleMod(BundleEntity bundle, List<CreateItemHistoryModel> toAddHistory)
    {
        var bundleItems = await db.BundleItems
            .Where(b => b.BundleId == bundle.Id)
            .ToListAsync();

        if (bundleItems.Count == 0)
            return false;

        var itemIds = bundleItems.Select(b => b.ItemId).Distinct().ToList();
        var items = await db.Products.OfType<ItemEntity>()
            .Where(i => itemIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id);

        foreach (var bundleItem in bundleItems)
        {
            if (!items.TryGetValue(bundleItem.ItemId, out var item))
            {
                log.LogWarning("Item with ID {ItemId} not found for bundle {BundleId}.", bundleItem.ItemId, bundle.Id);
                return false;
            }

            var result = ProductMod(item, toAddHistory, bundleItem.Quantity, bundleItem.Uses);
            if (!result)
                return false;
        }

        return true;
    }
}