using ERP_Government.Application.Assets.AssetGroups.Commands.CreateAssetGroup;
using FluentValidation;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class CreateAssetGroupCommandValidatorTests
{
    private CreateAssetGroupCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateAssetGroupCommandValidator();
    }

    [Test]
    public void Validate_EmptyCode_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Code = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAssetGroupCommand.Code));
    }

    [Test]
    public void Validate_CodeTooLong_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Code = new string('A', 51) };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAssetGroupCommand.Code));
    }

    [Test]
    public void Validate_EmptyName_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Name = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAssetGroupCommand.Name));
    }

    [Test]
    public void Validate_NameTooLong_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Name = new string('A', 201) };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAssetGroupCommand.Name));
    }

    [Test]
    public void Validate_EmptyAssetCategory_ShouldHaveError()
    {
        var command = CreateValidCommand() with { AssetCategory = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAssetGroupCommand.AssetCategory));
    }

    [Test]
    public void Validate_EmptyDepreciationMethod_ShouldHaveError()
    {
        var command = CreateValidCommand() with { DepreciationMethod = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateAssetGroupCommand.DepreciationMethod));
    }

    [Test]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = CreateValidCommand();
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeTrue();
    }

    private static CreateAssetGroupCommand CreateValidCommand() => new(
        Code: "AG-004",
        Name: "أجهزة الحاسوب",
        Description: null,
        ParentAssetGroupId: null,
        AssetCategory: "ComputerEquipment",
        IsDepreciable: true,
        DepreciationMethod: "StraightLine",
        DepreciationRate: null,
        DefaultUsefulLifeYears: null,
        ResidualValuePercentage: null,
        AssetAccountId: null,
        AccumulatedDepreciationAccountId: null,
        DepreciationExpenseAccountId: null,
        DisposalAccountId: null);
}
