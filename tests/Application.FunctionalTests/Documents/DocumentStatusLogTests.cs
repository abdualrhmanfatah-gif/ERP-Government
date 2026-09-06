using ERP_Government.Application.Documents.Queries.GetDocumentStatusLog;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Security.Entities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Documents;

[TestFixture]
public class DocumentStatusLogTests : TestBase
{
    [Test]
    public async Task T039_StatusLogIsAppendOnly_HistoryPreserved()
    {
        var adminId = await TestApp.RunAsAdministratorAsync();
        adminId.ShouldNotBeNull();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<IDocumentStatusLogger>();
        await logger.LogAsync("Budget", 11, "Draft", "Submitted", adminId!.Value, null);
        await logger.LogAsync("Budget", 11, "Submitted", "Approved", adminId!.Value, "ok");

        var rows = await TestApp.SendAsync(new GetDocumentStatusLogQuery("Budget", 11));

        rows.Count.ShouldBe(2);
        rows[0].ToStatus.ShouldBe("Submitted");
        rows[1].ToStatus.ShouldBe("Approved");
        rows[1].Reason.ShouldBe("ok");
    }

    [Test]
    public async Task T039_NoUpdateOrDeleteStatusLogCommandsExist()
    {
        var appAssembly = typeof(IDocumentStatusLogger).Assembly;

        var mutating = appAssembly.GetTypes()
            .Where(t => t.Name.Contains("DocumentStatusLog", StringComparison.Ordinal)
                && (t.Name.StartsWith("Update", StringComparison.Ordinal)
                    || t.Name.StartsWith("Delete", StringComparison.Ordinal)))
            .Select(t => t.FullName)
            .ToList();

        mutating.ShouldBeEmpty();
    }

    [Test]
    public async Task T040_LogAsync_CreatesRowWithCorrectFields()
    {
        var adminId = await TestApp.RunAsAdministratorAsync();
        adminId.ShouldNotBeNull();

        var before = DateTimeOffset.UtcNow;
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<IDocumentStatusLogger>();
        await logger.LogAsync("Appropriation", 7, "Draft", "PendingApproval", adminId!.Value, "submit");

        var rows = await TestApp.SendAsync(new GetDocumentStatusLogQuery("Appropriation", 7));

        rows.Count.ShouldBe(1);
        var row = rows[0];
        row.EntityName.ShouldBe("Appropriation");
        row.DocumentId.ShouldBe(7);
        row.FromStatus.ShouldBe("Draft");
        row.ToStatus.ShouldBe("PendingApproval");
        row.ChangedById.ShouldBe(adminId!.Value);
        row.Reason.ShouldBe("submit");
        row.ChangedAt.ShouldBeGreaterThanOrEqualTo(before);
    }
}
