using ERP_Government.Application.Assets.Assets.Queries.GetAssetById;
using ERP_Government.Domain.Assets.Entities;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Assets.Assets;

[TestFixture]
public class GetAssetByIdTests : TestBase
{
    [SetUp]
    public override async Task SetUp()
    {
        await base.SetUp();
        await TestApp.RunAsDefaultUserAsync();
    }

    [Test]
    public async Task Handle_AssetExists_ReturnsDetailResponse()
    {
        var group = new AssetGroup
        {
            Code = "AG-GB01", Name = "أجهزة مكتبية", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var asset = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000001", Name = "طابعة HP", Description = "طابعة ليزر",
            Status = "Active", AssetGroupId = group.Id, OriginalValue = 1500m,
            CurrentValue = 1200m, PurchaseDate = new DateOnly(2025, 1, 15),
            DepreciationStartDate = new DateOnly(2025, 2, 1),
            AcquisitionType = "Purchase", UsefulLifeYears = 5,
            IsActive = true, Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(asset);

        var query = new GetAssetByIdQuery(asset.Id);
        var result = await TestApp.SendAsync(query);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.Code.ShouldBe("AST-000001");
        result.Value.Name.ShouldBe("طابعة HP");
        result.Value.Description.ShouldBe("طابعة ليزر");
        result.Value.Status.ShouldBe("Active");
        result.Value.AssetGroupName.ShouldBe("أجهزة مكتبية");
        result.Value.OriginalValue.ShouldBe(1500m);
        result.Value.CurrentValue.ShouldBe(1200m);
        result.Value.AcquisitionType.ShouldBe("Purchase");
        result.Value.UsefulLifeYears.ShouldBe(5);
    }

    [Test]
    public async Task Handle_AssetNotFound_ReturnsNotFoundFailure()
    {
        var query = new GetAssetByIdQuery(999);
        var result = await TestApp.SendAsync(query);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetNotFound");
    }

    [Test]
    public async Task Handle_AssetWithNavigationProperties_ReturnsNames()
    {
        var group = new AssetGroup
        {
            Code = "AG-GB02", Name = "مجموعة فحص", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var asset = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Code = "AST-000001", Name = "أصل مع علاقات", Status = "Active",
            AssetGroupId = group.Id, OriginalValue = 5000m,
            PurchaseDate = new DateOnly(2025, 1, 1),
            DepreciationStartDate = new DateOnly(2025, 1, 1),
            AcquisitionType = "Purchase", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(asset);

        var query = new GetAssetByIdQuery(asset.Id);
        var result = await TestApp.SendAsync(query);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value!.AssetGroupName.ShouldBe("مجموعة فحص");
    }
}
