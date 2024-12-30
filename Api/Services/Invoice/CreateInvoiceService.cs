using API.Db;
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
    public async Task<(bool ok, BulkFailure<CreateInvoiceModel>?fail)> CreateInvoice(
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
            return (false, new BulkFailure<CreateInvoiceModel>
            {
                Input = invoice,
                Errors = isValid.Errors
                    .ToDictionary(e => e.PropertyName,
                        e => e.ErrorMessage)
            });

        foreach (var item in invoice.InvoiceItems)
        {
            var itemEntity = await db.Products.FirstOrDefaultAsync(p => p.Id == item.ProductId);
            if (itemEntity == null)
                throw new Exception();

            newInvoiceItemsEntity.Add(PropCopier.Copy(item,
                new InvoiceItemEntity { InvoiceId = newInvoiceEntity.Id, PurchasePrice = itemEntity.RetailPrice }
            ));

            var result = ProductMod(item.ProductId, item.ItemsSold, toAddHistory);

            if (!result.Result)
                throw new Exception();
        }

        newInvoiceEntity.InvoiceItems = newInvoiceItemsEntity;

        await db.Invoices.AddAsync(newInvoiceEntity);
        await ih.AddItemHistoryRange(toAddHistory);
        await db.SaveChangesAsync();
        return (true, null);
    }

    private async Task<bool> ProductMod(Guid productId, int quantity, List<AddItemHistoryModel> toAddHistory)
    {
        try
        {
            var product = await db.Products.OfType<ItemEntity>().FirstOrDefaultAsync(p => p.Id == productId);
            if (product == null)
            {
                log.LogWarning("Item {x} not found.", productId);
                return false;
            }

            product.Stock -= quantity;

            var newHash = Cryptics.ComputeHash(product);

            product.Hash = newHash;

            toAddHistory.Add(
                PropCopier.Copy(product,
                    new AddItemHistoryModel { ItemId = product.Id, Action = ActionType.Purchased.ToString() }));

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error modding item {x}. Exception: {ex}", productId, ex);
            return false;
        }
    }
}