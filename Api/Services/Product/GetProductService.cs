using API.Db;
using API.Entities.Products;
using API.Services.Item;
using API.Services.Product.Interfaces;

namespace API.Services.Product;

public class GetProductService(
    Context db,
    ILogger<GetItemService> log) : IGetProductService
{
    public IQueryable<ProductEntity> GetAllProducts()
    {
        log.LogInformation("Getting all products.");
        return db.Products;
    }
}