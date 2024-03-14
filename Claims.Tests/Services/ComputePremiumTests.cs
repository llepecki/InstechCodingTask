using Xunit;
using Claims.Services;

namespace Claims.Tests;

public class ComputePremiumTests
{
    private readonly ComputePremium _computePremium;

    public ComputePremiumTests() => _computePremium = new ComputePremium();

    [Fact]
    public void Compute_ThrowsException_WhenStartDateIsAfterEndDate()
    {
        Assert.Throws<ArgumentException>(() => _computePremium.Compute(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), DateOnly.FromDateTime(DateTime.UtcNow), CoverType.Yacht));
    }

    [Theory]
    [InlineData(CoverType.Yacht, 1, 1.1 * 1250)]
    [InlineData(CoverType.PassengerShip, 1, 1.2 * 1250)]
    [InlineData(CoverType.Tanker, 1, 1.5 * 1250)]
    [InlineData(CoverType.ContainerShip, 1, 1.3 * 1250)]
    public void Compute_ReturnsCorrectPremium_ForSingleDay(CoverType coverType, int days, decimal expectedPremium)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

        var result = _computePremium.Compute(startDate, endDate, coverType);

        Assert.Equal(expectedPremium, result);
    }

    [Theory]
    [InlineData(CoverType.Yacht, 30, 1.1 * 1250 * 30)]
    [InlineData(CoverType.PassengerShip, 30, 1.2 * 1250 * 30)]
    [InlineData(CoverType.Tanker, 30, 1.5 * 1250 * 30)]
    [InlineData(CoverType.BulkCarrier, 30, 1.3 * 1250 * 30)]
    public void Compute_ReturnsCorrectPremium_ForFirstPeriod(CoverType coverType, int days, decimal expectedPremium)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

        var result = _computePremium.Compute(startDate, endDate, coverType);

        Assert.Equal(expectedPremium, result);
    }

    [Theory]
    [InlineData(CoverType.Yacht, 180, 1.1 * 1250 * 30 + 1.1 * 1250 * 150 * 0.95)]
    [InlineData(CoverType.PassengerShip, 180, 1.2 * 1250 * 30 + 1.2 * 1250 * 150 * 0.98)]
    [InlineData(CoverType.Tanker, 180, 1.5 * 1250 * 30 + 1.5 * 1250 * 150 * 0.98)]
    public void Compute_ReturnsCorrectPremium_ForSecondPeriod(CoverType coverType, int days, decimal expectedPremium)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));

        var result = _computePremium.Compute(startDate, endDate, coverType);

        Assert.Equal(expectedPremium, result);
    }
}
