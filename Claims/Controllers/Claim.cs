using System.Text.Json.Serialization;
using Claims.Domain;

namespace Claims.Controllers;

public record ClaimReadModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("coverId")] Guid CoverId,
    [property: JsonPropertyName("created")] DateTime Created,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("claimType")] ClaimType Type,
    [property: JsonPropertyName("damageCost")] decimal DamageCost);

public record ClaimWriteModel(
    [property: JsonPropertyName("coverId")] Guid CoverId,
    [property: JsonPropertyName("created")] DateTime Created,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("claimType")] ClaimType Type,
    [property: JsonPropertyName("damageCost")] decimal DamageCost);