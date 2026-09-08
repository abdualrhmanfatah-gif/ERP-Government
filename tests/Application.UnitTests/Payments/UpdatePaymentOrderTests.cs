using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Payments.Commands.PaymentOrders.UpdatePaymentOrder;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Payments;

[TestFixture]
public class UpdatePaymentOrderTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private PaymentOrder _entity = null!;
    private Mock<DbSet<PaymentOrderLine>> _linesMock = null!;
    private Mock<DbSet<PaymentOrderDeduction>> _deductionsMock = null!;

    private const int OrderId = 1;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _entity = new PaymentOrder
        {
            Id = OrderId,
            PaymentOrderNumber = "PO-000001",
            Status = PaymentOrderStatus.Draft,
            AmountGross = 1000m,
            DeductionAmount = 100m,
            VendorId = 1,
            FundId = 1,
            FiscalYearId = 1,
            AppropriationId = 1,
            CurrencyId = 1,
            BeneficiaryName = "Vendor",
            RowVersion = [1, 2, 3]
        };

        var ordersMock = new Mock<DbSet<PaymentOrder>>();
        ordersMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult(
                ((int)kvs[0]) == OrderId ? _entity : null));
        _contextMock.Setup(x => x.PaymentOrders).Returns(ordersMock.Object);

        var lines = new List<PaymentOrderLine>
        {
            new() { Id = 10, PaymentOrderId = OrderId, LineNumber = 1, LineType = PaymentOrderLineType.Invoice, AccountId = 1, Amount = 900m },
            new() { Id = 11, PaymentOrderId = OrderId, LineNumber = 2, LineType = PaymentOrderLineType.Other, AccountId = 2, Amount = 0m }
        };
        _linesMock = lines.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.PaymentOrderLines).Returns(_linesMock.Object);

        var deductions = new List<PaymentOrderDeduction>
        {
            new() { Id = 20, PaymentOrderId = OrderId, LineNumber = 1, DeductionType = DeductionType.Tax, AccountId = 3, Amount = 100m }
        };
        _deductionsMock = deductions.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.PaymentOrderDeductions).Returns(_deductionsMock.Object);

        var partiesMock = new Mock<DbSet<Domain.Parties.Entities.Party>>();
        partiesMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult<Domain.Parties.Entities.Party?>(
                new Domain.Parties.Entities.Party { Id = (int)kvs[0], IsActive = true, PartyType = Domain.Parties.Enums.PartyType.Supplier }));
        _contextMock.Setup(x => x.Parties).Returns(partiesMock.Object);

        var fundsMock = new Mock<DbSet<Domain.Budgeting.Entities.Fund>>();
        fundsMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult<Domain.Budgeting.Entities.Fund?>(
                new Domain.Budgeting.Entities.Fund { Id = (int)kvs[0], IsActive = true }));
        _contextMock.Setup(x => x.Funds).Returns(fundsMock.Object);

        var fyMock = new Mock<DbSet<Domain.FinancialSettings.Entities.FiscalYear>>();
        fyMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult<Domain.FinancialSettings.Entities.FiscalYear?>(
                new Domain.FinancialSettings.Entities.FiscalYear { Id = (int)kvs[0], Status = Domain.FinancialSettings.Enums.FiscalYearStatus.Open }));
        _contextMock.Setup(x => x.FiscalYears).Returns(fyMock.Object);

        var appropMock = new Mock<DbSet<Domain.Budgeting.Entities.Appropriation>>();
        appropMock.Setup(x => x.FindAsync(It.IsAny<object[]>()))
            .Returns<object[]>(kvs => ValueTask.FromResult<Domain.Budgeting.Entities.Appropriation?>(
                new Domain.Budgeting.Entities.Appropriation { Id = (int)kvs[0] }));
        _contextMock.Setup(x => x.Appropriations).Returns(appropMock.Object);

        _contextMock.Setup(x => x.BankAccounts).Returns(new Mock<DbSet<Domain.Payments.Entities.BankAccount>>().Object);

        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
    }

    private static UpdatePaymentOrderCommand ValidCommand(
        byte[]? rowVersion = null,
        List<UpdatePaymentOrderLineDto>? lines = null,
        List<UpdatePaymentOrderDeductionDto>? deductions = null,
        string beneficiaryName = "Vendor")
        => new()
        {
            Id = OrderId,
            RowVersion = rowVersion ?? [1, 2, 3],
            PaymentOrderDate = DateTime.Today,
            PaymentOrderType = "Standard",
            VendorId = 1,
            FundId = 1,
            FiscalYearId = 1,
            AppropriationId = 1,
            CurrencyId = 1,
            AmountGross = 1000m,
            DeductionAmount = 100m,
            BeneficiaryName = beneficiaryName,
            Lines = lines ??
            [
                new UpdatePaymentOrderLineDto { LineType = PaymentOrderLineType.Invoice, AccountId = 1, Amount = 900m }
            ],
            Deductions = deductions ??
            [
                new UpdatePaymentOrderDeductionDto { DeductionType = DeductionType.Tax, AccountId = 3, Amount = 100m }
            ]
        };

    [Test]
    public async Task Update_NonDraft_ShouldReject()
    {
        _entity.Status = PaymentOrderStatus.Submitted;

        var handler = new UpdatePaymentOrderCommandHandler(_contextMock.Object);
        var result = await handler.Handle(ValidCommand(), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Only draft payment orders can be updated"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_RowVersionConflict_ShouldReject()
    {
        var handler = new UpdatePaymentOrderCommandHandler(_contextMock.Object);
        var result = await handler.Handle(
            ValidCommand(rowVersion: [9, 9, 9]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("RowVersion conflict"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_RemovingMandatoryDeduction_ShouldReject()
    {
        var deductions = new List<PaymentOrderDeduction>
        {
            new() { Id = 20, PaymentOrderId = OrderId, LineNumber = 1, DeductionType = DeductionType.Tax, AccountId = 3, Amount = 100m, IsMandatory = true, IsTaxDeduction = true, TaxAuthorityId = 7 }
        };
        _deductionsMock = deductions.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.PaymentOrderDeductions).Returns(_deductionsMock.Object);

        var handler = new UpdatePaymentOrderCommandHandler(_contextMock.Object);
        var result = await handler.Handle(
            ValidCommand(deductions: []), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Mandatory deduction"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task Update_DraftWithValidPayload_ShouldSucceedAndReplaceLines()
    {
        var handler = new UpdatePaymentOrderCommandHandler(_contextMock.Object);
        var result = await handler.Handle(
            ValidCommand(
                beneficiaryName: "Updated Beneficiary",
                lines:
                [
                    new UpdatePaymentOrderLineDto { LineType = PaymentOrderLineType.Invoice, AccountId = 5, Amount = 600m },
                    new UpdatePaymentOrderLineDto { LineType = PaymentOrderLineType.Other, AccountId = 6, Amount = 300m }
                ]),
            CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _entity.BeneficiaryName.ShouldBe("Updated Beneficiary");
        _entity.AmountGross.ShouldBe(1000m);
        _linesMock.Verify(x => x.RemoveRange(It.Is<IEnumerable<PaymentOrderLine>>(l => l.Count() == 2)), Times.Once);
        _contextMock.Verify(x => x.PaymentOrderLines.Add(It.Is<PaymentOrderLine>(l => l.AccountId == 5)), Times.Once);
        _contextMock.Verify(x => x.PaymentOrderLines.Add(It.Is<PaymentOrderLine>(l => l.AccountId == 6)), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Test]
    public async Task Update_LinesNotSummingToNet_ShouldReject()
    {
        var handler = new UpdatePaymentOrderCommandHandler(_contextMock.Object);
        var result = await handler.Handle(
            ValidCommand(lines: [new UpdatePaymentOrderLineDto { LineType = PaymentOrderLineType.Invoice, AccountId = 1, Amount = 500m }]),
            CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Lines must sum to the net amount"));
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
