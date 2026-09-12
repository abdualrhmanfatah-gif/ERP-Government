# Feature Context: BudgetItemAllocations — إعداد الموازنة ومخصصات البنود

## Document Metadata
- **Generated**: 2026-09-10
- **Input Type**: simple_description
- **Source**: User request + Yemeni Financial Law No. 8/1990
- **Status**: DISCOVERY_COMPLETE

---

## Original Request

مواصفات لإعداد الموازنة عبر جدول «مخصصات بنود الموازنة» BudgetItemAllocations المرتبط بـ Budgets، ويحتوي على BudgetItemId وProposedAmount وApprovedAmount وRemarks وحقول التدقيق وRowVersion، مع منع تكرار البند داخل الموازنة. دورة إعداد الموازنة: مسودة ← قيد المراجعة، مع إمكانية الإعادة بالملاحظات، ← معتمدة ← مرحّلة، وتُثبّت المبالغ عند الاعتماد. اربط أوامر الصرف بمخصصات البنود في المستوى الذي يحمل بند الصرف فعليًا وفق النموذج الحالي. احسب المتبقي من المخصص بالمعادلة: المبلغ المخصص المعتمد للبند − ما صُرف فعليًا، ويُستخرج المصروف الفعلي من سطور القيود اليومية المرحّلة عبر الحساب المرتبط ببند الموازنة، مع مراعاة السنة المالية وصافي الأثر المدين والدائن والإلغاءات والقيود العكسية وفق طبيعة الحساب وقواعد الترحيل القائمة. لا تستخدم صافي BudgetTransactions أو مبالغ أوامر الصرف بوصفها مصدرًا للمصروف الفعلي، ولا تخزّن الرصيد المحسوب. تحقّق من علاقة البنود بالحسابات، وعالج اشتراك عدة بنود في حساب واحد إن كان مسموحًا بما يضمن إسناد المصروف إلى البند الصحيح دون تكرار احتسابه. اجعل كل سجل في BudgetTransactions خاصًا بمخصص بند واحد ومرتبطًا به عبر BudgetItemAllocationId، ويحتوي على نوع العملية وتاريخها ومبلغها واتجاهها وحالتها ومرجعها، واستغنِ عن BudgetTransactionLines. حدّد أثر العمليات التي تغيّر المبلغ المخصص على القيمة المعتمدة في BudgetItemAllocations بصورة ذرّية وقابلة للتدقيق، دون احتساب العملية مرتين أو السماح بتعديل المبلغ المعتمد خارج دورة عمل معتمدة. ألغِ المناقلة بين البنود بالكامل ونوع Transfer وواجهاته ومنطقه، دون إعادة ترقيم قيم أنواع العمليات الأخرى المخزنة. استخدم ApprovalHistory وDocumentStatusLog لتسجيل الاعتماد وانتقالات الحالة، وصحّح العمليات المرحّلة بالعكس المرتبط بالأصل دون تعديلها أو حذفها. حافظ على ضوابط الارتباط والصرف القائمة مع التمييز بين المتبقي بعد الصرف الفعلي والمتاح بعد الالتزامات، ومنع خصم المصروف نفسه مرتين. حدّث واجهات إعداد الموازنة والمخصصات والمعاملات وأوامر الصرف بالعربية وRTL والعقود والتوثيق المتأثر. راجع الكيانات والمواصفات الحالية وروابط بنود الموازنة بالحسابات والقيود اليومية قبل تحديد تفاصيل التنفيذ، وحدّد ترحيلًا يحفظ البيانات والمراجع والأثر التدقيق للعمليات وأوامر الصرف السابقة.

---

## Problem Statement

**User Pain**: نظام الموازنة الحالي لا يربط مخصصات البنود بأوامر الصرف بشكل صحيح، ولا يحسب المتبقي من المخصص بناءً على المصروف الفعلي من القيود اليومية. الفجوات تسبب: (1) عدم دقة رصيد المتبقي، (2) عدم إمكانية التتبع من البند إلى أمر الصرف، (3) عدم تطبيق مبدأ عدم التخصيص القانوني.

**Frequency**: يومي — كل أمر صرف جديد يحتاج ربط بمخصص بند الموازنة.

**Impact**: مخالفة القانون المالي رقم 8/1990 (مبدأ عدم التخصيص) + عدم دقة البيانات المالية + صعوبة التدقيق.

---

## Core Intent Analysis

### WHO (Target Users)
- محاسب الموازنة (إعداد الموازنة ومخصصات البنود)
- مدير الحسابات (اعتماد الموازنة والمعاملات)
- مسؤول أوامر الصرف (ربط الصرف بالمخصص)
- المدقيق الداخلي (تتبع المصروفات وفق البند)

### WHAT (Desired Outcome)
1. جدول مخصصات بنود الموازنة يربط كل بند بمبلغ مقترح ومعتمد
2. دورة حياة موازنة: مسودة → مراجعة → اعتماد → ترحيل
3. ربط أوامر الصرف بمخصص البند الصحيح
4. حساب المتبقي = المبلغ المعتمد − المصروف الفعلي (من القيود اليومية)
5. منع التكرار والتعديل خارج الدورة المعتمدة

### WHEN (Trigger Conditions)
- إعداد موازنة جديدة أو تعديل مخصصات
- تقديم أمر صرف ↔ ربط بمخصص
- اعتماد موازنة ↔ تثبيت المبالغ
- طلب تقرير المتبقي من المخصص

### WHY (Problem Being Solves)
- تطبيق القانون المالي اليمني (مبدأ عدم التخصيص + الشمولية)
- ربط المصروفات بالبنود لضمان الدقة
- تتبع المتبقي بشكل موثوق

---

## Non-Goals (Explicit Exclusions)

| Non-Goal | Rationale |
|----------|-----------|
| إنشاء موازنة من ملف Excel | تعقيد الإدخال — يُنفّذ يدوياً عبر الواجهة |
| تنبيهات تلقائية عند تجاوز المخصص | ميزة مستقبلية — الأولوية للربط والحساب |
| التكامل مع نظام خارجي للبنوك | يتطلب تكامل خارجي — نطاق النظام الداخلي |
| حساب التضخم على المخصصات | مبدأ النقدي في القانون اليمني |

---

## Codebase Research

### الكيانات الحالية

#### BudgetItemAllocation (الموجود)
```csharp
public class BudgetItemAllocation : BaseAuditableEntity
{
    public int BudgetId { get; set; }
    public int BudgetItemId { get; set; }
    public decimal ProposedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }   // يُثبت عند اعتماد الموازنة
    public string? Remarks { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
```
**الملاحظة**: الكيان موجود بالكامل. لا يحتاج تعديلات هيكلية.

#### BudgetTransaction (الموجود)
```csharp
public class BudgetTransaction : BaseAuditableEntity
{
    public string TransactionNumber { get; set; }
    public int BudgetId { get; set; }
    public int BudgetItemAllocationId { get; set; }  // ✅ مرتبط بمخصص البند
    public BudgetTransactionType TransactionType { get; set; }
    public DateOnly TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public TransactionDirection Direction { get; set; }
    public string? DocumentType { get; set; }
    public int? DocumentId { get; set; }
    public string? Description { get; set; }
    public BudgetTransactionStatus Status { get; set; }
    // ... audit fields
}
```
**الملاحظة**: يحتوي على `BudgetItemAllocationId` و `Amount` و `Direction` — متوافق مع المتطلب.

#### PaymentOrder (الموجود)
```csharp
public class PaymentOrder : BaseAuditableEntity
{
    public int? BudgetItemId { get; set; }
    public int? BudgetItemAllocationId { get; set; }  // ✅ مرتبط بمخصص البند
    // ... other fields
}
```
**الملاحظة**: يحتوي على `BudgetItemAllocationId` — جاهز للربط.

#### BudgetAvailabilityService (الموجود)
```csharp
// ComputeActualExpenditureAsync:
SUM(Debit - Credit) from JournalEntryLines
WHERE AccountId IN (accounts linked to budget items)
  AND JournalEntry.Status == Posted
  AND not reversal
```
**الملاحظة**: يحسب المصروف الفعلي من القيود اليومية ✅ — متوافق مع المتطلب.

### الفجوات المكتشفة

| # | الفجوة | الملف |
|---|--------|-------|
| 1 | لا يوجد **منع تكرار البند** في نفس الموازنة | CreateBudgetItemAllocation handler |
| 2 | `BudgetTransactionType.Transfer` (3) **محظور لكن القيمة موجودة** في Enum | enums + frontend labels |
| 3 | `BudgetTransactionLines` **لا زال موجود** رغم أن كل سجل مرتبط بمخصص واحد | domain + commands |
| 4 | واجهة **إعداد الموازنة** لا تعرض مخصصات البنود بشكل كامل | BudgetDetailPage |
| 5 | **ربط أمر الصرف بالمخصص** لا يتحقق من صحة المخصص | SubmitPaymentOrder |
| 6 | **المتبقي** لا يُحسب في الواجهة | BudgetAvailabilitySummary |
| 7 | التسمية: `budgetItemId` مسمّى "التخصيص" وهو خاطئ | PaymentOrderForm |

### المبادئ القانونية المطبقة

| المبدأ | كيف يُطبّق |
|--------|-----------|
| **الأساس النقدي** | المصروف الفعلي من القيود اليومية المرحّلة فقط |
| **مبدأ عدم التخصيص** | لا يُسمح بتعديل ApprovedAmount خارج الدورة المعتمدة |
| **مبدأ الشمولية** | جميع البنود يجب أن يكون لها مخصص |
| **التثبيت عند الاعتماد** | ProposedAmount → ApprovedAmount عند اعتماد الموازنة |

---

## Use Scenarios

### Scenario 1: إعداد موازنة جديدة
**Actor**: محاسب الموازنة
**Trigger**: بداية السنة المالية
**Goal**: إنشاء موازنة بمخصصات لكل بنود
**Expected Outcome**: موازنة في حالة Draft مع مخصصات لكل بنود (ProposedAmount)

### Scenario 2: اعتماد الموازنة
**Actor**: مدير الحسابات
**Trigger**: مراجعة المخصصات والموافقة
**Goal**: تثبيت المبالغ المعتمدة
**Expected Outcome**: جميع ApprovedAmount = ProposedAmount، الحالة = Approved

### Scenario 3: تقديم أمر صرف
**Actor**: مسؤول أوامر الصرف
**Trigger**: طلب صرف من جهة حكومية
**Goal**: ربط الصرف بالمخصص الصحيح والتحقق من المتاح
**Expected Outcome**: أمر صرف مرتبط بمخصص البند + فحص المتاح

### Scenario 4: حساب المتبقي
**Actor**: محاسب الموازنة / المدقيق
**Trigger**: طلب تقرير أو فحص دوري
**Goal**: معرفة المتبقي من كل مخصص
**Expected Outcome**: المتبقي = ApprovedAmount −ActualExpenditure (من القيود اليومية)

---

## Gap Analysis

### Identified Gaps

| # | الفئة | الفجوة | التأثير |
|---|--------|--------|---------|
| 1 | سلوك | لا يوجد منع تكرار البند في نفس الموازنة | يمكن إضافة بند مكرر → تضخم المخصص |
| 2 | سلوك | Transfer محظور لكن القيمة موجودة في Enum | واجهات تعرض نوع غير متاح |
| 3 | سلوك | BudgetTransactionLines لا يزال موجود | بيانات غير مستخدمة → ارتباك |
| 4 | واجهة | لا توجد واجهة إدارة مخصصات البنود | لا يمكن إنشاء/تعديل المخصصات |
| 5 | واجهة | ربط أمر الصرف لا يتحقق من صحة المخصص | يمكن ربط بمخصص غير موجود |
| 6 | واجهة | المتبقي لا يُحسب في الواجهة | لا يمكن معرفة المتاح |
| 7 | تسمية | "التخصيص" يُستخدم لاختيار BudgetItem | ارتباك في المعنى |

---

## Questions Requiring Resolution

### Q1: حذف Transfer من Enum أم تعطيل فقط؟
- **الفئة**: سلوك
- **المالك**: المستخدم
- **حاجز**: نعم
- **الفجوة**: هل نحذف `Transfer = 3` من Enum (يتطلب migration) أم نتركه محظوراً فقط؟
- **السؤال**: هل تحذف القيمة 3 من Enum (يتطلب db migration)؟
- **الخيارات**:
  - A) حذف القيمة من Enum + migration (نظيف لكن يتطلب تعديل DB)
  - B) ترك القيمة محظورة فقط (بدون حذف — أبسط)
- **لماذا Matters**: يؤثر على تاريخ الترحيل وبيانات سابقة

### Q2: هل يُسمح بتعديل مخصص بعد اعتماد الموازنة؟
- **الفئة**: سلوك
- **المالك**: المستخدم
- **حاجز**: نعم
- **الفجوة**: القانون يقول "تُثبّت المبالغ عند الvestment". هل هذا يعني عدم التعديل نهائياً؟
- **السؤال**: هل يمكن تعديل ApprovedAmount بعد اعتماد الموازنة؟
- **الخيارات**:
  - A) لا يُسمح بالتعديل إطلاقاً (myla law)
  - B) يُسمح بتعديل عبر معاملة موازنة (Supplement/Reduction)
- **لماذا Matters**: يؤثر على مرونة النظام

### Q3: ماذا يحدث عند إلغاء أمر صرف مرتبط بمخصص؟
- **الفئة**: سلوك
- **المالك**: المستخدم
- **حاجز**: لا
- **الفجوة**: عند إلغاء/إبطال أمر صرف، هل يُعاد المبلغ للمخصص تلقائياً؟
- **السؤال**: عند إلغاء أمر صرف، هل يُزاد المتبقي من المخصص؟
- **الخيارات**:
  - A) نعم — يُعاد المبلغ تلقائياً (عبر قيد عكسي)
  - B) لا — يبقى المبلغ كمصروف (لا إلغاء تلقائي)
- **لماذا Matters**: يؤثر على دقة المتبقي

### Q4: هل توجد بنود مكررة في نفس الحساب حالياً؟
- **الفئة**: بيانات
- **المالك**: المستخدم
- **حاجز**: لا
- **الفجوة**: هل يمكن لعدة بنود موازنة أن تحمل نفس AccountId؟
- **السؤال**: هل تسمح البنود الحالية باشتراك عدة بنود في حساب واحد؟
- **الخيارات**:
  - A) نعم — مسموح (يتطلب منطق خاص لتجنب التكرار)
  - B) لا — كل بند له حساب فريد (أبسط)
- **لماذا Matters**: يؤثر على خوارزمية حساب المصروف الفعلي

---

## Parking Lot (Deferred Ideas)

| الفكرة | المصدر | الأولوية |
|--------|--------|---------|
| تنبيهات تجاوز المخصص | المستخدم | متوسطة |
| تقارير الموازنة التفصيلية | المستخدم | متوسطة |
| تصدير الموازنة لـ Excel | المستخدم | منخفضة |
| موازنتات متعددة السنوات | المستخدم | منخفضة |

---

## Goals (Pending Resolution)

1. ✅ منع تكرار البند في نفس الموازنة
2. ✅ إزالة Transfer من الواجهات (أو حذفه من Enum)
3. ✅ حذف BudgetTransactionLines واستخدام BudgetItemAllocationId مباشرة
4. ✅ واجهة إدارة مخصصات البنود (إضافة/تعديل/حذف)
5. ✅ ربط أمر الصرف بمخصص البند + فحص صحة المخصص
6. ✅ حساب المتبقي في الواجهة (ApprovedAmount − ActualExpenditure)
7. ✅ تصحيح التسمية (التخصيص → بند الموازنة)
8. ✅ حماية ApprovedAmount من التعديل خارج الدورة المعتمدة

---

## Current State Summary

| المكون | الحالة | ملاحظات |
|--------|--------|---------|
| BudgetItemAllocation entity | ✅ موجود | لا يحتاج تعديل هيكلي |
| BudgetTransaction entity | ✅ موجود | يحتوي على BudgetItemAllocationId |
| BudgetTransactionLine entity | ⚠️ موجود | يجب حذفه |
| Budget lifecycle commands | ✅ موجود | Draft→Submitted→Approved→Active |
| Budget approve → Proposed→Approved | ✅ موجود | ApproveBudgetCommand |
| BudgetAvailabilityService | ✅ موجود | يحسب من JournalEntryLines |
| PaymentOrder.BudgetItemAllocationId | ✅ موجود | جاهز للربط |
| Prevent duplicate item | ❌ مفقود | يجب إضافته |
| Transfer type removal | ⚠️ محظور فقط | يجب حذفه من Enum |
| BudgetItemAllocations UI | ❌ مفقود | يجب بناؤه |
| Remaining amount display | ❌ مفقود | يجب إضافته |
| PaymentOrder→Allocation validation | ❌ مفقود | يجب إضافته |

---

## Next Steps

1. **ؤكيد** الإجابات عن Q1-Q4
2. تحديث الأهداف بناءً على الإجابات
3. تحديد تفاصيل التنفيذ (migrations, commands, handlers, UI)
4. إنشاء spec مفصلة في `specs/` 
