using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
using ERP_Government.Application.Procurement.Commands.RequestForQuotations.CreateRFQ;
using ERP_Government.Application.Procurement.Commands.RequestForQuotations.PublishRFQ;
using ERP_Government.Application.Procurement.Commands.RequestForQuotations.CloseRFQCollection;
using ERP_Government.Application.Procurement.Commands.Quotations.CreateQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.SubmitQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.StartEvaluation;
using ERP_Government.Application.Procurement.Commands.Quotations.CompleteEvaluation;
using ERP_Government.Application.Procurement.Commands.Quotations.SelectQuotation;
using ERP_Government.Application.Procurement.Commands.Quotations.AwardQuotation;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.CreatePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.SubmitPurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.ApprovePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.IssuePurchaseOrder;
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.CancelPurchaseOrder;
using ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrders;
using ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrderById;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Domain.Procurement.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Procurement;

[TestFixture]
public class PurchaseOrderLifecycleTests : TestBase
{
    private int _itemId;
    private int _unitId;
    private int _supplierId;
    private int _prId;
    private int _prDetailId;
    private int _rfqId;
    private int _quotationId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var unit = new Unit { Code = "PO-001", Name = "Piece", IsActive = true };
        await TestApp.AddAsync(unit);
        _unitId = unit.Id;

        var item = new Item
        {
            Code = "PO-ITM-01",
            Name = "PO Test Item",
            UnitId = _unitId,
            ItemType = "Goods",
            IsActive = true
        };
        await TestApp.AddAsync(item);
        _itemId = item.Id;

        var supplier = new Party
        {
            PartyCode = "SUP-PO-01",
            PartyType = PartyType.Supplier,
            NameAr = "po supplier",
            IsActive = true
        };
        await TestApp.AddAsync(supplier);
        _supplierId = supplier.Id;

        var prResult = await TestApp.SendAsync(new CreatePurchaseRequestCommand(
            DateTime.UtcNow, null, null, null, PurchaseRequestPriority.Normal, "PR for PO",
            [new PurchaseRequestLineDto(_itemId, _unitId, 10, 100m, null)]));
        prResult.Succeeded.ShouldBeTrue();
        _prId = prResult.Value;

        var prDetail = await TestApp.SendAsync(new ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById.GetPurchaseRequestByIdQuery(_prId));
        _prDetailId = prDetail.Details.First().Id;

        var rfqResult = await TestApp.SendAsync(new CreateRFQCommand(
            _prId, null, null, null, null, [_supplierId]));
        rfqResult.Succeeded.ShouldBeTrue();
        _rfqId = rfqResult.Value;
        await TestApp.SendAsync(new PublishRFQCommand(_rfqId));
        await TestApp.SendAsync(new CloseRFQCollectionCommand(_rfqId));

        var qResult = await TestApp.SendAsync(new CreateQuotationCommand(
            _rfqId, 1, _supplierId, DateTime.UtcNow, null, null, null, null, null,
            null, null, null, null, null,
            [new QuotationLineDto(_prDetailId, _itemId, _unitId, 10, 100m, null, null, null)]));
        qResult.Succeeded.ShouldBeTrue();
        _quotationId = qResult.Value;
        await TestApp.SendAsync(new SubmitQuotationCommand(_quotationId));
        await TestApp.SendAsync(new StartEvaluationCommand(_quotationId));
        await TestApp.SendAsync(new CompleteEvaluationCommand(_quotationId, 90m, 85m, null));
        await TestApp.SendAsync(new SelectQuotationCommand(_quotationId, "Best value"));
        await TestApp.SendAsync(new AwardQuotationCommand(_quotationId));
    }

    private async Task<int> CreatePO()
    {
        var result = await TestApp.SendAsync(new CreatePurchaseOrderCommand(
            _prId, _quotationId, _supplierId, null, null, null, null, null, null, null, null,
            [new PurchaseOrderLineDto(
                _prDetailId, null, _itemId, _unitId, 10, 100m, null, null, null, null)]));
        result.Succeeded.ShouldBeTrue();
        return result.Value;
    }

    [Test]
    public async Task FullLifecycle_DraftToIssued_ShouldComplete()
    {
        var poId = await CreatePO();

        var po = await TestApp.SendAsync(new GetPurchaseOrderByIdQuery(poId));
        po.Status.ShouldBe(PurchaseOrderStatus.Draft);

        await TestApp.SendAsync(new SubmitPurchaseOrderCommand(poId));
        po = await TestApp.SendAsync(new GetPurchaseOrderByIdQuery(poId));
        po.Status.ShouldBe(PurchaseOrderStatus.Submitted);

        await TestApp.SendAsync(new ApprovePurchaseOrderCommand(poId));
        po = await TestApp.SendAsync(new GetPurchaseOrderByIdQuery(poId));
        po.Status.ShouldBe(PurchaseOrderStatus.Approved);

        await TestApp.SendAsync(new IssuePurchaseOrderCommand(poId));
        po = await TestApp.SendAsync(new GetPurchaseOrderByIdQuery(poId));
        po.Status.ShouldBe(PurchaseOrderStatus.Issued);
    }

    [Test]
    public async Task CancelFromIssued_ShouldSucceed()
    {
        var poId = await CreatePO();
        await TestApp.SendAsync(new SubmitPurchaseOrderCommand(poId));
        await TestApp.SendAsync(new ApprovePurchaseOrderCommand(poId));
        await TestApp.SendAsync(new IssuePurchaseOrderCommand(poId));

        var cancelResult = await TestApp.SendAsync(new CancelPurchaseOrderCommand(poId));
        cancelResult.Succeeded.ShouldBeTrue();

        var po = await TestApp.SendAsync(new GetPurchaseOrderByIdQuery(poId));
        po.Status.ShouldBe(PurchaseOrderStatus.Cancelled);
    }

    [Test]
    public async Task GetPOList_ShouldReturnResults()
    {
        await CreatePO();

        var list = await TestApp.SendAsync(new GetPurchaseOrdersQuery());
        list.Value.Items.ShouldNotBeEmpty();
    }

    [Test]
    public async Task GetPOList_ShouldFilterByStatus()
    {
        var poId = await CreatePO();
        await TestApp.SendAsync(new SubmitPurchaseOrderCommand(poId));

        var submitted = await TestApp.SendAsync(new GetPurchaseOrdersQuery(Status: PurchaseOrderStatus.Submitted));
        submitted.Value.Items.ShouldContain(x => x.Id == poId);
    }
}
