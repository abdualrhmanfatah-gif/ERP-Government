using ERP_Government.Application.Assets.AssetGroups.Commands.UpdateAssetGroup;
using FluentValidation;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class UpdateAssetGroupCommandValidatorTests
{
    private UpdateAssetGroupCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateAssetGroupCommandValidator();
    }

    [Test]
    public void Validate_ZeroId_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Id = 0 };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAssetGroupCommand.Id));
    }

    [Test]
    public void Validate_EmptyName_ShouldHaveError()
    {
        var command = CreateValidCommand() with { Name = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAssetGroupCommand.Name));
    }

    [Test]
    public void Validate_EmptyAssetCategory_ShouldHaveError()
    {
        var command = CreateValidCommand() with { AssetCategory = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAssetGroupCommand.AssetCategory));
    }

    [Test]
    public void Validate_EmptyDepreciationMethod_ShouldHaveError()
    {
        var command = CreateValidCommand() with { DepreciationMethod = "" };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAssetGroupCommand.DepreciationMethod));
    }

    [Test]
    public void Validate_EmptyRowVersion_ShouldHaveError()
    {
        var command = CreateValidCommand() with { RowVersion = [] };
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateAssetGroupCommand.RowVersion));
    }

    [Test]
    public void Validate_ValidCommand_ShouldPass()
    {
        var command = CreateValidCommand();
        var result = _validator.Validate(command);
        result.IsValid.ShouldBeTrue();
    }

    private static UpdateAssetGroupCommand CreateValidCommand() => new(
        Id: 1,
        Name: "مباني محدثة",
        Description: null,
        ParentAssetGroupId: null,
        AssetCategory: "Building",
        IsDepreciable: true,
        DepreciationMethod: "StraightLine",
        DepreciationRate: null,
        DefaultUsefulLifeYears: null,
        ResidualValuePercentage: null,
        AssetAccountId: null,
        AccumulatedDepreciationAccountId: null,
        DepreciationExpenseAccountId: null,
        DisposalAccountId: null,
        RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);
}
