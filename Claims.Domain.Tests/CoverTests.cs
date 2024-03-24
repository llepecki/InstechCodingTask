using Xunit;
using FluentAssertions;

namespace Claims.Domain.Tests;

public class CoverTests
{
    [Fact]
    public void Create_ShouldReturnFailure_WhenStartDateIsGreaterThanEndDate()
    {
        var startDate = new DateOnly(2022, 12, 31);
        var endDate = new DateOnly(2022, 1, 1);
        var type = CoverType.BulkCarrier;

        var result = Cover.Create(startDate, endDate, type);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainKey($"{nameof(startDate)}_{nameof(endDate)}");
    }

    [Fact]
    public void Create_ShouldReturnFailure_WhenCoverIsLongerThanAYear()
    {
        var startDate = new DateOnly(2022, 1, 1);
        var endDate = new DateOnly(2023, 1, 2);
        var type = CoverType.BulkCarrier;

        var result = Cover.Create(startDate, endDate, type);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().ContainKey("coverPeriod");
    }
    
    // TODO: add more tests also for Claim
}