using API.Models.Bundles;

namespace API.Services.Bundle.Interfaces;

public interface ICreateBundleService
{
    Task<bool> CreateBundle(CreateBundleModel createBundle);
}