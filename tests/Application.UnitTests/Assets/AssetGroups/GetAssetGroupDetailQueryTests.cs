using ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroupDetail;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class GetAssetGroupDetailQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private GetAssetGroupDetailQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAssetGroupDetailQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ExistingId_ShouldReturnDetail()
    {
        var group = new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            ParentAssetGroupId = null,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            DepreciationRate = 10m,
            DefaultUsefulLifeYears = 10,
            ResidualValuePercentage = 5m,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow,
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        };
        var groups = new List<AssetGroup> { group }.AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);
        _contextMock.Setup(x => x.AssetGroupAttributes)
            .Returns(new List<AssetGroupAttribute>().AsQueryable().BuildMockForAsync().Object);

        var query = new GetAssetGroupDetailQuery(1);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Code.ShouldBe("AG-001");
        result.Value.DepreciationMethod.ShouldBe("StraightLine");
        result.Value.RowVersion.ShouldNotBeNull();
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnFailure()
    {
        var groups = new List<AssetGroup>().AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupDetailQuery(999);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetGroupNotFound");
    }
}
