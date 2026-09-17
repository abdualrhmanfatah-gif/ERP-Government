using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Inventory.Locations.Commands.UpdateLocation;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Inventory.Locations;

[TestFixture]
public class UpdateLocationCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<DbSet<Location>> _locationsMock = null!;
    private List<Location> _locations = null!;
    private UpdateLocationCommandHandler _handler = null!;
    private UpdateLocationCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _locations =
        [
            new() { Id = 1, Code = "A", Name = "أ", Level = null, Breadcrumb = null, IsActive = true, RowVersion = [1] },
            new() { Id = 2, Code = "B", Name = "ب", ParentLocationId = 1, Level = 1, Breadcrumb = "A", IsActive = true, RowVersion = [1] },
            new() { Id = 3, Code = "C", Name = "ج", ParentLocationId = 2, Level = 2, Breadcrumb = "A/B", IsActive = true, RowVersion = [1] },
            new() { Id = 4, Code = "D", Name = "د", Level = null, Breadcrumb = null, IsActive = true, RowVersion = [1] },
        ];
        _locationsMock = _locations.AsQueryable().BuildMockForAsync();
        _locationsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
                ValueTask.FromResult<Location?>(_locations.FirstOrDefault(l => l.Id == (int)keys[0])));
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.Locations).Returns(_locationsMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _handler = new UpdateLocationCommandHandler(_contextMock.Object);
        _validator = new UpdateLocationCommandValidator(_contextMock.Object);
    }

    private static UpdateLocationCommand ValidCommand() => new(
        2, "B", "ب", null, 1, null, null, null, [1]);

    [Test]
    public async Task Handle_MoveSubtree_ShouldCascadeLevelsAndBreadcrumbs()
    {
        var result = await _handler.Handle(ValidCommand() with { ParentLocationId = 4 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        var b = _locations.First(l => l.Id == 2);
        var c = _locations.First(l => l.Id == 3);
        b.Level.ShouldBe(1);
        b.Breadcrumb.ShouldBe("D");
        c.Level.ShouldBe(2);
        c.Breadcrumb.ShouldBe("D/B");
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnNotFound()
    {
        var result = await _handler.Handle(ValidCommand() with { Id = 999 }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationNotFound");
    }

    [Test]
    public async Task Validate_SelfParent_ShouldFail()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { ParentLocationId = 2 });

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task Validate_CycleThroughDescendant_ShouldFail()
    {
        // A(1) -> B(2) -> C(3); setting A's parent to C creates a cycle.
        var command = ValidCommand() with { Id = 1, ParentLocationId = 3 };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public async Task Validate_DuplicateCode_ShouldFail()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { Code = "A" });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(UpdateLocationCommand.Code));
    }
}
