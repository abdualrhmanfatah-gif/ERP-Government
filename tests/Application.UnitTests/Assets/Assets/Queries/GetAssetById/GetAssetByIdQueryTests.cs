using ERP_Government.Application.Assets.Assets.Queries.GetAssetById;
using ERP_Government.Application.Common.Errors;
using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.UnitTests.Accounting;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Security.Entities;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.Assets.Queries.GetAssetById;

[TestFixture]
public class GetAssetByIdQueryTests
{
    private GetAssetByIdQueryHandler _handler = null!;
    private Mock<IApplicationDbContext> _contextMock = null!;

    [SetUp]
    public void SetUp()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _handler = new GetAssetByIdQueryHandler(_contextMock.Object);
    }

    [Test]
    public async Task Handle_AssetExists_ReturnsDetailResponse()
    {
        var asset = new Asset
        {
            Id = 1, Code = "AST-000001", Name = "طابعة HP", Description = "طابعة ليزر",
            Status = "Active", AssetGroupId = 1, OriginalValue = 1500m, CurrentValue = 1200m,
            PurchaseDate = new DateOnly(2025, 1, 15), DepreciationStartDate = new DateOnly(2025, 2, 1),
            AcquisitionType = "Purchase", IsActive = true, RowVersion = [1, 2, 3, 4, 5, 6, 7, 8],
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow,
            AssetGroup = new AssetGroup { Id = 1, Code = "GRP-001", Name = "أجهزة مكتبية" }
        };

        _contextMock.Setup(c => c.Assets).Returns(new List<Asset> { asset }.AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.AssetAttributeValues).Returns(new List<AssetAttributeValue>().AsQueryable().BuildMockForAsync().Object);
        _contextMock.Setup(c => c.Users).Returns(new List<User>().AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetByIdQuery(1), CancellationToken.None);

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
    }

    [Test]
    public async Task Handle_AssetNotFound_ReturnsNotFoundFailure()
    {
        _contextMock.Setup(c => c.Assets).Returns(new List<Asset>().AsQueryable().BuildMockForAsync().Object);

        var result = await _handler.Handle(new GetAssetByIdQuery(999), CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Code.ShouldBe(ErrorCodes.Assets.AssetNotFound);
    }
}
