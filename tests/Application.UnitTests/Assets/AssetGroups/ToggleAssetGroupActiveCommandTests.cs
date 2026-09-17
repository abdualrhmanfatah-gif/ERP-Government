using ERP_Government.Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class ToggleAssetGroupActiveCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private ToggleAssetGroupActiveCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new ToggleAssetGroupActiveCommandHandler(_contextMock.Object);
    }

    private void SetupAssetGroups(params AssetGroup[] groups)
    {
        var mock = groups.AsQueryable().BuildMockForAsync();
        mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
            {
                var id = (int)keys[0];
                return ValueTask.FromResult<AssetGroup?>(groups.FirstOrDefault(g => g.Id == id));
            });
        _contextMock.Setup(c => c.AssetGroups).Returns(mock.Object);
    }

    [Test]
    public async Task Handle_DeactivateActiveGroup_ShouldReturnSuccess()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });
        var assets = new List<Asset>().AsQueryable();
        _contextMock.Setup(x => x.Assets)
            .Returns(assets.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new ToggleAssetGroupActiveCommand(1, false, [1, 2, 3, 4, 5, 6, 7, 8]);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBe(1);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ActivateInactiveGroup_ShouldReturnSuccess()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = false,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new ToggleAssetGroupActiveCommand(1, true, [1, 2, 3, 4, 5, 6, 7, 8]);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBe(1);
    }

    [Test]
    public async Task Handle_AlreadyActive_ShouldReturnFailure()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });

        var command = new ToggleAssetGroupActiveCommand(1, true, [1, 2, 3, 4, 5, 6, 7, 8]);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AlreadyActive");
    }

    [Test]
    public async Task Handle_AlreadyInactive_ShouldReturnFailure()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = false,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });

        var command = new ToggleAssetGroupActiveCommand(1, false, [1, 2, 3, 4, 5, 6, 7, 8]);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AlreadyActive");
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnFailure()
    {
        SetupAssetGroups();

        var command = new ToggleAssetGroupActiveCommand(999, false, [1, 2, 3, 4, 5, 6, 7, 8]);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetGroupNotFound");
    }

    [Test]
    public async Task Handle_DeactivateWithAssets_ShouldReturnFailure()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });
        var assets = new List<Asset> { new() { Id = 1, AssetGroupId = 1 } }.AsQueryable();
        _contextMock.Setup(x => x.Assets)
            .Returns(assets.BuildMockForAsync().Object);

        var command = new ToggleAssetGroupActiveCommand(1, false, [1, 2, 3, 4, 5, 6, 7, 8]);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.DeactivationBlocked");
    }
}
