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
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.CreateGRN;
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.ConfirmGRN;
using ERP_Government.Application.Procurement.Commands.GoodsReceiptNotes.RejectGRN;
using ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNs;
using ERP_Government.Application.Procurement.Queries.GoodsReceiptNotes.GetGRNById;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Domain.Procurement.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Procurement;

[TestFixture]
public class GoodsReceiptLifecycleTests : TestBase
{
    private int _itemId;
    private int _unitId;
    private int _supplierId;
    private int _warehouseId;
    private int _locationId;
    private int _prDetailId;
    private int _poId;
    private int _poDetailId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var unit = new Unit { Code = "GR-001", Name = "Piece", IsActive = true };
        await TestApp.AddAsync(unit);
        _unitId = unit.Id;

        var item = new Item
        {
            Code = "GR-ITM-01",
            Name = "GRN Test Item",
            UnitId = _unitId,
            ItemType = "Goods",
            IsActive = true
        };
        await TestApp.AddAsync(item);
        _itemId = item.Id;

        var supplier = new Party
        {
            PartyCode = "SUP-GR-01",
            PartyType = PartyType.Supplier,
            NameAr = "grn supplier",
            IsActive = true
        };
        await TestApp.AddAsync(supplier);
        _supplierId = supplier.Id;

        var warehouse = new ERP_Government.Domain.Inventory.Entities.Warehouse
        {
            Code = "WH-GR-01",
            Name = "GRN Warehouse",
            IsActive = true
        };
        await TestApp.AddAsync(warehouse);
        _warehouseId = warehouse.Id;

        var location = new ERP_Government.Domain.Inventory.Entities.Location
        {
            Code = "LOC-GR-01",
            Name = "GRN Location",
            WarehouseId = _warehouseId,
            IsActive = true
        };
        await TestApp.AddAsync(location);
        _locationId = location.Id;

        var prResult = await TestApp.SendAsync(new CreatePurchaseRequestCommand(
            DateTime.UtcNow, null, null, null, PurchaseRequestPriority.Normal, "PR for GRN",
            [new PurchaseRequestLineDto(_itemId, _unitId, 10, 100m, null)]));
        prResult.Succeeded.ShouldBeTrue();

        var prDetail = await TestApp.SendAsync(new ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById.GetPurchaseRequestByIdQuery(prResult.Value));
        _prDetailId = prDetail.Details.First().Id;

        var rfqResult = await TestApp.SendAsync(new CreateRFQCommand(
            prResult.Value, null, null, null, null, [_supplierId]));
        rfqResult.Succeeded.ShouldBeTrue();
        await TestApp.SendAsync(new PublishRFQCommand(rfqResult.Value));
        await TestApp.SendAsync(new CloseRFQCollectionCommand(rfqResult.Value));

        var qResult = await TestApp.SendAsync(new CreateQuotationCommand(
            rfqResult.Value, 1, _supplierId, DateTime.UtcNow, null, null, null, null, null,
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

        var poDetail = await TestApp.SendAsync(new ERP_Government.Application.Procurement.Queries.PurchaseOrders.GetPurchaseOrderById.GetPurchaseOrderByIdQuery(_poId));
        _poDetailId = poDetail.Details.First().Id;

        await TestApp.SendAsync(new SubmitPurchaseOrderCommand(_poId));
        await TestApp.SendAsync(new ApprovePurchaseOrderCommand(_poId));
        await TestApp.SendAsync(new IssuePurchaseOrderCommand(_poId));
    }

    [Test]
    public async Task CreateAndConfirmGRN_ShouldComplete()
    {
        var createResult = await TestApp.SendAsync(new CreateGRNCommand(
            DateTime.UtcNow, _poId, _warehouseId, _locationId, null, null,
            [new CreateGRNDetailDto(
                _poDetailId, _itemId, _unitId, 10, 10, 10, 0, 0, 100m, 1000m, null, null, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var grnId = createResult.Value;

        var grn = await TestApp.SendAsync(new GetGRNByIdQuery(grnId));
        grn.Status.ShouldBe(GRNStatus.Draft);

        await TestApp.SendAsync(new ConfirmGRNCommand(grnId));
        grn = await TestApp.SendAsync(new GetGRNByIdQuery(grnId));
        grn.Status.ShouldBe(GRNStatus.Confirmed);
    }

    [Test]
    public async Task RejectGRN_ShouldTransitionToRejected()
    {
        var createResult = await TestApp.SendAsync(new CreateGRNCommand(
            DateTime.UtcNow, _poId, _warehouseId, _locationId, null, null,
            [new CreateGRNDetailDto(
                _poDetailId, _itemId, _unitId, 10, 10, 5, 5, 0, 100m, 1000m, null, null, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var grnId = createResult.Value;

        await TestApp.SendAsync(new RejectGRNCommand(grnId, "Quality issues"));
        var grn = await TestApp.SendAsync(new GetGRNByIdQuery(grnId));
        grn.Status.ShouldBe(GRNStatus.Rejected);
    }

    [Test]
    public async Task GetGRNs_ShouldFilterByStatus()
    {
        var createResult = await TestApp.SendAsync(new CreateGRNCommand(
            DateTime.UtcNow, _poId, _warehouseId, _locationId, null, null,
            [new CreateGRNDetailDto(
                _poDetailId, _itemId, _unitId, 10, 10, 10, 0, 0, 100m, 1000m, null, null, null)]));
        var grnId = createResult.Value;
        await TestApp.SendAsync(new ConfirmGRNCommand(grnId));

        var confirmed = await TestApp.SendAsync(new GetGRNsQuery(Status: GRNStatus.Confirmed));
        confirmed.Value.Items.ShouldContain(x => x.Id == grnId);

        var draft = await TestApp.SendAsync(new GetGRNsQuery(Status: GRNStatus.Draft));
        draft.Value.Items.ShouldNotContain(x => x.Id == grnId);
    }
}
