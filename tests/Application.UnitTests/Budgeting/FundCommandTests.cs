using ERP_Government.Application.Budgeting.Commands.Funds;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Budgeting.Entities;
using ERP_Government.Domain.Budgeting.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class FundCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    [Test]
    public async Task CreateFund_ValidCommand_ShouldCreateAndReturnId()
    {
        var funds = new List<Fund>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.Funds).Returns(funds.Object);

        funds.Setup(x => x.Add(It.IsAny<Fund>()))
            .Callback<Fund>(e => e.Id = 1);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateFundCommandHandler(_contextMock.Object);

        var command = new CreateFundCommand(
            "F001", "General Fund", FundType.General, FundCategory.Operating,
            "Legal Authority 123", null!, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        funds.Verify(x => x.Add(It.IsAny<Fund>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateFund_DuplicateFundNumber_ShouldReturnFailure()
    {
        var funds = new List<Fund>
        {
            new() { Id = 1, FundNumber = "F001", FundName = "Existing" }
        }.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.Funds).Returns(funds.Object);

        var handler = new CreateFundCommandHandler(_contextMock.Object);

        var command = new CreateFundCommand(
            "F001", "Duplicate", FundType.General, FundCategory.Operating,
            "Legal Authority", null!, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task CreateFund_WithNullOptionalParams_ShouldSucceed()
    {
        var funds = new List<Fund>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.Funds).Returns(funds.Object);

        funds.Setup(x => x.Add(It.IsAny<Fund>()))
            .Callback<Fund>(e => e.Id = 1);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateFundCommandHandler(_contextMock.Object);

        var command = new CreateFundCommand(
            "F001", "Fund", FundType.General, FundCategory.Operating,
            "Legal Authority", null, null, null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task Validator_EmptyFundNumber_ShouldHaveError()
    {
        var validator = new CreateFundCommandValidator();
        var command = new CreateFundCommand(
            "", "Fund Name", FundType.General, FundCategory.Operating,
            "Legal Authority", null!, null, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FundNumber");
    }

    [Test]
    public async Task Validator_EmptyFundName_ShouldHaveError()
    {
        var validator = new CreateFundCommandValidator();
        var command = new CreateFundCommand(
            "F001", "", FundType.General, FundCategory.Operating,
            "Legal Authority", null, null, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FundName");
    }

    [Test]
    public async Task Validator_EmptyLegalAuthority_ShouldHaveError()
    {
        var validator = new CreateFundCommandValidator();
        var command = new CreateFundCommand(
            "F001", "Fund Name", FundType.General, FundCategory.Operating,
            "", null!, null, null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "LegalAuthority");
    }
}
