using API.Db;
using API.Entities.Bundles;
using API.Services.Bundle.Interfaces;

namespace API.Services.Bundle;

public class GetBundleService(ILogger<GetBundleService> log, Context db) : IGetBundleService
{
    public IQueryable<BundleEntity> GetAllBundles()
    {
        log.LogInformation("Getting all bundles.");
        return db.Products.OfType<BundleEntity>();
    }
}