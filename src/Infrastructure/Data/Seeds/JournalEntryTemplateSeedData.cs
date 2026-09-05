using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;

namespace ERP_Government.Infrastructure.Data.Seeds;

/// <summary>
/// JournalEntryTemplate seed data — 5 templates.
/// </summary>
public static class JournalEntryTemplateSeedData
{
    public static List<JournalEntryTemplate> GetTemplates() =>
    [
        new() { TemplateName="قيد شراء بضاعة", JournalId=2, TemplateType=JournalEntryTemplateType.Standard, IsSystemTemplate=true },
        new() { TemplateName="قيد دفع نقدي", JournalId=4, TemplateType=JournalEntryTemplateType.Standard, IsSystemTemplate=true },
        new() { TemplateName="قيد إيراد", JournalId=3, TemplateType=JournalEntryTemplateType.Standard, IsSystemTemplate=true },
        new() { TemplateName="قيد هلاك أصول", JournalId=6, TemplateType=JournalEntryTemplateType.Recurring, IsSystemTemplate=true },
        new() { TemplateName="قيد إقفال أرباح", JournalId=7, TemplateType=JournalEntryTemplateType.Standard, IsSystemTemplate=true },
    ];
}
