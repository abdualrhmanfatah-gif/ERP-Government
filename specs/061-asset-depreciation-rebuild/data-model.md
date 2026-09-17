# Data Model: إعادة بناء ميزة الإهلاك

**Date**: 2026-09-17 | الدقة المالية: السجلات `decimal(23,6)`، القيود `decimal(23,2)` (تقريب نهائي عند القيد فقط)

## 1. DepreciationRuns (رأس عملية الإهلاك) — جدول جديد

| العمود | النوع | القواعد |
|--------|-------|---------|
| Id | int identity | PK |
| RunNumber | nvarchar(30) | Required، فريد، تسلسل `DEP` |
| FiscalYearId | int FK Restrict | Required |
| FiscalPeriodId | int FK Restrict | Required، تابع للسنة |
| DepreciationDate | date | Required، داخل الفترة |
| MissedPeriodsPolicy | nvarchar(20) | `CatchUp` / `CurrentPeriodOnly` |
| Status | nvarchar(20) | `Draft` / `Posting` / `Posted` |
| TotalDepreciation | decimal(23,6) | مجموع سجلات التشغيل |
| Notes | nvarchar(500)? | |
| JournalEntryId | int? FK Restrict | فريد بفلتر `IS NOT NULL` |
| PostedAt / PostedBy | datetimeoffset? / nvarchar(450)? | تعبى عند الاكتمال |
| RowVersion | rowversion | concurrency |
| Created/CreatedBy/LastModified/LastModifiedBy | audit | |

**الفهارس**: فريد `(FiscalYearId, FiscalPeriodId)` — غير مفلتر (لا عكس → لا إعفاء)؛ فريد `RunNumber`؛ `Status`؛ `FiscalPeriodId`.

**ملاحظة سلوكية**: تعطيل مسودة يحذفها نهائيا مع سجلاتها (Cascade) قبل الترحيل فقط.

## 2. DepreciationScheduleLines (سجل لكل أصل لكل فترة) — جدول جديد

| العمود | النوع | القواعد |
|--------|-------|---------|
| Id | int identity | PK |
| DepreciationRunId | int FK Restrict→DepreciationRuns | Cascade (حذف مسودة) |
| AssetId | int FK Restrict→Assets | Required |
| FiscalYearId / FiscalPeriodId | int | من الرأس (للاستعلام) — الفترة تابعة للسنة |
| DepreciationDate | date | تاريخ الفترة التي يخدمها |
| Method | nvarchar(50) | من المجموعة (لقطة) |
| Rate | decimal(18,6) | لقطة |
| PeriodNumber | int | **قياسي من FiscalPeriod** |
| TotalPeriods | int? | من العمر الإنتاجي × 12 |
| DepreciationBase | decimal(23,6) | لقطة |
| ResidualValue | decimal(23,6) | لقطة |
| OpeningAccumulatedDepreciation | decimal(23,6) | قبل القسط |
| Amount | decimal(23,6) | قسط الفترة (قد يكون صفرا — لقطة اكتمال) |
| ClosingAccumulatedDepreciation | decimal(23,6) | بعد القسط |
| ClosingBookValue | decimal(23,6) | بعد القسط |
| RowVersion | rowversion | concurrency |
| Created/CreatedBy/LastModified/LastModifiedBy | audit | |

**الفهارس**: فريد `(AssetId, FiscalPeriodId)` — سجل واحد لكل أصل لكل فترة؛ `DepreciationRunId`؛ `AssetId`.

## 3. تعديلات على جداول قائمة

| الجدول | التعديل | السبب |
|--------|---------|-------|
| JournalEntryLines | + `DepreciationScheduleLineId int?` FK Restrict + فهرس | وسم كل سطر بسجل الأصل (زوجان لكل سجل) |
| JournalEntryTemplateLines | أدوار جديدة: `DepreciationExpense` (مدين) و`AccumulatedDepreciation` (دائن) بديل `GroupDepreciationAccount` | الحسابان من القالب (FR-2) |
| Assets | `AccumulatedDepreciation`, `CurrentValue`: (23,2) → (23,6) | FR-6.4 لا فقدان دقة |
| AssetGroups | − `DepreciationAccountId` | القالب المصدر الوحيد (FR-2.4) |
| JournalEntryTemplates (seed) | قالب `AssetDepreciation` سطران بدورين + SystemKey (موجود) | FR-2.1 |

## 4. الجداول المسقطة (إحلال نهائي — بيانات تُهجر)

- `AssetDepreciationRuns` (الحالية)
- `DepreciationSchedules` (الحالية)
- عمود `DepreciationAccountId` من `AssetGroups`

## 5. حالات وانتقالات

```
DepreciationRun: Draft ──(post)──> Posting ──(event consumer success)──> Posted
       │                              │
       └──(delete, Draft only)        └──(event consumer failure)──> يعود Draft? لا — يبقى Posting حتى إعادة المعالجة (outbox retry)
Posted: نهائي — لا عكس، لا تعديل، لا حذف
```

- `Posting` يضبط داخل أمر الترحيل قبل إصدار الحدث (احتكار انتقال).
- فشل المستهلك يعيد المحاولة عبر Outbox (lease/retry القائم)؛ لا مسودة جديدة للفترة حتى يكتمل.

## 6. قواعد التحقق (Validation Rules)

1. أمر التشغيل: `FiscalYearId > 0`, `FiscalPeriodId > 0`, `DepreciationDate` مطلوب، `MissedPeriodsPolicy` من القيمتين، `Notes ≤ 500`.
2. التشغيل يرفض إن: الفترة مفقودة/مغلقة/غير نشطة/لا تحتوي التاريخ؛ تشغيل موجود للفترة؛ لا أصول مؤهلة؛ أصول بسياسة ناقصة (تسمية أسماء).
3. الترحيل يرفض إن: الحالة ≠ Draft؛ لا سجلات أو الإجمالي ≤ 0؛ الفترة مغلقة/غير نشطة؛ القالب مفقود أو دوران غير معرفين؛ حسابات القالب غير قابلة للترحيل؛ تعارض RowVersion.
4. الحاسبة: الأساس ≥ 0؛ المبلغ ≤ المتبقي؛ التخريدي ≥ 0 و < الكلفة.

## 7. معادلات الحساب (StraightLine)

```
Residual        = round(OriginalValue × ResidualValuePercentage% , 6)
Base            = OriginalValue − Residual − Accumulated      (≥ 0)
Amount          = min( round( (OriginalValue − Residual) × Rate% , 6 ), Base )
ClosingAccum    = Accumulated + Amount
ClosingBook     = OriginalValue − ClosingAccum
```

للقيد: `DebitExpense = CreditAccumulated = round(Amount, 2)` لكل سجل.
