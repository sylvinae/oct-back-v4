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
    public async Task<bool> CreateInvoice(
        CreateInvoiceModel invoice)
    {
        log.LogInformation("Create invoice called.");
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);

        var newInvoiceEntity = PropCopier.Copy(invoice, new InvoiceEntity
            { UserId = user!.Id, IsVoided = false });
        var newInvoiceItemsEntity = new List<InvoiceItemEntity>();
        var toAddHistory = new List<AddItemHistoryModel>();

        var isValid = await createValidator.ValidateAsync(invoice);
        if (!isValid.IsValid)
            return false;

        foreach (var item in invoice.InvoiceItems)
        {
            var result = true;
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (product == null)
                throw new Exception();

            newInvoiceItemsEntity.Add(PropCopier.Copy(item,
                new InvoiceItemEntity { InvoiceId = newInvoiceEntity.Id, PurchasePrice = product.RetailPrice }
            ));

            if (product is ItemEntity i)
                result = ProductMod(i, item.ItemsSold, toAddHistory);

            else if (product is BundleEntity b) result = await BundleMod(b, toAddHistory);

            if (!result)
                throw new Exception();
        }

        newInvoiceEntity.InvoiceItems = newInvoiceItemsEntity;

        await db.Invoices.AddAsync(newInvoiceEntity);
        await ih.AddItemHistoryRange(toAddHistory);

        await db.SaveChangesAsync();
        return true;
    }

    private bool ProductMod(ItemEntity item, int quantity, List<AddItemHistoryModel> toAddHistory)
    {
        try
        {
            item.Stock -= quantity;
            item.IsLow = item.Stock <= item.LowThreshold;
            var newHash = Cryptics.ComputeHash(item);

            item.Hash = newHash;

            toAddHistory.Add(
                PropCopier.Copy(item,
                    new AddItemHistoryModel { ItemId = item.Id, Action = Actions.Purchased.ToString() }));

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error modding item {x}. Exception: {ex}", item.Id, ex);
            return false;
        }
    }

    private async Task<bool> BundleMod(BundleEntity bundle, List<AddItemHistoryModel> toAddHistory)
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

            var result = ProductMod(item, bundleItem.Quantity, toAddHistory);
            if (!result)
                return false;
        }

        return true;
    }
}