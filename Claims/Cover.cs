using System.ComponentModel.DataAnnotations;
using Claims.Storage;
using Newtonsoft.Json;

namespace Claims;

public record CoverReadModel(
    [property: JsonProperty(PropertyName = "id")] string Id,
    [property: JsonProperty(PropertyName = "startDate")] DateOnly StartDate,
    [property: JsonProperty(PropertyName = "endDate")] DateOnly EndDate,
    [property: JsonProperty(PropertyName = "claimType")] CoverType Type,
    [property: JsonProperty(PropertyName = "premium")] decimal Premium)
{
    public static CoverReadModel FromDbModel(CoverDbModel dbModel) => new CoverReadModel(
        dbModel.Id,
        dbModel.StartDate,
        dbModel.EndDate,
        dbModel.Type,
        dbModel.Premium);
}

public record CoverWriteModel
(
    [property: JsonProperty(PropertyName = "startDate")] DateOnly StartDate,
    [property: JsonProperty(PropertyName = "endDate")] DateOnly EndDate,
    [property: JsonProperty(PropertyName = "claimType")] CoverType Type)
    : IValidatableObject
{
    public CoverDbModel ToDbModel(Guid id, decimal premium) => new CoverDbModel
    {
        Id = id.ToString(),
        StartDate = StartDate,
        EndDate = EndDate,
        Type = Type,
        Premium = premium
    };

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate >= EndDate)
        {
            yield return new ValidationResult("Start date can't be greater than or equal to end date", new[] { nameof(StartDate), nameof(EndDate) });
        }

        if (EndDate.ToDateTime(TimeOnly.MinValue) - StartDate.ToDateTime(TimeOnly.MinValue) > TimeSpan.FromDays(365))
        {
            yield return new ValidationResult("Cover can't be longer than a year", new[] { nameof(StartDate), nameof(EndDate) });
        }
    }
}

public enum CoverType
{
    Yacht = 0,
    PassengerShip = 1,
    ContainerShip = 2,
    BulkCarrier = 3,
    Tanker = 4
}