using ERP_Government.Application.Assets.PhysicalCounts.Commands;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.PhysicalCounts;

[TestFixture]
public class PhysicalCountFlowTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _user = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _user = new Mock<IUser>();
        _user.SetupGet(u => u.Id).Returns(7);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private void SeedAssets()
    {
        _context.Employees.AddRange(
            new Employee { Id = 1, EmployeeNumber = "E-1", Name = "أحمد", OrganizationalUnitId = 5, HireDate = new DateOnly(2020, 1, 1), JobTitle = "موظف" },
            new Employee { Id = 2, EmployeeNumber = "E-2", Name = "سارة", OrganizationalUnitId = 6, HireDate = new DateOnly(2020, 1, 1), JobTitle = "موظفة" });

        _context.Assets.AddRange(
            new Asset { Id = 1, Code = "AST-1", Name = "أصل 1", AssetGroupId = 1, LocationId = 1, EmployeeId = 1, CurrencyId = 1, Status = "Active" },
            new Asset { Id = 2, Code = "AST-2", Name = "أصل 2", AssetGroupId = 1, LocationId = 1, EmployeeId = 2, CurrencyId = 1, Status = "Active" },
            new Asset { Id = 3, Code = "AST-3", Name = "أصل 3", AssetGroupId = 1, LocationId = 2, EmployeeId = null, CurrencyId = 1, Status = "Active" },
            new Asset { Id = 4, Code = "AST-4", Name = "أصل 4", AssetGroupId = 1, LocationId = 2, EmployeeId = 1, CurrencyId = 1, Status = "Active" });

        _context.SaveChanges();
    }

    private int SeedCount(int? locationId, int? departmentId, CountStatus status = CountStatus.Draft)
    {
        var count = new AssetPhysicalCount
        {
            CountNumber = $"CNT-{Guid.NewGuid():N}",
            CountDate = new DateOnly(2026, 9, 1),
            CountType = "سنوي",
            LocationId = locationId,
            DepartmentId = departmentId,
            ResolvedScopeLabel = "نطاق",
            Status = status
        };
        _context.AssetPhysicalCounts.Add(count);
        _context.SaveChanges();
        return count.Id;
    }

    [Test]
    public async Task Start_LocationScope_GeneratesOneLinePerAssetOfThatLocation()
    {
        SeedAssets();
        var countId = SeedCount(1, null);
        var handler = new StartAssetPhysicalCountCommandHandler(_context, _user.Object);

        var result = await handler.Handle(new StartAssetPhysicalCountCommand(countId), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var lines = _context.AssetPhysicalCountDetails.Where(l => l.AssetPhysicalCountId == countId).ToList();
        lines.Count.ShouldBe(2);
        lines.Select(l => l.AssetId).ShouldBe([1, 2], ignoreOrder: true);
        lines.ShouldAllBe(l => l.IsFound == CountFoundState.NotExamined);
        lines.Single(l => l.AssetId == 1).SystemEmployeeId.ShouldBe(1);
        lines.Single(l => l.AssetId == 1).SystemLocationId.ShouldBe(1);

        var count = _context.AssetPhysicalCounts.Single(c => c.Id == countId);
        count.Status.ShouldBe(CountStatus.InProgress);
        count.StartedAt.ShouldNotBeNull();
        count.CountedById.ShouldBe(7);
    }

    [Test]
    public async Task Start_DepartmentScope_ResolvesThroughCustodianEmployeeAndExcludesUnprovableMembership()
    {
        SeedAssets();
        var countId = SeedCount(null, 5);
        var handler = new StartAssetPhysicalCountCommandHandler(_context, _user.Object);

        await handler.Handle(new StartAssetPhysicalCountCommand(countId), CancellationToken.None);

        var lines = _context.AssetPhysicalCountDetails.Where(l => l.AssetPhysicalCountId == countId).ToList();
        lines.Select(l => l.AssetId).ShouldBe([1, 4], ignoreOrder: true);
    }

    [Test]
    public async Task Start_ComprehensiveScope_IncludesAssetsWithoutCustodian()
    {
        SeedAssets();
        var countId = SeedCount(null, null);
        var handler = new StartAssetPhysicalCountCommandHandler(_context, _user.Object);

        await handler.Handle(new StartAssetPhysicalCountCommand(countId), CancellationToken.None);

        var lines = _context.AssetPhysicalCountDetails.Where(l => l.AssetPhysicalCountId == countId).ToList();
        lines.Count.ShouldBe(4);
        lines.ShouldContain(l => l.AssetId == 3);
    }

    [Test]
    public async Task Start_OnNonDraftCount_IsRejected()
    {
        SeedAssets();
        var countId = SeedCount(null, null, CountStatus.Completed);
        var handler = new StartAssetPhysicalCountCommandHandler(_context, _user.Object);

        var result = await handler.Handle(new StartAssetPhysicalCountCommand(countId), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidCountStatus);
    }

    [Test]
    public async Task Observe_UpdatesTheSameLineInPlace_AndGuardsRowVersion()
    {
        SeedAssets();
        var countId = SeedCount(1, null, CountStatus.InProgress);
        var line = new AssetPhysicalCountDetail
        {
            AssetPhysicalCountId = countId,
            AssetId = 1,
            IsFound = CountFoundState.NotExamined,
            RowVersion = [1, 2, 3]
        };
        _context.AssetPhysicalCountDetails.Add(line);
        _context.SaveChanges();

        var handler = new UpdateAssetPhysicalCountLineCommandHandler(_context);

        var conflict = await handler.Handle(new UpdateAssetPhysicalCountLineCommand(
            countId, line.Id, CountFoundState.Found, null, null, null, null, [9, 9, 9]), CancellationToken.None);
        conflict.Succeeded.ShouldBeFalse();
        conflict.Code.ShouldBe(ErrorCodes.Request.ConcurrencyConflict);

        var ok = await handler.Handle(new UpdateAssetPhysicalCountLineCommand(
            countId, line.Id, CountFoundState.Found, 2, 2, "سليم", "ملاحظة", [1, 2, 3]), CancellationToken.None);
        ok.Succeeded.ShouldBeTrue();

        var updated = _context.AssetPhysicalCountDetails.Single();
        updated.IsFound.ShouldBe(CountFoundState.Found);
        updated.PhysicalLocationId.ShouldBe(2);
        updated.PhysicalEmployeeId.ShouldBe(2);
        updated.PhysicalStatus.ShouldBe("سليم");
        updated.DiscrepancyNotes.ShouldBe("ملاحظة");
        _context.AssetPhysicalCountDetails.Count().ShouldBe(1);
    }

    [Test]
    public async Task Observe_AfterCompletion_IsRejected()
    {
        SeedAssets();
        var countId = SeedCount(1, null, CountStatus.Completed);
        var line = new AssetPhysicalCountDetail
        {
            AssetPhysicalCountId = countId,
            AssetId = 1,
            IsFound = CountFoundState.Found,
            RowVersion = [1]
        };
        _context.AssetPhysicalCountDetails.Add(line);
        _context.SaveChanges();

        var handler = new UpdateAssetPhysicalCountLineCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetPhysicalCountLineCommand(
            countId, line.Id, CountFoundState.NotFound, null, null, null, null, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidCountStatus);
    }

    [Test]
    public async Task Complete_BlockedWhileAnyLineNotExamined_ThenSucceeds()
    {
        SeedAssets();
        var countId = SeedCount(1, null, CountStatus.InProgress);
        _context.AssetPhysicalCountDetails.AddRange(
            new AssetPhysicalCountDetail { AssetPhysicalCountId = countId, AssetId = 1, IsFound = CountFoundState.Found },
            new AssetPhysicalCountDetail { AssetPhysicalCountId = countId, AssetId = 2, IsFound = CountFoundState.NotExamined });
        _context.SaveChanges();

        var handler = new CompleteAssetPhysicalCountCommandHandler(_context);

        var blocked = await handler.Handle(new CompleteAssetPhysicalCountCommand(countId), CancellationToken.None);
        blocked.Succeeded.ShouldBeFalse();
        blocked.Code.ShouldBe(ErrorCodes.Assets.CountLinesPending);

        var pending = _context.AssetPhysicalCountDetails.Single(l => l.AssetId == 2);
        pending.IsFound = CountFoundState.NotFound;
        _context.SaveChanges();

        var ok = await handler.Handle(new CompleteAssetPhysicalCountCommand(countId), CancellationToken.None);
        ok.Succeeded.ShouldBeTrue();

        var count = _context.AssetPhysicalCounts.Single(c => c.Id == countId);
        count.Status.ShouldBe(CountStatus.Completed);
        count.CompletedAt.ShouldNotBeNull();
    }

    [Test]
    public async Task Review_RequiresCompletedCount_AndRecordsReviewerWithoutApprovalCycle()
    {
        SeedAssets();
        var countId = SeedCount(1, null, CountStatus.InProgress);
        var handler = new ReviewAssetPhysicalCountCommandHandler(_context, _user.Object);

        var tooEarly = await handler.Handle(new ReviewAssetPhysicalCountCommand(countId), CancellationToken.None);
        tooEarly.Succeeded.ShouldBeFalse();

        var count = _context.AssetPhysicalCounts.Single(c => c.Id == countId);
        count.Status = CountStatus.Completed;
        _context.SaveChanges();

        var result = await handler.Handle(new ReviewAssetPhysicalCountCommand(countId), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        count.ReviewedById.ShouldBe(7);
        count.Status.ShouldBe(CountStatus.Reviewed);
    }
}
