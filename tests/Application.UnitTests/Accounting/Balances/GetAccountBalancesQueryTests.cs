using AutoMapper;
using FluentValidation.TestHelper;
using ERP_Government.Application.Accounting.Common;
using ERP_Government.Application.Accounting.Queries.Balances.GetAccountBalances;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.FinancialSettings.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Accounting.Balances;

[TestFixture]
public class GetAccountBalancesQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IMapper> _mapperMock = null!;
    private GetAccountBalancesQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetAccountBalancesQueryHandler(_contextMock.Object, _mapperMock.Object);
    }

    private static AccountBalance Balance(int id, int accountId, string code, string name, int periodId, string periodName)
    {
        return new AccountBalance
        {
            Id = id,
            AccountId = accountId,
            Account = new Account { Id = accountId, Code = code, Name = name, AccountGroupId = 1 },
            FiscalYearId = 1,
            FiscalYear = new FiscalYear { Id = 1, Name = "2026" },
            FiscalPeriodId = periodId,
            FiscalPeriod = new FiscalPeriod { Id = periodId, Name = periodName },
            CurrencyId = 1,
            Currency = new Currency { Id = 1, Code = "YER" },
            ClosingDebit = 100m,
        };
    }

    [Test]
    public async Task Handle_WithoutPeriod_ReturnsAllPeriodsForYear()
    {
        var balances = new List<AccountBalance>
        {
            Balance(1, 100, "1001", "Cash", 1, "January"),
            Balance(2, 100, "1001", "Cash", 2, "February"),
        }.AsQueryable();

        _contextMock.Setup(x => x.AccountBalances)
            .Returns(balances.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<AccountBalanceDto>>(It.IsAny<List<AccountBalance>>()))
            .Returns((List<AccountBalance> src) => src.Select(b => new AccountBalanceDto
            {
                Id = b.Id,
                AccountId = b.AccountId,
                AccountCode = b.Account.Code,
                FiscalPeriodId = b.FiscalPeriodId,
                PeriodName = b.FiscalPeriod.Name,
            }).ToList());

        var query = new GetAccountBalancesQuery { FiscalYearId = 1 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.Select(r => r.FiscalPeriodId).ShouldBe(new[] { 1, 2 }, true);
    }

    [Test]
    public async Task Handle_WithSpecificPeriod_ReturnsOnlyThatPeriod()
    {
        var balances = new List<AccountBalance>
        {
            Balance(1, 100, "1001", "Cash", 1, "January"),
            Balance(2, 100, "1001", "Cash", 2, "February"),
        }.AsQueryable();

        _contextMock.Setup(x => x.AccountBalances)
            .Returns(balances.BuildMockForAsync().Object);
        _mapperMock.Setup(x => x.Map<List<AccountBalanceDto>>(It.IsAny<List<AccountBalance>>()))
            .Returns((List<AccountBalance> src) => src.Select(b => new AccountBalanceDto
            {
                Id = b.Id,
                FiscalPeriodId = b.FiscalPeriodId,
            }).ToList());

        var query = new GetAccountBalancesQuery { FiscalYearId = 1, FiscalPeriodId = 2 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].FiscalPeriodId.ShouldBe(2);
    }

    [Test]
    public void Validator_WithoutPeriod_ShouldPass()
    {
        var validator = new GetAccountBalancesQueryValidator();
        var result = validator.TestValidate(new GetAccountBalancesQuery { FiscalYearId = 1 });
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void Validator_WithInvalidPeriod_ShouldFail()
    {
        var validator = new GetAccountBalancesQueryValidator();
        var result = validator.TestValidate(new GetAccountBalancesQuery { FiscalYearId = 1, FiscalPeriodId = 0 });
        result.IsValid.ShouldBeFalse();
    }
}
