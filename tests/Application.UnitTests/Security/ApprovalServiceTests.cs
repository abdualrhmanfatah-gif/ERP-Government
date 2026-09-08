using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Security;

[TestFixture]
public class ApprovalServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private Mock<IIdentityService> _identityServiceMock = null!;
    private ApprovalService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _identityServiceMock = new Mock<IIdentityService>();
        _service = new ApprovalService(_dbContext, _identityServiceMock.Object);

        // Seed a test user
        _dbContext.Users.Add(new User
        {
            Id = 1, Login = "testuser", IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        _dbContext.SaveChangesAsync().Wait();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task ValidateAndRecordAsync_NoRules_ReturnsFailure()
    {
        var result = await _service.ValidateAndRecordAsync(
            "PurchaseOrder", 1, 1, "Approved", null, [], CancellationToken.None);

        result.Success.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("No approval rules"));
    }

    [Test]
    public async Task ValidateAndRecordAsync_HasRequiredRole_ReturnsSuccess()
    {
        var evaluationResults = new List<ApprovalRuleResult>
        {
            new() { Sequence = 1, RequiredRole = "PROC_MGR" }
        };
        _identityServiceMock.Setup(i => i.IsInRoleAsync(1, "PROC_MGR"))
            .ReturnsAsync(true);

        var result = await _service.ValidateAndRecordAsync(
            "PurchaseOrder", 1, 1, "Approved", null, evaluationResults, CancellationToken.None);

        result.Success.ShouldBeTrue();
        result.ApprovalHistoryId.ShouldNotBeNull();

        var history = await _dbContext.ApprovalHistory.FindAsync(result.ApprovalHistoryId);
        history.ShouldNotBeNull();
        history!.Decision.ShouldBe("Approved");
        history.RequiredRole.ShouldBe("PROC_MGR");
    }

    [Test]
    public async Task ValidateAndRecordAsync_NoRole_NoDelegation_ReturnsFailure()
    {
        var evaluationResults = new List<ApprovalRuleResult>
        {
            new() { Sequence = 1, RequiredRole = "PROC_MGR" }
        };
        _identityServiceMock.Setup(i => i.IsInRoleAsync(1, "PROC_MGR"))
            .ReturnsAsync(false);

        var result = await _service.ValidateAndRecordAsync(
            "PurchaseOrder", 1, 1, "Approved", null, evaluationResults, CancellationToken.None);

        result.Success.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.Contains("does not have any required approval role"));
    }

    [Test]
    public async Task ValidateAndRecordAsync_MultipleRules_ChecksEachRole()
    {
        var evaluationResults = new List<ApprovalRuleResult>
        {
            new() { Sequence = 1, RequiredRole = "PROC_MGR" },
            new() { Sequence = 2, RequiredRole = "FIN_MGR" }
        };
        _identityServiceMock.Setup(i => i.IsInRoleAsync(1, "PROC_MGR"))
            .ReturnsAsync(false);
        _identityServiceMock.Setup(i => i.IsInRoleAsync(1, "FIN_MGR"))
            .ReturnsAsync(true);

        var result = await _service.ValidateAndRecordAsync(
            "PurchaseOrder", 1, 1, "Approved", null, evaluationResults, CancellationToken.None);

        result.Success.ShouldBeTrue();
        var history = await _dbContext.ApprovalHistory.FindAsync(result.ApprovalHistoryId);
        history!.RequiredRole.ShouldBe("FIN_MGR");
    }

}
