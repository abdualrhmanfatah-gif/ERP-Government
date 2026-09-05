using ERP_Government.Domain.BackgroundJobs.Common;
using ERP_Government.Domain.BackgroundJobs.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.BackgroundJobs;

[TestFixture]
public class BackgroundJobEngineTests
{
    [Test]
    public void BackgroundJobOptions_DefaultValues_AreCorrect()
    {
        var options = new ERP_Government.Infrastructure.Services.BackgroundJobOptions();
        options.PollIntervalMs.ShouldBe(5000);
        options.BatchSize.ShouldBe(50);
        options.MaxRetries.ShouldBe(5);
        options.TimeoutSeconds.ShouldBe(300);
        options.RetentionDays.ShouldBe(90);
    }

    [Test]
    public void BackgroundJobDefinition_DefaultValues_AreCorrect()
    {
        var def = new ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobDefinition();
        def.TypeName.ShouldBe(string.Empty);
        def.DisplayName.ShouldBe(string.Empty);
        def.MaxRetries.ShouldBe(5);
        def.TimeoutSeconds.ShouldBe(300);
        def.IsActive.ShouldBeTrue();
        def.ScheduleType.ShouldBe(BackgroundJobScheduleType.Interval);
        def.ScheduleValue.ShouldBe(string.Empty);
    }

    [Test]
    public void BackgroundJobDefinition_CanSetProperties()
    {
        var def = new ERP_Government.Domain.BackgroundJobs.Entities.BackgroundJobDefinition
        {
            TypeName = "TestJob",
            DisplayName = "Test",
            ScheduleType = BackgroundJobScheduleType.Cron,
            ScheduleValue = "0 * * * *",
            MaxRetries = 3,
            TimeoutSeconds = 60,
            IsActive = false
        };

        def.TypeName.ShouldBe("TestJob");
        def.DisplayName.ShouldBe("Test");
        def.ScheduleType.ShouldBe(BackgroundJobScheduleType.Cron);
        def.ScheduleValue.ShouldBe("0 * * * *");
        def.MaxRetries.ShouldBe(3);
        def.TimeoutSeconds.ShouldBe(60);
        def.IsActive.ShouldBeFalse();
    }
}
