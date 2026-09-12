using ERP_Government.Domain.Security.Entities;

namespace ERP_Government.Infrastructure.Data.Seeds;

public static class DisbursementRequestAttachmentSeedData
{
    public static List<DocumentAttachmentRequirement> GetRequirements() =>
    [
        new() { DocumentType = "DisbursementRequest", AttachmentTypeCode = "Contract", TitleAr = "عقد أو اتفاقية", IsMandatory = true, IsActive = true },
        new() { DocumentType = "DisbursementRequest", AttachmentTypeCode = "ApprovalLetter", TitleAr = "خطاب الاعتماد", IsMandatory = true, IsActive = true },
        new() { DocumentType = "DisbursementRequest", AttachmentTypeCode = "BankDetails", TitleAr = "بيانات الحساب البنكي", IsMandatory = false, IsActive = true },
        new() { DocumentType = "DisbursementRequest", AttachmentTypeCode = "Invoice", TitleAr = "فاتورة أو عرض سعر", IsMandatory = false, IsActive = true },
        new() { DocumentType = "DisbursementRequest", AttachmentTypeCode = "SupportingDoc", TitleAr = "مستندات داعمة أخرى", IsMandatory = false, IsActive = true },
    ];
}
