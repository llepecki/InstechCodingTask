using Newtonsoft.Json;

namespace Claims.Storage;

public record CoverDbModel : ICosmosEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; init; }

    [JsonProperty(PropertyName = "startDate")]
    public DateOnly StartDate { get; init; }

    [JsonProperty(PropertyName = "endDate")]
    public DateOnly EndDate { get; init; }

    [JsonProperty(PropertyName = "claimType")]
    public CoverType Type { get; init; }

    [JsonProperty(PropertyName = "premium")]
    public decimal Premium { get; init; }
}
