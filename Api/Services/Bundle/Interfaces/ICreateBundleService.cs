using API.Models.Bundles;

namespace API.Services.Bundle.Interfaces;

public interface ICreateBundle
{
    Task<bool> CreateBundle(CreateBundleModel createBundle);
}