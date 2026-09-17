using ERP_Government.Application.Assets.Assets.Queries.GetAssets;
using ERP_Government.Domain.Assets.Entities;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Assets.Assets;

[TestFixture]
public class GetAssetsTests : TestBase
{
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        await TestApp.RunAsDefaultUserAsync();
    }

    [Test]
    public async Task Handle_DefaultFilter_ReturnsActiveAssetsOnly()
    {
        var group = new AssetGroup
        {
            Code = "AG-GA01", Name = "أجهزة مكتبية", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var activeAsset = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000001", Name = "أصل نشط", Status = "Active",
            AssetGroupId = group.Id, OriginalValue = 1000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        var draftAsset = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000002", Name = "أصل مسودة", Status = "Draft",
            AssetGroupId = group.Id, OriginalValue = 2000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(activeAsset);
        await TestApp.AddAsync(draftAsset);

        var query = new GetAssetsQuery();
        var result = await TestApp.SendAsync(query);

        result.Items.Count.ShouldBe(1);
        result.Items[0].Status.ShouldBe("Active");
    }

    [Test]
    public async Task Handle_SearchAcrossFields_FindsMatch()
    {
        var group = new AssetGroup
        {
            Code = "AG-GA02", Name = "أجهزة مكتبية", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var asset1 = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000001", Name = "طابعة HP", Status = "Active",
            AssetGroupId = group.Id, OriginalValue = 1500m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        var asset2 = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000002", Name = "Monitor Dell", Status = "Active",
            AssetGroupId = group.Id, OriginalValue = 2000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(asset1);
        await TestApp.AddAsync(asset2);

        var query = new GetAssetsQuery(Search: "HP");
        var result = await TestApp.SendAsync(query);

        result.Items.Count.ShouldBe(1);
        result.Items[0].Name.ShouldBe("طابعة HP");
    }

    [Test]
    public async Task Handle_FilterByAssetGroupId_ReturnsMatchingAssets()
    {
        var group1 = new AssetGroup
        {
            Code = "AG-GA03", Name = "مجموعة 1", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        var group2 = new AssetGroup
        {
            Code = "AG-GA04", Name = "مجموعة 2", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group1);
        await TestApp.AddAsync(group2);

        var asset1 = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000001", Name = "أصل 1", Status = "Active",
            AssetGroupId = group1.Id, OriginalValue = 1000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        var asset2 = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000002", Name = "أصل 2", Status = "Active",
            AssetGroupId = group2.Id, OriginalValue = 2000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(asset1);
        await TestApp.AddAsync(asset2);

        var query = new GetAssetsQuery(AssetGroupId: group1.Id);
        var result = await TestApp.SendAsync(query);

        result.Items.Count.ShouldBe(1);
        result.Items[0].AssetGroupId.ShouldBe(group1.Id);
    }

    [Test]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        var group = new AssetGroup
        {
            Code = "AG-GA05", Name = "مجموعة فحص", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        for (int i = 1; i <= 25; i++)
        {
            var asset = new ERP_Government.Domain.Assets.Entities.Asset
            {
                Code = $"AST-{i:D6}", Name = $"أصل {i}", Status = "Active",
                AssetGroupId = group.Id, OriginalValue = i * 1000m,
                PurchaseDate = new DateOnly(2025, 1, 1),
                DepreciationStartDate = new DateOnly(2025, 1, 1),
                AcquisitionType = "Purchase", IsActive = true,
                Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
            };
            await TestApp.AddAsync(asset);
        }

        var query = new GetAssetsQuery(Page: 2, PageSize: 10);
        var result = await TestApp.SendAsync(query);

        result.Items.Count.ShouldBe(10);
        result.TotalCount.ShouldBe(25);
        result.Page.ShouldBe(2);
    }

    [Test]
    public async Task Handle_EmptySearch_ReturnsAllMatchingAssets()
    {
        var group = new AssetGroup
        {
            Code = "AG-GA06", Name = "مجموعة فارغة", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var asset1 = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000001", Name = "أصل 1", Status = "Active",
            AssetGroupId = group.Id, OriginalValue = 1000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        var asset2 = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000002", Name = "أصل 2", Status = "Active",
            AssetGroupId = group.Id, OriginalValue = 2000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(asset1);
        await TestApp.AddAsync(asset2);

        var query = new GetAssetsQuery(Search: "");
        var result = await TestApp.SendAsync(query);

        result.Items.Count.ShouldBe(2);
    }
}
