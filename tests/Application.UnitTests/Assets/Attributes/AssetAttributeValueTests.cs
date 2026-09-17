using ERP_Government.Application.Assets.AssetAttributes.Values;
using ERP_Government.Application.Assets.Assets.Commands.CreateAsset;
using ERP_Government.Application.Assets.Assets.Commands.UpdateAsset;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using ERP_Government.Infrastructure.Data.Seeds;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Attributes;

[TestFixture]
public class AssetAttributeValueTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IDocumentSequenceService> _sequence = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _sequence = new Mock<IDocumentSequenceService>();
        _sequence.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("AST-000001");

        _context.AssetGroups.Add(new AssetGroup { Id = 1, Code = "AG-1", Name = "مجموعة" });
        _context.AssetAttributeDefinitions.AddRange(
            new AssetAttributeDefinition { Id = 1, Code = "COLOR", Name = "اللون", AttributeDataType = AssetAttributeDataType.Text },
            new AssetAttributeDefinition { Id = 2, Code = "WEIGHT", Name = "الوزن", AttributeDataType = AssetAttributeDataType.Decimal });
        _context.AssetGroupAttributes.AddRange(
            new AssetGroupAttribute { AssetGroupId = 1, AssetAttributeDefinitionId = 1, IsRequired = true },
            new AssetGroupAttribute { AssetGroupId = 1, AssetAttributeDefinitionId = 2, IsRequired = false });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private CreateAssetCommand CreateCommand(params AttributeValueInput[] values) => new(
        Name: "أصل بمواصفات", Description: null, AssetGroupId: 1,
        LocationId: null, EmployeeId: null,
        AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
        OriginalValue: 100m, AcquisitionCost: null,
        PurchaseDate: new DateOnly(2026, 1, 1), DepreciationStartDate: new DateOnly(2026, 2, 1),
        AcquisitionType: "Purchase", UsefulLifeYears: 5, Notes: null,
        AttributeValues: values.ToList());

    [TestCase("DEVICE_TYPE", AssetAttributeDataType.Text)]
    [TestCase("MODEL", AssetAttributeDataType.Text)]
    [TestCase("PROCESSOR", AssetAttributeDataType.Text)]
    [TestCase("RAM_GB", AssetAttributeDataType.Integer)]
    [TestCase("RAM_TYPE", AssetAttributeDataType.Text)]
    [TestCase("STORAGE_TYPE", AssetAttributeDataType.Text)]
    [TestCase("STORAGE_GB", AssetAttributeDataType.Integer)]
    [TestCase("GRAPHICS_CARD", AssetAttributeDataType.Text)]
    [TestCase("OPERATING_SYSTEM", AssetAttributeDataType.Text)]
    [TestCase("SCREEN_SIZE_INCH", AssetAttributeDataType.Decimal)]
    [TestCase("SCREEN_RESOLUTION", AssetAttributeDataType.Text)]
    [TestCase("ARABIC_KEYBOARD", AssetAttributeDataType.Boolean)]
    public void ComputerSeed_DefinesAndBindsOptionalAttribute(string code, AssetAttributeDataType dataType)
    {
        var definition = AssetAttributeDefinitionSeedData.GetDefinitions().Single(d => d.Code == code);
        definition.AttributeDataType.ShouldBe(dataType);
        definition.IsActive.ShouldBeTrue();

        var binding = AssetGroupAttributeSeedData.Bindings.Single(b =>
            b.GroupCode == "AG-004-01" && b.DefinitionCode == code);
        binding.IsRequired.ShouldBeFalse();
    }

    [Test]
    public void ComputerSeed_ReusesExistingDefinitionsWithUniqueBindingsAndSortOrders()
    {
        var definitions = AssetAttributeDefinitionSeedData.GetDefinitions();
        definitions.Select(d => d.Code).Distinct().Count().ShouldBe(definitions.Count);

        var bindings = AssetGroupAttributeSeedData.Bindings
            .Where(b => b.GroupCode == "AG-004-01").ToList();
        bindings.Select(b => b.DefinitionCode).Distinct().Count().ShouldBe(bindings.Count);
        bindings.Select(b => b.SortOrder).Distinct().Count().ShouldBe(bindings.Count);

        foreach (var code in new[] { "MANUFACTURER", "SERIAL_NO", "MODEL_YEAR", "WARRANTY_END", "CONDITION" })
        {
            definitions.Count(d => d.Code == code).ShouldBe(1);
            bindings.Count(b => b.DefinitionCode == code).ShouldBe(1);
        }
    }

    [Test]
    public async Task Create_WithTypedValues_PersistsExactlyOneTypedColumnPerValue()
    {
        var handler = new CreateAssetCommandHandler(_context, _sequence.Object);

        var result = await handler.Handle(CreateCommand(
            new AttributeValueInput(1, "أحمر", null, null, null, null),
            new AttributeValueInput(2, null, null, 12.5m, null, null)), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var values = _context.AssetAttributeValues.OrderBy(v => v.AssetAttributeDefinitionId).ToList();
        values.Count.ShouldBe(2);

        values[0].TextValue.ShouldBe("أحمر");
        values[0].DecimalValue.ShouldBeNull();
        values[0].DateValue.ShouldBeNull();
        values[0].BooleanValue.ShouldBeNull();

        values[1].DecimalValue.ShouldBe(12.5m);
        values[1].TextValue.ShouldBeNull();
    }

    [Test]
    public async Task Create_MissingRequiredBinding_FailsValidation()
    {
        var handler = new CreateAssetCommandHandler(_context, _sequence.Object);

        var result = await handler.Handle(CreateCommand(), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidAttributeValue);
        _context.Assets.Count().ShouldBe(0);
        _context.AssetAttributeValues.Count().ShouldBe(0);
    }

    [Test]
    public async Task Create_ValueOfWrongType_FailsValidation()
    {
        var handler = new CreateAssetCommandHandler(_context, _sequence.Object);

        var result = await handler.Handle(CreateCommand(
            new AttributeValueInput(1, null, null, 99m, null, null)), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidAttributeValue);
    }

    [Test]
    public async Task Update_ClearingOptionalValue_RemovesRow_AndKeepsRequiredValue()
    {
        _context.Assets.Add(new Asset
        {
            Id = 5, Code = "AST-5", Name = "أصل", AssetGroupId = 1, CurrencyId = 1,
            OriginalValue = 100m, Status = "Draft", RowVersion = [1]
        });
        _context.AssetAttributeValues.Add(new AssetAttributeValue
        {
            Id = 50, AssetId = 5, AssetAttributeDefinitionId = 2, DecimalValue = 5m
        });
        _context.SaveChanges();

        var handler = new UpdateAssetCommandHandler(_context);

        var result = await handler.Handle(new UpdateAssetCommand(
            Id: 5, Name: "أصل", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 100m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2026, 1, 1), DepreciationStartDate: new DateOnly(2026, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null,
            Status: null, RowVersion: [1],
            AttributeValues:
            [
                new AttributeValueInput(1, "أزرق", null, null, null, null),
                new AttributeValueInput(2, null, null, null, null, null)
            ]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var values = _context.AssetAttributeValues.Where(v => v.AssetId == 5).ToList();
        values.Count.ShouldBe(1);
        values[0].AssetAttributeDefinitionId.ShouldBe(1);
        values[0].TextValue.ShouldBe("أزرق");
    }
}
