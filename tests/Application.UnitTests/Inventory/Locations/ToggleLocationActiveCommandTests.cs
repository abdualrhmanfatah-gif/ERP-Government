using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Inventory.Locations.Commands.ToggleLocationActive;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Inventory.Locations;

[TestFixture]
public class ToggleLocationActiveCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<DbSet<Location>> _locationsMock = null!;
    private List<Location> _locations = null!;
    private List<Asset> _assets = null!;
    private ToggleLocationActiveCommandHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _locations =
        [
            new() { Id = 1, Code = "A", Name = "أ", IsActive = true, RowVersion = [1] },
            new() { Id = 2, Code = "B", Name = "ب", ParentLocationId = 1, IsActive = true, RowVersion = [1] },
            new() { Id = 3, Code = "C", Name = "ج", IsActive = false, RowVersion = [1] },
        ];
        _assets = [];
        _locationsMock = _locations.AsQueryable().BuildMockForAsync();
        _locationsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
                ValueTask.FromResult<Location?>(_locations.FirstOrDefault(l => l.Id == (int)keys[0])));
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.Locations).Returns(_locationsMock.Object);
        _contextMock.Setup(x => x.Assets).Returns(_assets.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _handler = new ToggleLocationActiveCommandHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_DeactivateLeafWithoutAssets_ShouldSucceed()
    {
        var result = await _handler.Handle(new ToggleLocationActiveCommand(2, false, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _locations.First(l => l.Id == 2).IsActive.ShouldBeFalse();
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_AlreadyInactive_ShouldReturnUnchanged()
    {
        var result = await _handler.Handle(new ToggleLocationActiveCommand(3, false, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationStateUnchanged");
    }

    [Test]
    public async Task Handle_DeactivateWithLinkedAssets_ShouldBeBlocked()
    {
        _assets.Add(new Asset { Id = 10, LocationId = 2 });

        var result = await _handler.Handle(new ToggleLocationActiveCommand(2, false, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationDeactivationBlocked");
    }

    [Test]
    public async Task Handle_DeactivateParentWithActiveChildren_ShouldBeBlocked()
    {
        var result = await _handler.Handle(new ToggleLocationActiveCommand(1, false, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationDeactivationBlocked");
    }

    [Test]
    public async Task Handle_ActivateUnderInactiveParent_ShouldBeBlocked()
    {
        _locations.Add(new Location { Id = 4, Code = "D", Name = "د", ParentLocationId = 3, IsActive = false, RowVersion = [1] });

        var result = await _handler.Handle(new ToggleLocationActiveCommand(4, true, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationParentInactive");
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnNotFound()
    {
        var result = await _handler.Handle(new ToggleLocationActiveCommand(999, false, [1]), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationNotFound");
    }
}
