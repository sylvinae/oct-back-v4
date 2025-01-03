using API.Entities.Bundles;

namespace API.Services.Bundle.Interfaces;

public interface IGetBundleService
{
    IQueryable<BundleEntity> GetAllBundles();
}