using API.Db;
using API.Entities.Invoice;
using API.Entities.User;
using API.Models;
using API.Models.Invoice;
using API.Models.Item;
using API.Services.Invoice.Interfaces;
using API.Services.Item.Interfaces;
using API.Utils;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

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
            newInvoiceItemsEntity.Add(PropCopier.Copy(item, new InvoiceItemEntity { InvoiceId = newInvoiceEntity.Id }
            ));

            var sold = item.ItemsSold ?? 0;
            var used = item.UsesConsumed ?? 0;

            var result = ItemMod(item.ItemId, used, sold, toAddHistory);

            if (!result.Result)
                throw new Exception();
        }

        newInvoiceEntity.InvoiceItems = newInvoiceItemsEntity;

        await db.Invoices.AddAsync(newInvoiceEntity);
        await ih.AddItemHistoryRange(toAddHistory);
        await db.SaveChangesAsync();
        return (true, null);
    }

    private async Task<bool> ItemMod(Guid itemId, int uses, int quantity, List<AddItemHistoryModel> toAddHistory)
    {
        try
        {
            var item = await db.Items.FindAsync(itemId);
            if (item == null)
            {
                log.LogWarning("Item {x} not found.", itemId);
                return false;
            }


            var newHash = Cryptics.ComputeHash(item);

            // if (uses != 0 && item.UsesLeft < uses)
            // {
            //     var temp = uses - item.UsesLeft;
            //
            //     item.Stock -= 1;
            //     item.UsesLeft = item.Stock > 0 ? item.UsesMax : null;
            //
            //     if (temp > 0 && item.Stock > 0) item.UsesLeft = item.UsesLeft - temp > 0 ? item.UsesLeft - temp : 0;
            // }
            // else
            // {
            //     item.Stock -= quantity;
            //     item.UsesLeft -= uses;
            // }

            item.Hash = newHash;

            toAddHistory.Add(
                PropCopier.Copy(item,
                    new AddItemHistoryModel { ItemId = item.Id, Action = ActionType.Purchased.ToString() }));

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error modding item {x}. Exception: {ex}", itemId, ex);
            return false;
        }
    }
}