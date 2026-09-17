using ERP_Government.Application.Assets.AssetAttributes.Definitions.Queries;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetAttributes;

[TestFixture]
public class GetAssetAttributeDefinitionsQueryTests
{
    private ApplicationDbContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _context.AssetAttributeDefinitions.AddRange(
            new AssetAttributeDefinition { Id = 1, Code = "PLATE_NO", Name = "رقم اللوحة", AttributeDataType = AssetAttributeDataType.Text, SortOrder = 1, IsActive = true },
            new AssetAttributeDefinition { Id = 2, Code = "MODEL_YEAR", Name = "سنة الصنع", AttributeDataType = AssetAttributeDataType.Integer, SortOrder = 2, IsActive = true },
            new AssetAttributeDefinition { Id = 3, Code = "WARRANTY_END", Name = "نهاية الضمان", AttributeDataType = AssetAttributeDataType.Date, SortOrder = 3, IsActive = false },
            new AssetAttributeDefinition { Id = 4, Code = "SERIAL_NO", Name = "الرقم التسلسلي", AttributeDataType = AssetAttributeDataType.Text, SortOrder = 4, IsActive = true },
            new AssetAttributeDefinition { Id = 5, Code = "CONDITION", Name = "الحالة", AttributeDataType = AssetAttributeDataType.Text, SortOrder = 5, IsActive = true });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task Handle_ReturnsActiveDefinitionsByDefault()
    {
        var handler = new GetAssetAttributeDefinitionsQueryHandler(_context);
        var result = await handler.Handle(new GetAssetAttributeDefinitionsQuery(), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Items.Count.ShouldBe(4);
    }

    [Test]
    public async Task Handle_SearchByName_FiltersCorrectly()
    {
        var handler = new GetAssetAttributeDefinitionsQueryHandler(_context);
        var result = await handler.Handle(new GetAssetAttributeDefinitionsQuery(Search: "سنة"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Items.Count.ShouldBe(1);
        result.Value.Items[0].Code.ShouldBe("MODEL_YEAR");
    }

    [Test]
    public async Task Handle_SearchByCode_FiltersCorrectly()
    {
        var handler = new GetAssetAttributeDefinitionsQueryHandler(_context);
        var result = await handler.Handle(new GetAssetAttributeDefinitionsQuery(Search: "PLATE"), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Items.Count.ShouldBe(1);
        result.Value.Items[0].Code.ShouldBe("PLATE_NO");
    }

    [Test]
    public async Task Handle_FilterByDataType_ReturnsOnlyMatchingTypes()
    {
        var handler = new GetAssetAttributeDefinitionsQueryHandler(_context);
        var result = await handler.Handle(new GetAssetAttributeDefinitionsQuery(DataType: AssetAttributeDataType.Text), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Items.Count.ShouldBe(3);
    }

    [Test]
    public async Task Handle_FilterByIsActiveFalse_IncludesInactiveDefinitions()
    {
        var handler = new GetAssetAttributeDefinitionsQueryHandler(_context);
        var result = await handler.Handle(new GetAssetAttributeDefinitionsQuery(IsActive: false), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Items.Count.ShouldBe(1);
        result.Value.Items[0].Code.ShouldBe("WARRANTY_END");
    }

    [Test]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        var handler = new GetAssetAttributeDefinitionsQueryHandler(_context);
        var result = await handler.Handle(new GetAssetAttributeDefinitionsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Value!.Items.Count.ShouldBe(2);
        result.Value.TotalCount.ShouldBe(4);
    }
}
