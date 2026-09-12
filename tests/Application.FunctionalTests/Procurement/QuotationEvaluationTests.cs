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
using ERP_Government.Application.Procurement.Queries.Quotations.GetQuotations;
using ERP_Government.Application.Procurement.Queries.Quotations.GetQuotationById;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Parties.Entities;
using ERP_Government.Domain.Parties.Enums;
using ERP_Government.Domain.Procurement.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Procurement;

[TestFixture]
public class QuotationEvaluationTests : TestBase
{
    private int _itemId;
    private int _unitId;
    private int _supplierId1;
    private int _supplierId2;
    private int _prId;
    private int _prDetailId;
    private int _rfqId;

    [SetUp]
    public async Task SeedTestData()
    {
        await TestApp.RunAsAdministratorAsync();

        var unit = new Unit { Code = "QU-001", Name = "Piece", IsActive = true };
        await TestApp.AddAsync(unit);
        _unitId = unit.Id;

        var item = new Item
        {
            Code = "QI-001",
            Name = "Quotation Test Item",
            UnitId = _unitId,
            ItemType = "Goods",
            IsActive = true
        };
        await TestApp.AddAsync(item);
        _itemId = item.Id;

        var supplier1 = new Party
        {
            PartyCode = "SUP-QUO-01",
            PartyType = PartyType.Supplier,
            NameAr = "quotation supplier one",
            IsActive = true
        };
        await TestApp.AddAsync(supplier1);
        _supplierId1 = supplier1.Id;

        var supplier2 = new Party
        {
            PartyCode = "SUP-QUO-02",
            PartyType = PartyType.Supplier,
            NameAr = "quotation supplier two",
            IsActive = true
        };
        await TestApp.AddAsync(supplier2);
        _supplierId2 = supplier2.Id;

        var prResult = await TestApp.SendAsync(new CreatePurchaseRequestCommand(
            DateTime.UtcNow, null, null, null, PurchaseRequestPriority.Normal, "PR for Quotation",
            [new PurchaseRequestLineDto(_itemId, _unitId, 10, 100m, null)]));
        prResult.Succeeded.ShouldBeTrue();
        _prId = prResult.Value;

        var prDetail = await TestApp.SendAsync(new ERP_Government.Application.Procurement.Queries.PurchaseRequests.GetPurchaseRequestById.GetPurchaseRequestByIdQuery(_prId));
        _prDetailId = prDetail.Details.First().Id;

        var rfqResult = await TestApp.SendAsync(new CreateRFQCommand(
            _prId, null, null, null, null, [_supplierId1, _supplierId2]));
        rfqResult.Succeeded.ShouldBeTrue();
        _rfqId = rfqResult.Value;

        await TestApp.SendAsync(new PublishRFQCommand(_rfqId));
        await TestApp.SendAsync(new CloseRFQCollectionCommand(_rfqId));
    }

    private async Task<int> CreateAndSubmitQuotation(
        int supplierId, int rfqSupplierId, decimal unitPrice, List<QuotationLineDto>? lines = null)
    {
        var result = await TestApp.SendAsync(new CreateQuotationCommand(
            _rfqId, rfqSupplierId, supplierId, DateTime.UtcNow, null, null, null, null, null,
            null, null, null, null, null,
            lines ?? [new QuotationLineDto(_prDetailId, _itemId, _unitId, 10, unitPrice, null, null, null)]));
        result.Succeeded.ShouldBeTrue();
        await TestApp.SendAsync(new SubmitQuotationCommand(result.Value));
        return result.Value;
    }

    [Test]
    public async Task FullEvaluationCycle_ShouldComplete()
    {
        var q1 = await CreateAndSubmitQuotation(_supplierId1, 1, 100m);
        var q2 = await CreateAndSubmitQuotation(_supplierId2, 2, 120m);

        await TestApp.SendAsync(new StartEvaluationCommand(q1));
        var q = await TestApp.SendAsync(new GetQuotationByIdQuery(q1));
        q.Status.ShouldBe(QuotationStatus.UnderEvaluation);

        await TestApp.SendAsync(new CompleteEvaluationCommand(q1, 85m, 90m, null));
        await TestApp.SendAsync(new CompleteEvaluationCommand(q2, 80m, 85m, null));

        await TestApp.SendAsync(new SelectQuotationCommand(q1, "Best value"));
        q = await TestApp.SendAsync(new GetQuotationByIdQuery(q1));
        q.Status.ShouldBe(QuotationStatus.Selected);

        await TestApp.SendAsync(new AwardQuotationCommand(q1));
        q = await TestApp.SendAsync(new GetQuotationByIdQuery(q1));
        q.Status.ShouldBe(QuotationStatus.Awarded);
    }

    [Test]
    public async Task StartEvaluation_ShouldCreateAuditRecord()
    {
        var q1 = await CreateAndSubmitQuotation(_supplierId1, 1, 100m);

        await TestApp.SendAsync(new StartEvaluationCommand(q1));

        var q = await TestApp.SendAsync(new GetQuotationByIdQuery(q1));
        q.Status.ShouldBe(QuotationStatus.UnderEvaluation);
    }

    [Test]
    public async Task GetQuotations_ShouldFilterBySupplier()
    {
        var q1 = await CreateAndSubmitQuotation(_supplierId1, 1, 100m);
        await CreateAndSubmitQuotation(_supplierId2, 2, 120m);

        var filtered = await TestApp.SendAsync(new GetQuotationsQuery(SupplierPartyId: _supplierId1));
        filtered.Value.Items.Count.ShouldBe(1);
        filtered.Value.Items.First().SupplierPartyId.ShouldBe(_supplierId1);
    }
}
