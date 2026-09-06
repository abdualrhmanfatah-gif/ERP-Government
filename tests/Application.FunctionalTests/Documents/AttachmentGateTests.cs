using ERP_Government.Application.Security.Common;
using ERP_Government.Domain.Security.Entities;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace ERP_Government.Application.FunctionalTests.Documents;

[TestFixture]
public class AttachmentGateTests : TestBase
{
    [Test]
    public async Task T063_MissingMandatoryAttachment_Blocked()
    {
        await TestApp.RunAsAdministratorAsync();
        await TestApp.AddAsync(new DocumentAttachmentRequirement
        {
            DocumentType = "Budget",
            AttachmentTypeCode = "CONTRACT",
            TitleAr = "عقد",
            IsMandatory = true,
            IsActive = true
        });

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var gate = scope.ServiceProvider.GetRequiredService<IAttachmentGateService>();
        var missing = await gate.CheckMandatoryAttachmentsAsync("Budget", 21, CancellationToken.None);
        missing.ShouldContain("CONTRACT");
    }

    [Test]
    public async Task T064_MandatoryAttachmentPresent_Passes()
    {
        var adminId = await TestApp.RunAsAdministratorAsync();
        adminId.ShouldNotBeNull();

        await TestApp.AddAsync(new DocumentAttachmentRequirement
        {
            DocumentType = "Appropriation",
            AttachmentTypeCode = "CONTRACT",
            TitleAr = "عقد",
            IsMandatory = true,
            IsActive = true
        });
        await TestApp.AddAsync(new Attachment
        {
            EntityName = "Appropriation",
            DocumentType = "Appropriation",
            DocumentId = 22,
            AttachmentTypeCode = "CONTRACT",
            FileName = "contract.pdf",
            StoragePath = "2026/01/contract.pdf",
            MimeType = "application/pdf",
            SizeBytes = 10,
            UploadedById = adminId!.Value,
            CreatedAt = DateTimeOffset.UtcNow
        });

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var gate = scope.ServiceProvider.GetRequiredService<IAttachmentGateService>();
        var missing = await gate.CheckMandatoryAttachmentsAsync("Appropriation", 22, CancellationToken.None);
        missing.ShouldBeEmpty();
    }

    [Test]
    public async Task T065_NoRequirements_NoCheck()
    {
        await TestApp.RunAsAdministratorAsync();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var gate = scope.ServiceProvider.GetRequiredService<IAttachmentGateService>();
        var missing = await gate.CheckMandatoryAttachmentsAsync("NoSuchDocType", 23, CancellationToken.None);
        missing.ShouldBeEmpty();
    }
}
