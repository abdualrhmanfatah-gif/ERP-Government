using ERP_Government.Application.Budgeting.Commands.BudgetClassifications;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Budgeting.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Budgeting;

[TestFixture]
public class BudgetClassificationCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
    }

    [Test]
    public async Task CreateBudgetClassification_ValidCommand_ShouldCreateAndReturnId()
    {
        var classifications = new List<BudgetClassification>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetClassifications).Returns(classifications.Object);

        classifications.Setup(x => x.Add(It.IsAny<BudgetClassification>()))
            .Callback<BudgetClassification>(e => e.Id = 1);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreateBudgetClassificationCommandHandler(_contextMock.Object);

        var command = new CreateBudgetClassificationCommand("BC001", "Personnel Services", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        classifications.Verify(x => x.Add(It.IsAny<BudgetClassification>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CreateBudgetClassification_DuplicateCode_ShouldReturnFailure()
    {
        var classifications = new List<BudgetClassification>
        {
            new() { Id = 1, Code = "BC001", Name = "Existing" }
        }.AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetClassifications).Returns(classifications.Object);

        var handler = new CreateBudgetClassificationCommandHandler(_contextMock.Object);

        var command = new CreateBudgetClassificationCommand("BC001", "Duplicate", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("already exists"));
    }

    [Test]
    public async Task CreateBudgetClassification_InvalidParentId_ShouldReturnFailure()
    {
        var classifications = new List<BudgetClassification>().AsQueryable().BuildMockForAsync();
        _contextMock.Setup(x => x.BudgetClassifications).Returns(classifications.Object);

        var handler = new CreateBudgetClassificationCommandHandler(_contextMock.Object);

        var command = new CreateBudgetClassificationCommand("BC001", "Child", 999);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("Parent budget classification not found"));
    }

    [Test]
    public async Task Validator_EmptyCode_ShouldHaveError()
    {
        var validator = new CreateBudgetClassificationCommandValidator();
        var command = new CreateBudgetClassificationCommand("", "Name", null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Code");
    }

    [Test]
    public async Task Validator_EmptyName_ShouldHaveError()
    {
        var validator = new CreateBudgetClassificationCommandValidator();
        var command = new CreateBudgetClassificationCommand("BC001", "", null);

        var result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }
}
