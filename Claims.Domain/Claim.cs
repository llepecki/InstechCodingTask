namespace Claims.Domain;

using CSharpFunctionalExtensions;

public enum ClaimType
{
    Collision = 0,
    Grounding = 1,
    BadWeather = 2,
    Fire = 3
}

public record Claim
{
    public static Result<Claim, IReadOnlyDictionary<string, string>> Create(Cover cover, DateTime created, string name, ClaimType type, decimal damageCost)
    {
        var errors = new Dictionary<string, string>();

        DateTime coverPeriodStart = cover.StartDate.ToDateTime(TimeOnly.MinValue);
        DateTime coverPeriodEnd = cover.EndDate.ToDateTime(TimeOnly.MaxValue);

        if (created < coverPeriodStart || created > coverPeriodEnd)
        {
            errors.Add(nameof(created), $"Claim date is not within cover period (from {coverPeriodStart} to {coverPeriodEnd})");
        }

        const decimal maxDamageCost = 100000;

        if (damageCost > maxDamageCost)
        {
            errors.Add(nameof(damageCost), $"Damage cost can't be more than {maxDamageCost}");
        }

        if (errors.Count == 0)
        {
            return Result.Success<Claim, IReadOnlyDictionary<string, string>>(new Claim
            {
                Id = Guid.NewGuid(),
                Cover = cover,
                Created = created,
                Name = name,
                Type = type,
                DamageCost = damageCost
            });
        }

        return Result.Failure<Claim, IReadOnlyDictionary<string, string>>(errors);
    }

    public Guid Id { get; init; }

    public Cover Cover { get; init; }

    public DateTime Created { get; init; }

    public string Name { get; init; }

    public ClaimType Type { get; init; }
    public decimal DamageCost { get; init; }
}