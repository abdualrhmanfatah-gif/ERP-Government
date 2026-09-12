using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.RejectPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CancelPurchaseRequest;
using ERP_Government.Domain.Procurement.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Procurement;

[TestFixture]
public class PurchaseRequestTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;
    private Mock<IUser> _userMock = null!;
    private const int UserId = 1;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new ApplicationDbContext(options);

        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _userMock = new Mock<IUser>();
        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("PR-000001");
        _userMock.Setup(x => x.Id).Returns(UserId);

        SeedReferenceData();
    }

    [TearDown]
    public void TearDown() => _dbContext?.Dispose();

    private void SeedReferenceData()
    {
        _dbContext.Users.Add(new Domain.Security.Entities.User { Id = UserId, Login = "test", IsActive = true });
        _dbContext.SaveChanges();
    }

    private CreatePurchaseRequestCommand ValidCommand() => new(
        RequestDate: DateTime.UtcNow,
        RequiredDate: null,
        DepartmentId: null,
        CostCenterId: null,
        Priority: PurchaseRequestPriority.Normal,
        Notes: null,
        Lines:
        [
            new PurchaseRequestLineDto(ItemId: 1, UnitId: 1, RequestedQuantity: 10, UnitCostEstimate: 1000m, Notes: null)
        ]);

    private async Task<Domain.Procurement.Entities.PurchaseRequest> CreateAndSubmit()
    {
        var createResult = await new CreatePurchaseRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);
        createResult.Succeeded.ShouldBeTrue();
        var id = createResult.Value!;

        var submitResult = await new SubmitPurchaseRequestCommandHandler(
                _dbContext)
            .Handle(new SubmitPurchaseRequestCommand(id), CancellationToken.None);
        submitResult.Succeeded.ShouldBeTrue();
        return (await _dbContext.PurchaseRequests.FindAsync(id))!;
    }

    [Test]
    public async Task Create_ValidCommand_ShouldReturnDraftStatus()
    {
        var result = await new CreatePurchaseRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var pr = await _dbContext.PurchaseRequests.FindAsync(result.Value);
        pr.ShouldNotBeNull();
        pr.Status.ShouldBe(PurchaseRequestStatus.Draft);
        pr.RequestNumber.ShouldBe("PR-000001");
    }

    [Test]
    public async Task Submit_DraftStatus_ShouldTransitionToSubmitted()
    {
        var pr = await CreateAndSubmit();
        pr.Status.ShouldBe(PurchaseRequestStatus.Submitted);
    }

    [Test]
    public async Task Approve_SubmittedStatus_ShouldTransitionToApproved()
    {
        var pr = await CreateAndSubmit();
        var result = await new ApprovePurchaseRequestCommandHandler(
                _dbContext)
            .Handle(new ApprovePurchaseRequestCommand(pr.Id), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var updated = await _dbContext.PurchaseRequests.FindAsync(pr.Id);
        updated!.Status.ShouldBe(PurchaseRequestStatus.Approved);
    }

    [Test]
    public async Task Reject_SubmittedStatus_ShouldTransitionToRejected()
    {
        var pr = await CreateAndSubmit();
        var result = await new RejectPurchaseRequestCommandHandler(
                _dbContext)
            .Handle(new RejectPurchaseRequestCommand(pr.Id, "Not needed"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var updated = await _dbContext.PurchaseRequests.FindAsync(pr.Id);
        updated!.Status.ShouldBe(PurchaseRequestStatus.Rejected);
    }

    [Test]
    public async Task Cancel_ApprovedStatus_ShouldTransitionToCancelled()
    {
        var createResult = await new CreatePurchaseRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);
        var id = createResult.Value!;

        await new SubmitPurchaseRequestCommandHandler(_dbContext)
            .Handle(new SubmitPurchaseRequestCommand(id), CancellationToken.None);
        await new ApprovePurchaseRequestCommandHandler(_dbContext)
            .Handle(new ApprovePurchaseRequestCommand(id), CancellationToken.None);

        var result = await new CancelPurchaseRequestCommandHandler(
                _dbContext)
            .Handle(new CancelPurchaseRequestCommand(id, "No longer needed"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var pr = await _dbContext.PurchaseRequests.FindAsync(id);
        pr!.Status.ShouldBe(PurchaseRequestStatus.Cancelled);
    }

    [Test]
    public async Task Submit_AlreadySubmitted_ShouldFail()
    {
        var pr = await CreateAndSubmit();
        var result = await new SubmitPurchaseRequestCommandHandler(
                _dbContext)
            .Handle(new SubmitPurchaseRequestCommand(pr.Id), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task Approve_DraftStatus_ShouldFail()
    {
        var createResult = await new CreatePurchaseRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);

        var result = await new ApprovePurchaseRequestCommandHandler(
                _dbContext)
            .Handle(new ApprovePurchaseRequestCommand(createResult.Value!), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task Create_ShouldComputeTotalEstimatedCost()
    {
        var result = await new CreatePurchaseRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object)
            .Handle(ValidCommand(), CancellationToken.None);

        var pr = await _dbContext.PurchaseRequests.FindAsync(result.Value);
        pr!.TotalEstimatedCost.ShouldBe(10000m); // 10 * 1000
    }

    [Test]
    public async Task Create_EmptyLines_ShouldFail()
    {
        var command = new CreatePurchaseRequestCommand(
            RequestDate: DateTime.UtcNow,
            RequiredDate: null,
            DepartmentId: null,
            CostCenterId: null,
            Priority: PurchaseRequestPriority.Normal,
            Notes: null,
            Lines: []);

        var result = await new CreatePurchaseRequestCommandHandler(
                _dbContext, _sequenceServiceMock.Object)
            .Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
    }
}
