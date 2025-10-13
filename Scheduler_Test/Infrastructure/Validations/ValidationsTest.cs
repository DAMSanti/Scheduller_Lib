using Scheduler_Lib.Core.Model;
using Scheduler_Lib.Resources;

namespace Scheduler_Lib.Infrastructure.Validations;

public class ValidationsTest {
    [Fact]
    public void NullOffset_Recurrent_Invalid() {
        var requestedDate = new RequestedDate {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Period = null,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void NegativeOffset_Recurrent_Invalid() {
        var requestedDate = new RequestedDate {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Period = -1,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void Recurrent_NoOffset() {
        var requestedDate = new RequestedDate {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            Period = null,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorPositiveOffsetRequired, result.Error);
    }

    [Fact]
    public void Once_ChangeDate_Null() {
        var requestedDate = new RequestedDate {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            ChangeDate = null,
            Periodicity = EnumPeriodicity.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorOnceMode, result.Error);
        Assert.Contains(Messages.ErrorChangeDateNull, result.Error);
    }

    [Fact]
    public void Once_ChangeDate_OutOfRange() {
        var requestedDate = new RequestedDate {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            ChangeDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumPeriodicity.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorChangeDateAfterEndDate, result.Error);
    }

    [Fact]
    public void Once_EndDate_Null() {
        var requestedDate = new RequestedDate {
            StartDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            EndDate = null,
            ChangeDate = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumPeriodicity.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorEndDateNull, result.Error);
    }

    [Fact]
    public void Recurrent_StartDateAfterEndDate() {
        var requestedDate = new RequestedDate {
            Date = new DateTimeOffset(2025, 10, 3, 0, 0, 0, TimeSpan.Zero),
            StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Period = 1,
            Periodicity = EnumPeriodicity.Recurrent
        };

        var result = Validations.ValidateRecurrent(requestedDate);
        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorStartDatePostEndDate, result.Error);
    }
    [Fact]
    public void Once_StartDateAfterEndDate_ReturnsError() {
        var requestedDate = new RequestedDate {
            StartDate = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            EndDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ChangeDate = new DateTimeOffset(2025, 6, 1, 0, 0, 0, TimeSpan.Zero),
            Periodicity = EnumPeriodicity.OneTime
        };

        var result = Validations.ValidateOnce(requestedDate);

        Assert.False(result.IsSuccess);
        Assert.Contains(Messages.ErrorChangeDateAfterEndDate, result.Error);
    }
}