# PLAN-047: خطوات التنفيذ التفصيلية

## نظرة عامة

هذا الملف يحتوي الخطوات التفصيلية لكل مرحلة تنفيذ.

---

## المرحلة 1: دليل الحسابات

### 1.1 AccountGroupSeedData.cs

**الملف**: `src/Infrastructure/Data/Seeds/AccountGroupSeedData.cs`

**الموقع**: نهاية `GetChildAccountGroups()` — بعد السطر 121

**إضافة 18 مجموعة**:

```csharp
// ═══════════════════════════════════════════════════════
// Level 2 — Under 2 (الموارد الرأسمالية) — الدائنون
// ═══════════════════════════════════════════════════════
new() { Code="25", Name="الدائنون",                       Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="2",  Description="الائتمان والمدفوعات المستحقة للغير" },
new() { Code="26", Name="السلف والتأمينات الدائنة",       Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="2",  Description="سلف مقدمة للغير والتأمينات والتوقيفات" },
new() { Code="27", Name="الحسابات الانتقالية الدائنة",    Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=2, ParentCode="2",  Description="إيرادات محصلة مقدماً ومصاريف مستحقة" },

// ═══════════════════════════════════════════════════════
// Level 3 — Under 25 — الدائنون
// ═══════════════════════════════════════════════════════
new() { Code="251", Name="الموردون",                      Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="الموردون — مشتريات النشاط الجاري" },
new() { Code="252", Name="أوراق الدفع",                   Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="سندات الالتزام والأوراق التجارية" },
new() { Code="253", Name="دائنون متنوعون",                Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="التزامات متنوعة تجاه الغير" },
new() { Code="254", Name="ذمم دائنة مختلفة",              Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="ضرائب و戍款 وتوقيفات" },
new() { Code="255", Name="دائنو توزيعات الأرباح",        Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="توزيعات أرباح للمُسهمين والدولة" },
new() { Code="256", Name="الشيكات والحوالات",              Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="شيكات م dates للتحصيل" },
new() { Code="257", Name="البنك المركزي",                  Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="حسابات لدى البنك المركزي" },
new() { Code="258", Name="الفروع والمراسلون",             Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="25", Description="حسابات الفروع والمراسلين" },

// ═══════════════════════════════════════════════════════
// Level 3 — Under 26 — السلف والتأمينات الدائنة
// ═══════════════════════════════════════════════════════
new() { Code="261", Name="السلف الدائنة",                  Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="26", Description="سلف مقدمة للزبائن والوكلاء والمقاولين" },
new() { Code="262", Name="التأمينات الدائنة",              Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="26", Description="تأمينات للغير والمقاولين والمناقصات" },
new() { Code="263", Name="التوقيفات",                      Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="26", Description="توقيفات للغير والمقاولين" },
new() { Code="264", Name="التأمينات لقاء سلف وقروض",      Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="26", Description="ضمانات سلف وقروض" },

// ═══════════════════════════════════════════════════════
// Level 3 — Under 27 — الحسابات الانتقالية الدائنة
// ═══════════════════════════════════════════════════════
new() { Code="271", Name="إيرادات محصلة مقدماً",           Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="27", Description="إيرادات فوائد وإيجارات وأوراق مالية" },
new() { Code="272", Name="مصاريف جارية وتخصيصية مستحقة",  Type=AccountGroupType.Equity, NormalBalance=NormalBalanceType.Credit, Level=3, ParentCode="27", Description="رواتب وصيانة وfwaid مستحقة الدفع" },
```

> **IDs المتوقعة**: 41-57 (تلقائياً via SQL IDENTITY)

### 1.2 AccountSeedData.cs

**الملف**: `src/Infrastructure/Data/Seeds/AccountSeedData.cs`

**الموقع**: داخل `GetEquityAccounts()` — بعد حسابات 2822 (السطر 91) وقبل `GetExpenseAccounts()`

**إضافة الحسابات**:

```csharp
// ═══════════════════════════════════════════════════════
// 251 — الموردون
// ═══════════════════════════════════════════════════════
new() { Code="251",  Name="الموردون",                                  AccountGroupId=42, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2511", Name="موردون محليون — قطاع عام",                  AccountGroupId=42, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2512", Name="موردون محليون — قطاع خاص",                  AccountGroupId=42, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2513", Name="موردون خارجيون",                            AccountGroupId=42, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 252 — أوراق الدفع
// ═══════════════════════════════════════════════════════
new() { Code="252",  Name="أوراق الدفع",                               AccountGroupId=43, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2521", Name="أوراق دفع تسهيل محلية",                     AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2522", Name="أوراق دفع عقود محلية",                      AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2523", Name="أوراق دفع عقود خارجية",                     AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2524", Name="أوراق دفع مستحقة",                          AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2525", Name="أوراق دفع مؤجلة",                           AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2526", Name="أوراق دفع موقوفة",                          AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2527", Name="أوراق دفع بالعملة الأجنبية",                AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2528", Name="أوراق دفع بالعملة الأجنبية المستحقة",       AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2529", Name="أوراق دفع أخرى ومختلفة",                    AccountGroupId=43, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 253 — دائنون متنوعون
// ═══════════════════════════════════════════════════════
new() { Code="253",  Name="دائنون متنوعون",                             AccountGroupId=44, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2531", Name="دائنون متنوعون محليون",                      AccountGroupId=44, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2532", Name="دائنون متنوعون خارجيون",                     AccountGroupId=44, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2533", Name="دائنون ذمم موقوفة",                          AccountGroupId=44, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 254 — ذمم دائنة مختلفة
// ═══════════════════════════════════════════════════════
new() { Code="254",  Name="ذمم دائنة مختلفة",                           AccountGroupId=45, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2541", Name="مصلحة الضرائب — ضريبة غير مباشرة",         AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2542", Name="مصلحة الضرائب — ضريبة كسب العمل",          AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2543", Name="مصلحة الضرائب — ضريبة الدمغة",             AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2544", Name="صندوق الغرامات والجزاءات",                   AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2545", Name="صندوق الضمان الاجتماعي",                     AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2546", Name="صندوق التقاعد",                              AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2547", Name="وزارة العدل — حجوزات رواتب",                 AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2548", Name="الجمعيات التعاونية — حجوزات رواتب",          AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2549", Name="ذمم دائنة أخرى",                             AccountGroupId=45, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 255 — دائنو توزيعات الأرباح
// ═══════════════════════════════════════════════════════
new() { Code="255",  Name="دائنو توزيعات الأرباح",                      AccountGroupId=46, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2551", Name="الدولة — وزارة المالية",                      AccountGroupId=46, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2552", Name="الدولة — مؤسسات عامة مشاركة",                AccountGroupId=46, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2553", Name="الشركات — القطاع المختلط المشارك",           AccountGroupId=46, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2554", Name="القطاع الخاص المحلي",                         AccountGroupId=46, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2555", Name="القطاع الخاص الأجنبي",                        AccountGroupId=46, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2556", Name="دائنو التوزيعات مختلفون",                     AccountGroupId=46, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 256, 257, 258 — ترويسات فقط
// ═══════════════════════════════════════════════════════
new() { Code="256",  Name="الشيكات والحوالات",                           AccountGroupId=47, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="257",  Name="البنك المركزي",                               AccountGroupId=48, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="258",  Name="الفروع والمراسلون",                          AccountGroupId=49, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },

// ═══════════════════════════════════════════════════════
// 261 — السلف الدائنة
// ═══════════════════════════════════════════════════════
new() { Code="261",  Name="السلف الدائنة",                               AccountGroupId=51, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2611", Name="سلف الزبائن",                                 AccountGroupId=51, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2612", Name="سلف الوكلاء",                                 AccountGroupId=51, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2613", Name="سلف على عقود",                                AccountGroupId=51, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 262 — التأمينات الدائنة
// ═══════════════════════════════════════════════════════
new() { Code="262",  Name="التأمينات الدائنة",                           AccountGroupId=52, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2621", Name="تأمينات للغير",                                AccountGroupId=52, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2622", Name="تأمينات المقاولين",                            AccountGroupId=52, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2623", Name="تأمينات المناقصات",                            AccountGroupId=52, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2624", Name="تأمينات الوكلاء",                              AccountGroupId=52, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 263 — التوقيفات
// ═══════════════════════════════════════════════════════
new() { Code="263",  Name="التوقيفات",                                   AccountGroupId=53, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2631", Name="توقيفات للغير",                               AccountGroupId=53, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2632", Name="توقيفات المقاولين",                           AccountGroupId=53, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 264 — التأمينات لقاء سلف وقروض
// ═══════════════════════════════════════════════════════
new() { Code="264",  Name="التأمينات لقاء سلف وقروض",                   AccountGroupId=54, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },

// ═══════════════════════════════════════════════════════
// 271 — إيرادات محصلة مقدماً
// ═══════════════════════════════════════════════════════
new() { Code="271",  Name="إيرادات محصلة مقدماً",                        AccountGroupId=56, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2711", Name="إيرادات الفوائد",                              AccountGroupId=56, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2712", Name="إيرادات الإيجارات",                            AccountGroupId=56, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2713", Name="إيرادات الأوراق المالية",                      AccountGroupId=56, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2714", Name="إيرادات التعويضات",                            AccountGroupId=56, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2715", Name="إيرادات الإعانات",                             AccountGroupId=56, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 272 — مصاريف جارية مستحقة
// ═══════════════════════════════════════════════════════
new() { Code="272",  Name="مصاريف جارية وتخصيصية مستحقة",               AccountGroupId=57, ParentId=null,  Level=3, NormalBalance=NormalBalanceType.Credit, IsPostable=false },
new() { Code="2721", Name="الرواتب والأجور المحلية",                      AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2722", Name="رواتب وأجور الخبراء الأجانب",                 AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2723", Name="المزايا الفنية",                               AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2724", Name="الصيانة والتصليحات",                           AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2725", Name="الدعاية والإعلان",                             AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2726", Name="الفوائد الدائنة",                              AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2727", Name="الإيجارات",                                    AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2728", Name="الإعانات",                                     AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },
new() { Code="2729", Name="أخرى ومختلفة",                                 AccountGroupId=57, ParentId=XX,    Level=4, NormalBalance=NormalBalanceType.Credit, IsPostable=true },

// ═══════════════════════════════════════════════════════
// 352216 — مساعدات مرضية (مصروف)
// ═══════════════════════════════════════════════════════
// يُضاف في GetExpenseAccounts() بعد 352215
new() { Code="352216", Name="مساعدات مرضية",                              AccountGroupId=35, ParentId=XX,    Level=6, NormalBalance=NormalBalanceType.Debit, IsPostable=true },
```

> **ملاحظة**: `ParentId=XX` — يجب حساب الموقع الدقيق في القائمة المسطحة. يُحسب كـ `position in flat list` (1-based).

---

## المرحلة 2: Entity + Configuration

### 2.1 DisbursementRequest.cs

**الملف**: `src/Domain/Payments/Entities/DisbursementRequest.cs`

**إضافة** (بعد السطر 18، قبل `RowVersion`):
```csharp
public int? AccrualJournalEntryId { get; set; }
```

### 2.2 DisbursementRequestConfiguration.cs

**الملف**: `src/Infrastructure/Data/Configurations/Payments/DisbursementRequestConfiguration.cs`

**إضافة** (في `Configure` method):
```csharp
builder.Property<int?>("AccrualJournalEntryId")
    .IsRequired(false);

builder.HasIndex("AccrualJournalEntryId")
    .IsUnique()
    .HasFilter("[AccrualJournalEntryId] IS NOT NULL");
```

---

## المرحلة 3: DTO + Queries

### 3.1 DisbursementRequestDto.cs

**الملف**: `src/Application/Payments/Common/DTOs/DisbursementRequestDto.cs`

**إضافة** في `DisbursementRequestDetailDto`:
```csharp
public int? AccrualJournalEntryId { get; init; }
public string? AccrualEntryNumber { get; init; }
```

### 3.2 GetDisbursementRequestByIdQuery.cs

**الملف**: `src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequestById/GetDisbursementRequestByIdQuery.cs`

**تعديل** — تحميل القيد المرتبط:
```csharp
// بعد تحميل entity
JournalEntry? accrualEntry = null;
if (entity.AccrualJournalEntryId.HasValue)
{
    accrualEntry = await context.JournalEntries
        .FindAsync(entity.AccrualJournalEntryId.Value, cancellationToken);
}

// في Projection
AccrualJournalEntryId = entity.AccrualJournalEntryId,
AccrualEntryNumber = accrualEntry?.EntryNumber,
```

### 3.3 GetDisbursementRequestsQuery.cs

**الملف**: `src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequests/GetDisbursementRequestsQuery.cs`

**تعديل** — إضافة الحقل للقائمة:
```csharp
// في Projection
AccrualJournalEntryId = entity.AccrualJournalEntryId,
```

---

## المرحلة 4: الأمر الجديد

### 4.1 CreateAccrualEntryCommand.cs

**ملف جديد**: `src/Application/Payments/Commands/DisbursementRequests/CreateAccrualEntry/CreateAccrualEntryCommand.cs`

```csharp
using ERP_Government.Application.Common.Security;
using ERP_Government.Application.FinancialSettings.Common.Services;
using ERP_Government.Application.Parties.Common;
using ERP_Government.Domain.Accounting.Entities;
using ERP_Government.Domain.Accounting.Enums;
using ERP_Government.Domain.Payments.Entities;
using ERP_Government.Domain.Payments.Enums;
using Microsoft.EntityFrameworkCore;

namespace ERP_Government.Application.Payments.Commands.DisbursementRequests.CreateAccrualEntry;

[Authorize(Policy = PermissionCodes.DisbursementRequestsCreateAccrual)]
public class CreateAccrualEntryCommand : IRequest<Result<int>>
{
    public int DisbursementRequestId { get; init; }
    public int ExpenseAccountId { get; init; }
    public int LiabilityAccountId { get; init; }
    public decimal Amount { get; init; }
    public int CurrencyId { get; init; }
    public int? CostCenterId { get; init; }
    public string? Narration { get; init; }
    public byte[] RowVersion { get; init; } = [];
}

public class CreateAccrualEntryCommandHandler(
    IApplicationDbContext context,
    IDocumentSequenceService sequenceService,
    IUser user) : IRequestHandler<CreateAccrualEntryCommand, Result<int>>
{
    public async Task<Result<int>> Handle(
        CreateAccrualEntryCommand request,
        CancellationToken cancellationToken)
    {
        if (user.Id is not int userId)
            return Result<int>.Failure(["User identity is required for this operation."]);

        var entity = await context.DisbursementRequests
            .FindAsync(request.DisbursementRequestId, cancellationToken);

        if (entity is null)
            return Result<int>.Failure(["Disbursement request not found."]);

        if (entity.Status != DisbursementRequestStatus.Approved)
            return Result<int>.Failure(["Only approved disbursement requests can have accrual entries."]);

        if (entity.AccrualJournalEntryId.HasValue)
            return Result<int>.Failure(["An accrual entry already exists for this disbursement request."]);

        // Validate accounts
        var expenseAccount = await context.Accounts
            .FindAsync(request.ExpenseAccountId, cancellationToken);
        if (expenseAccount is null || !expenseAccount.IsPostable || !expenseAccount.IsActive)
            return Result<int>.Failure(["Invalid expense account."]);

        var liabilityAccount = await context.Accounts
            .FindAsync(request.LiabilityAccountId, cancellationToken);
        if (liabilityAccount is null || !liabilityAccount.IsPostable || !liabilityAccount.IsActive)
            return Result<int>.Failure(["Invalid liability account."]);

        if (request.Amount <= 0)
            return Result<int>.Failure(["Amount must be greater than zero."]);

        if (request.Amount > entity.RequestedAmount)
            return Result<int>.Failure(["Amount cannot exceed the requested amount."]);

        // Get active fiscal period
        var today = DateOnly.FromDateTime(DateTime.Today);
        var period = await context.FiscalPeriods
            .Where(p => p.IsActive && p.StartDate <= today && p.EndDate >= today)
            .FirstOrDefaultAsync(cancellationToken);

        if (period is null)
            return Result<int>.Failure(["No active fiscal period found for today."]);

        // Generate entry number
        string entryNumber;
        try
        {
            entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);
        }
        catch (DocumentSequenceException ex)
        {
            return Result<int>.Failure([ex.Message]);
        }

        // Create journal entry
        var journalEntry = new JournalEntry
        {
            EntryNumber = entryNumber,
            DocumentDate = today,
            EntryStatus = EntryStatus.Draft,
            JournalId = null,
            PeriodId = period.Id,
            FiscalYearId = period.FiscalYearId,
            IsSystemGenerated = true,
            Narration = request.Narration ?? $"قيد استحقاق — طلب صرف #{entity.RequestNumber}",
            Created = DateTimeOffset.UtcNow,
            CreatedBy = userId.ToString(),
            LastModified = DateTimeOffset.UtcNow,
            LastModifiedBy = userId.ToString()
        };

        context.JournalEntries.Add(journalEntry);

        // Line 1: Debit expense account
        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 1,
            AccountId = request.ExpenseAccountId,
            Description = $"استحقاق — {entity.BeneficiaryName}",
            Debit = request.Amount,
            Credit = 0,
            CurrencyId = request.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = request.CostCenterId,
            RowVersion = []
        });

        // Line 2: Credit liability account
        journalEntry.Lines.Add(new JournalEntryLine
        {
            Sequence = 2,
            AccountId = request.LiabilityAccountId,
            Description = $"استحقاق — {entity.BeneficiaryName}",
            Debit = 0,
            Credit = request.Amount,
            CurrencyId = request.CurrencyId,
            ExchangeRate = 1,
            CostCenterId = request.CostCenterId,
            RowVersion = []
        });

        // Link to disbursement request
        entity.AccrualJournalEntryId = journalEntry.Id;
        entity.LastModified = DateTimeOffset.UtcNow;
        entity.LastModifiedBy = userId.ToString();

        await context.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(journalEntry.Id);
    }
}

public class CreateAccrualEntryCommandValidator : AbstractValidator<CreateAccrualEntryCommand>
{
    public CreateAccrualEntryCommandValidator()
    {
        RuleFor(x => x.DisbursementRequestId)
            .GreaterThan(0).WithMessage("Invalid disbursement request ID.");

        RuleFor(x => x.ExpenseAccountId)
            .GreaterThan(0).WithMessage("Expense account is required.");

        RuleFor(x => x.LiabilityAccountId)
            .GreaterThan(0).WithMessage("Liability account is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("Currency is required.");

        RuleFor(x => x.ExpenseAccountId)
            .NotEqual(x => x.LiabilityAccountId)
            .WithMessage("Expense and liability accounts must be different.");
    }
}
```

---

## المرحلة 5: Endpoint + Permissions

### 5.1 DisbursementRequests.cs (Endpoint)

**الملف**: `src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs`

**إضافة route** (في `MapEndpoints`):
```csharp
app.MapPost("/{id}/accrual-entry", HandleCreateAccrualEntry)
    .RequireAuthorization(PermissionCodes.DisbursementRequestsCreateAccrual);
```

**Handler**:
```csharp
static async Task<IResult> HandleCreateAccrualEntry(
    int id, CreateAccrualEntryRequest request, ISender mediator)
{
    var command = new CreateAccrualEntryCommand
    {
        DisbursementRequestId = id,
        ExpenseAccountId = request.ExpenseAccountId,
        LiabilityAccountId = request.LiabilityAccountId,
        Amount = request.Amount,
        CurrencyId = request.CurrencyId,
        CostCenterId = request.CostCenterId,
        Narration = request.Narration
    };
    var result = await mediator.Send(command);
    return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
}
```

**Request DTO**:
```csharp
public record CreateAccrualEntryRequest(
    int ExpenseAccountId,
    int LiabilityAccountId,
    decimal Amount,
    int CurrencyId,
    int? CostCenterId,
    string? Narration);
```

### 5.2 PermissionCodes.cs

**الملف**: `src/Application/Common/Security/PermissionCodes.cs`

**إضافة** (في قسم DisbursementRequests):
```csharp
public const string DisbursementRequestsCreateAccrual = "DisbursementRequests.CreateAccrual";
```

### 5.3 DependencyInjection.cs

**الملف**: `src/Web/DependencyInjection.cs`

**إضافة** (في تسجيل السياسات):
```csharp
options.AddPolicy(PermissionCodes.DisbursementRequestsCreateAccrual, policy =>
    policy.RequireAssertion(_ => true)); // placeholder — RBAC enforcement pending
```

### 5.4 RolePermissionSeedData.cs

**الملف**: `src/Infrastructure/Data/Seeds/RolePermissionSeedData.cs`

**إضافة** (للدور المسؤول):
```csharp
new() { SecurityRoleId = X, PermissionId = Y },  // DisbursementRequestsCreateAccrual
```

---

## المرحلة 6: تعديلات الأوامر

### 6.1 SubmitPaymentOrderCommand.cs

**الملف**: `src/Application/Payments/Commands/PaymentOrders/SubmitPaymentOrder/SubmitPaymentOrderCommand.cs`

**إضافة فحص** (بعد فحص الحالة، قبل فحص الموازنة — حوالي السطر 40):
```csharp
// فحص قيد الاستحقاق
if (entity.DisbursementRequestId.HasValue)
{
    var dr = await context.DisbursementRequests
        .FindAsync(entity.DisbursementRequestId.Value, cancellationToken);
    if (dr?.AccrualJournalEntryId is null)
        return Result.Failure(["يجب ترحيل قيد الاستحقاق أولاً. سجّل قيد الاستحقاق من صفحة طلب الصرف."]);
}
```

### 6.2 RecordPaymentCommand.cs

**الملف**: `src/Application/Payments/Commands/Payments/RecordPayment/RecordPaymentCommand.cs`

**التغيير الرئيسي**: استبدال `PostingRuleMatcher` + `JournalEntryGenerator` بـ logika ديناميكية.

**الكود الحالي** (السطور 62-89):
```csharp
var rules = await postingRuleMatcher.MatchAsync(eventTypeStr, cancellationToken);
// ... uses PostingRule → hardcoded accounts
```

**الكود الجديد**:
```csharp
// استخراج حساب الخصم من قيد الاستحقاق
int liabilityAccountId;
JournalEntry? accrualEntry = null;

if (paymentOrder.DisbursementRequestId.HasValue)
{
    var dr = await context.DisbursementRequests
        .FindAsync(paymentOrder.DisbursementRequestId.Value, cancellationToken);
    if (dr?.AccrualJournalEntryId is not null)
    {
        accrualEntry = await context.JournalEntries
            .Include(e => e.Lines)
            .FirstOrDefaultAsync(e => e.Id == dr.AccrualJournalEntryId.Value, cancellationToken);
        liabilityAccountId = accrualEntry!.Lines.First(l => l.Credit > 0).AccountId;
    }
    else
    {
        // fallback: حساب افتراضي (مورد عام)
        liabilityAccountId = 2511;
    }
}
else
{
    // أمر صرف مستقل — يستخدم PostingRule العادي
    var rules = await postingRuleMatcher.MatchAsync(eventTypeStr, cancellationToken);
    if (rules.Count == 0)
        return Result<PaymentDto>.Failure(["لا توجد قواعد ترحيل محاسبي معرّفة لأوامر الدفع."]);

    // ... الكود الحالي بدون تغيير
}

// إنشاء قيد السداد: مدين الخصم / دائن البنك
var period = await context.FiscalPeriods
    .Where(p => p.IsActive && p.StartDate <= documentDate && p.EndDate >= documentDate)
    .FirstOrDefaultAsync(cancellationToken);

if (period is null)
    throw new InvalidOperationException("No active fiscal period found.");

var entryNumber = await sequenceService.GenerateNextNumberAsync("JournalEntry", cancellationToken);

var paymentJournalEntry = new JournalEntry
{
    EntryNumber = entryNumber,
    DocumentDate = documentDate,
    EntryStatus = EntryStatus.Draft,
    JournalId = 4,  // دفتر المدفوعات
    PeriodId = period.Id,
    FiscalYearId = period.FiscalYearId,
    IsSystemGenerated = true,
    Narration = $"سداد أمر صرف #{paymentOrder.PaymentOrderNumber}"
};

// سطر 1: مدين حساب الخصم (من الاستحقاق)
paymentJournalEntry.Lines.Add(new JournalEntryLine
{
    Sequence = 1,
    AccountId = liabilityAccountId,
    Description = $"سداد — {paymentOrder.BeneficiaryName}",
    Debit = netTotal,
    Credit = 0,
    CurrencyId = paymentOrder.CurrencyId,
    ExchangeRate = 1,
    PaymentOrderId = paymentOrder.Id,
    RowVersion = []
});

// سطر 2: دائن حساب البنك
paymentJournalEntry.Lines.Add(new JournalEntryLine
{
    Sequence = 2,
    AccountId = paymentOrder.BankAccountId!.Value,
    Description = $"سداد — {paymentOrder.BeneficiaryName}",
    Debit = 0,
    Credit = netTotal,
    CurrencyId = paymentOrder.CurrencyId,
    ExchangeRate = 1,
    PaymentOrderId = paymentOrder.Id,
    RowVersion = []
});

context.JournalEntries.Add(paymentJournalEntry);

// ربط القيد بأمر الصرف
paymentOrder.JournalEntryId = paymentJournalEntry.Id;
```

### 6.3 ReverseJournalEntryCommand.cs

**الملف**: `src/Application/Accounting/Commands/JournalEntries/ReverseJournalEntry/ReverseJournalEntryCommand.cs`

**إضافة** (بعد إنشاء القيد العكسي):
```csharp
// حذف ربط الاستحقاق إذا كان القيد المُalukan مقترناً بطلب صرف
if (sourceEntry.IsSystemGenerated)
{
    var dr = await context.DisbursementRequests
        .FirstOrDefaultAsync(d => d.AccrualJournalEntryId == sourceEntry.Id, cancellationToken);
    if (dr is not null)
    {
        dr.AccrualJournalEntryId = null;
        dr.LastModified = DateTimeOffset.UtcNow;
        dr.LastModifiedBy = userId.ToString();
    }
}
```

---

## المرحلة 7: Void/Cancel

### 7.1 VoidPaymentOrderCommand.cs

**الملف**: `src/Application/Payments/Commands/PaymentOrders/VoidPaymentOrder/VoidPaymentOrderCommand.cs`

**إضافة** (بعد تغيير الحالة):
```csharp
// عكس قيد السداد إذا كان مدفوعاً
if (paymentOrder.JournalEntryId.HasValue)
{
    var paymentEntry = await context.JournalEntries
        .FindAsync(paymentOrder.JournalEntryId.Value, cancellationToken);
    if (paymentEntry is not null && paymentEntry.EntryStatus == EntryStatus.Posted)
    {
        // إنشاء قيد عكسي
        // (نفس logika ReverseJournalEntryCommand)
    }
}

// حذف ربط الاستحقاق
if (paymentOrder.DisbursementRequestId.HasValue)
{
    var dr = await context.DisbursementRequests
        .FindAsync(paymentOrder.DisbursementRequestId.Value, cancellationToken);
    if (dr is not null)
    {
        dr.Status = DisbursementRequestStatus.Invalidated;
        dr.AccrualJournalEntryId = null;
        dr.LastModified = DateTimeOffset.UtcNow;
        dr.LastModifiedBy = userId.ToString();
    }
}
```

### 7.2 CancelPaymentOrderCommand.cs

**الملف**: `src/Application/Payments/Commands/PaymentOrders/CancelPaymentOrder/CancelPaymentOrderCommand.cs`

**نفس التعديلات** كما في Void.

---

## المرحلة 8: التقارير

### 8.1 DisbursementRegisterDto.cs

**الملف**: `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/DisbursementRegisterDto.cs`

**إضافة**:
```csharp
public int? AccrualJournalEntryId { get; init; }
public string? AccrualEntryNumber { get; init; }
public string? AccrualEntryStatus { get; init; }
```

### 8.2 GetDisbursementRegisterQueryHandler.cs

**الملف**: `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/GetDisbursementRegisterQueryHandler.cs`

**تعديل** — تحميل القيد المرتبط:
```csharp
// في joins
join accrualEntry in context.JournalEntries on dr.AccrualJournalEntryId equals accrualEntry.Id into accrualGroup
from accrualEntry in accrualGroup.DefaultIfEmpty()

// في Projection
AccrualJournalEntryId = dr.AccrualJournalEntryId,
AccrualEntryNumber = accrualEntry.EntryNumber,
AccrualEntryStatus = accrualEntry.EntryStatus.ToString(),
```

### 8.3 DisbursementRegisterDetailDto.cs

**الملف**: `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/DisbursementRegisterDetailDto.cs`

**نفس التعديلات** كما في DTO القائمة.

---

## المرحلة 9: Seed Data

### 9.1 PostingRuleLineSeedData.cs

**الملف**: `src/Infrastructure/Data/Seeds/PostingRuleLineSeedData.cs`

**ملاحظة**: لا نغير `PaymentOrderExecuted` لأننا نريد قيد سداد ديناميكي. لكن يجب التأكد من أن القيد القديم لا يتعارض.

**قرار**: نترك `PaymentOrderExecuted` كما هو. إذا كان هناك أمر صرف مستقل (بدون طلب صرف)، يستخدم PostingRule القديم.

---

## المرحلة 10: Migration + التوثيق

### 10.1 EF Migration

**الأمر**:
```bash
dotnet ef migrations add AddAccrualJournalEntryToDisbursementRequest \
  --project src/Infrastructure \
  --startup-project src/Web
```

### 10.2 database-schema.md

**الملف**: `docs/database-schema.md`

**إضافة** في جدول DisbursementRequests:
```
| AccrualJournalEntryId | int | nullable | FK → JournalEntries.Id |
```

### 10.3 CONTEXT.md

**الملف**: `CONTEXT.md`

**تحديث** تعريف Beneficiary:
```
- **AccrualJournalEntryId**: ربط قيد الاستحقاق بطلب الصرف. يُنشأ عندما يُسجّل المستخدم قيد الاستحقاق من صفحة تفاصيل طلب الصرف.
```

---

## المرحلة 11: الاختبارات

### ملفات الاختبار المتأثرة

| # | الملف | التعديل |
|---|---|---|
| 1 | `DisbursementLifecycleTests.cs` | إضافة اختبارات CreateAccrualEntry + فحص AccrualJournalEntryId |
| 2 | `DisbursementAuditGuardTests.cs` | تحديث fixtures |
| 3 | `GetDisbursementRequestsTests.cs` | اختبار الحقل الجديد |
| 4 | `CreateDisbursementRequestTests.cs` | لا يحتاج تعديل (لا ي affect الإنشاء) |
| 5 | `SendToTreasuryTests.cs` | تحديث fixtures |
| 6 | `RejectPaymentOrderTests.cs` | تحديث fixtures |
| 7 | `PaymentOrderAuditGuardTests.cs` | تحديث fixtures |
| 8 | `JournalEntryLifecycleTests.cs` | اختبار إنشاء قيد استحقاق |
| 9 | `ReverseJournalEntryTests.cs` | اختبار عكس قيد الاستحقاق |
| 10 | `JournalEntryLineTests.cs` | اختبار السطور الجديدة |

### اختبارات جديدة مطلوبة

```csharp
// CreateAccrualEntryCommandTests
- Should_CreateAccrualEntry_When_AllDataValid
- Should_Fail_When_DisbursementRequestNotFound
- Should_Fail_When_RequestNotApproved
- Should_Fail_When_AccrualAlreadyExists
- Should_Fail_When_ExpenseAccountInvalid
- Should_Fail_When_LiabilityAccountInvalid
- Should_Fail_When_AmountExceedsRequested
- Should_Fail_When_AccountsSame
- Should_LinkAccrualToRequest_When_Success

// RecordPaymentCommand (تعديل)
- Should_UseLiabilityFromAccrual_When_AccrualExists
- Should_FallbackToDefaultAccount_When_NoAccrual
```

---

## المرحلة 12: الواجهة الأمامية

### ملفات TypeScript المتأثرة

| # | الملف | التعديل |
|---|---|---|
| 1 | `disbursement-requests/shared/types.ts` | إضافة `AccrualJournalEntryId`, `AccrualEntryNumber` |
| 2 | `disbursement-requests/shared/schemas.ts` | إضافة Zod schema لـ CreateAccrualEntry |
| 3 | `disbursement-requests/shared/client.ts` | إضافة `createAccrualEntry(id, data)` |
| 4 | `disbursement-requests/hooks/useDisbursementRequests.ts` | إضافة mutation جديدة |
| 5 | `disbursement-requests/pages/DisbursementRequestDetailPage.tsx` | إضافة قسم الاستحقاق |
| 6 | `components/DisbursementRequestForm.tsx` | إضافة حقول الحسابات |
| 7 | `disbursement-requests/pages/DisbursementRequestsListPage.tsx` | إضافة عمود الاستحقاق |
| 8 | `web-api-client.ts` | يُعاد توليده تلقائياً |

### واجهة المستخدم

**في صفحة التفاصيل** (DisbursementRequestDetailPage):
- عرض حالة قيد الاستحقاق (موجود/غير موجود)
- زر "إنشاء قيد الاستحقاق" (يظهر فقط إذا كان Approved ولم يوجد قيد)
- نموذج اختيار الحسابات (Dropdown من دليل الحسابات)
- رابط لقيد اليومية عند وجوده

---

## ملخص Execution

```
الأسبوع 1:
  Day 1: AccountGroupSeedData.cs (18 مجموعة)
  Day 2: AccountSeedData.cs (~70 حساب)
  Day 3: DisbursementRequest.cs + Configuration
  Day 4: DTO + Queries (3 ملفات)
  Day 5: CreateAccrualEntryCommand.cs

الأسبوع 2:
  Day 1: Endpoint + Permissions (4 ملفات)
  Day 2: SubmitPayment + RecordPayment
  Day 3: Void + Cancel
  Day 4: التقارير (5 ملفات)
  Day 5: Seed + Migration

الأسبوع 3:
  Day 1-2: الاختبارات (10 ملفات)
  Day 3-4: الواجهة الأمامية (8 ملفات)
  Day 5: التوثيق + المراجعة النهائية
```
