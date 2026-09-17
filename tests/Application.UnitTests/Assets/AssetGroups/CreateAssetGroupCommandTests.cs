using ERP_Government.Application.Assets.AssetGroups.Commands.CreateAssetGroup;
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
public class CreateAssetGroupCommandTests
{
    private Mock<IApplicationDbContext> _contextMock = null!;
    private CreateAssetGroupCommandHandler _handler = null!;
    private CreateAssetGroupCommandValidator _validator = null!;
    private int _nextId = 1;

    [SetUp]
    public void Setup()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new CreateAssetGroupCommandHandler(_contextMock.Object);
        _validator = new CreateAssetGroupCommandValidator();
        _nextId = 1;
    }

    private void SetupAssetGroups(params AssetGroup[] existingGroups)
    {
        _nextId = existingGroups.Length > 0 ? existingGroups.Max(g => g.Id) + 1 : 1;
        var mock = existingGroups.AsQueryable().BuildMockForAsync();
        mock.Setup(m => m.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
            .Returns((object[] keys, CancellationToken _) =>
            {
                var id = (int)keys[0];
                return ValueTask.FromResult<AssetGroup?>(existingGroups.FirstOrDefault(g => g.Id == id));
            });
        mock.Setup(m => m.Add(It.IsAny<AssetGroup>()))
            .Callback<AssetGroup>(g => g.Id = _nextId++);
        _contextMock.Setup(c => c.AssetGroups).Returns(mock.Object);
    }

    [Test]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnSuccess()
    {
        SetupAssetGroups();
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new CreateAssetGroupCommand(
            Code: "AG-004",
            Name: "أجهزة الحاسوب",
            Description: null,
            ParentAssetGroupId: null,
            AssetCategory: "ComputerEquipment",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: 20m,
            DefaultUsefulLifeYears: 5,
            ResidualValuePercentage: 10m,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value.ShouldBeGreaterThan(0);
        _contextMock.Verify(x => x.AssetGroups.Add(It.IsAny<AssetGroup>()), Times.Once);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_DuplicateCode_ShouldReturnFailure()
    {
        SetupAssetGroups(new AssetGroup { Id = 1, Code = "AG-004", Name = "موجودة", IsActive = true });

        var command = new CreateAssetGroupCommand(
            Code: "AG-004",
            Name: "جديدة",
            Description: null,
            ParentAssetGroupId: null,
            AssetCategory: "ComputerEquipment",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.DuplicateAssetGroupCode");
    }

    [Test]
    public async Task Handle_SelfParent_ShouldReturnFailure()
    {
        SetupAssetGroups();

        var command = new CreateAssetGroupCommand(
            Code: "AG-004",
            Name: "جديدة",
            Description: null,
            ParentAssetGroupId: 0,
            AssetCategory: "ComputerEquipment",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.SelfParent");
    }

    [Test]
    public async Task Handle_MaxDepthExceeded_ShouldReturnFailure()
    {
        SetupAssetGroups(
            new AssetGroup { Id = 1, Code = "AG-001", Name = "جذر", ParentAssetGroupId = null, IsActive = true },
            new AssetGroup { Id = 2, Code = "AG-002", Name = "مستوى 2", ParentAssetGroupId = 1, IsActive = true },
            new AssetGroup { Id = 3, Code = "AG-003", Name = "مستوى 3", ParentAssetGroupId = 2, IsActive = true });

        var command = new CreateAssetGroupCommand(
            Code: "AG-004",
            Name: "مستوى 4",
            Description: null,
            ParentAssetGroupId: 3,
            AssetCategory: "ComputerEquipment",
            IsDepreciable: true,
            DepreciationMethod: "StraightLine",
            DepreciationRate: null,
            DefaultUsefulLifeYears: null,
            ResidualValuePercentage: null,
            AssetAccountId: null,
            AccumulatedDepreciationAccountId: null,
            DepreciationExpenseAccountId: null,
            DisposalAccountId: null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe("Assets.MaxDepthExceeded");
    }
}
