namespace Claims.Domain;

public static class ComputePremium
{
    public static decimal Compute(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        if (startDate > endDate) throw new ArgumentException("Start date must be before end date", nameof(startDate));

        var multiplier = GetMultiplier(coverType);
        var premiumPerDay = 1250 * multiplier;
        var totalDays = endDate.DayNumber - startDate.DayNumber;

        var firstPeriodDays = Math.Min(totalDays, 30);
        var secondPeriodDays = Math.Min(totalDays - firstPeriodDays, 150);
        var thirdPeriodDays = totalDays - firstPeriodDays - secondPeriodDays;

        var firstPeriodPremium = firstPeriodDays * premiumPerDay;
        var secondPeriodPremium = secondPeriodDays * premiumPerDay * (1 - GetDiscountRate(1, coverType));
        var thirdPeriodPremium = thirdPeriodDays * premiumPerDay * (1 - GetDiscountRate(2, coverType));

        return firstPeriodPremium + secondPeriodPremium + thirdPeriodPremium;
    }

    private static decimal GetMultiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m,
    };

    private static decimal GetDiscountRate(int period, CoverType coverType)
    {
        if (period == 1) return coverType == CoverType.Yacht ? 0.05m : 0.02m;
        if (period == 2) return coverType == CoverType.Yacht ? 0.08m : 0.03m;
        return 0m;
    }
}