using ERP_Government.Application.Procurement.Commands.PurchaseRequests.CreatePurchaseRequest;
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
using ERP_Government.Application.Procurement.Commands.PurchaseOrders.ClosePurchaseOrder;
using ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrderById;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Domain.Procurement.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Procurement;

[TestFixture]
public class PaymentClosureTests : TestBase
{
    private int _itemId;
    private int _unitId;
    private int _supplierId;
    private int _prDetailId;
    private int _poId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var unit = new Unit { Code = "PC-001", Name = "Piece", IsActive = true };
        await TestApp.AddAsync(unit);
        _unitId = unit.Id;

        var item = new Item
        {
            Code = "PC-ITM-01",
            Name = "PaymentClosure Test Item",
            UnitId = _unitId,
            ItemType = "Goods",
            IsActive = true
        };
        await TestApp.AddAsync(item);
        _itemId = item.Id;

        var supplier = new Party
        {
            PartyCode = "SUP-PC-01",
            PartyType = PartyType.Supplier,
            NameAr = "payment supplier",
            IsActive = true
        };
        await TestApp.AddAsync(supplier);
        _supplierId = supplier.Id;

        var prResult = await TestApp.SendAsync(new CreatePurchaseRequestCommand(
            DateTime.UtcNow, null, null, null, PurchaseRequestPriority.Normal, "PR for Payment",
            [new PurchaseRequestLineDto(_itemId, _unitId, 10, 100m, null)]));
        prResult.Succeeded.ShouldBeTrue();

        var prDetail = await TestApp.SendAsync(new ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById.GetPurchaseRequestByIdQuery(prResult.Value));
        _prDetailId = prDetail.Details.First().Id;

        var qResult = await TestApp.SendAsync(new CreateQuotationCommand(
            _supplierId, DateTime.UtcNow, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null,
            [new QuotationLineDto(_prDetailId, _itemId, _unitId, 10, 100m, null, null, null)]));
        qResult.Succeeded.ShouldBeTrue();
        await TestApp.SendAsync(new SubmitQuotationCommand(qResult.Value));
        await TestApp.SendAsync(new StartEvaluationCommand(qResult.Value));
        await TestApp.SendAsync(new CompleteEvaluationCommand(qResult.Value, 90m, 85m, null));
        await TestApp.SendAsync(new SelectQuotationCommand(qResult.Value, "Best value"));
        await TestApp.SendAsync(new AwardQuotationCommand(qResult.Value));

        var poResult = await TestApp.SendAsync(new CreatePurchaseOrderCommand(
            prResult.Value, qResult.Value, _supplierId, null, null, null, null, null, null, null, null,
            [new PurchaseOrderLineDto(
                _prDetailId, null, _itemId, _unitId, 10, 100m, null, null, null, null)]));
        poResult.Succeeded.ShouldBeTrue();
        _poId = poResult.Value;

        await TestApp.SendAsync(new SubmitPurchaseOrderCommand(_poId));
        await TestApp.SendAsync(new ApprovePurchaseOrderCommand(_poId));
        await TestApp.SendAsync(new IssuePurchaseOrderCommand(_poId));
    }

    [Test]
    public async Task ClosePO_WhenIssued_ShouldTransitionToClosed()
    {
        var closeResult = await TestApp.SendAsync(new ClosePurchaseOrderCommand(_poId));
        closeResult.Succeeded.ShouldBeTrue();

        var po = await TestApp.SendAsync(new GetPurchaseOrderByIdQuery(_poId));
        po.Status.ShouldBe(PurchaseOrderStatus.Closed);
    }

    [Test]
    public async Task ClosePO_WhenDraft_ShouldFail()
    {
        var poResult = await TestApp.SendAsync(new CreatePurchaseOrderCommand(
            null, null, _supplierId, null, null, null, null, null, null, null, null,
            [new PurchaseOrderLineDto(
                _prDetailId, null, _itemId, _unitId, 10, 100m, null, null, null, null)]));
        var draftPoId = poResult.Value;

        var closeResult = await TestApp.SendAsync(new ClosePurchaseOrderCommand(draftPoId));
        closeResult.Succeeded.ShouldBeFalse();
    }
}
