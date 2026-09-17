using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransfers;

internal static class TransferTestData
{
    public static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static Mock<IDocumentSequenceService> Sequence(string number = "TRF-000001")
    {
        var sequence = new Mock<IDocumentSequenceService>();
        sequence.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(number);
        return sequence;
    }

    public static void SeedBasics(ApplicationDbContext context, string assetStatus = "Active")
    {
        context.OrganizationalUnits.AddRange(
            new OrganizationalUnit { Id = 5, Name = "إدارة تقنية المعلومات", Code = "IT" },
            new OrganizationalUnit { Id = 6, Name = "إدارة المشتريات", Code = "PR" },
            new OrganizationalUnit { Id = 7, Name = "إدارة المالية", Code = "FN" });

        context.Employees.AddRange(
            new Employee { Id = 1, EmployeeNumber = "E-1", Name = "أحمد", OrganizationalUnitId = 5, HireDate = new DateOnly(2020, 1, 1), JobTitle = "موظف" },
            new Employee { Id = 2, EmployeeNumber = "E-2", Name = "سارة", OrganizationalUnitId = 6, HireDate = new DateOnly(2020, 1, 1), JobTitle = "موظفة" });

        context.Locations.AddRange(
            new Location { Id = 3, Code = "LOC-1", Name = "المبنى أ", IsActive = true },
            new Location { Id = 4, Code = "LOC-2", Name = "المبنى ب", IsActive = true },
            new Location { Id = 8, Code = "LOC-3", Name = "مخزن مغلق", IsActive = false });

        context.Assets.Add(new Asset
        {
            Id = 1,
            Code = "AST-1",
            Name = "حاسوب محمول",
            AssetGroupId = 1,
            LocationId = 3,
            EmployeeId = 1,
            CurrencyId = 1,
            Status = assetStatus
        });

        context.SaveChanges();
    }
}
