using ERP_Government.Application.Assets.Assets.Commands.CreateAsset;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Common.Models;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Assets.Entities;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Assets.Commands.CreateAsset;

[TestFixture]
public class CreateAssetCommandTests
{
    private CreateAssetCommandHandler _handler = null!;
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<IDocumentSequenceService> _sequenceServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _sequenceServiceMock = new Mock<IDocumentSequenceService>();
        _handler = new CreateAssetCommandHandler(_contextMock.Object, _sequenceServiceMock.Object);
    }

    private void SetupAssets(params Asset[] assets)
    {
        var mock = assets.AsQueryable().BuildMockForAsync();
        mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
            {
                var id = (int)keys[0];
                return ValueTask.FromResult<Asset?>(assets.FirstOrDefault(a => a.Id == id));
            });
        mock.Setup(m => m.Add(It.IsAny<Asset>()))
            .Callback<Asset>(a => a.Id = assets.Length > 0 ? assets.Max(x => x.Id) + 1 : 1);
        _contextMock.Setup(c => c.Assets).Returns(mock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ReturnsSuccessWithNewId()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: "طابعة ليزر", AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: 5, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1, Code = "GRP-001", Name = "أجهزة مكتبية" } }.AsQueryable().BuildMockForAsync().Object);
        SetupAssets();
        _contextMock.Setup(c => c.AssetAttributeDefinitions).Returns(new List<AssetAttributeDefinition>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.AssetAttributeValues).Returns(new List<AssetAttributeValue>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.AssetGroupAttributes).Returns(new List<AssetGroupAttribute>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task Handle_DuplicateAssetTag_ReturnsFailure()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: "TAG-001", Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Assets).Returns(new List<Asset> { new() { Id = 1, AssetTag = "TAG-001" } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.DuplicateAssetTag);
    }

    [Test]
    public async Task Handle_DuplicateBarcode_ReturnsFailure()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: "123456789", SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Assets).Returns(new List<Asset> { new() { Id = 1, Barcode = "123456789" } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.DuplicateBarcode);
    }

    [Test]
    public async Task Handle_DuplicateSerialNumber_ReturnsFailure()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: "SN-001", CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Assets).Returns(new List<Asset> { new() { Id = 1, SerialNumber = "SN-001" } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.DuplicateSerialNumber);
    }

    [Test]
    public async Task Handle_InvalidAssetGroupId_ReturnsFailure()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 999,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup>().AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.AssetGroupNotFound);
    }

    [Test]
    public async Task Handle_DocumentSequenceServiceFails_ReturnsFailure()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("Service unavailable"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Category.ShouldBe(ErrorCategory.Internal);
    }

    [Test]
    public void Validator_NameEmpty_ReturnsError()
    {
        var validator = new CreateAssetCommandValidator();
        var command = new CreateAssetCommand(
            Name: "", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public void Validator_AssetGroupIdZero_ReturnsError()
    {
        var validator = new CreateAssetCommandValidator();
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 0,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "AssetGroupId");
    }

    [Test]
    public void Validator_AcquisitionTypeEmpty_ReturnsError()
    {
        var validator = new CreateAssetCommandValidator();
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "", UsefulLifeYears: null, Notes: null);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "AcquisitionType");
    }

    [Test]
    public void Validator_AcquisitionTypeInvalid_ReturnsError()
    {
        var validator = new CreateAssetCommandValidator();
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "شراء", UsefulLifeYears: null, Notes: null);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "AcquisitionType");
    }

    [Test]
    public async Task Handle_UnknownLocation_ReturnsLocationNotFound()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: 999, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: 5, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1, Code = "GRP-001", Name = "أجهزة مكتبية" } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Locations).Returns(new List<ERP_Government.Domain.Inventory.Entities.Location>
            { new() { Id = 1, Code = "LOC-001", Name = "موقع آخر", IsActive = true } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationNotFound");
    }

    [Test]
    public async Task Handle_InactiveLocation_ReturnsLocationInactive()
    {
        var command = new CreateAssetCommand(
            Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: 5, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: 5, Notes: null);

        _sequenceServiceMock.Setup(s => s.GenerateNextNumberAsync("Asset", It.IsAny<CancellationToken>())).ReturnsAsync("AST-000001");
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1, Code = "GRP-001", Name = "أجهزة مكتبية" } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Locations).Returns(new List<ERP_Government.Domain.Inventory.Entities.Location>
            { new() { Id = 5, Code = "LOC-005", Name = "معطل", IsActive = false } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationInactive");
    }
}
