using ERP_Government.Application.Budgeting.Commands.BudgetTypes;
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
public class BudgetTypeCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    [Test]
    public async Task CreateBudgetType_ValidCommand_ShouldCreateAndReturnId()
    {
        var budgetTypes = new List<BudgetType>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetTypes).Returns(budgetTypes.Object);

        budgetTypes.Setup(x => x.Add(It.IsAny<BudgetType>()))
            .Callback<BudgetType>(e => e.Id = 1);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateBudgetTypeCommandHandler(_contextMock.Object);

        var command = new CreateBudgetTypeCommand(
            "BT001", "Operating Budget", "Standard operating budget",
            BudgetControlMethod.Warning, false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        budgetTypes.Verify(x => x.Add(It.IsAny<BudgetType>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateBudgetType_DuplicateCode_ShouldReturnFailure()
    {
        var budgetTypes = new List<BudgetType>
        {
            new() { Id = 1, Code = "BT001", Name = "Existing" }
        }.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetTypes).Returns(budgetTypes.Object);

        var handler = new CreateBudgetTypeCommandHandler(_contextMock.Object);

        var command = new CreateBudgetTypeCommand(
            "BT001", "Duplicate", null, BudgetControlMethod.None, false);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task Validator_EmptyCode_ShouldHaveError()
    {
        var validator = new CreateBudgetTypeCommandValidator();
        var command = new CreateBudgetTypeCommand(
            "", "Name", null, BudgetControlMethod.None, false);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Code");
    }

    [Test]
    public async Task Validator_EmptyName_ShouldHaveError()
    {
        var validator = new CreateBudgetTypeCommandValidator();
        var command = new CreateBudgetTypeCommand(
            "BT001", "", null, BudgetControlMethod.None, false);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task Validator_CodeTooLong_ShouldHaveError()
    {
        var validator = new CreateBudgetTypeCommandValidator();
        var command = new CreateBudgetTypeCommand(
            new string('X', 51), "Name", null, BudgetControlMethod.None, false);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Code");
    }

    [Test]
    public async Task Validator_DescriptionTooLong_ShouldHaveError()
    {
        var validator = new CreateBudgetTypeCommandValidator();
        var command = new CreateBudgetTypeCommand(
            "BT001", "Name", new string('X', 501), BudgetControlMethod.None, false);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Description");
    }
}
