using Claims.Domain;
using Newtonsoft.Json;

namespace Claims.Storage;

public record ClaimDbModel : ICosmosEntity
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; init; }

    [JsonProperty(PropertyName = "coverId")]
    public string CoverId { get; init; }

    [JsonProperty(PropertyName = "created")]
    public DateTime Created { get; init; }

    [JsonProperty(PropertyName = "name")]
    public string Name { get; init; }

    [JsonProperty(PropertyName = "claimType")]
    public ClaimType Type { get; init; }

    [JsonProperty(PropertyName = "damageCost")]
    public decimal DamageCost { get; init; }
}