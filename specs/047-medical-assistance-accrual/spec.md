# SPEC-047: ترحيل المساعدات المرضية — قيد الاستحقاق والسداد

> **DEP-027 أثر**: كل إشارة إلى `PaymentOrderExecuted`/PostingRules داخل هذه المواصفة تاريخية. محرك القواعد ومصادره أُزيلت؛ السداد يُرحَّل أصليًّا في `RecordPayment` (قيد متوازن مباشر عبر outbox)، والاستحقاق عبر `CreateAccrualEntry` كما في الملفات محل المفاضلة. عدّل مفردات التخطيط أينما ذكرت `PostingRuleLineSeedData`/`PostingRuleMatcher`/`JournalEntryGenerator`.

## النظرة العامة

**المشكلة**: النظام السابق كان يرحّل عند الدفع عبر محرك قواعد (أُزيل نهائيًّا، DEP-027). لا يوجد قيد استحقاق عند اعتماد طلب الصرف.

**الحل**: أمر جديد `CreateAccrualEntryCommand` يُنشئ قيد استحقاق بحسابات يختارها المستخدم، مع ربطه بطلب الصرف. قيد السداد يُنشأ أصليًّا (ديناميكيًا) داخل `RecordPayment` دون قاعدة ترحيل.

**الحالة**: مقترح — لم يُبدأ التنفيذ

---

## السياق

### الدليل المحاسبي اليمني الموحد

الحساب 2 = الموارد الرأسمالية (يشمل الخصوم):

```
25  الدائنون
├── 251  الموردون
│   ├── 2511  موردون محليون — قطاع عام
│   ├── 2512  موردون محليون — قطاع خاص
│   └── 2513  موردون خارجيون
├── 252  أوراق الدفع
├── 253  دائنون متنوعون
├── 254  ذمم دائنة مختلفة
├── 255  دائنو توزيعات الأرباح
├── 256  الشيكات والحوالات
├── 257  البنك المركزي
└── 258  الفروع والمراسلون

26  السلف والتأمينات الدائنة
├── 261  السلف الدائنة
├── 262  التأمينات الدائنة
├── 263  التوقيفات
└── 264  التأمينات لقاء سلف وقروض

27  الحسابات الانتقالية الدائنة
├── 271  إيرادات محصلة مقدماً
└── 272  مصاريف جارية مستحقة
```

### التسلسل المقترح

```
1. إنشاء طلب مساعدة
2. اعتماد الطلب (توقيعان)
3. المستخدم ينشئ قيد الاستحقاق (حسابات يختارها)
4. المستخدم يرحّل القيد (Submit → Approve → Post)
5. إعداد أمر الصرف → فحص: هل يوجد قيد استحقاق؟
6. اعتماد أمر الصرف
7. إرسال للخزينة
8. تسجيل الدفع → قيد سداد تلقائي (مدين الخصم / دائن البنك)
9. إغلاق
```

### القيود المحاسبية

**قيد الاستحقاق** (عند اعتماد طلب الصرف):
```
من ح/ [مصروف] (المستخدم يختاره)     100,000
    إلى ح/ [مورد/خصم] (المستخدم يختاره)  100,000
```

**قيد السداد** (عند تسجيل الدفع):
```
من ح/ [مورد/خصم] (من قيد الاستحقاق)  100,000
    إلى ح/ 1821 الحسابات الجارية المحلية  100,000
```

---

## المتطلبات الوظيفية

### FR-01: إنشاء قيد الاستحقاق

- المستخدم يُنشئ قيد استحقاق من صفحة تفاصيل طلب الصرف
- يختار حساب المصروف من دليل الحسابات
- يختار حساب الخصم/المورد من دليل الحسابات
- يُحدد المبلغ (لا يتجاوز المبلغ المطلوب)
- يُنشأ القيد بحالة Draft
- يُربط القيد بطلب الصرف

### FR-02: شرط الاعتماد

- لا يمكن إنشاء أمر صرف لأمر صرف مرتبط بطلب صرف إلا إذا كان `AccrualJournalEntryId` موجوداً
- إذا لم يكن هناك طلب صرف مرتبط، لا يُطلب قيد الاستحقاق

### FR-03: قيد السداد الديناميكي

- عند تسجيل الدفع، يُنشأ قيد سداد تلقائياً
- حساب الدائن = حساب البنك (من أمر الصرف)
- حساب المدين = حساب الخصم (من سطر الدائن في قيد الاستحقاق)

### FR-04: عكس القيد

- عند عكس قيد الاستحقاق، يُحذف `AccrualJournalEntryId` من طلب الصرف
- عند إبطال أمر صرف مدفوع، يجب عكس قيد السداد

---

## التصميم التقني

### Mécanismes Existantes Réutilisées

| المكون | الملف | السبب |
|---|---|---|
| JournalEntry | `src/Domain/Accounting/Entities/JournalEntry.cs` | القيد اليومي |
| JournalEntryLine | `src/Domain/Accounting/Entities/JournalEntryLine.cs` | سطر القيد |
| CreateJournalEntryCommand | `src/Application/Accounting/Commands/JournalEntries/CreateJournalEntry/` | إنشاء ترويسة |
| CreateJournalEntryLineCommand | `src/Application/Accounting/Commands/JournalEntryLines/CreateJournalEntryLine/` | إضافة سطور |
| IDocumentSequenceService | `src/Application/FinancialSettings/Common/Services/` | ترقيم القيد |
| IUser | `src/Application/Common/Interfaces/` | هوية المستخدم |

### Mécanismes Nouveaux

#### CreateAccrualEntryCommand

**الملف**: `src/Application/Payments/Commands/DisbursementRequests/CreateAccrualEntry/CreateAccrualEntryCommand.cs`

```csharp
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
```

**Handler**:
1. `FindAsync(DisbursementRequestId)` — تحقق من الوجود
2. Validate `Status == Approved`
3. Validate `AccrualJournalEntryId is null` — لا يوجد قيد مسبقاً
4. Validate حسابين: موجودين + نشطين + منشورين
5. Validate `Amount > 0 && Amount <= RequestedAmount`
6. Get `FiscalPeriod` النشط لتاريخ اليوم
7. Generate entry number عبر `IDocumentSequenceService`
8. Create `JournalEntry` (Draft, IsSystemGenerated=true)
9. Create line 1: Debit ExpenseAccountId
10. Create line 2: Credit LiabilityAccountId
11. Set `dr.AccrualJournalEntryId = journalEntry.Id`
12. `SaveChanges` atomically
13. Return `JournalEntryId`

**Validator**:
- `DisbursementRequestId > 0`
- `ExpenseAccountId > 0`
- `LiabilityAccountId > 0`
- `Amount > 0`
- `CurrencyId > 0`
- `ExpenseAccountId != LiabilityAccountId`

---

## قائمة الملفات المتأثرة (46 ملف)

### المرحلة 1: دليل الحسابات (2 ملف)

| # | الملف | التعديل |
|---|---|---|
| 1 | `src/Infrastructure/Data/Seeds/AccountGroupSeedData.cs` | إضافة 18 مجموعة (25, 251-258, 26, 261-264, 27, 271-272) |
| 2 | `src/Infrastructure/Data/Seeds/AccountSeedData.cs` | إضافة ~70 حساب تحت المجموعات الجديدة + `352216` مساعدات مرضية |

### المرحلة 2: Entity + Configuration (2 ملف)

| # | الملف | التعديل |
|---|---|---|
| 3 | `src/Domain/Payments/Entities/DisbursementRequest.cs` | إضافة `int? AccrualJournalEntryId` |
| 4 | `src/Infrastructure/Data/Configurations/Payments/DisbursementRequestConfiguration.cs` | إضافة FK + index |

### المرحلة 3: DTO + Queries (3 ملف)

| # | الملف | التعديل |
|---|---|---|
| 5 | `src/Application/Payments/Common/DTOs/DisbursementRequestDto.cs` | إضافة `AccrualJournalEntryId` + `AccrualEntryNumber` |
| 6 | `src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequestById/GetDisbursementRequestByIdQuery.cs` | تحميل القيد المرتبط |
| 7 | `src/Application/Payments/Queries/DisbursementRequests/GetDisbursementRequests/GetDisbursementRequestsQuery.cs` | إضافة الحقل للقائمة |

### المرحلة 4: أمر جديد (1 ملف)

| # | الملف | التعديل |
|---|---|---|
| 8 | `src/Application/Payments/Commands/DisbursementRequests/CreateAccrualEntry/CreateAccrualEntryCommand.cs` | **جديد** — أمر + handler + validator |

### المرحلة 5: Endpoint + Permissions (4 ملف)

| # | الملف | التعديل |
|---|---|---|
| 9 | `src/Web/Endpoints/DisbursementRequests/DisbursementRequests.cs` | route جديد `/{id}/accrual-entry` |
| 10 | `src/Application/Common/Security/PermissionCodes.cs` | إضافة `DisbursementRequests.CreateAccrual` |
| 11 | `src/Web/DependencyInjection.cs` | تسجيل السياسة الجديدة |
| 12 | `src/Infrastructure/Data/Seeds/RolePermissionSeedData.cs` | ربط الصلاحية بالدور |

### المرحلة 6: تعديلات الأوامر (3 ملف)

| # | الملف | التعديل |
|---|---|---|
| 13 | `src/Application/Payments/Commands/PaymentOrders/SubmitPaymentOrder/SubmitPaymentOrderCommand.cs` | فحص `AccrualJournalEntryId` |
| 14 | `src/Application/Payments/Commands/Payments/RecordPayment/RecordPaymentCommand.cs` | قيد سداد ديناميكي |
| 15 | `src/Application/Accounting/Commands/JournalEntries/ReverseJournalEntry/ReverseJournalEntryCommand.cs` | حذف `AccrualJournalEntryId` عند عكس |

### المرحلة 7: Void/Cancel (2 ملف)

| # | الملف | التعديل |
|---|---|---|
| 16 | `src/Application/Payments/Commands/PaymentOrders/VoidPaymentOrder/VoidPaymentOrderCommand.cs` | التعامل مع الاستحقاق عند الإبطال |
| 17 | `src/Application/Payments/Commands/PaymentOrders/CancelPaymentOrder/CancelPaymentOrderCommand.cs` | التعامل مع الاستحقاق عند الإلغاء |

### المرحلة 8: التقارير (5 ملف)

| # | الملف | التعديل |
|---|---|---|
| 18 | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/GetDisbursementRegisterQueryHandler.cs` | إضافة حقل الاستحقاق |
| 19 | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/GetDisbursementRegisterQuery.cs` | فلتر حسب حالة الاستحقاق |
| 20 | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterQuery/DisbursementRegisterDto.cs` | إضافة الحقول |
| 21 | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/GetDisbursementRegisterDetailQueryHandler.cs` | تفاصيل الاستحقاق |
| 22 | `src/Application/Reporting/DisbursementRegister/GetDisbursementRegisterDetail/DisbursementRegisterDetailDto.cs` | DTO التفاصيل |
| 23 | `src/Web/Endpoints/Reporting/DisbursementRegisterReports.cs` | لا يحتاج تعديل (يُعالج تلقائياً) |

### المرحلة 9: Seed Data (1 ملف)

| # | الملف | التعديل |
|---|---|---|
| 24 | `src/Infrastructure/Data/Seeds/PostingRuleLineSeedData.cs` | مراجعة: هل نغير حساب المدين في `PaymentOrderExecuted`؟ |

### المرحلة 10: Migration + التوثيق (3 ملف)

| # | الملف | التعديل |
|---|---|---|
| 25 | `src/Infrastructure/Data/Migrations/` | EF Migration جديد |
| 26 | `docs/database-schema.md` | تحديث جدول DisbursementRequests |
| 27 | `CONTEXT.md` | تحديث تعريف المستفيد |

### المرحلة 11: الاختبارات (10 ملفات)

| # | الملف | التعديل |
|---|---|---|
| 28 | `tests/Application.UnitTests/Payments/DisbursementLifecycleTests.cs` | تحديث fixtures + اختبارات جديدة |
| 29 | `tests/Application.UnitTests/Payments/DisbursementAuditGuardTests.cs` | تحديث fixtures |
| 30 | `tests/Application.UnitTests/Payments/GetDisbursementRequestsTests.cs` | اختبار الحقل الجديد |
| 31 | `tests/Application.UnitTests/Payments/CreateDisbursementRequestTests.cs` | اختبار الإنشاء |
| 32 | `tests/Application.UnitTests/Payments/SendToTreasuryTests.cs` | تحديث fixtures |
| 33 | `tests/Application.UnitTests/Payments/RejectPaymentOrderTests.cs` | تحديث fixtures |
| 34 | `tests/Application.UnitTests/Payments/PaymentOrderAuditGuardTests.cs` | تحديث fixtures |
| 35 | `tests/Application.FunctionalTests/Accounting/JournalEntryLifecycleTests.cs` | اختبار القيود |
| 36 | `tests/Application.UnitTests/Accounting/JournalEntries/ReverseJournalEntryTests.cs` | اختبار العكس |
| 37 | `tests/Application.UnitTests/Accounting/JournalEntryLines/JournalEntryLineTests.cs` | اختبار السطور |

### المرحلة 12: الواجهة الأمامية (8 ملفات)

| # | الملف | التعديل |
|---|---|---|
| 38 | `src/Web/ClientApp/src/features/payments/disbursement-requests/shared/types.ts` | تحديث الواجهة |
| 39 | `src/Web/ClientApp/src/features/payments/disbursement-requests/shared/schemas.ts` | Zod schemas |
| 40 | `src/Web/ClientApp/src/features/payments/disbursement-requests/shared/client.ts` | API client |
| 41 | `src/Web/ClientApp/src/features/payments/disbursement-requests/hooks/useDisbursementRequests.ts` | React Query hooks |
| 42 | `src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestDetailPage.tsx` | صفحة التفاصيل |
| 43 | `src/Web/ClientApp/src/components/DisbursementRequestForm.tsx` | النموذج |
| 44 | `src/Web/ClientApp/src/web-api-client.ts` | يُعاد توليده تلقائياً |
| 45 | `src/Web/ClientApp/src/features/payments/disbursement-requests/pages/DisbursementRequestsListPage.tsx` | تحديث القائمة |

### المرحلة 13: التحقق (1 ملف)

| # | الملف | التعديل |
|---|---|---|
| 46 | `docs/database-schema.md` | مراجعة نهائية |

---

## ملخص التغييرات حسب النوع

| النوع | العدد | الملفات |
|---|---|---|
| بيانات أولية (Seed) | 3 | AccountGroupSeedData, AccountSeedData, PostingRuleLineSeedData |
| Entity + Config | 2 | DisbursementRequest, DisbursementRequestConfiguration |
| DTO + Query | 3 | DisbursementRequestDto, GetById, GetList |
| أمر جديد | 1 | CreateAccrualEntryCommand |
| تعديل أمر | 4 | SubmitPayment, RecordPayment, Void, Cancel |
| عكس قيد | 1 | ReverseJournalEntry |
| Endpoint | 1 | DisbursementRequests |
| أذونات | 3 | PermissionCodes, DependencyInjection, RolePermissionSeedData |
| تقارير | 5 | DisbursementRegister (5 ملفات) |
| Migration | 1 | EF Migration |
| توثيق | 2 | database-schema, CONTEXT |
| اختبارات | 10 | Unit + Functional tests |
| واجهة أمامية | 8 | TypeScript files |
| **الإجمالي** | **46** | |

---

## التسلسل الزمني

### الأسبوع 1: الأساس

| اليوم | المراحل | الملفات |
|---|---|---|
| Day 1 | المرحلة 1 | AccountGroupSeedData.cs |
| Day 2 | المرحلة 1 | AccountSeedData.cs |
| Day 3 | المرحلة 2 | DisbursementRequest.cs + Configuration |
| Day 4 | المرحلة 3 | DTO + Queries |
| Day 5 | المرحلة 4 | CreateAccrualEntryCommand |

### الأسبوع 2: التكامل

| اليوم | المراحل | الملفات |
|---|---|---|
| Day 1 | المرحلة 5 | Endpoint + Permissions |
| Day 2 | المرحلة 6 | SubmitPayment + RecordPayment |
| Day 3 | المرحلة 7 | Void + Cancel |
| Day 4 | المرحلة 8 | التقارير |
| Day 5 | المرحلة 9 + 10 | Seed + Migration |

### الأسبوع 3: التحقق

| اليوم | المراحل | الملفات |
|---|---|---|
| Day 1-2 | المرحلة 11 | الاختبارات |
| Day 3-4 | المرحلة 12 | الواجهة الأمامية |
| Day 5 | المرحلة 13 | التوثيق + المراجعة النهائية |

---

## ملاحظات تقنية

1. **لا نحتاج حدث domain جديد** — نستخدم نظام القيود اليومية الموجود
2. **لا نحتاج توسيع JournalEntryGenerator** — أُزيل (DEP-027)؛ الأمر الجديد ينشئ القيد مباشرة
3. **الحسابات تختارها المستخدم** — الاستحقاق والسداد أصليّان؛ لا PostingRule
4. **قيد الاستحقاق يبقى Draft** حتى يرحّله المستخدم يدوياً
5. **قيد السداد يُنشأ تلقائياً** — الحسابات تُستخرج من قيد الاستحقاق
6. **`AccountGroupId` في AccountSeedData** ثابت رقمي — يجب حساب IDs الجديدة بعناية
7. **`ParentId` في AccountSeedData** يعتمد على الموقع في القائمة المسطحة

---

## المخاطر

| المخاطرة | التأثير | التخفيف |
|---|---|---|
| تغير `AccountGroupId` مع الإضافة | حسابات موجودة ت linking خاطئ | إضافة في نهاية القائمة |
| `ReverseJournalEntry` لا يعرف `DisbursementRequest` | قيد مقلوب بدون تعديل الطلب | إضافة فحص في Handler |
| قيود الاستحقاق Draft لا تُحتسب في الموازنة | التوفر المالي غير دقيق | سلوك مقصود — فقط Posted تُحتسب |
| `BudgetAvailabilityService` لا يعرف الاستحقاق | حساب غير دقيق | لا يحتاج تعديل — سلوك صحيح |
