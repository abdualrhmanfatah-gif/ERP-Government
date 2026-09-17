using ERP_Government.Domain.Assets.Entities;
using NUnit.Framework;

namespace ERP_Government.Application.FunctionalTests.Assets.AssetGroups;

[TestFixture]
public class ErrorHandlingTests : TestBase
{
    [Test]
    public async Task Create_DuplicateCode_ShouldReturnArabicError()
    {
        var existing = new AssetGroup
        {
            Code = "AG-E01",
            Name = "קיימת",
            IsActive = true,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            Created = DateTimeOffset.UtcNow,
            LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(existing);

        var command = new Application.Assets.AssetGroups.Commands.CreateAssetGroup.CreateAssetGroupCommand(
            "AG-E01", "חדשה", null, null, "Building", true, "StraightLine", null, null, null, null, null, null, null, null);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.DuplicateAssetGroupCode");
    }

    [Test]
    public async Task Create_EmptyCode_ShouldReturnValidationError()
    {
        var command = new Application.Assets.AssetGroups.Commands.CreateAssetGroup.CreateAssetGroupCommand(
            "", "חדשה", null, null, "Building", true, "StraightLine", null, null, null, null, null, null, null, null);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task Update_NonExisting_ShouldReturnArabicError()
    {
        var command = new Application.Assets.AssetGroups.Commands.UpdateAssetGroup.UpdateAssetGroupCommand(
            999, "اسم", null, null, "Building", true, "StraightLine", null, null, null, null, null, null, null, null,
            [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetGroupNotFound");
    }

    [Test]
    public async Task Deactivate_AlreadyInactive_ShouldReturnArabicError()
    {
        var group = new AssetGroup
        {
            Code = "AG-E02",
            Name = "לא פעילה",
            IsActive = false,
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

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AlreadyActive");
    }

    [Test]
    public async Task MaxDepthExceeded_ShouldReturnArabicError()
    {
        var root = new AssetGroup
        {
            Code = "AG-E03", Name = "جذر", IsActive = true, AssetCategory = "Building",
            IsDepreciable = true, DepreciationMethod = "StraightLine",
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(root);

        var level2 = new AssetGroup
        {
            Code = "AG-E04", Name = "مستوى 2", IsActive = true, AssetCategory = "Building",
            IsDepreciable = true, DepreciationMethod = "StraightLine", ParentAssetGroupId = root.Id,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(level2);

        var level3 = new AssetGroup
        {
            Code = "AG-E05", Name = "مستوى 3", IsActive = true, AssetCategory = "Building",
            IsDepreciable = true, DepreciationMethod = "StraightLine", ParentAssetGroupId = level2.Id,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        };
        await TestApp.AddAsync(level3);

        var command = new Application.Assets.AssetGroups.Commands.CreateAssetGroup.CreateAssetGroupCommand(
            "AG-E06", "مستوى 4", null, level3.Id, "Building", true, "StraightLine", null, null, null, null, null, null, null, null);

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.MaxDepthExceeded");
    }
}
