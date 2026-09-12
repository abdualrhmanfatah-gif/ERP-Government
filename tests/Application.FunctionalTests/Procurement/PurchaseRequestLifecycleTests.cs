using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.UpdatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.SubmitPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.ApprovePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.RejectPurchaseRequest;
using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CancelPurchaseRequest;
using ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequests;
using ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Domain.Procurement.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Procurement;

[TestFixture]
public class PurchaseRequestLifecycleTests : TestBase
{
    private int _itemId;
    private int _unitId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var unit = new Unit { Code = "UN-001", Name = "Piece", IsActive = true };
        await TestApp.AddAsync(unit);
        _unitId = unit.Id;

        var item = new Item
        {
            Code = "IT-001",
            Name = "Test Item",
            UnitId = _unitId,
            ItemType = "Goods",
            IsActive = true
        };
        await TestApp.AddAsync(item);
        _itemId = item.Id;
    }

    private async Task<int> CreatePR(string? notes = null, PurchaseRequestPriority priority = PurchaseRequestPriority.Normal)
    {
        var result = await TestApp.SendAsync(new CreatePurchaseRequestCommand(
            DateTime.UtcNow, null, null, null, priority, notes,
            [new PurchaseRequestLineDto(_itemId, _unitId, 10, 100m, null)]));
        result.Succeeded.ShouldBeTrue();
        return result.Value;
    }

    [Test]
    public async Task FullLifecycle_DraftToApproved_ShouldComplete()
    {
        var prId = await CreatePR("Full lifecycle test");

        var pr = await TestApp.SendAsync(new GetPurchaseRequestByIdQuery(prId));
        pr.Status.ShouldBe(PurchaseRequestStatus.Draft);

        await TestApp.SendAsync(new SubmitPurchaseRequestCommand(prId));
        pr = await TestApp.SendAsync(new GetPurchaseRequestByIdQuery(prId));
        pr.Status.ShouldBe(PurchaseRequestStatus.Submitted);

        await TestApp.SendAsync(new ApprovePurchaseRequestCommand(prId));
        pr = await TestApp.SendAsync(new GetPurchaseRequestByIdQuery(prId));
        pr.Status.ShouldBe(PurchaseRequestStatus.Approved);
    }

    [Test]
    public async Task SubmitAndReject_ShouldTransitionCorrectly()
    {
        var prId = await CreatePR("Reject test");

        await TestApp.SendAsync(new SubmitPurchaseRequestCommand(prId));
        var pr = await TestApp.SendAsync(new GetPurchaseRequestByIdQuery(prId));
        pr.Status.ShouldBe(PurchaseRequestStatus.Submitted);

        await TestApp.SendAsync(new RejectPurchaseRequestCommand(prId, "Budget not available"));
        pr = await TestApp.SendAsync(new GetPurchaseRequestByIdQuery(prId));
        pr.Status.ShouldBe(PurchaseRequestStatus.Rejected);
    }

    [Test]
    public async Task CancelFromDraft_ShouldBlock()
    {
        var prId = await CreatePR("Cancel from draft");

        var result = await TestApp.SendAsync(new CancelPurchaseRequestCommand(prId, "No longer needed"));
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task EditDraft_ShouldUpdateLines()
    {
        var prId = await CreatePR("Edit test");

        await TestApp.SendAsync(new UpdatePurchaseRequestCommand(
            prId, DateTime.UtcNow, null, null, null,
            PurchaseRequestPriority.Urgent, "Updated notes",
            [new UpdatePurchaseRequestLineDto(null, _itemId, _unitId, 20, 150m, null)]));

        var pr = await TestApp.SendAsync(new GetPurchaseRequestByIdQuery(prId));
        pr.Priority.ShouldBe(PurchaseRequestPriority.Urgent);
        pr.Notes.ShouldBe("Updated notes");
    }

    [Test]
    public async Task GetList_ShouldFilterByStatus()
    {
        var prId = await CreatePR("Filter test");
        await TestApp.SendAsync(new SubmitPurchaseRequestCommand(prId));

        var submitted = await TestApp.SendAsync(new GetPurchaseRequestsQuery(Status: PurchaseRequestStatus.Submitted));
        submitted.Value.Items.ShouldContain(x => x.Id == prId);

        var draft = await TestApp.SendAsync(new GetPurchaseRequestsQuery(Status: PurchaseRequestStatus.Draft));
        draft.Value.Items.ShouldNotContain(x => x.Id == prId);
    }

    [Test]
    public async Task GetList_ShouldFilterByPriority()
    {
        var prId = await CreatePR("Priority filter", PurchaseRequestPriority.Urgent);

        var urgent = await TestApp.SendAsync(new GetPurchaseRequestsQuery(Priority: PurchaseRequestPriority.Urgent));
        urgent.Value.Items.ShouldContain(x => x.Id == prId);

        var low = await TestApp.SendAsync(new GetPurchaseRequestsQuery(Priority: PurchaseRequestPriority.Low));
        low.Value!.Items.ShouldNotContain(x => x.Id == prId);
    }
}
