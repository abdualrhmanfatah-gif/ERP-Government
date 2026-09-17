using ERP_Government.Application.Assets.AssetGroups.Commands.UpdateAssetGroup;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Application.UnitTests.Accounting;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetGroups;

[TestFixture]
public class UpdateAssetGroupCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private UpdateAssetGroupCommandHandler _handler = null!;
    private UpdateAssetGroupCommandValidator _validator = null!;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new UpdateAssetGroupCommandHandler(_contextMock.Object);
        _validator = new UpdateAssetGroupCommandValidator();
    }

    private void SetupAssetGroups(params AssetGroup[] groups)
    {
        var mock = groups.AsQueryable().BuildMockForAsync();
        mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
            {
                var id = (int)keys[0];
                return ValueTask.FromResult<AssetGroup?>(groups.FirstOrDefault(g => g.Id == id));
            });
        _contextMock.Setup(c => c.AssetGroups).Returns(mock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldUpdateAndReturnSuccess()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            ParentAssetGroupId = null,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateAssetGroupCommand(
            Id: 1,
            Name: "مباني محدثة",
            Description: null,
            ParentAssetGroupId: null,
            AssetCategory: "Building",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBe(1);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_InactiveGroup_ShouldReturnFailure()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = false,
            ParentAssetGroupId = null,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });

        var command = new UpdateAssetGroupCommand(
            Id: 1,
            Name: "مباني محدثة",
            Description: null,
            ParentAssetGroupId: null,
            AssetCategory: "Building",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.InactiveGroup");
    }

    [Test]
    public async Task Handle_NonExistingId_ShouldReturnFailure()
    {
        SetupAssetGroups();

        var command = new UpdateAssetGroupCommand(
            Id: 999,
            Name: "مباني محدثة",
            Description: null,
            ParentAssetGroupId: null,
            AssetCategory: "Building",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.AssetGroupNotFound");
    }

    [Test]
    public async Task Handle_ConcurrencyConflict_ShouldReturnFailure()
    {
        SetupAssetGroups(new AssetGroup
        {
            Id = 1,
            Code = "AG-001",
            Name = "مباني",
            IsActive = true,
            ParentAssetGroupId = null,
            AssetCategory = "Building",
            IsDepreciable = true,
            DepreciationMethod = "StraightLine",
            RowVersion = [1, 2, 3, 4, 5, 6, 7, 8]
        });
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var command = new UpdateAssetGroupCommand(
            Id: 1,
            Name: "مباني محدثة",
            Description: null,
            ParentAssetGroupId: null,
            AssetCategory: "Building",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null,
            RowVersion: [1, 2, 3, 4, 5, 6, 7, 8]);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Request.ConcurrencyConflict");
    }
}
