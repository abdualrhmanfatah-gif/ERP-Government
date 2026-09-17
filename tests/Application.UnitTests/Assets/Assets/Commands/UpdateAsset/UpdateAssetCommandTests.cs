using ERP_Government.Application.Assets.Assets.Commands.UpdateAsset;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Assets.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Assets.Commands.UpdateAsset;

[TestFixture]
public class UpdateAssetCommandTests
{
    private UpdateAssetCommandHandler _handler = null!;
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateAssetCommandHandler(_contextMock.Object);
    }

    private void SetupAssetsWithFindAsync(params Asset[] assets)
    {
        var mock = assets.AsQueryable().BuildMockForAsync();
        mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
            {
                var id = (int)keys[0];
                return ValueTask.FromResult<Asset?>(assets.FirstOrDefault(a => a.Id == id));
            });
        _contextMock.Setup(c => c.Assets).Returns(mock.Object);
    }

    [Test]
    public void Validator_AcquisitionTypeInvalid_ReturnsError()
    {
        var validator = new UpdateAssetCommandValidator();
        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "شراء", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "AcquisitionType");
    }

    [Test]
    public async Task Handle_ValidCommand_ReturnsSuccess()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Draft",
            AssetGroupId = 1, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP محدثة", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        SetupAssetsWithFindAsync(existingAsset);
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task Handle_AssetNotFound_ReturnsFailure()
    {
        var command = new UpdateAssetCommand(
            Id: 999, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        _contextMock.Setup(c => c.Assets).Returns(new List<Asset>().AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.AssetNotFound);
    }

    [Test]
    public async Task Handle_EditOriginalValueOnActiveAsset_ReturnsFieldLockedFailure()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Active",
            AssetGroupId = 1, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 2000m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        SetupAssetsWithFindAsync(existingAsset);
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.FieldLockedAfterActivation);
    }

    [Test]
    public async Task Handle_EditAssetGroupOnActiveAsset_ReturnsFieldLockedFailure()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Active",
            AssetGroupId = 1, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 2,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        SetupAssetsWithFindAsync(existingAsset);
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 }, new() { Id = 2 } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.FieldLockedAfterActivation);
    }

    [Test]
    public async Task Handle_DuplicateAssetTag_ReturnsFailure()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Draft",
            AssetGroupId = 1, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: "TAG-EXISTING", Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        SetupAssetsWithFindAsync(existingAsset, new Asset { Id = 2, AssetTag = "TAG-EXISTING" });
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.DuplicateAssetTag);
    }

    [Test]
    public async Task Handle_InvalidStatusTransition_ReturnsFailure()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Draft",
            AssetGroupId = 1, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: "Disposed",
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        SetupAssetsWithFindAsync(existingAsset);
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Handle_ValidStatusTransition_ReturnsSuccess()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Draft",
            AssetGroupId = 1, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: null, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: "Active",
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        SetupAssetsWithFindAsync(existingAsset);
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        existingAsset.Status.ShouldBe("Active");
    }

    [Test]
    public async Task Handle_ChangedToUnknownLocation_ReturnsLocationNotFound()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Draft",
            AssetGroupId = 1, LocationId = null, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 15), RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new UpdateAssetCommand(
            Id: 1, Name: "طابعة HP", Description: null, AssetGroupId: 1,
            LocationId: 999, EmployeeId: null,
            AssetTag: null, Barcode: null, SerialNumber: null, CurrencyId: 1, ExchangeRateId: null,
            OriginalValue: 1500m, AcquisitionCost: null,
            PurchaseDate: new DateOnly(2025, 1, 15), DepreciationStartDate: new DateOnly(2025, 2, 1),
            AcquisitionType: "Purchase", UsefulLifeYears: null, Notes: null, Status: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        var assetsMock = new List<Asset> { existingAsset }.AsQueryable().BuildMockForAsync();
        assetsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
                ValueTask.FromResult<Asset?>(existingAsset.Id == (int)keys[0] ? existingAsset : null));
        _contextMock.Setup(c => c.Assets).Returns(assetsMock.Object);
        _contextMock.Setup(c => c.AssetGroups).Returns(new List<AssetGroup> { new() { Id = 1 } }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Locations).Returns(new List<ERP_Government.Domain.Inventory.Entities.Location>
            { new() { Id = 1, Code = "LOC-001", Name = "موقع آخر", IsActive = true } }.AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationNotFound");
    }
}
