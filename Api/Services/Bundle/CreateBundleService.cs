using API.Db;
using API.Entities.Bundles;
using API.Entities.User;
using API.Models.Bundles;
using API.Services.Bundle.Interfaces;
using API.Services.Item.Interfaces;
using API.Utils;
using Microsoft.AspNetCore.Identity;

namespace API.Services.Bundle;

public class CreateBundleService(
    ILogger<CreateBundleService> log,
    Context db,
    UserManager<UserEntity> userManager,
    IItemHistoryService ih,
    IHttpContextAccessor httpContextAccessor
) : ICreateBundleService
{
    public async Task<bool> CreateBundle(CreateBundleModel createBundle)
    {
        try
        {
            var bundle = PropCopier.Copy(createBundle, new BundleEntity { IsDeleted = false });

            foreach (var item in createBundle.Items)
                bundle.BundleItems.Add(PropCopier.Copy(item, new BundleItemEntity { BundleId = bundle.Id }));

            await db.Products.AddAsync(bundle);
            await db.SaveChangesAsync();

            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
}