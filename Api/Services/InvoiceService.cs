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
    IValidator<InvoiceItemModel> _invoiceItemValidator,
    UserManager<UserEntity> _userManager,
    IHttpContextAccessor _httpContextAccessor
// IValidator<VoidInvoiceModel> _voidValidator
) : IInvoiceService
{
    public async Task<ResponseInvoiceModel?> CreateSale(CreateInvoiceModel invoice)
    {
        var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User!);

        if (user == null)
            return null;

        _log.LogInformation("Create Invoice called.");
        var newInvoice = PropCopier.Copy(
            invoice,
            new InvoiceEntity { UserId = user.Id, IsVoided = false }
        );

        var responseInvoice = PropCopier.Copy(
            invoice,
            new ResponseInvoiceModel { InvoiceItems = null }
        );
        try
        {
            (bool isValid, string? error) = await SuperValidator.Check(_createValidator, invoice);

            if (!isValid && error != null)
            {
                _log.LogWarning("Invalid model state. {x}", error);
                return null;
            }

            foreach (var item in invoice.InvoiceItems)
            {
                (bool v, string? e) = await SuperValidator.Check(_invoiceItemValidator, item);

                if (!v && e != null)
                {
                    _log.LogWarning("Invalid model state. {x}", error);
                    return null;
                }

                newInvoice.InvoiceItems.Add(PropCopier.Copy(item, new InvoiceItemEntity()));
            }

            var result = await _db.Invoices.AddAsync(newInvoice);
            _log.LogInformation("Added invoice {}. Updating item stocks.", result.Entity.Id);

            if (result.Entity != null)
            {
                foreach (var iitem in result.Entity.InvoiceItems)
                {
                    var itemModResult = ItemMod(
                        iitem.Id,
                        ActionType.Purchased,
                        (int)iitem.ItemQuantity,
                        (int)iitem.UsesConsumed
                    );

                    responseInvoice.InvoiceItems.Add(
                        PropCopier.Copy(iitem, new InvoiceItemModel())
                    );

                    if (!itemModResult.Result)
                        throw new Exception();
                }
            }
            await _db.SaveChangesAsync();
            return responseInvoice;
        }
        catch (System.Exception)
        {
            return null;
        }
    }

    public async Task<List<ResponseInvoiceModel>?> GetSales()
    {
        throw new NotImplementedException();
    }

    public Task<bool> VoidSale(VoidInvoiceModel invoice)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ItemMod(Guid itemId, ActionType action, int quantity = 0, int uses = 0)
    {
        try
        {
            var item = await _db.Items.FindAsync(itemId);
            if (item == null)
                return false;

            item.Stock -= quantity;
            item.UsesLeft -= uses;

            string newHash = Cryptics.ComputeHash(item);
            await _ih.AddItemHistory(
                PropCopier.Copy(item, new AdddItemHistoryModel { Hash = newHash }),
                action
            );
            return true;
        }
        catch (System.Exception)
        {
            return false;
        }
    }
}
