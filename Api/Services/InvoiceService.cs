using System.Linq.Expressions;
using System.Xml.Schema;
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
    ILogger<InvoiceService> _log,
    Context _db,
    IItemHistoryService _ih,
    IValidator<CreateInvoiceModel> _createValidator,
    IValidator<InvoiceItemModel> _InvoiceItemValidator,
    UserManager<UserEntity> _userManager,
    IHttpContextAccessor _httpContextAccessor
) : IInvoiceService
{
    public async Task<(
        FailedResponseInvoiceModel? failed,
        ResponseInvoiceModel? success
    )> CreateInvoice(CreateInvoiceModel invoice)
    {
        _log.LogInformation("Create Invoice called.");

        var failed = PropCopier.Copy(invoice, new FailedResponseInvoiceModel());
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);

        var newInvoice = PropCopier.Copy(invoice, new InvoiceEntity());
        var newInvoiceItems = new List<InvoiceItemEntity>();
        var responseInvoiceItems = new List<InvoiceItemModel>();
        var responseInvoice = new ResponseInvoiceModel();
        bool? isValid;
        string? error;

        if (user == null)
        {
            failed.Error = "user is null";
            return (failed, null);
        }

        newInvoice.UserId = user.Id;
        newInvoice.IsVoided = false;

        _log.LogInformation("Create Invoice called.");

        try
        {
            (isValid, error) = await SuperValidator.Check(_createValidator, invoice);

            if (!(bool)isValid && error != null)
            {
                _log.LogWarning("Invalid model state. {x}", error);
                failed.Error = error;
                return (failed, null);
            }

            foreach (var item in invoice.InvoiceItems)
            {
                (isValid, error) = await SuperValidator.Check(_InvoiceItemValidator, item);

                if (!(bool)isValid && error != null)
                {
                    _log.LogWarning("Invalid model state. {x}", error);
                    failed.Error = error;
                    return (failed, null);
                }

                newInvoiceItems.Add(
                    PropCopier.Copy(item, new InvoiceItemEntity { InvoiceId = newInvoice.Id })
                );
            }
            newInvoice.InvoiceItems = newInvoiceItems;

            var result = await _db.Invoices.AddAsync(newInvoice);
            _log.LogInformation("Added Invoice {}. Updating item stocks.", result.Entity.Id);

            if (result.Entity != null)
            {
                foreach (var iitem in result.Entity.InvoiceItems)
                {
                    int sold = iitem.ItemsSold == null ? 0 : (int)iitem.ItemsSold;
                    int uses = iitem.UsesConsumed == null ? 0 : (int)iitem.UsesConsumed;

                    var itemModResult = ItemMod(iitem.ItemId, ActionType.Purchased, sold, uses);

                    responseInvoiceItems.Add(PropCopier.Copy(iitem, new InvoiceItemModel()));

                    if (itemModResult.Result == false)
                        throw new Exception();
                }
                responseInvoice = PropCopier.Copy(
                    invoice,
                    new ResponseInvoiceModel { InvoiceItems = responseInvoiceItems }
                );
            }
            await _db.SaveChangesAsync();

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
        _log.LogInformation("Get invoices called");
        return _db.Invoices;
    }

    public async Task<bool> VoidInvoice(VoidInvoiceModel Invoice)
    {
        try
        {
            _log.LogInformation("Void Invoice called.");
            var InvoiceItem = await _db.Invoices.FindAsync(Invoice.Id);
            if (InvoiceItem == null)
                return false;

            InvoiceItem.IsVoided = true;
            InvoiceItem.VoidReason = Invoice.VoidReason;

            foreach (var item in InvoiceItem.InvoiceItems)
            {
                int sold = item.ItemsSold == null ? 0 : (int)item.ItemsSold;
                int uses = item.UsesConsumed == null ? 0 : (int)item.UsesConsumed;
                var result = ItemMod(item.Id, ActionType.Voided, sold, uses);
            }

            await _db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError("Error voiding. {x}", ex.ToString());
            return false;
        }
    }

    public async Task<bool> ItemMod(Guid itemId, ActionType action, int quantity, int uses)
    {
        try
        {
            _log.LogInformation("Modding item {x} with action {action}.", itemId, action);

            var item = await _db.Items.FindAsync(itemId);
            if (item == null)
            {
                _log.LogWarning("Item {x} not found.", itemId);
                return false;
            }

            if (quantity == 0 && uses == 0)
            {
                _log.LogWarning(
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

            string newHash = Cryptics.ComputeHash(forIh);
            item.Hash = newHash;

            await _ih.AddItemHistory(
                PropCopier.Copy(item, new AddItemHistoryModel { ItemId = item.Id }),
                action
            );

            _log.LogInformation(
                "Item {x} modded successfully with action {action}.",
                itemId,
                action
            );
            return true;
        }
        catch (Exception ex)
        {
            _log.LogError("Error modding item {x}. Exception: {ex}", itemId, ex);
            return false;
        }
    }
}
