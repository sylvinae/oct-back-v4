using API.Db;
using API.Entities.Invoice;
using API.Entities.User;
using API.Interfaces;
using API.Models.Invoice;
using API.Models.Item;
using API.Utils;
using API.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace API.Services;

public class InvoiceService(
    ILogger<InvoiceService> log,
    Context db,
    IItemHistoryService ih,
    IValidator<CreateInvoiceModel> createValidator,
    IValidator<InvoiceItemModel> invoiceItemValidator,
    UserManager<UserEntity> userManager,
    IHttpContextAccessor httpContextAccessor
) : IInvoiceService
{
    public async Task<(
        FailedResponseInvoiceModel? failed,
        ResponseInvoiceModel? success
        )> CreateInvoice(CreateInvoiceModel invoice)
    {
        log.LogInformation("Create Invoice called.");

        var failed = PropCopier.Copy(invoice, new FailedResponseInvoiceModel());
        var user = await userManager.GetUserAsync(httpContextAccessor.HttpContext?.User!);

        var newInvoice = PropCopier.Copy(invoice, new InvoiceEntity());
        var newInvoiceItems = new List<InvoiceItemEntity>();
        var responseInvoiceItems = new List<InvoiceItemModel>();
        var responseInvoice = new ResponseInvoiceModel();

        if (user == null)
        {
            failed.Error = "user is null";
            return (failed, null);
        }

        newInvoice.UserId = user.Id;
        newInvoice.IsVoided = false;

        log.LogInformation("Create Invoice called.");

        try
        {
            (bool? isValid, var error) = await SuperValidator.Check(createValidator, invoice);

            if (!(bool)isValid && error != null)
            {
                log.LogWarning("Invalid model state. {x}", error);
                failed.Error = error;
                return (failed, null);
            }

            foreach (var item in invoice.InvoiceItems)
            {
                (isValid, error) = await SuperValidator.Check(invoiceItemValidator, item);

                if (!(bool)isValid && error != null)
                {
                    log.LogWarning("Invalid model state. {x}", error);
                    failed.Error = error;
                    return (failed, null);
                }

                newInvoiceItems.Add(
                    PropCopier.Copy(item, new InvoiceItemEntity { InvoiceId = newInvoice.Id })
                );
            }

            newInvoice.InvoiceItems = newInvoiceItems;

            var result = await db.Invoices.AddAsync(newInvoice);
            log.LogInformation("Added Invoice {}. Updating item stocks.", result.Entity.Id);


            foreach (var iitem in result.Entity.InvoiceItems)
            {
                var sold = iitem.ItemsSold ?? 0;
                var uses = iitem.UsesConsumed ?? 0;

                var itemModResult = ItemMod(iitem.ItemId, ActionType.Purchased, sold, uses);

                responseInvoiceItems.Add(PropCopier.Copy(iitem, new InvoiceItemModel()));

                if (itemModResult.Result == false)
                    throw new Exception();
            }

            responseInvoice = PropCopier.Copy(
                invoice,
                new ResponseInvoiceModel { InvoiceItems = responseInvoiceItems }
            );


            await db.SaveChangesAsync();

            return (null, responseInvoice);
        }
        catch (Exception ex)
        {
            failed.Error = ex.ToString();
            return (failed, null);
        }
    }

    public IQueryable<InvoiceEntity> GetInvoices()
    {
        log.LogInformation("Get invoices called");
        return db.Invoices;
    }

    public async Task<bool> VoidInvoice(VoidInvoiceModel invoice)
    {
        try
        {
            log.LogInformation("Void Invoice called.");
            var invoiceItem = await db.Invoices.FindAsync(invoice.Id);
            if (invoiceItem == null)
                return false;

            invoiceItem.IsVoided = true;
            invoiceItem.VoidReason = invoice.VoidReason;

            foreach (var item in invoiceItem.InvoiceItems)
            {
                var sold = item.ItemsSold ?? 0;
                var uses = item.UsesConsumed ?? 0;
                var result = ItemMod(item.Id, ActionType.Voided, sold, uses);
            }

            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error voiding. {x}", ex.ToString());
            return false;
        }
    }

    public async Task<bool> ItemMod(Guid itemId, ActionType action, int quantity, int uses)
    {
        try
        {
            log.LogInformation("Modding item {x} with action {action}.", itemId, action);

            var item = await db.Items.FindAsync(itemId);
            if (item == null)
            {
                log.LogWarning("Item {x} not found.", itemId);
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

            var forIh = PropCopier.Copy(item, new UpdateItemModel { Hash = "" });

            if (action == ActionType.Purchased)
            {
                item.Stock -= quantity;
                item.UsesLeft -= uses;
                forIh.Stock -= quantity;
                forIh.UsesLeft -= uses;
            }
            else if (action == ActionType.Voided)
            {
                item.Stock += quantity;
                item.UsesLeft += uses;
                forIh.Stock += quantity;
                forIh.UsesLeft += uses;
            }

            var newHash = Cryptics.ComputeHash(forIh);
            item.Hash = newHash;

            await ih.AddItemHistory(
                PropCopier.Copy(item, new AddItemHistoryModel { ItemId = item.Id }),
                action
            );

            log.LogInformation(
                "Item {x} modded successfully with action {action}.",
                itemId,
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