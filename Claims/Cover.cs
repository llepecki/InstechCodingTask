using System.Text.Json.Serialization;
using Claims.Domain;

namespace Claims;

public record CoverReadModel(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("startDate")] DateOnly StartDate,
    [property: JsonPropertyName("endDate")] DateOnly EndDate,
    [property: JsonPropertyName("claimType")] CoverType Type,
    [property: JsonPropertyName("premium")] decimal Premium);

public record CoverWriteModel(
    [property: JsonPropertyName("startDate")] DateOnly StartDate,
    [property: JsonPropertyName("endDate")] DateOnly EndDate,
    [property: JsonPropertyName("claimType")] CoverType Type);