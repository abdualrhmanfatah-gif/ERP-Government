using ERP_Government.Application.Assets.AssetGroups.Common;
using ERP_Government.Application.Assets.AssetGroups.Queries.GetAssetGroups;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class GetAssetGroupsQueryTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private GetAssetGroupsQueryHandler _handler = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAssetGroupsQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnAllGroups()
    {
        var groups = new List<AssetGroup>
        {
            new() { Id = 1, Code = "AG-001", Name = "مباني", IsActive = true, ParentAssetGroupId = null },
            new() { Id = 2, Code = "AG-002", Name = "أثاث", IsActive = true, ParentAssetGroupId = null }
        }.AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupsQuery(null, null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(2);
    }

    [Test]
    public async Task Handle_WithSearch_ShouldFilterByName()
    {
        var groups = new List<AssetGroup>
        {
            new() { Id = 1, Code = "AG-001", Name = "مباني", IsActive = true, ParentAssetGroupId = null },
            new() { Id = 2, Code = "AG-002", Name = "أثاث", IsActive = true, ParentAssetGroupId = null }
        }.AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupsQuery("مباني", null, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("مباني");
    }

    [Test]
    public async Task Handle_WithIsActiveFilter_ShouldFilterByStatus()
    {
        var groups = new List<AssetGroup>
        {
            new() { Id = 1, Code = "AG-001", Name = "مباني", IsActive = true, ParentAssetGroupId = null },
            new() { Id = 2, Code = "AG-002", Name = "أثاث", IsActive = false, ParentAssetGroupId = null }
        }.AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupsQuery(null, false, null);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_WithParentId_ShouldFilterByParent()
    {
        var groups = new List<AssetGroup>
        {
            new() { Id = 1, Code = "AG-001", Name = "مباني", IsActive = true, ParentAssetGroupId = null },
            new() { Id = 2, Code = "AG-002", Name = "أثاث", IsActive = true, ParentAssetGroupId = 1 }
        }.AsQueryable();
        _contextMock.Setup(x => x.AssetGroups)
            .Returns(groups.BuildMockForAsync().Object);

        var query = new GetAssetGroupsQuery(null, null, 1);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].ParentAssetGroupId.ShouldBe(1);
    }
}
