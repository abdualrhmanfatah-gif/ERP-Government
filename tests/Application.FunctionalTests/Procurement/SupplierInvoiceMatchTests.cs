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
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.CreateSupplierInvoice;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.SubmitSupplierInvoice;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.MatchSupplierInvoice;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.AcceptInvoiceWithNotes;
using ERP_Government.Application.Procurement.Commands.SupplierInvoices.CancelSupplierInvoice;
using ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoices;
using ERP_Government.Application.Procurement.Queries.SupplierInvoices.GetSupplierInvoiceById;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Domain.Procurement.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Procurement;

[TestFixture]
public class SupplierInvoiceMatchTests : TestBase
{
    private int _itemId;
    private int _unitId;
    private int _supplierId;
    private int _prDetailId;
    private int _poId;
    private int _poDetailId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var unit = new Unit { Code = "SI-001", Name = "Piece", IsActive = true };
        await TestApp.AddAsync(unit);
        _unitId = unit.Id;

        var item = new Item
        {
            Code = "SI-ITM-01",
            Name = "SupplierInvoice Test Item",
            UnitId = _unitId,
            ItemType = "Goods",
            IsActive = true
        };
        await TestApp.AddAsync(item);
        _itemId = item.Id;

        var supplier = new Party
        {
            PartyCode = "SUP-SI-01",
            PartyType = PartyType.Supplier,
            NameAr = "invoice supplier",
            IsActive = true
        };
        await TestApp.AddAsync(supplier);
        _supplierId = supplier.Id;

        var prResult = await TestApp.SendAsync(new CreatePurchaseRequestCommand(
            DateTime.UtcNow, null, null, null, PurchaseRequestPriority.Normal, "PR for Invoice",
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
    public async Task CreateAndMatchInvoice_ShouldSucceed()
    {
        var createResult = await TestApp.SendAsync(new CreateSupplierInvoiceCommand(
            _poId, "SUP-INV-001", DateOnly.FromDateTime(DateTime.UtcNow), null, null,
            1000m, null, null, null, null, 1000m, null, null,
            [new CreateSupplierInvoiceDetailDto(
                _poDetailId, null, _itemId, 10, 100m, null, null, 1000m, null)]));
        createResult.Succeeded.ShouldBeTrue();
        var invoiceId = createResult.Value;

        var invoice = await TestApp.SendAsync(new GetSupplierInvoiceByIdQuery(invoiceId));
        invoice.Status.ShouldBe(SupplierInvoiceStatus.Draft);

        await TestApp.SendAsync(new SubmitSupplierInvoiceCommand(invoiceId));
        invoice = await TestApp.SendAsync(new GetSupplierInvoiceByIdQuery(invoiceId));
        invoice.Status.ShouldBe(SupplierInvoiceStatus.Submitted);

        await TestApp.SendAsync(new MatchSupplierInvoiceCommand(invoiceId));
        invoice = await TestApp.SendAsync(new GetSupplierInvoiceByIdQuery(invoiceId));
        invoice.Status.ShouldBe(SupplierInvoiceStatus.Matched);
    }

    [Test]
    public async Task CancelInvoice_ShouldSucceed()
    {
        var createResult = await TestApp.SendAsync(new CreateSupplierInvoiceCommand(
            _poId, "SUP-INV-002", DateOnly.FromDateTime(DateTime.UtcNow), null, null,
            1000m, null, null, null, null, 1000m, null, null,
            [new CreateSupplierInvoiceDetailDto(
                _poDetailId, null, _itemId, 10, 100m, null, null, 1000m, null)]));
        var invoiceId = createResult.Value;

        await TestApp.SendAsync(new CancelSupplierInvoiceCommand(invoiceId, "Cancelled"));
        var invoice = await TestApp.SendAsync(new GetSupplierInvoiceByIdQuery(invoiceId));
        invoice.Status.ShouldBe(SupplierInvoiceStatus.Cancelled);
    }

    [Test]
    public async Task GetInvoiceList_ShouldFilterByStatus()
    {
        var createResult = await TestApp.SendAsync(new CreateSupplierInvoiceCommand(
            _poId, "SUP-INV-003", DateOnly.FromDateTime(DateTime.UtcNow), null, null,
            1000m, null, null, null, null, 1000m, null, null,
            [new CreateSupplierInvoiceDetailDto(
                _poDetailId, null, _itemId, 10, 100m, null, null, 1000m, null)]));
        var invoiceId = createResult.Value;
        await TestApp.SendAsync(new SubmitSupplierInvoiceCommand(invoiceId));

        var submitted = await TestApp.SendAsync(new GetSupplierInvoicesQuery(Status: SupplierInvoiceStatus.Submitted));
        submitted.Value.Items.ShouldContain(x => x.Id == invoiceId);

        var draft = await TestApp.SendAsync(new GetSupplierInvoicesQuery(Status: SupplierInvoiceStatus.Draft));
        draft.Value.Items.ShouldNotContain(x => x.Id == invoiceId);
    }

    [Test]
    public async Task DuplicateInvoice_ShouldFail()
    {
        var createResult = await TestApp.SendAsync(new CreateSupplierInvoiceCommand(
            _poId, "SUP-INV-DUP", DateOnly.FromDateTime(DateTime.UtcNow), null, null,
            1000m, null, null, null, null, 1000m, null, null,
            [new CreateSupplierInvoiceDetailDto(
                _poDetailId, null, _itemId, 10, 100m, null, null, 1000m, null)]));
        createResult.Succeeded.ShouldBeTrue();

        var duplicateResult = await TestApp.SendAsync(new CreateSupplierInvoiceCommand(
            _poId, "SUP-INV-DUP", DateOnly.FromDateTime(DateTime.UtcNow), null, null,
            1000m, null, null, null, null, 1000m, null, null,
            [new CreateSupplierInvoiceDetailDto(
                _poDetailId, null, _itemId, 10, 100m, null, null, 1000m, null)]));
        duplicateResult.Succeeded.ShouldBeFalse();
    }
}
