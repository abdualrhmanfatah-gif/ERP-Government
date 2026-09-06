using ERP_Government.Application.Documents.Commands.DeleteAttachment;
using ERP_Government.Application.Documents.Commands.UploadAttachment;
using ERP_Government.Application.Documents.Queries.GetDocumentApprovals;
using ERP_Government.Application.Documents.Queries.GetDocumentStatusLog;
using ERP_Government.Domain.Security.Entities;
using ERP_Government.Domain.Security.Enums;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Documents;

[TestFixture]
public class DocumentEndpointTests : TestBase
{
    [Test]
    public async Task T084_GetApprovals_ReturnsHistoryForEntity()
    {
        var adminId = await TestApp.RunAsAdministratorAsync();
        adminId.ShouldNotBeNull();

        await TestApp.AddAsync(new ApprovalHistory
        {
            DocumentType = "Budget",
            DocumentId = 31,
            ApprovalStep = 1,
            Action = ApprovalAction.Submit,
            ApproverUserId = adminId!.Value,
            RequiredRole = string.Empty,
            Decision = "Draft -> Submitted",
            DecisionAt = DateTimeOffset.UtcNow.AddMinutes(-2)
        });
        await TestApp.AddAsync(new ApprovalHistory
        {
            DocumentType = "Budget",
            DocumentId = 31,
            ApprovalStep = 2,
            Action = ApprovalAction.Approve,
            ApproverUserId = adminId!.Value,
            RequiredRole = string.Empty,
            Decision = "Approved",
            DecisionAt = DateTimeOffset.UtcNow
        });

        var rows = await TestApp.SendAsync(new GetDocumentApprovalsQuery("Budget", 31));

        rows.Count.ShouldBe(2);
        rows.ShouldAllBe(h => h.DocumentType == "Budget" && h.DocumentId == 31);
        rows[0].DecisionAt.ShouldBeGreaterThanOrEqualTo(rows[1].DecisionAt);
    }

    [Test]
    public async Task T085_GetStatusLog_ReturnsOrderedByChangedAt()
    {
        await TestApp.RunAsAdministratorAsync();
        var now = DateTimeOffset.UtcNow;

        await TestApp.AddAsync(new DocumentStatusLog
        {
            EntityName = "Budget",
            DocumentId = 32,
            FromStatus = "Draft",
            ToStatus = "Submitted",
            ChangedById = 1,
            ChangedAt = now.AddMinutes(-5)
        });
        await TestApp.AddAsync(new DocumentStatusLog
        {
            EntityName = "Budget",
            DocumentId = 32,
            FromStatus = "Submitted",
            ToStatus = "Approved",
            ChangedById = 1,
            ChangedAt = now
        });

        var rows = await TestApp.SendAsync(new GetDocumentStatusLogQuery("Budget", 32));

        rows.Count.ShouldBe(2);
        rows[0].ToStatus.ShouldBe("Submitted");
        rows[1].ToStatus.ShouldBe("Approved");
    }

    [Test]
    public async Task T086_UploadAttachment_CreatesRowAndStoresFile()
    {
        await TestApp.RunAsAdministratorAsync();
        var bytes = System.Text.Encoding.UTF8.GetBytes("test-content");
        using var stream = new MemoryStream(bytes);

        var result = await TestApp.SendAsync(new UploadAttachmentCommand(
            "Budget", 33, "GENERAL", "test.txt", stream));

        result.Succeeded.ShouldBeTrue();
        var row = await TestApp.FindAsync<Attachment>(result.Value);
        row.ShouldNotBeNull();
        row!.FileName.ShouldBe("test.txt");
        row.StoragePath.ShouldNotBeNullOrWhiteSpace();
        row.DocumentType.ShouldBe("Budget");
        row.DocumentId.ShouldBe(33);
    }

    [Test]
    public async Task T087_DeleteAttachment_RemovesRowAndFile()
    {
        await TestApp.RunAsAdministratorAsync();
        var bytes = System.Text.Encoding.UTF8.GetBytes("delete-me");
        using var stream = new MemoryStream(bytes);

        var upload = await TestApp.SendAsync(new UploadAttachmentCommand(
            "Budget", 34, "GENERAL", "del.txt", stream));
        upload.Succeeded.ShouldBeTrue();

        var delete = await TestApp.SendAsync(new DeleteAttachmentCommand(upload.Value));
        delete.Succeeded.ShouldBeTrue();

        var row = await TestApp.FindAsync<Attachment>(upload.Value);
        row.ShouldBeNull();
    }
}
