using API.Db;
using API.Entities.Invoice;
using API.Entities.User;
using API.Models.Invoice;
using API.Models.Item;
using API.Services.Invoice.Interfaces;
using API.Services.Item.Interfaces;
using API.Utils;
using API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace API.Services.Invoice;

public class CreateInvoiceService(
    ILogger<CreateInvoiceService> log,
    Context db,
    IItemHistoryService ih,
    IValidator<CreateInvoiceModel> createValidator,
    IValidator<InvoiceItemModel> invoiceItemValidator,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor) : ICreateInvoiceService
{
    public async Task<(FailedResponseInvoiceModel? failed, ResponseInvoiceModel? success)> CreateInvoice(
        CreateInvoiceModel invoice)
    {
        log.LogInformation("Create invoice called.");
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext!.User);
        var failed = PropCopier.Copy(invoice, new FailedResponseInvoiceModel());

        var newInvoiceEntity = PropCopier.Copy(invoice, new InvoiceEntity { UserId = user!.Id, IsVoided = false });
        var newInvoiceItemsEntity = new List<InvoiceItemEntity>();
        var responseInvoiceItems = new List<InvoiceItemModel>();
        var responseInvoice = PropCopier.Copy(newInvoiceEntity, new ResponseInvoiceModel());

        var (isValid, error) = await SuperValidator.Check(createValidator, invoice);
        if (!isValid)
        {
            failed.Error = error;
            return (failed, null);
        }

        foreach (var item in invoice.InvoiceItems)
        {
            (isValid, error) = await SuperValidator.Check(invoiceItemValidator, item);

            if (!isValid)
            {
                failed.Error = error;
                return (failed, null);
            }

            responseInvoiceItems.Add(item);
            newInvoiceItemsEntity.Add(PropCopier.Copy(item, new InvoiceItemEntity { InvoiceId = newInvoiceEntity.Id }
            ));

            var sold = item.ItemsSold ?? 0;
            var used = item.UsesConsumed ?? 0;

            var result = ItemMod(item.ItemId, used, sold);

            if (result.Result) continue;
            failed.Error = "Failed to deduct item quantity/uses.";
            return (failed, null);
        }

        responseInvoice.InvoiceItems = responseInvoiceItems;
        newInvoiceEntity.InvoiceItems = newInvoiceItemsEntity;
        await db.Invoices.AddAsync(newInvoiceEntity);
        await db.SaveChangesAsync();
        return (null, responseInvoice);
    }

    private async Task<bool> ItemMod(Guid itemId, int uses, int quantity)
    {
        const ActionType action = ActionType.Purchased;
        try
        {
            log.LogInformation("Modding item {x} with action {action}.", itemId, action);

            var item = await db.Items.FindAsync(itemId);
            if (item == null)
            {
                log.LogWarning("Interfaces {x} not found.", itemId);
                return false;
            }

            if (quantity == 0 && uses == 0)
            {
                log.LogWarning(
                    "No modification needed for item {x}. Quantity and uses are zero.",
                    itemId
                );
                return false;
            }

            var forIh = PropCopier.Copy(item, new UpdateItemModel());


            item.Stock -= quantity;
            item.UsesLeft -= uses;
            forIh.Stock -= quantity;
            forIh.UsesLeft -= uses;


            var newHash = Cryptics.ComputeHash(forIh);
            item.Hash = newHash;

            await ih.AddItemHistory(
                PropCopier.Copy(item, new AddItemHistoryModel { ItemId = item.Id }),
                action
            );

            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error modding item {x}. Exception: {ex}", itemId, ex);
            return false;
        }
    }
}