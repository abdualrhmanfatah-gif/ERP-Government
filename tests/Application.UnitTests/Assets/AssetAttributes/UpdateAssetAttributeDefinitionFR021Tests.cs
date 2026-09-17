using ERP_Government.Application.Assets.AssetAttributes.Definitions.Commands;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetAttributes;

[TestFixture]
public class UpdateAssetAttributeDefinitionFR021Tests
{
    private ApplicationDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.AssetGroups.Add(new AssetGroup { Id = 1, Code = "AG-1", Name = "مجموعة" });
        _context.AssetAttributeDefinitions.AddRange(
            new AssetAttributeDefinition
            {
                Id = 1, Code = "COLOR", Name = "اللون",
                AttributeDataType = AssetAttributeDataType.Text, IsActive = true,
                RowVersion = [1]
            },
            new AssetAttributeDefinition
            {
                Id = 2, Code = "WEIGHT", Name = "الوزن",
                AttributeDataType = AssetAttributeDataType.Decimal, IsActive = true,
                RowVersion = [1]
            });
        _context.Assets.Add(new Asset
        {
            Id = 10, Code = "AST-10", Name = "أصل", AssetGroupId = 1, CurrencyId = 1,
            OriginalValue = 100m, Status = "Draft", RowVersion = [1]
        });
        _context.AssetAttributeValues.Add(new AssetAttributeValue
        {
            Id = 100, AssetId = 10, AssetAttributeDefinitionId = 2, DecimalValue = 50m
        });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task Handle_ChangingDataTypeWhenValuesExist_ReturnsFailure()
    {
        var handler = new UpdateAssetAttributeDefinitionCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetAttributeDefinitionCommand(
            Id: 2, Name: "الوزن", Description: null, Unit: "كغ", SortOrder: 1,
            AttributeDataType: AssetAttributeDataType.Integer,
            IsActive: true, RowVersion: [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidAttributeValue);
    }

    [Test]
    public async Task Handle_ChangingDataTypeWhenNoValuesExist_Succeeds()
    {
        var handler = new UpdateAssetAttributeDefinitionCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetAttributeDefinitionCommand(
            Id: 1, Name: "اللون", Description: null, Unit: null, SortOrder: 1,
            AttributeDataType: AssetAttributeDataType.Integer,
            IsActive: true, RowVersion: [1]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_KeepingSameDataType_Succeeds()
    {
        var handler = new UpdateAssetAttributeDefinitionCommandHandler(_context);
        var result = await handler.Handle(new UpdateAssetAttributeDefinitionCommand(
            Id: 2, Name: "الوزن الجديد", Description: "الوصف", Unit: "كغ", SortOrder: 2,
            AttributeDataType: AssetAttributeDataType.Decimal,
            IsActive: false, RowVersion: [1]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }
}
