using ERP_Government.Application.Common.Interfaces;
using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.UnitTests.Security;

[TestFixture]
public class ApprovalRuleEvaluationServiceTests
{
    private ApplicationDbContext _dbContext = null!;
    private ApprovalRuleEvaluationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new ApplicationDbContext(options);
        _service = new ApprovalRuleEvaluationService(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public async Task EvaluateAsync_NoRules_ReturnsEmpty()
    {
        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task EvaluateAsync_ActiveRule_MatchesAmount_ReturnsRule()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", AmountThreshold = 500m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);

        result.Count.ShouldBe(1);
        result[0].RequiredRole.ShouldBe("PROC_MGR");
        result[0].Sequence.ShouldBe(1);
    }

    [Test]
    public async Task EvaluateAsync_ActiveRule_ExactlyThreshold_ReturnsRule()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", AmountThreshold = 1000m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task EvaluateAsync_ActiveRule_BelowThreshold_ReturnsEmpty()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", AmountThreshold = 2000m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task EvaluateAsync_NullThreshold_CatchAll_ReturnsRule()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", AmountThreshold = null,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1m, null, null, CancellationToken.None);
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task EvaluateAsync_InactiveRule_ReturnsEmpty()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", AmountThreshold = 500m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = false,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task EvaluateAsync_FundAgnosticRule_MatchesAnyFund()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", FundId = null, AmountThreshold = 500m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, 42, null, CancellationToken.None);
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task EvaluateAsync_FundSpecificRule_MatchesSpecificFund()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", FundId = 42, AmountThreshold = 500m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, 42, null, CancellationToken.None);
        result.Count.ShouldBe(1);
    }

    [Test]
    public async Task EvaluateAsync_FundSpecificRule_DifferentFund_ReturnsEmpty()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PurchaseOrder", FundId = 42, AmountThreshold = 500m,
            ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, 99, null, CancellationToken.None);
        result.ShouldBeEmpty();
    }

    [Test]
    public async Task EvaluateAsync_MultipleRules_ReturnsSortedBySequence()
    {
        _dbContext.ApprovalRules.AddRange(
            new ApprovalRule
            {
                Id = 2, DocumentType = "PurchaseOrder", AmountThreshold = 500m,
                ApproverRole = "FIN_MGR", Sequence = 2, IsActive = true,
                Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
            },
            new ApprovalRule
            {
                Id = 1, DocumentType = "PurchaseOrder", AmountThreshold = 100m,
                ApproverRole = "PROC_MGR", Sequence = 1, IsActive = true,
                Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
            });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);

        result.Count.ShouldBe(2);
        result[0].RequiredRole.ShouldBe("PROC_MGR");
        result[1].RequiredRole.ShouldBe("FIN_MGR");
    }

    [Test]
    public async Task EvaluateAsync_DifferentDocumentType_ReturnsEmpty()
    {
        _dbContext.ApprovalRules.Add(new ApprovalRule
        {
            Id = 1, DocumentType = "PaymentOrder", AmountThreshold = 500m,
            ApproverRole = "PAY_MGR", Sequence = 1, IsActive = true,
            Created = DateTimeOffset.UtcNow, LastModified = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();

        var result = await _service.EvaluateAsync("PurchaseOrder", 1000m, null, null, CancellationToken.None);
        result.ShouldBeEmpty();
    }
}
