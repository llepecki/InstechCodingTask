namespace Claims.Services;

public interface IComputePremium
{
    decimal Compute(DateOnly startDate, DateOnly endDate, CoverType coverType);
}

public class ComputePremium : IComputePremium
{
    public decimal Compute(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        if (startDate > endDate) throw new ArgumentException("Start date must be before end date", nameof(startDate));

        var multiplier = GetMultiplier(coverType);
        var premiumPerDay = 1250 * multiplier;
        var insuranceLength = endDate.DayNumber - startDate.DayNumber;

        return ComputeTotalPremium(insuranceLength, premiumPerDay, coverType);
    }

    private static decimal GetMultiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m,
    };

    private decimal GetDiscountRate(int dayNumber, CoverType coverType)
    {
        if (dayNumber < 30) return 0m;
        if (dayNumber < 180) return coverType == CoverType.Yacht ? 0.05m : 0.02m;
        if (dayNumber < 365) return coverType == CoverType.Yacht ? 0.08m : 0.03m;
        return 0m;
    }

    private decimal ComputeTotalPremium(int insuranceLength, decimal premiumPerDay, CoverType coverType) => 
        Enumerable
            .Range(0, insuranceLength)
            .Select(dayNumber => premiumPerDay * (1 - GetDiscountRate(dayNumber, coverType)))
            .Sum();
}