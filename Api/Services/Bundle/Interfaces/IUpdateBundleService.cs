using API.Models.Bundles;

namespace API.Services.Bundle.Interfaces;

public interface IUpdateBundleService
{
    Task<bool> UpdateBundle(UpdateBundleModel bundle);
}