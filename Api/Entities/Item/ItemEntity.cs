using API.Entities.Products;

namespace API.Entities.Item;

public class ItemEntity : ProductEntity
{
    public string? Brand { get; set; }
    public string? Generic { get; set; }
    public string? Company { get; set; }
    public string Hash { get; set; } = null!;
    public int LowThreshold { get; set; }

    public DateTime? Expiry { get; set; }

    public bool IsReagent { get; set; }
    public bool IsLow { get; set; }
    public bool IsExpired { get; set; }
    public bool IsDeleted { get; set; }

    public ICollection<ItemHistoryEntity> ItemHistory { get; set; } = new List<ItemHistoryEntity>();
}