namespace Claims.Domain;

using CSharpFunctionalExtensions;

public enum CoverType
{
    Yacht = 0,
    PassengerShip = 1,
    ContainerShip = 2,
    BulkCarrier = 3,
    Tanker = 4
}

public record Cover
{
    public static Result<Cover, IReadOnlyDictionary<string, string>> Create(DateOnly startDate, DateOnly endDate, CoverType type)
    {
        var errors = new Dictionary<string, string>();
        
        if (startDate >= endDate)
        {
            errors.Add($"{startDate}_{endDate}", "Start date can't be greater than or equal to end date");
        }

        if (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue) > TimeSpan.FromDays(365))
        {
            errors.Add("coverPeriod", "Cover can't be longer than a year");
        }

        if (errors.Count == 0)
        {
            decimal premium = ComputePremium.Compute(startDate, endDate, type);

            return Result.Success<Cover, IReadOnlyDictionary<string, string>>(new Cover
            {
                Id = Guid.NewGuid(),
                StartDate = startDate,
                EndDate = endDate,
                Type = type,
                Premium = premium
            });
        }
        
        return Result.Failure<Cover, IReadOnlyDictionary<string, string>>(errors);
    }

    public Guid Id { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public CoverType Type { get; init; }
    
    public decimal Premium { get; init; }
}