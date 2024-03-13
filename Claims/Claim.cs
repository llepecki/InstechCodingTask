using Claims.Storage;
using Newtonsoft.Json;

namespace Claims;

public record ClaimReadModel(
    [property: JsonProperty(PropertyName = "id")] string Id,
    [property: JsonProperty(PropertyName = "coverId")] string CoverId,
    [property: JsonProperty(PropertyName = "created")] DateTime Created,
    [property: JsonProperty(PropertyName = "name")] string Name,
    [property: JsonProperty(PropertyName = "claimType")] ClaimType Type,
    [property: JsonProperty(PropertyName = "damageCost")] decimal DamageCost)
{
    public static ClaimReadModel FromDbModel(ClaimDbModel dbModel) => new ClaimReadModel(
        dbModel.Id,
        dbModel.CoverId,
        dbModel.Created,
        dbModel.Name,
        dbModel.Type,
        dbModel.DamageCost);
}

public record ClaimWriteModel(
    [property: JsonProperty(PropertyName = "coverId")] string CoverId,
    [property: JsonProperty(PropertyName = "created")] DateTime Created,
    [property: JsonProperty(PropertyName = "name")] string Name,
    [property: JsonProperty(PropertyName = "claimType")] ClaimType Type,
    [property: JsonProperty(PropertyName = "damageCost")] decimal DamageCost)
{
    public ClaimDbModel ToDbModel(Guid id) => new ClaimDbModel
    {
        Id = id.ToString(),
        CoverId = CoverId,
        Created = Created,
        Name = Name,
        Type = Type,
        DamageCost = DamageCost
    };
}

public enum ClaimType
{
    Collision = 0,
    Grounding = 1,
    BadWeather = 2,
    Fire = 3
}
