using ERP_Government.Application.Assets.Assets.Commands.DeactivateAsset;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Assets.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Assets.Commands.DeactivateAsset;

[TestFixture]
public class DeactivateAssetCommandTests
{
    private DeactivateAssetCommandHandler _handler = null!;
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new DeactivateAssetCommandHandler(_contextMock.Object);
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
        _contextMock.Setup(c => c.Assets).Returns(mock.Object);
    }

    [Test]
    public async Task Handle_ActiveAsset_ReturnsSuccess()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Active",
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new DeactivateAssetCommand(1, [1, 2, 3, 4, 5, 6, 7, 8]);
        SetupAssets(existingAsset);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        existingAsset.Status.ShouldBe("Disposed");
    }

    [Test]
    public async Task Handle_UnderMaintenanceAsset_ReturnsSuccess()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "UnderMaintenance",
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new DeactivateAssetCommand(1, [1, 2, 3, 4, 5, 6, 7, 8]);
        SetupAssets(existingAsset);
        _contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        existingAsset.Status.ShouldBe("Disposed");
    }

    [Test]
    public async Task Handle_DraftAsset_ReturnsInvalidTransitionFailure()
    {
        var existingAsset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Draft",
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };

        var command = new DeactivateAssetCommand(1, [1, 2, 3, 4, 5, 6, 7, 8]);
        SetupAssets(existingAsset);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.InvalidStatusTransition);
    }

    [Test]
    public async Task Handle_AssetNotFound_ReturnsNotFoundFailure()
    {
        var command = new DeactivateAssetCommand(999, [1, 2, 3, 4, 5, 6, 7, 8]);
        SetupAssets();

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.AssetNotFound);
    }
}
