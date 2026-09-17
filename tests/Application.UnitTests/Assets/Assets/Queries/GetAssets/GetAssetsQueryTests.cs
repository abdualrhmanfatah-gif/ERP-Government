using ERP_Government.Application.Assets.Assets.Queries.GetAssets;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Assets.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Assets.Queries.GetAssets;

[TestFixture]
public class GetAssetsQueryTests
{
    private GetAssetsQueryHandler _handler = null!;
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAssetsQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_DefaultFilter_ReturnsActiveAssetsOnly()
    {
        var assets = new List<Asset>
        {
            new() { Id = 1, Code = "AST-000001", Name = "أصل 1", Status = "Active", AssetGroupId = 1, OriginalValue = 1000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
            new() { Id = 2, Code = "AST-000002", Name = "أصل 2", Status = "Draft", AssetGroupId = 1, OriginalValue = 2000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
            new() { Id = 3, Code = "AST-000003", Name = "أصل 3", Status = "Disposed", AssetGroupId = 1, OriginalValue = 3000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
        }.AsQueryable();

        _contextMock.Setup(c => c.Assets).Returns(assets.BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetsQuery(), CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.Items[0].Status.ShouldBe("Active");
    }

    [Test]
    public async Task Handle_SearchAcrossFields_FindsMatch()
    {
        var assets = new List<Asset>
        {
            new() { Id = 1, Code = "AST-000001", Name = "طابعة HP", Status = "Active", AssetGroupId = 1, OriginalValue = 1000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
            new() { Id = 2, Code = "AST-000002", Name = "Monitor Dell", Status = "Active", AssetGroupId = 1, OriginalValue = 2000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
        }.AsQueryable();

        _contextMock.Setup(c => c.Assets).Returns(assets.BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetsQuery(Search: "HP"), CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("طابعة HP");
    }

    [Test]
    public async Task Handle_FilterByAssetGroupId_ReturnsMatchingAssets()
    {
        var assets = new List<Asset>
        {
            new() { Id = 1, Code = "AST-000001", Name = "أصل 1", Status = "Active", AssetGroupId = 1, OriginalValue = 1000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
            new() { Id = 2, Code = "AST-000002", Name = "أصل 2", Status = "Active", AssetGroupId = 2, OriginalValue = 2000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
        }.AsQueryable();

        _contextMock.Setup(c => c.Assets).Returns(assets.BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetsQuery(AssetGroupId: 1), CancellationToken.None);

        result.Items.Count.ShouldBe(1);
        result.Items[0].AssetGroupId.ShouldBe(1);
    }

    [Test]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        var assets = Enumerable.Range(1, 25)
            .Select(i => new Asset
            {
                Id = i, Code = $"AST-{i:D6}", Name = $"أصل {i}", Status = "Active",
                AssetGroupId = 1, OriginalValue = i * 1000,
                PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = []
            }).AsQueryable();

        _contextMock.Setup(c => c.Assets).Returns(assets.BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetsQuery(Page: 2, PageSize: 10), CancellationToken.None);

        result.Items.Count.ShouldBe(10);
        result.TotalCount.ShouldBe(25);
        result.Page.ShouldBe(2);
    }

    [Test]
    public async Task Handle_EmptySearch_ReturnsAllMatchingAssets()
    {
        var assets = new List<Asset>
        {
            new() { Id = 1, Code = "AST-000001", Name = "أصل 1", Status = "Active", AssetGroupId = 1, OriginalValue = 1000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
            new() { Id = 2, Code = "AST-000002", Name = "أصل 2", Status = "Active", AssetGroupId = 1, OriginalValue = 2000, PurchaseDate = new DateOnly(2025, 1, 1), RowVersion = [] },
        }.AsQueryable();

        _contextMock.Setup(c => c.Assets).Returns(assets.BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetsQuery(Search: ""), CancellationToken.None);

        result.Items.Count.ShouldBe(2);
    }
}
