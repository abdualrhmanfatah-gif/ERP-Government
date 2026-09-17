using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Inventory.Locations.Commands.CreateLocation;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Inventory.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Inventory.Locations;

[TestFixture]
public class CreateLocationCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private Mock<DbSet<Location>> _locationsMock = null!;
    private List<Location> _locations = null!;
    private CreateLocationCommandHandler _handler = null!;
    private CreateLocationCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _locations = [];
        _locationsMock = _locations.AsQueryable().BuildMockForAsync();
        _contextMock = new Mock<IApplicationDbContext>();
        _contextMock.Setup(x => x.Locations).Returns(_locationsMock.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _handler = new CreateLocationCommandHandler(_contextMock.Object);
        _validator = new CreateLocationCommandValidator(_contextMock.Object);
    }

    private static CreateLocationCommand ValidCommand() => new(
        "LOC-001", "موقع رئيسي", null, null, "صنعاء", "شارع حدة", null);

    [Test]
    public async Task Handle_RootLocation_ShouldCreateWithNullLevel()
    {
        var result = await _handler.Handle(ValidCommand(), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _locationsMock.Verify(x => x.Add(It.Is<Location>(l =>
            l.Code == "LOC-001" && l.Level == null && l.Breadcrumb == null)), Times.Once);
    }

    [Test]
    public async Task Handle_ChildLocation_ShouldComputeLevelAndBreadcrumb()
    {
        var parent = new Location { Id = 7, Code = "PARENT", Name = "أب", Level = 1, Breadcrumb = "ROOT/PARENT" };
        _locationsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
                ValueTask.FromResult<Location?>(_locations.Append(parent).FirstOrDefault(l => l.Id == (int)keys[0])));

        var result = await _handler.Handle(ValidCommand() with { ParentLocationId = 7 }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        _locationsMock.Verify(x => x.Add(It.Is<Location>(l =>
            l.Level == 2 && l.Breadcrumb == "ROOT/PARENT/PARENT")), Times.Once);
    }

    [Test]
    public async Task Handle_MissingParent_ShouldReturnParentNotFound()
    {
        _locationsMock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns(ValueTask.FromResult<Location?>(null));

        var result = await _handler.Handle(ValidCommand() with { ParentLocationId = 999 }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Inventory.LocationParentNotFound");
    }

    [Test]
    public async Task Validate_DuplicateCode_ShouldFail()
    {
        _locations.Add(new Location { Id = 1, Code = "LOC-001", Name = "موجود" });

        var result = await _validator.ValidateAsync(ValidCommand());

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateLocationCommand.Code));
    }

    [Test]
    public async Task Validate_MissingParent_ShouldFail()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { ParentLocationId = 999 });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateLocationCommand.ParentLocationId));
    }

    [Test]
    public async Task Validate_NegativeCapacity_ShouldFail()
    {
        var result = await _validator.ValidateAsync(ValidCommand() with { Capacity = -1 });

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateLocationCommand.Capacity));
    }
}
