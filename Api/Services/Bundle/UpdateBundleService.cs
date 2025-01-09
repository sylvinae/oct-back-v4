using API.Db;
using API.Entities.Bundles;
using API.Models.Bundles;
using API.Services.Bundle.Interfaces;
using API.Services.Item;
using API.Services.Item.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Bundle;

public class UpdateBundleService(
    Context db,
    ILogger<UpdateItemService> log,
    IItemHistoryService ih,
    IValidator<UpdateBundleModel> updateValidator) : IUpdateBundleService
{
    public async Task<bool> UpdateBundle(UpdateBundleModel bundle)
    {
        log.LogInformation("Updating bundle {x}", bundle.Id);

        var validationResult = await updateValidator.ValidateAsync(bundle);
        if (!validationResult.IsValid)
            return false;

        var existingBundle = await db.Products.OfType<BundleEntity>()
            .Include(b => b.BundleItems)
            .FirstOrDefaultAsync(b => b.Id == bundle.Id);

        if (existingBundle == null)
            return false;

        var currentItems = existingBundle.BundleItems.ToDictionary(bi => bi.ItemId);
        var updatedItems = bundle.Items.ToDictionary(ui => ui.ItemId);

        foreach (var updatedItem in updatedItems.Values)
            if (!currentItems.TryGetValue(updatedItem.ItemId, out var existingItem))
            {
                existingBundle.BundleItems.Add(new BundleItemEntity
                {
                    BundleId = bundle.Id,
                    ItemId = updatedItem.ItemId,
                    Quantity = updatedItem.Quantity,
                    Uses = updatedItem.Uses
                });
            }
            else if (existingItem.Quantity != updatedItem.Quantity || existingItem.Uses != updatedItem.Uses)
            {
                existingItem.Quantity = updatedItem.Quantity;
                existingItem.Uses = updatedItem.Uses;
            }

        foreach (var currentItem in currentItems.Values.Where(currentItem =>
                     !updatedItems.ContainsKey(currentItem.ItemId)))
            existingBundle.BundleItems.Remove(currentItem);


        existingBundle.BundleName = bundle.BundleName;
        existingBundle.RetailPrice = bundle.RetailPrice;
        existingBundle.Barcode = bundle.Barcode;


        await db.SaveChangesAsync();
        return true;
    }
}