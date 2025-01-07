using API.Db;
using API.Entities.Bundles;
using API.Entities.Item;
using API.Models;
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
            var toAddHistory = new List<CreateItemHistoryModel>();
            log.LogInformation("Void Interfaces called.");
            var invoiceEntity =
                await db.Invoices.Include(i => i.InvoiceItems).FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (invoiceEntity == null)
                return false;

            invoiceEntity.IsVoided = true;
            invoiceEntity.VoidReason = invoice.VoidReason;


            foreach (var item in invoiceEntity.InvoiceItems) log.LogInformation("Returning item {x}", item.Id);
            // return result;

            await db.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            log.LogError("Error voiding. {x}", ex.ToString());
            return false;
        }
    }


    private bool ProductMod(ItemEntity item, int quantity, List<CreateItemHistoryModel> toAddHistory)
    {
        try
        {
            item.Stock -= quantity;
            item.IsLow = item.Stock <= item.LowThreshold;
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

            var result = ProductMod(item, bundleItem.Quantity, toAddHistory);
            if (!result)
                return false;
        }

        return true;
    }
}