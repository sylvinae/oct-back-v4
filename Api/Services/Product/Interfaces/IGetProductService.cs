using API.Entities.Products;

namespace API.Services.Product.Interfaces;

public interface IGetProductService
{
    IQueryable<ProductEntity> GetAllProducts();
}