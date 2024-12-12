using Newtonsoft.Json;

namespace API.Models.Item;

public class BaseItemHistoryModel
{
    [JsonProperty("itemId")] public Guid ItemId { get; set; }

    [JsonProperty("userId")] public Guid UserId { get; set; }

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

    [JsonProperty("isLow")] public bool IsLow { get; set; }

    [JsonProperty("isReagent")] public bool IsReagent { get; set; }

    [JsonProperty("usesLeft")] public int? UsesLeft { get; set; }

    [JsonProperty("usesMax")] public int? UsesMax { get; set; }

    [JsonProperty("hasExpiry")] public bool HasExpiry { get; set; }

    [JsonProperty("expiry")] public DateTime? Expiry { get; set; }

    [JsonProperty("isExpired")] public bool IsExpired { get; set; }

    [JsonProperty("isDeleted")] public bool IsDeleted { get; set; }

    [JsonProperty("hash")] public string Hash { get; set; } = null!;

    [JsonProperty("action")] public string? Action { get; set; }
}

public class AddItemHistoryModel : BaseItemHistoryModel;

public class ResponseItemHistoryModel : BaseItemHistoryModel
{
    [JsonProperty("id")] public Guid Id { get; set; }
}

public enum ActionType
{
    Created,
    Updated,
    Deleted,
    Restored,
    Voided,
    Purchased,
    Restocked
}