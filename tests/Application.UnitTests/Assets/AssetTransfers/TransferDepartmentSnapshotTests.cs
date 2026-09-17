using ERP_Government.Application.Assets.AssetTransactions.Transfers.Commands;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Domain.Assets.Entities;
using ERP_Government.Domain.Assets.Enums;
using ERP_Government.Domain.Inventory.Entities;
using ERP_Government.Domain.Organization.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Assets.AssetTransfers;

[TestFixture]
public class TransferDepartmentSnapshotTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IDocumentSequenceService> _sequence = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);

        _sequence = new Mock<IDocumentSequenceService>();
        _sequence.Setup(s => s.GenerateNextNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("TRF-000001");

        _context.Employees.AddRange(
            new Employee { Id = 1, EmployeeNumber = "E-1", Name = "أحمد", OrganizationalUnitId = 5, HireDate = new DateOnly(2020, 1, 1), JobTitle = "موظف" },
            new Employee { Id = 2, EmployeeNumber = "E-2", Name = "سارة", OrganizationalUnitId = 6, HireDate = new DateOnly(2020, 1, 1), JobTitle = "موظفة" });
        _context.Locations.AddRange(
            new Location { Id = 3, Code = "LOC-1", Name = "المبنى أ", IsActive = true },
            new Location { Id = 4, Code = "LOC-2", Name = "المبنى ب", IsActive = true });
        _context.Assets.Add(new Asset
        {
            Id = 1, Code = "AST-1", Name = "أصل", AssetGroupId = 1,
            LocationId = 3, EmployeeId = 1, CurrencyId = 1, Status = "Active"
        });
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    [Test]
    public async Task Create_SnapshotsDepartmentsFromCustodianEmployeeOrganizationalUnit()
    {
        var handler = new CreateAssetTransferCommandHandler(_context, _sequence.Object);

        var result = await handler.Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: 2,
            Notes: null), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var detail = _context.AssetTransferDetails.Single();
        detail.FromDepartmentId.ShouldBe(5);
        detail.ToDepartmentId.ShouldBe(6);
        detail.FromEmployeeId.ShouldBe(1);
        detail.ToEmployeeId.ShouldBe(2);
    }

    [Test]
    public async Task Create_UnchangedCustodian_CarriesCurrentDepartmentToDestination()
    {
        var handler = new CreateAssetTransferCommandHandler(_context, _sequence.Object);

        await handler.Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: null,
            Notes: null), CancellationToken.None);

        var detail = _context.AssetTransferDetails.Single();
        detail.FromDepartmentId.ShouldBe(5);
        detail.ToDepartmentId.ShouldBe(5);
        detail.ToEmployeeId.ShouldBe(1);
    }

    [Test]
    public async Task Execute_ReSnapshotsDepartmentsAtExecutionTime_AndUpdatesCard()
    {
        var createHandler = new CreateAssetTransferCommandHandler(_context, _sequence.Object);
        var transactionId = (await createHandler.Handle(new CreateAssetTransferCommand(
            AssetId: 1, TransactionDate: new DateOnly(2026, 9, 1),
            ToLocationId: 4, ToEmployeeId: 2,
            Notes: null), CancellationToken.None)).Value;

        var transaction = _context.AssetTransactions.Single(t => t.Id == transactionId);
        var asset = _context.Assets.Single();
        transaction.RowVersion = [1, 2, 3];
        asset.RowVersion = [4, 5, 6];

        // custodian department changes after the draft was created
        var employee = _context.Employees.Single(e => e.Id == 1);
        employee.OrganizationalUnitId = 7;
        await _context.SaveChangesAsync();

        var executeHandler = new ExecuteAssetTransferCommandHandler(_context);
        var result = await executeHandler.Handle(
            new ExecuteAssetTransferCommand(transactionId, [1, 2, 3], [4, 5, 6]), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();

        var detail = _context.AssetTransferDetails.Single();
        detail.FromDepartmentId.ShouldBe(7);
        detail.ToDepartmentId.ShouldBe(6);

        asset = _context.Assets.Single();
        asset.LocationId.ShouldBe(4);
        asset.EmployeeId.ShouldBe(2);
    }
}
