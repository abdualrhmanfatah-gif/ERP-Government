using ERP_Government.Domain.BackgroundJobs.Common;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.BackgroundJobs;

[TestFixture]
public class BackgroundJobScheduleTests
{
    [Test]
    public void Interval_ScheduleTypeIsInterval()
    {
        var schedule = BackgroundJobSchedule.Interval(30);
        schedule.ScheduleType.ShouldBe(ERP_Government.Domain.BackgroundJobs.Enums.BackgroundJobScheduleType.Interval);
    }

    [Test]
    public void Interval_ScheduleValueIsSecondsString()
    {
        var schedule = BackgroundJobSchedule.Interval(30);
        schedule.ScheduleValue.ShouldBe("30");
    }

    [Test]
    public void Interval_GetInterval_ReturnsCorrectTimeSpan()
    {
        var schedule = BackgroundJobSchedule.Interval(30);
        schedule.GetInterval().ShouldBe(TimeSpan.FromSeconds(30));
    }

    [Test]
    public void Cron_ScheduleTypeIsCron()
    {
        var schedule = BackgroundJobSchedule.Cron("0 0 * * *");
        schedule.ScheduleType.ShouldBe(ERP_Government.Domain.BackgroundJobs.Enums.BackgroundJobScheduleType.Cron);
    }

    [Test]
    public void Cron_ScheduleValueIsExpression()
    {
        var schedule = BackgroundJobSchedule.Cron("0 0 * * *");
        schedule.ScheduleValue.ShouldBe("0 0 * * *");
    }

    [Test]
    public void Cron_GetInterval_ThrowsInvalidOperationException()
    {
        var schedule = BackgroundJobSchedule.Cron("0 0 * * *");
        Should.Throw<InvalidOperationException>(() => schedule.GetInterval());
    }
}
