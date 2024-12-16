using API.Db;
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
            var invoiceItem =
                await db.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (invoiceItem == null)
                return false;

            invoiceItem.IsVoided = true;
            invoiceItem.VoidReason = invoice.VoidReason;


            foreach (var item in invoiceItem.InvoiceItems)
            {
                log.LogInformation("Returning item {x}", item.Id);
                var sold = item.ItemsSold ?? 0;
                var uses = item.UsesConsumed ?? 0;
                var itemMod = ItemMod(item.ItemId, sold, uses);

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

    private async Task<bool> ItemMod(Guid itemId, int uses, int quantity)
    {
        const ActionType action = ActionType.Voided;
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


            item.Stock += quantity;
            item.UsesLeft += uses;


            var newHash = Cryptics.ComputeHash(item);
            item.Hash = newHash;

            log.LogInformation("Adding {x} history to {y}", action, item);
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