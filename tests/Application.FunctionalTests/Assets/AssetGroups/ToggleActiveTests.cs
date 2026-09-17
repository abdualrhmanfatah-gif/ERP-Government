using ERP_Government.Domain.Assets.Entities;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Assets.AssetGroups;

[TestFixture]
public class ToggleActiveTests : TestBase
{
    [Test]
    public async Task Deactivate_ActiveGroup_ShouldSetIsActiveFalse()
    {
        var group = new AssetGroup
        {
            Code = "AG-T01",
            Name = "קבוצה לבדיקה",
            IsActive = true,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var command = new Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive.ToggleAssetGroupActiveCommand(
            group.Id, false, group.RowVersion);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeTrue();
        var updated = await TestApp.FindAsync<AssetGroup>(group.Id);
        updated!.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task Activate_InactiveGroup_ShouldSetIsActiveTrue()
    {
        var group = new AssetGroup
        {
            Code = "AG-T02",
            Name = "קבוצה לא פעילה",
            IsActive = false,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var command = new Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive.ToggleAssetGroupActiveCommand(
            group.Id, true, group.RowVersion);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeTrue();
        var updated = await TestApp.FindAsync<AssetGroup>(group.Id);
        updated!.IsActive.ShouldBeTrue();
    }

    [Test]
    public async Task Deactivate_GroupWithAssets_ShouldFail()
    {
        var group = new AssetGroup
        {
            Code = "AG-T03",
            Name = "קבוצה עם נכסים",
            IsActive = true,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(group);

        var asset = new ERP_Government.Domain.Assets.Entities.Asset
        {
            Name = "נכס לבדיקה",
            AssetGroupId = group.Id,
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(asset);

        var command = new Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive.ToggleAssetGroupActiveCommand(
            group.Id, false, group.RowVersion);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.DeactivationBlocked");
    }

    [Test]
    public async Task Deactivate_NonExistingGroup_ShouldFail()
    {
        var command = new Application.Assets.AssetGroups.Commands.ToggleAssetGroupActive.ToggleAssetGroupActiveCommand(
            999, false, [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetGroupNotFound");
    }
}
