using API.Db;
using API.Entities.Bundles;
using API.Models.Bundles;
using API.Services.Bundle.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace API.Services.Bundle;

public class UpdateBundleService(
    Context db,
    ILogger<UpdateBundleService> log,
    IValidator<UpdateBundleModel> updateValidator) : IUpdateBundleService
{
    public async Task<bool> UpdateBundle(UpdateBundleModel bundle)
    {
        log.LogInformation("Updating bundle {x}", bundle.Id);

        var validationResult = await updateValidator.ValidateAsync(bundle);
        if (!validationResult.IsValid)
        {
            log.LogInformation("Validation failed");
            foreach (var error in validationResult.Errors)
                log.LogInformation("Validation error: {x}", error.ErrorMessage);

            return false;
        }

        var existingBundle = await db.Products.OfType<BundleEntity>()
            .Include(b => b.BundleItems)
            .FirstOrDefaultAsync(b => b.Id == bundle.Id);

        if (existingBundle == null)
        {
            log.LogInformation("Bundle not found");
            return false;
        }

        existingBundle.BundleName = bundle.BundleName;
        existingBundle.RetailPrice = bundle.RetailPrice;
        existingBundle.Barcode = bundle.Barcode;
        log.LogInformation("removing");

        db.BundleItems.RemoveRange(existingBundle.BundleItems);

        log.LogInformation("removed");
        var newBundleItems = bundle.BundleItems.Select(bi => new BundleItemEntity
        {
            BundleId = bundle.Id,
            ItemId = bi.ItemId,
            Quantity = bi.Quantity,
            Uses = bi.Uses
        }).ToList();

        log.LogInformation("made new bundleitems");
        log.LogInformation("added new items");

        db.BundleItems.AddRange(newBundleItems);
        await db.SaveChangesAsync();

        log.LogInformation("Bundle updated successfully");
        return true;
    }
}