using API.Db;
using API.Entities.Item;
using API.Models.Invoice;
using API.Models.Item;
using API.Services.Invoice.Interfaces;
using API.Services.Item.Interfaces;
using API.Utils;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Invoice;

public class VoidInvoiceService(
    ILogger<VoidInvoiceService> log,
    Context db,
    IItemHistoryService ih
) : IVoidInvoiceService
{
    public async Task<bool> VoidInvoice(VoidInvoiceModel invoice)
    {
        try
        {
            log.LogInformation("Void Interfaces called.");
            var invoiceEntity =
                await db.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (invoiceEntity == null)
                return false;

            invoiceEntity.IsVoided = true;
            invoiceEntity.VoidReason = invoice.VoidReason;


            foreach (var item in invoiceEntity.InvoiceItems)
            {
                log.LogInformation("Returning item {x}", item.Id);

                var itemMod = ItemMod(item.ProductId, item.QuantitySold);

                if (!itemMod.Result) return false;
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

    private async Task<bool> ItemMod(Guid itemId, int quantity)
    {
        const ActionType action = ActionType.Voided;
        try
        {
            log.LogInformation("Modding item {x} with action {action}.", itemId, action);

            var product = await db.Products.OfType<ItemEntity>().FirstOrDefaultAsync(p => p.Id == itemId);
            if (product == null)
            {
                log.LogWarning("Interfaces {x} not found.", itemId);
                return false;
            }

            product.Stock += quantity;


            var newHash = Cryptics.ComputeHash(product);
            product.Hash = newHash;

            log.LogInformation("Adding {x} history to {y}", action, product);
            await ih.AddItemHistory(
                PropCopier.Copy(product,
                    new AddItemHistoryModel { ItemId = product.Id, Action = ActionType.Voided.ToString() })
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