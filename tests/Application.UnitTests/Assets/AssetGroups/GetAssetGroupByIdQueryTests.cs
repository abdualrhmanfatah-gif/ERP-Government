using ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroupById;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class GetAssetGroupByIdQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private GetAssetGroupByIdQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAssetGroupByIdQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ExistingId_ShouldReturnGroup()
    {
        var group = new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            ParentAssetGroupId = null
        };
        var groups = new List<AssetGroup> { group }.AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupByIdQuery(1);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Code.ShouldBe("AG-001");
        result.Value.Name.ShouldBe("مباني");
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnFailure()
    {
        var groups = new List<AssetGroup>().AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupByIdQuery(999);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetGroupNotFound");
    }
}
