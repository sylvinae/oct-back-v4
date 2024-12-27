namespace API.Models.Bundles;

public class BaseBundleModel
{
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Barcode { get; set; }
    public bool IsDeleted { get; set; }
}

public class CreateBundle : BaseBundleModel
{
    public List<Guid>? Items { get; set; } = [];
}

public class UpdateBundle : BaseBundleModel
{
    public Guid Id { get; set; }
    public List<Guid>? Items { get; set; } = [];
}

public class DeleteBundle
{
    public Guid Id { get; set; }
}