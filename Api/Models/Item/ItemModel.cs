using Newtonsoft.Json;

namespace API.Models.Item;

public class BaseItemModel
{
    [JsonProperty("barcode")] public string? Barcode { get; set; }

    [JsonProperty("brand")] public string? Brand { get; set; }

    [JsonProperty("generic")] public string? Generic { get; set; }

    [JsonProperty("classification")] public string? Classification { get; set; }

    [JsonProperty("formulation")] public string? Formulation { get; set; }

    [JsonProperty("location")] public string? Location { get; set; }

    [JsonProperty("company")] public string? Company { get; set; }

    [JsonProperty("wholesale")] public decimal Wholesale { get; set; }

    [JsonProperty("retail")] public decimal Retail { get; set; }

    [JsonProperty("stock")] public int Stock { get; set; }

    [JsonProperty("lowThreshold")] public int LowThreshold { get; set; }

    [JsonProperty("isReagent")] public bool IsReagent { get; set; }

    [JsonProperty("usesLeft")] public int? UsesLeft { get; set; }

    [JsonProperty("usesMax")] public int? UsesMax { get; set; }

    [JsonProperty("hasExpiry")] public bool HasExpiry { get; set; }

    [JsonProperty("expiry")] public DateTime? Expiry { get; set; }
}

public class CreateItemModel : BaseItemModel
{
}

public class UpdateItemModel : BaseItemModel
{
    [JsonProperty("id")] public Guid Id { get; set; }

    [JsonProperty("hash")] public string Hash { get; set; } = null!;
}

public class ResponseItemModel : BaseItemModel
{
    [JsonProperty("id")] public Guid Id { get; set; }

    [JsonProperty("hash")] public string Hash { get; set; } = null!;

    [JsonProperty("isLow")] public bool IsLow { get; set; }

    [JsonProperty("isExpired")] public bool IsExpired { get; set; }

    [JsonProperty("isDeleted")] public bool IsDeleted { get; set; }

    [JsonProperty("itemHistory")] public ICollection<ResponseItemHistoryModel>? ItemHistory { get; set; } = [];
}

public class FailedResponseItemModel : ResponseItemModel
{
    [JsonProperty("error")] public string Error { get; set; } = null!;
}