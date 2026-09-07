---
title: "مواصفات واجهة المستخدم — المسار الثاني (Frontend Track 2)"
version: 2.0
date: 2026-09-07
status: ready-for-speckit
spec_count: 28
authoritative_source: "src/Web/ClientApp/src/web-api-client.ts (NSwag generated — verbatim backend contract)"
supporting_sources:
  - "src/Web/Endpoints/** (route surfaces)"
  - "src/Application/Common/Security/PermissionCodes.cs (permission codes)"
  - "AGENTS.md (conventions) · DESIGN.md (UI tokens) · .specify/memory/constitution.md (v1.3.0)"
consumption: "Each ## SPEC section is self-contained — paste it (with its CONTRACT NOTE) as the argument to /speckit.specify"
supersedes: "v1.0 (prose data contracts, single global note)"
---

# مواصفات واجهة المستخدم — المسار الثاني (Frontend Track 2 Specs) v2.0

28 spec مستقلة لواجهة ERP المالي الحكومي (القانون المالي 8/1990). كل spec **ذاتي الاحتواء**: يعاد لصقه كاملاً في `/speckit.specify`. الحقول حرفية من عقد NSwag الموثق — لا استنتاج.

---

## 1. طريقة الاستخدام (Pipeline)

لكل ميزة، بالترتيب:

1. **سُدّ OQ الحاجزة** (المعلّمة `B` في جدول OQ الخاص بالـ spec) — لا specify قبلها
2. `/speckit.specify` ← الصق قسم الـ spec كاملاً (يشمل CONTRACT NOTE)
3. `/speckit.plan` ← القالب المحمي عبر `.specify/templates/overrides/` (بوابات: جدول تغطية الحقول + قسم تصميم + مصفوفة حالات + خريطة اختبارات)
4. `/speckit.tasks` ← `/speckit.analyze` ← `/speckit.implement`
5. `/speckit.converge` ← كرر مع implement حتى **Converged** (كل اسم حقل من spec.md يتحقق في المصدر)

**موجات التنفيذ المقترحة** (حسب الاعتماديات):

| الموجة | الميزات | السبب |
|---|---|---|
| W1 | ACC-03 · ACC-04 · ACC-05 | أساس المحاسبة (الدفاتر/القوالب/المراقبة) — تستهلكها 023/025 |
| W2 | TRE-01 · TRE-02 · TRE-03 | سلسلة الخزينة مترتبة (سند ← بطاقة ← شيك) |
| W3 | PAY-04 · PAY-01 · PAY-02 · PAY-03 | سلسلة المدفوعات (بنوك أولاً — منتقي للأوامر) |
| W4 | CTRL-01 · CTRL-02 · CTRL-03 | الرقابة بعد مصادر بياناتها |
| W5 | RPT-01..06 | التقارير بعد كل المصادر |
| W6 | SEC-01 · SEC-02 · WF-01 · WF-02 · COM-01 · BANK-01 | البنية المستقلة |
| W7 | SYS-01 · SYS-02 · SYS-03 | التركيبية أخيراً (SYS-01 يستهلك تقارير W5) |

**إصلاحات متزامنة (Living Spec / Flow-Back):** 023 → 025 (عدّل spec.md أولاً ← أعد plan/tasks ← analyze) · فجوات 027 من converge.md.

---

## 2. اصطلاحات هذا الملف

### 2.1 وسم التحقق (لكل عنصر عقد)

| الوسم | المعنى |
|---|---|
| ✓ | موثق حرفياً من web-api-client.ts (رقم السطر مرفق) |
| GAP-ADD | غير موجود في PermissionCodes.cs — أضفه وفق صيغة `{Module}.{Action}` وسجّل الإضافة |
| GAP-READ | الجسم/الحقل غير مقروء بعد — اقرأه قبل التنفيذ (مدرج في سجل OQ العام) |
| ASSUMED | مستنتج من سلوك الخادم — يُتحقق عند التنفيذ، لا يُعامل كمؤكد |

### 2.2 أعمدة جداول الحقول

`Field` الاسم الحرفي من العقد · `Type` النوع (int/str/dec(23,2)/bool/date/datetime/enum/array) · `Req` ✓ مطلوب / ○ اختياري (من `?` في TS) / ✓? يتطلب تحققاً · `Rule` قاعدة التحقق أو العرض

### 2.3 تصنيف Open Questions

| Class | المعنى | المعالجة |
|---|---|---|
| **B** (Blocking) | يمنع بدء /speckit.specify أو التنفيذ | يحسمه المالك (User/Engineering) قبل الموجة |
| **N** (Non-blocking) | يمكن التقدم بافتراض موثق | يُسجّل الافتراض في Assumptions ويُرشد عند الحاجة |

### 2.4 مرجع الأسطر
كل `L####` = سطر في `src/Web/ClientApp/src/web-api-client.ts` (NSwag — يُعاد توليده؛ الأرقام إرشادية للتحقق، الأسماء هي العقد).

---

## 3. الفهرس الرئيسي

| ID | الميزة | slug | المجموعة | الأولوية | يعتمد على | حالة الصلاحيات | OQ B/N |
|---|---|---|---|---|---|---|---|
| ACC-03 | الدفاتر والقوالب | journals-templates | محاسبة | P2 | 023 (مستهلك)، FS (تسلسلات) | GAP-ADD | 0/3 |
| ACC-04 | القيود الدورية | recurring-entries | محاسبة | P1 | ACC-03، 023/025، SYS-03 | GAP-ADD | 0/3 |
| ACC-05 | مراقبة المحاسبة | accounting-monitoring | محاسبة | P1 | 023/025، CTRL-02/03 | GAP-ADD | 0/3 |
| TRE-01 | سندات القبض | receipt-vouchers | خزينة | P1 | 022، FS (DSL)، TRE-02 | GAP-ADD | 0/2 |
| TRE-02 | بطاقات الإيداع | deposit-slips | خزينة | P1 | TRE-01، TRE-03 | GAP-ADD | 0/2 |
| TRE-03 | الشيكات والكشف | checks-statements | خزينة | P1 | TRE-01/02 | GAP-ADD | 0/2 |
| PAY-01 | أوامر الدفع | payment-orders | مدفوعات | P1 | 022، BGT، PAY-02..04 | GAP-ADD | 0/3 |
| PAY-02 | طلبات الصرف | disbursement-requests | مدفوعات | P1 | PAY-01، 021، PAY-03 | GAP-ADD | 0/2 |
| PAY-03 | تنفيذ الدفع | payment-execution | مدفوعات | P1 | PAY-02، PAY-01 | ✓ مؤكدة | 0/2 |
| PAY-04 | الحسابات البنكية | bank-accounts | مدفوعات | P2 | PAY-01، BANK-01 | ✓ مؤكدة (5) | 0/3 |
| CTRL-01 | لوحة توافر الموازنة | availability-dashboard | رقابة | P1 | 021، BGT، TRE، PAY | GAP-ADD | 0/2 |
| CTRL-02 | إغلاق/إعادة السنة | year-closing | رقابة | P1 | FS، ACC-05، CTRL-03 | GAP-ADD | 0/2 |
| CTRL-03 | الحسابات النهائية | final-accounts | رقابة | P1 | CTRL-02، ACC-05، TRE، PAY | GAP-ADD | 0/2 |
| RPT-01 | تقرير تنفيذ الموازنة | budget-execution-report | تقارير | P1 | BGT، TRE، PAY | ✓ مؤكدة | 0/1 |
| RPT-02 | تحصيل الإيرادات | revenue-collections-report | تقارير | P2 | TRE-01..03 | ✓ مؤكدة | 0/1 |
| RPT-03 | سجل الصرف | disbursement-register-report | تقارير | P2 | PAY-01..03 | ✓ مؤكدة | 0/1 |
| RPT-04 | لقطة التوفر | availability-snapshot-report | تقارير | P2 | BGT، TRE، PAY | ✓ مؤكدة | 0/2 |
| RPT-05 | ميزان المراجعة | trial-balance-report | تقارير | P1 | ACC-05، 023 | ✓ مؤكدة | 0/1 |
| RPT-06 | القوائم المالية | financial-statements | تقارير | P1 | 023، ACC-05 | ✓ Accounting.Reports | 0/2 |
| SEC-01 | الصلاحيات | permissions | أمان | P3 | RBAC (موجود) | ✓ Permissions.View | 0/1 |
| SEC-02 | قواعد الاعتماد والتفويضات | approval-rules-delegations | أمان | P1 | RBAC، كل lifecycle، WF | ✓ مؤكدة (4) | 0/2 |
| WF-01 | تعريفات سير العمل | workflow-definitions | سير عمل | P2 | RBAC، SEC-02، WF-02 | GAP-ADD | 0/2 |
| WF-02 | الحالات والتاريخ | workflow-instances-history | سير عمل | P2 | WF-01، SEC-02 | GAP-ADD | 0/2 |
| COM-01 | اللجان | committees | لجان | P2 | PO (مرجع)، 022 | ✓ مؤكدة (6) | 0/3 |
| BANK-01 | الكشوف والتسويات | bank-statements-reconciliation | مصرفي | P2 | PAY-04، 023 | ✓部分 (3) + GAP-ADD | 0/4 |
| SYS-01 | لوحة التحكم | dashboard | نظم | P2 | RPT-01/03، SYS-02 | مركّبة | **1**/0 |
| SYS-02 | الإشعارات | notifications | نظم | P2 | كل الميزات، routes | بيانات شخصية | 0/1 |
| SYS-03 | المهام الخلفية وOutbox | background-outbox | نظم | P3 | ACC-04، الترحيل | ✓部分 (2) + GAP-ADD | 0/3 |

**الإجمالي:** OQ حاجزة = 1 (SYS-01) · OQ غير حاجزة = 56 · أكواد صلاحيات مؤكدة = 7 ميزات · تتطلب إضافة/تحقق = 21 ميزة.

---

## 4. المتطلبات المشتركة (Cross-Cutting — ترثها كل spec)

هذه قواعد المشروع من AGENTS.md/DESIGN.md — **لا تُكرر في كل spec**، لكنها ملزمة لكل تنفيذ وتُفحص في بوابة converge:

### CC-1 الهوية البصرية والتصميم
- النمط **حصراً** من `DESIGN.md` + `src/components/tokens.ts` (SSOT، للقراءة) — لا قيم ألوان/مسافات مخترعة
- **Dark + Light** معاً لكل شاشة · **RTL** بخصائص CSS منطقية فقط (`ms-/me-`, `ps-/pe-`, `start/end`) — الفيزيائية ممنوعة
- الخط: IBM Plex Sans Arabic بمقياس DESIGN.md · **عربية فقط** — نصوص مباشرة في JSX (لا t()، لا ملفات locale)

### CC-2 الحالات الإلزامية لكل شاشة بيانات (State Matrix)
كل query: `isPending` skeleton (لا spinner افتراضي) · `isError` + retry · empty صريح (رسالة عربية + إجراء إن وجد) · unauthorized مخفي · أزرار: pending disable · نماذج: validation summary · عمليات خطرة: confirm dialog · تعارض RowVersion: toast + refetch.

### CC-3 بيانات وأخطاء
- عملاء NSwag المولدة **لا تُحرر أبداً** — `npm run generate-api` بعد أي تغيير endpoint
- كل أخطاء API عبر `shared/api/result-to-ui.ts`: 4xx → خطأ حقل inline (Zod refine) · 5xx → toast · **لا ابتلاع أبداً**
- الرفض الخادمي يُعرض **حرفياً** (verbatim) — لا إعادة صياغة
- الأرقام المالية **خادمية حصراً** — صفر حساب عميل للمجاميع/الأرصدة/الصافي (مجاميع العرض التجميعية البسيطة مسموحة فقط إن وسمت)

### CC-4 أشكال وبيانات
- كل نموذج: Zod schema في `features/<domain>/<entity>/shared/schemas.ts` — نفس المخطط لـ defaultValues + التحقق
- TanStack Query لكل حالة خادم · Zustand للـ UI فقط · query keys من المصنع المشترك + invalidation سليم بعد كل mutation
- lifecycle actions: PATCH/POST بأسلوب العقد + الأزرار **مشروطة بالحالة** (valid-only) + confirm للخطير

### CC-5 الاختبارات (Frontend = Vitest + Testing Library، بلا TDD إلزامي للواجهة*)
*حسب AGENTS.md: الواجهة بلا TDD — لكن **اختبارات الواجهة مطلوبة في هذا الملف** لكل spec (قسم Tests) لأنها تحقق التغطية والعقد. MSW/mock servers **ممنوعة** — الاختبارات على مستوى المكوّن مع mocks لعميل NSwag عبر msw-like stubbing ممنوع أيضاً؛ استخدم اختبار العرض/التفاعل مع بيانات fixture ثابتة (لا طبقة mock شبكة).
> ملاحظة دقيقة: AGENTS.md يمنع mock data layer (MSW/fake APIs) في التطبيق؛ اختبارات المكونات بـ fixtures محلية مقبولة. عند التعارض — اتبع قرار الفريق وسجّله.

### CC-6 الأمان والصلاحيات
- كل عنصر تحكم (زر/حقل/تبويب) مربوط بكود صلاحية — `usePermission` + ثوابت `permissions.ts`
- المسارات `protected: true` في routes.tsx · الحالة الحالية للخادم: placeholder policies (`RequireAssertion(_ => true)`) — **لا تعامل الـ endpoints كمحمية فعلياً** لكن اربط الواجهة بالأكواد استعداداً لتفعيل RBAC

---


# المجموعة: المحاسبة (ACC)

---

## SPEC ACC-03 — إدارة الدفاتر والقوالب (journals-templates)

> **CONTRACT NOTE**: أسماء الحقول والمسارات وأكواد الصلاحيات وقيم الـ enums أدناه **متطلبات ملزمة** من العقد الخلفي الموثق (web-api-client.ts) — ليست تفاصيل تنفيذ. لا تُطرح ولا تُعاد صياغتها أثناء تحقق القوائم؛ إزالتها = فشل تغطية عند converge.

```yaml
id: ACC-03
slug: journals-templates
branch: acc03-journals-templates
group: accounting
priority: P2
depends_on: ["023 (يستهلك المنتقيات)", "FS تسلسلات (sequenceId)"]
blocked_by: []
consumers: [ACC-04, 023, 025]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 3}
```

### User Stories

**US1 (P1) — إدارة الدفاتر.** مسؤول مالي يعرّف كتب اليومية بشروطها (نوع، حساب تعليق، عملة أجنبية، تسلسل ترقيم، شرط اعتماد قبل الترحيل).
- **Given** دفاتر موجودة، **When** يتصفح بفلتر النوع/الحالة، **Then** يرى code/name/type/allowForeignCurrency/requireApprovalBeforePosting/isActive
- **Given** دفتراً جديداً بكود مكرر، **When** يحفظ، **Then** رفض خادمي بالرسالة حرفياً
- **Given** دفتراً مستخدماً في قيود، **When** يعدّل حقلاً حرجاً، **Then** قاعدة خادمية ترفض ويعرض السبب

**US2 (P2) — إدارة القوالب.** محاسب يصنّف أنماط القيود (Standard/Recurring/Adjustment) بدفترها — مصدر ACC-04.
- **Given** قالباً جديداً (templateName/journalId/templateType)، **When** يحفظ، **Then** يُدرج بالقائمة
- **Given** قالب نظامي (isSystemTemplate=true)، **When** يبحث عن حذف، **Then** لا حذف (بلا DELETE endpoint — محجوب شاشةً)

**US3 (P2) — تغذية شاشات القيود.** كاتب حسابات يرى المفعلة فقط في منتقيات 023.
- **Given** دفتراً معطلاً، **When** يفتح منتقي الدفاتر في إنشاء قيد، **Then** غير مدرج

### Edge Cases
كود مكرر (رفض) · دفتر بلا sequenceId (سلوك ترقيم — OQ-N2) · قالب بلا دفتر (رفض — journalId مطلوب) · تعطيل دفتر بقوالب مرتبطة (قاعدة خادمية — تعرض) · isSystemTemplate يمنع التحرير؟ (OQ-N3)

### Functional Requirements
- **FR-001**: إدارة الدفاتر: قائمة بفلاتر (type/isActive) + إنشاء + تعديل — بكل حقول العقد
- **FR-002**: منع الحذف شاشةً (لا DELETE في العقد)
- **FR-003**: إدارة القوالب: قائمة + إنشاء + تعديل (رأس فقط — **القالب بلا بنود**، مؤكد L32764/L29051)
- **FR-004**: المنتقيات (للـ 023) تعرض isActive=true حصراً
- **FR-005**: الحقول الشرطية في النموذج: suspenseAccountId يظهر لأنواع Cash/Bank؛ sequenceId من قائمة تسلسلات FS
- **FR-006**: كل فعل مربوط بصلاحية (جدول Permissions)

### Data Contract

**JournalDto** ✓ (L32414):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | خادمي |
| code | str | ✓ | فريد عالمياً |
| name | str | ✓ | — |
| type | enum JournalType | ✓ | General/Purchase/Sale/Cash/Bank/Adjustment/Closing |
| accountId | int | ○ | حساب الدفتر |
| suspenseAccountId | int | ○ | يظهر لأنواع نقدية |
| allowForeignCurrency | bool | ✓ | toggle |
| sequenceId | int | ○ | تسلسل الترقيم (FS) |
| requireApprovalBeforePosting | bool | ✓ | يفرض مسار 025 |
| isActive | bool | ✓ | — |

**JournalEntryTemplateDto** ✓ (L32764):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| templateName | str | ✓ | — |
| description | str | ○ | — |
| journalId / journalName | int / str | ✓ / — | الدفتر المرتبط |
| templateType | enum | ✓ | Standard/Recurring/Adjustment |
| isSystemTemplate | bool | ✓ | نظامي = بلا حذف |
| isActive | bool | ✓ | — |

**CreateJournalCommand** ✓ (L27868): code, name, type, accountId?, suspenseAccountId?, allowForeignCurrency, sequenceId?, requireApprovalBeforePosting
**CreateTemplateCommand** ✓ (L29051): templateName, description?, journalId, templateType, isSystemTemplate

### Lifecycle
لا دورة حالة — `isActive` عبر Update. (تعطيل = isActive=false)

### Permissions

| Code | Status |
|---|---|
| Accounting.Journals.View/Create/Update | GAP-ADD — تحقق من الصيغة الفعلية في PermissionCodes.cs (عائلة Accounting موجودة) |
| Accounting.Templates.View/Create/Update | GAP-ADD |

### API

| Method | Path | الغرض |
|---|---|---|
| GET | /api/Accounting/Journals | قائمة (فلاتر) |
| POST | /api/Accounting/Journals | إنشاء |
| GET | /api/Accounting/Journals/{id} | تفاصيل |
| PUT | /api/Accounting/Journals/{id} | تعديل |
| GET/POST/GET{id}/PUT{id} | /api/Accounting/Templates | نفس النمط |

### Business Rules
- **BR-1** code فريد — الرفض يعرض حرفياً
- **BR-2** القالب يشير لدفتر واحد؛ تعديل القالب يؤثر على الاستخدام المستقبلي فقط (لا رجعية)
- **BR-3** requireApprovalBeforePosting ينعكس في شاشات 025 (شارة/شرط)
- **BR-4** منتقيات المفعلة فقط — قاعدة عرض صارمة

### UI/UX
- `/acc/journals`: جدول + فلاتر + نافذة إنشاء/تعديل (أقسام: الهوية، النوع، الحسابات الشرطية، الضوابط)
- `/acc/templates`: جدول + نافذة (اسم/وصف/دفتر/نوع/نظام toggle)
- الحالات: حسب CC-2 (لا زيادات خاصة)

### Success Criteria
- **SC-001**: إنشاء دفتر أي نوع ×7 في <60 ثانية
- **SC-002**: 100% من المفعلة تظهر في منتقي 023 (اختبار تكاملي مع 023)
- **SC-003**: صفر نجاح لتعديل محجوب خادمياً (كل رفض يعرض سببه)

### Tests
- **T1** (FR-001/US1): CRUD دفاتر + تكرار كود مرفوض
- **T2** (FR-003/US2): CRUD قوالب + نظامي بلا حذف
- **T3** (FR-004/US3): منتقي يعرض المفعلة فقط
- **T4** (FR-005): الحقول الشرطية تظهر/تختفي حسب النوع
- **T5** (CC-6): حجب غير المصرح

### Open Questions
- **OQ-N1** (Engineering): الصيغة الحرفية لأكواد Journals/Templates في PermissionCodes.cs
- **OQ-N2** (Engineering): سلوك الترقيم عند sequenceId غائب (افتراضي عام؟)
- **OQ-N3** (User): هل القالب النظامي قابل للتعديل أم للقراءة؟

### Out of Scope
إنشاء/تعديل القيود (023) · دورة حياة القيد (025) · الجدولة الدورية (ACC-04 — يستهلك القوالب)

---

## SPEC ACC-04 — القيود الدورية (recurring-entries)

> **CONTRACT NOTE**: كما في ACC-03 — العقد أدناه ملزم حرفياً.

```yaml
id: ACC-04
slug: recurring-entries
branch: acc04-recurring-entries
group: accounting
priority: P1
depends_on: [ACC-03 (القالب), 023/025 (القيد المولد), SYS-03 (محرك التوليد)]
blocked_by: []
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 3}
```

### User Stories

**US1 (P1) — إنشاء جدول دوري.** محاسب يجدول قيداً متكرراً (إيجار/رواتب/استهلاك) من قالب.
- **Given** قالباً Recurring ودفتر، **When** ينشئ جدولاً (name/frequency/startDate + amount? + أبعاد?)، **Then** Active وnextExecutionDate محسوب
- **Given** endDate قبل startDate، **When** يحفظ، **Then** رفض تحقق

**US2 (P1) — التحكم بالجدولة.** إيقاف مؤقت/استئناف/إلغاء نهائي — كل قرار مسجل بسببه.
- **Given** جدولاً Active، **When** يوقفه بسبب، **Then** Paused (لا توليد)
- **Given** جدولاً Paused، **When** يستأنفه، **Then** Active مع nextExecutionDate محفوظ
- **Given** جدولاً ملغى، **When** يعرض التفاصيل، **Then** زر resume مخفي (cancel نهائي)

**US3 (P2) — المراقبة.** رؤية آخر تنفيذ والقيد الناتج.
- **Given** جدولاً ولّد قيداً، **When** يفتح التفاصيل، **Then** lastExecutedAt + رابط generatedJournalEntryId يفتح 023
- **Given** بلا توليد بعد، **When** يفتح التفاصيل، **Then** فارغ صريح

### Edge Cases
pause على Paused (رفض خادمي) · resume على Active (رفض) · بلوغ endDate → Completed تلقائياً · جدولة في فترة مغلقة FR-020 (رفض خادمي — يعرض حرفياً) · cancel ثم محاولة resume · جدول بلا amount وبلا مبلغ بالقالب (مصدر المبلغ — OQ-N3)

### Functional Requirements
- **FR-001**: قائمة بفلاتر (status/frequency) + أعمدة (entryNumber/name/frequency/nextExecutionDate/status)
- **FR-002**: إنشاء من CreateRecurringEntryCommand — **لا Update endpoint** (نمط إعادة الإنشاء)
- **FR-003**: nextExecutionDate ظاهر دائماً في القائمة والتفاصيل
- **FR-004**: pause/resume/cancel بأزرار مشروطة بالحالة (valid-only) + confirm + سبب حيث يتطلب الأمر
- **FR-005**: رابط القيد المولد (generatedJournalEntryId) يفتح شاشة 023/025
- **FR-006**: Completed عند بلوغ endDate — شارة نهائية

### Data Contract

**RecurringEntryDto** ✓ (L35831):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| entryNumber | str | ✓ | خادمي (ترقيم) |
| templateId / templateName | int? / str? | ○ | مصدر الخطوط (القالب) |
| journalId / journalName | int / str | ✓ | الدفتر |
| name | str | ✓ | — |
| frequency | enum RecurringFrequency | ✓ | **Weekly/Monthly/Quarterly/Yearly** |
| startDate | date | ✓ | — |
| endDate | date | ○ | بلوغه → Completed |
| nextExecutionDate | date | ✓ | محسوب خادمياً |
| lastExecutedAt | datetime | ○ | — |
| amount | dec | ○ | مبلغ الجدول |
| currencyId / fundId / costCenterId / projectId | int | ○ | أبعاد |
| descriptionTemplate | str | ○ | قالب نص الوصف |
| status | enum RecurringEntryStatus | ✓ | **Active/Paused/Completed** (بلا Cancelled!) |
| generatedJournalEntryId | int | ○ | آخر قيد مولد (واحد — لا قائمة) |
| isActive | bool | ✓ | — |

**CreateRecurringEntryCommand** ✓ (L28856): templateId?, journalId, name, frequency, startDate, endDate?, amount?, currencyId?, fundId?, costCenterId?, projectId?, descriptionTemplate?
**PauseRecurringEntryCommand** ✓ (L34143): `{id, reason?, rowVersion}` · **ResumeRecurringEntryCommand** ✓ (L36499): `{id, rowVersion}` · **CancelRecurringEntryCommand** ✓ (L24627): `{id, reason?, rowVersion}`

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Active | pause | Paused | reason? — أمر بـ rowVersion |
| Paused | resume | Active | nextExecution محفوظ |
| Active/Paused | cancel | (نهاية — OQ-N1) | reason? |
| Active | بلوغ endDate | Completed | خادمي |

### Permissions

| Code | Status |
|---|---|
| Accounting.RecurringEntries.View/Create | GAP-ADD — العائلة Accounting موجودة (JournalEntries/Templates/…) |
| Accounting.RecurringEntries.Pause/Resume/Cancel (أو ما يعادلها) | GAP-ADD — تحقق من الصيغة الفعلية |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Accounting/RecurringEntries |
| GET | /api/Accounting/RecurringEntries/{id} |
| POST | /{id}/pause · /{id}/resume · /{id}/cancel |

### Business Rules
- **BR-1** التوليد من templateId — لا بنود يدوية في الجدول
- **BR-2** cancel نهائي (resume مخفي بعده)
- **BR-3** فترة مغلقة (FR-020) تحجب الجدولة/التوليد — الرفض حرفي
- **BR-4** كل تحكم بسبب حيث يتطلب الأمر (pause/cancel)

### UI/UX
- `/acc/recurring`: جدول + فلاتر
- `/acc/recurring/create`: نموذج (قالب picker من ACC-03 + دفتر + دورية + مدى + مبلغ + أبعاد + قالب وصف)
- `/acc/recurring/{id}`: بطاقة حالة (next/last) + أزرار مشروطة + قسم "آخر قيد مولد" برابط
- الحالات: CC-2 + حالة Completed (شارة نهائية، أزرار مخفية)

### Success Criteria
- **SC-001**: إنشاء جدول أي دورية ×4 في <60 ثانية
- **SC-002**: 100% من الأفعال مقيدة بالحالة الصحيحة (زر غير صالح = مخفي/معطل)
- **SC-003**: كل إيقاف/إلغاء مسجل بسببه عند الطلب

### Tests
- **T1** (FR-002/US1): إنشاء + nextExecutionDate ظاهر
- **T2** (FR-004/US2): pause/resume/cancel بالشروط + resume مخفي بعد cancel
- **T3** (FR-005/US3): رابط القيد المولد
- **T4** (FR-006): Completed عند endDate
- **T5** (CC-2): فارغ صريح + RowVersion conflict toast

### Open Questions
- **OQ-N1** (Engineering): حالة ما بعد cancel فعلياً (enum بلا Cancelled — isActive=false؟) — حدد العرض
- **OQ-N2** (Engineering): محرك التوليد — وظيفة BackgroundJobs؟ (ينسق مع SYS-03)
- **OQ-N3** (User): التوليد بلا amount وبلا مبلغ قالب — من أين المبلغ؟

### Out of Scope
محرك التوليد الخلفي (SYS-03) · تحرير القيد المولد (025) · إدارة القوالب (ACC-03)

---

## SPEC ACC-05 — مراقبة المحاسبة (accounting-monitoring)

> **CONTRACT NOTE**: كما في ACC-03 — العقد أدناه ملزم حرفياً.

```yaml
id: ACC-05
slug: accounting-monitoring
branch: acc05-accounting-monitoring
group: accounting
priority: P1
depends_on: [023/025 (القيود), FS (فترات), CTRL-02 (تفاعل إسقاط)]
blocked_by: []
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 3}
```

### User Stories

**US1 (P1) — الأرصدة وإغلاق الفترة.** كنترولر يرى أرصدة كل حساب لكل (سنة، فترة، عملة) بحالة إغلاقها — ويشغّل FR-020.
- **Given** أرصدة فترة، **When** يتصفح بفلاتر (سنة/فترة/حساب/عملة)، **Then** 6 أعمدة (opening/debit/closing × debit/credit) + balanceDirection + isFinalized
- **Given** فترة مفتوحة، **When** يغلقها (تأكيد + تحذير FR-020: "القيود العكسية تصبح محظورة")، **Then** isFinalized=true + finalizedAt
- **Given** فترة مغلقة، **When** يعيد فتحها (تأكيد)، **Then** isFinalized=false

**US2 (P2) — التوافق وإعادة البناء.** كشف الانحراف بين المُجمَّع والمحسوب، وإصلاحه.
- **Given** فترة، **When** يشغّل التوافق، **Then** نتيجة (isBalanced/totalAccountsChecked/discrepancyCount) + جدول الفروق لكل عملة
- **Given** أرصدة منحرفة، **When** يشغّل إعادة البناء (تأكيد خطير)، **Then** إعادة حساب من سطور القيود

**US3 (P2) — طابور الأحداث.** شفافية خط أنابيب الترحيل.
- **Given** أحداثاً معلقة/فاشلة، **When** يفتح الطابور، **Then** (eventType/sourceTable/sourceId/status + errorMessage + retryCount + journalEntryId رابطاً) — **قراءة فقط**

**US4 (P2) — قواعد الترحيل.** إدارة قواعد الحدث→الدفتر ببنودها.
- **Given** قاعدة، **When** ينشئ/يعدل/يحذف (تأكيد)، **Then** الرأس + البنود (sequence/accountSource/debitOrCredit/amountSource/أبعاد مطلوبة) محفوظة

### Edge Cases
إغلاق فترة بأحداث pending (قاعدة خادمية — تعرض) · فروق متعددة العملات (جدول لكل عملة) · حذف قاعدة مستخدمة (رفض خادمي) · بند قاعدة بلا accountSource ولا fixedAccountId (رفض تحقق) · rebuild أثناء حركة متزامنة (RowVersion/خادمي) · finalize ثم unfinalize متتاليان

### Functional Requirements
- **FR-001**: جدول أرصدة بـ6 أعمدة + اتجاه + شارة isFinalized — **خادمية حصراً** (CC-3)
- **FR-002**: finalize/unfinalize بـ `{fiscalYearId, fiscalPeriodId}` + تحذير FR-020 إلزامي في الحوار
- **FR-003**: التوافق يعيد لوحة نتيجة + جدول فروق (مدين/دائن × materialized/calculated + differences + discrepancyType)
- **FR-004**: rebuild بتأكيد خطير (نص العواقب)
- **FR-005**: طابور الأحداث **بلا أي أزرار تحرير** — عرض errorMessage/retryCount + رابط القيد الناتج
- **FR-006**: CRUD قواعد الترحيل برؤوسها وبنودها (شبكة بنود قابلة للإضافة/الحذف بالترتيب sequence)

### Data Contract

**AccountBalanceDto** ✓ (L20032):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| accountId / accountCode / accountName | int / str / str | ✓ | — |
| fiscalYearId / fiscalYearName | int / str | ✓ | — |
| fiscalPeriodId / periodName | int / str | ✓ | — |
| periodStartDate / periodEndDate | date | ✓ | — |
| currencyId / currencyCode | int / str | ✓ | تعدد عملات |
| openingDebit / openingCredit | dec | ✓ | افتتاحي |
| debit / credit | dec | ✓ | دوران الفترة |
| closingDebit / closingCredit | dec | ✓ | ختامي |
| balanceDirection | str | ✓ | اتجاه الرصيد |
| isFinalized / finalizedAt | bool / date? | ✓ | حالة الإغلاق داخل الصف |

**AccountingEventDto** ✓ (L20488): id, eventType, sourceTable, sourceId, status, journalEntryId?, errorMessage?, processedAt?, retryCount

**PostingRuleDto** ✓ (L34933): id, name*, eventType*, journalId*, journalName, priority, isActive
**PostingRuleLineDto** ✓ (L35008): sequence, accountSource, fixedAccountId?, debitOrCredit, amountSource, fundDimensionRequired, costCenterDimensionRequired, projectDimensionRequired

**FinalizePeriodCommand / UnfinalizePeriodCommand** ✓ (L31350/L38278): `{fiscalYearId, fiscalPeriodId}`
**ReconciliationResultDto** ✓ (L35545): fiscalYearId, fiscalPeriodId, discrepancies[], isBalanced, totalAccountsChecked, discrepancyCount
**ReconciliationDiscrepancyDto** ✓ (L35448): accountId, accountCode, accountName, currencyId, currencyCode, materializedDebit, calculatedDebit, materializedCredit, calculatedCredit, debitDifference, creditDifference, discrepancyType

### Lifecycle
Period: `Open → Finalized → Open` (إغلاق/فتح) · Rule: isActive + حذف · Event: لا تحولات (خادمية)

### Permissions

| Code | Status |
|---|---|
| Accounting.Balances.View | GAP-ADD (العائلة Accounting موجودة) |
| Accounting.Balances.Rebuild/Finalize/Unfinalize | GAP-ADD |
| Accounting.AccountingEvents.View | GAP-ADD |
| Accounting.PostingRules.View/Create/Update/Delete | GAP-ADD |

### API

| Method | Path |
|---|---|
| GET | /api/Accounting/AccountingBalances |
| GET | /api/Accounting/AccountingBalances/reconcile |
| POST | /api/Accounting/AccountingBalances/rebuild |
| POST | /api/Accounting/AccountingBalances/finalize · /unfinalize |
| GET | /api/Accounting/AccountingEvents/pending |
| GET/POST/PUT/DELETE | /api/Accounting/PostingRules (+/{id}) |

### Business Rules
- **BR-1** FR-020: إغلاق الفترة يحظر القيود العكسية — نص التحذير إلزامي في حوار الإغلاق
- **BR-2** الأرصدة والفروق خادمية حصراً — صفر حساب عميل (CC-3)
- **BR-3** الأحداث قراءة فقط — لا إعادة محاولة من هذه الشاشة (إعادة المعالجة ملك خط الأنابيب/SYS-03)
- **BR-4** التوافق لكل عملة (currencyId في كل سطر فرق)

### UI/UX
- `/acc/balances`: جدول 6 أعمدة + فلاتر + شارة Finalized · حوارا إغلاق/فتح (تحذير FR-020 نصاً) · زر توافق → لوحة نتيجة (متوازن/عدد/فروق) · زر rebuild (تأكيد خطير)
- `/acc/events`: جدول بلا أزرار (نوع/مصدر/حالة/محاولات/خطأ/رابط)
- `/acc/posting-rules`: جدول + نافذة (رأس + شبكة بنود)
- الحالات: CC-2 + لوحة نتيجة التوافق (حالتي متوازن/فروق)

### Success Criteria
- **SC-001**: صفر حساب عميل للأرصدة (مراجعة كود)
- **SC-002**: 100% من الإغلاقات بتحذير FR-020 ظاهر (اختبار)
- **SC-003**: كل حدث فاشل يعرض سببه حرفياً

### Tests
- **T1** (FR-001): الأعمدة الستة + الاتجاه + الفلاتر
- **T2** (FR-002): إغلاق/فتح + نص التحذير + isFinalized
- **T3** (FR-003): نتيجة التوافق + جدول الفروق متعدد العملات
- **T4** (FR-004): rebuild بتأكيد
- **T5** (FR-005): الأحداث بلا أزرار + رابط القيد
- **T6** (FR-006): قواعد CRUD + شبكة البنود + حذف بتأكيد

### Open Questions
- **OQ-N1** (Engineering): قيم status الأحداث (حرّة string — وثّق القيم الفعلية للشارات)
- **OQ-N2** (Engineering): قيم accountSource/amountSource/debitOrCredit (enum خادمي؟ — لمنتقيات النموذج)
- **OQ-N3** (Engineering): جسم RebuildAccountBalancesCommand (L35206 — GAP-READ: فترة محددة أم كل شيء؟)

### Out of Scope
القيود (023/025) · إسقاط السنة (CTRL-02) · الحسابات النهائية (CTRL-03) · ميزان المراجعة (RPT-05)


---

# المجموعة: الخزينة (TRE)

---

## SPEC TRE-01 — سندات القبض (receipt-vouchers)

> **CONTRACT NOTE**: أسماء الحقول والمسارات وأكواد الصلاحيات وقيم الـ enums أدناه **متطلبات ملزمة** من العقد الخلفي الموثق — ليست تفاصيل تنفيذ. لا تُطرح ولا تُعاد صياغتها أثناء تحقق القوائم.

```yaml
id: TRE-01
slug: receipt-vouchers
branch: tre01-receipt-vouchers
group: treasury
priority: P1
depends_on: ["022 (جهات + لوحات)", "FS (تسلسل DSL)", "ACC (حسابات إيراد)"]
blocked_by: []
consumers: [TRE-02, TRE-03, RPT-02, CTRL-03]
permissions_status: GAP-ADD (Revenue family absent)
oq: {blocking: 0, non_blocking: 2}
```

### User Stories

**US1 (P1) — إنشاء سند قبض.** كاشير يصدر سنداً (نقدية أو شيكات) لجهة ببنود إيراد — الرقم فوري عند Draft.
- **Given** جهة وبنود إيراد، **When** ينشئ سنداً (voucherDate/partyId/paymentMethod/lines)، **Then** Draft وvoucherNumber (DSL-{D6}) معروض فوراً وtotalAmount خادمي
- **Given** paymentMethod=Check، **When** يختارها، **Then** شبكة الشيكات تظهر (bankName/checkNumber/checkDate/amount لكل شيك) وتصبح مطلوبة
- **Given** سنداً بلا بنود، **When** يحاول الإرسال، **Then** رفض

**US2 (P1) — المراجعة والاعتماد.** لا إيراد معترف به دون مراجعة موثقة.
- **Given** سنداً Draft، **When** يرسله، **Then** PendingReview + submittedById/At
- **Given** سنداً PendingReview، **When** يعتمده مراجع (reason اختياري)، **Then** Approved + reviewedById/At
- **Given** سنداً Draft/PendingReview، **When** يلغيه (reason **مطلوب**)، **Then** Cancelled + cancellationReason (نهائي)

**US3 (P2) — التصفح والربط.** إيجاد السندات ومتابعة إيداعها.
- **Given** سندات فترة، **When** يفلتر (party/period/status/method)، **Then** القائمة تتبع
- **Given** سنداً مودعاً، **When** يعرض التفاصيل، **Then** رابط depositSlipNumber يفتح TRE-02

### Edge Cases
Σشيكات ≠ totalAmount (قاعدة — OQ-N1) · جهة معطلة (المنتقي يستثني) · إلغاء بعد الانضمام لبطاقة (رفض خادمي حرفي) · تعارض RowVersion (toast+refetch) · فجوات أرقام (قابلة للتدقيق — لا معالجة شاشة) · سند Check بلا شيكات (رفض)

### Functional Requirements
- **FR-001**: الرقم DSL-{D6} يصدر عند إنشاء Draft ويعرض فوراً (لا انتظار)
- **FR-002**: بنود إيراد متعددة (revenueAccountId/amount/description) + **شيكات متعددة** (checks[])
- **FR-003**: حقول الشيك مشروطة بـ paymentMethod=Check (إظهار/إخفاء + طلب)
- **FR-004**: totalAmount خادمي (sum lines) — عرض حي للنموذج مسموح كمعاينة فقط (CC-3)
- **FR-005**: POST-lifecycle فقط: submit/approve/cancel — **بلا update/delete** (محجوب شاشةً)
- **FR-006**: الإلغاء بـ reason مطلوب (تحقق نموذج)
- **FR-007**: تفاصيل تعرض: المراجع/الوقت، البطاقة المرتبطة، لوحات 022 (ApprovalHistory/DocumentStatusLog)

### Data Contract

**ReceiptVoucherDto** ✓ (L35346):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| voucherNumber | str | ✓ | DSL-{D6} عند الإنشاء |
| voucherDate | date | ✓ | ≤ اليوم (ASSUMED — تحقق) |
| partyId / partyName | int / str | ✓ | منتقي 022 (نشط فقط) |
| paymentMethod | enum PaymentMethod | ✓ | **Cash/Check فقط** (L34315) |
| paymentMethodName | str | — | عرض |
| receivedFrom | str | ○ | — |
| notes | str | ○ | — |
| depositSlipId / depositSlipNumber | int? / str? | ○ | رابط البطاقة |
| status | enum ReceiptVoucherStatus | ✓ | **Draft/PendingReview/Approved/Cancelled** |
| statusName | str | — | عرض |
| totalAmount | dec | ✓ | خادمي |
| submittedById / submittedAt | int? / dt? | ○ | — |
| reviewedById / reviewedAt | int? / dt? | ○ | هوية المراجع |
| cancellationReason | str | ○ | — |
| lines[] | array | ✓ | ≥1 |
| checks[] | array | ○ | مطلوبة عند Check |
| rowVersion | str | ✓ | تفاؤل |
| created/createdBy/lastModified/lastModifiedBy | audit | ✓ | — |

**ReceiptVoucherLineDto** ✓ (L35377): revenueAccountId*, amount* (>0), description?
**CreateCheckDto** ✓ (L26924): bankName*, checkNumber*, checkDate*, amount*
**CreateReceiptVoucherCommand** ✓ (L28713): voucherDate, partyId, paymentMethod, receivedFrom?, notes?, lines[], checks[]
**Submit** ✓ (L37766): `{id, rowVersion}` · **Approve** ✓ (L21676): `{id, reason?, rowVersion}` · **Cancel** ✓ (L24619): `{id, reason*, rowVersion}`

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Draft | submit | PendingReview | ≥1 بند |
| PendingReview | approve | Approved | مراجع (هوية خادمية) + reason? |
| Draft | cancel | Cancelled | reason* |
| PendingReview | cancel | Cancelled | reason* (ASSUMED — تحقق) |

### Permissions

| Code | Status |
|---|---|
| Revenue.ReceiptVouchers.View/Create/Submit/Approve/Cancel | GAP-ADD — عائلة Revenue غائبة عن PermissionCodes.cs؛ أضف وفق صيغة {Module}.{Action} وسجل الإضافة في نفس المهمة |

### API

| Method | Path |
|---|---|
| GET | /api/Revenue/ReceiptVouchers · /by-party/{partyId} · /by-period |
| GET | /api/Revenue/ReceiptVouchers/{id} |
| POST | /api/Revenue/ReceiptVouchers |
| POST | /{id}/submit · /{id}/approve · /{id}/cancel |

### Business Rules
- **BR-1** الرقم عند Draft — فجوات قابلة للتدقيق (لا إعادة استخدام)
- **BR-2** الاعتماد يسجل reviewedById/At — هوية المراجع ظاهرة
- **BR-3** method=Check ⇒ checks[] ≥1 بحقولها الأربعة
- **BR-4** الإلغاء reason إلزامي · الاعتماد reason اختياري
- **BR-5** كل قرار يظهر في لوحات 022 — لا أعمدة موافقة مضمنة

### UI/UX
- `/treasury/receipt-vouchers`: جدول (رقم/تاريخ/جهة/طريقة/إجمالي/حالة/بطاقة) + فلاتر (party/period/status/method)
- `/treasury/receipt-vouchers/create`: منتقي جهة (022) + toggle طريقة + شبكة بنود + شبكة شيكات شرطية + معاينة إجمالي
- `/treasury/receipt-vouchers/{id}`: رأس + بنود + شيكات + لوحات 022 + رابط البطاقة + أزرار مشروطة
- حوار إلغاء (reason إلزامي) · الحالات: CC-2

### Success Criteria
- **SC-001**: إنشاء سند كامل (بنود+شيكات) <دقيقتين
- **SC-002**: كل معتمد يعرض مراجعه ووقته
- **SC-003**: 100% من الإلغاءات بسبب

### Tests
- **T1** (FR-001/US1): إنشاء → رقم فوري
- **T2** (FR-003): toggle الطريقة يظهر/يخفي شبكة الشيكات + الطلب
- **T3** (FR-002): بلا بنود → رفض الإرسال
- **T4** (FR-005/US2): دورة submit→approve (هوية) وcancel (سبب)
- **T5** (FR-007/US3): الفلاتر + رابط البطاقة + لوحات 022
- **T6** (CC-2/CC-6): RowVersion toast + حجب غير المصرح

### Open Questions
- **OQ-N1** (Engineering): هل Σchecks ≤ totalAmount مفروضة خادمياً؟
- **OQ-N2** (User): receivedFrom مقابل partyName — متى يُطلب كلاهما؟

### Out of Scope
البطاقات (TRE-02) · تصفية الشيكات (TRE-03) · شاشات الترحيل (023/ACC-05)

---

## SPEC TRE-02 — بطاقات الإيداع (deposit-slips)

> **CONTRACT NOTE**: كما في TRE-01 — العقد أدناه ملزم حرفياً.

```yaml
id: TRE-02
slug: deposit-slips
branch: tre02-deposit-slips
group: treasury
priority: P1
depends_on: [TRE-01 (السندات المعتمدة), TRE-03 (شيكات 48), FS (تسلسل)]
blocked_by: []
consumers: [TRE-03 (كشف شهري), RPT-02, CTRL-03]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
```

### User Stories

**US1 (P1) — إنشاء بطاقة متجانسة.** كاشير يجمع سندات اليوم المعتمدة في بطاقة واحدة بنوعها القانوني.
- **Given** سندات معتمدة نقدية، **When** ينشئ Form47 بـ voucherIds دفعة، **Then** Draft وslipNumber (DSL-{D6}) وtotalAmount خادمي
- **Given** منتقي الأعضاء لبطاقة 47، **When** يبحث عن سند شيكات، **Then** غير مدرج (تجانس مفلتر مسبقاً)
- **Given** سنداً مخالفاً (محاولة إضافة مباشرة)، **When** يضيفه، **Then** رفض خادمي برسالة التجانس حرفياً

**US2 (P1) — إدارة الأعضاء (Draft فقط).** تصحيح الدفعة قبل الاعتماد.
- **Given** بطاقة Draft، **When** يزيل عضواً (reason اختياري)، **Then** يُزال والإجمالي يتحدث
- **Given** بطاقة Approved، **When** يعرض التفاصيل، **Then** أزرار الإضافة/الإزالة مخفية

**US3 (P1) — الاعتماد وأثره.** الاعتماد = الاعتراف للإيراد النقدي، ونقل الشيكات للتحصيل.
- **Given** بطاقة 47 بأعضاء، **When** يعتمدها مدير الخزينة (تأكيد + reason?)، **Then** Approved + approvedById/At + ترحيل إيراد
- **Given** بطاقة 48، **When** تُعتمد، **Then** شيكاتها UnderCollection (TRE-03)
- **Given** بطاقة بلا أعضاء، **When** يحاول الاعتماد، **Then** منع

**US4 (P2) — الكشف الشهري.** تسوية الشهر مع الخزينة المركزية.
- **Given** شهراً وصندوقاً، **When** يفتح الكشف، **Then** summary بستة أرقام + vouchers[] + clearings[]
- **Given** شهراً بلا حركات، **When** يفتح الكشف، **Then** أصفار صريحة

### Edge Cases
slipDate < أحدث تاريخ عضو (رفض خادمي) · slipDate مستقبلي (رفض) · عضو سندُه أُلغي بعد الانضمام (قاعدة خادمية — تعرض حرفياً) · إزالة بعد الاعتماد (محجوبة + رفض خادمي) · تعارض RowVersion · كشف بصندوق بلا حركات

### Functional Requirements
- **FR-001**: التجانس الصارم: Form47 أعضاؤه paymentMethod=Cash حصراً، Form48 أعضاؤه Check حصراً — فلتر مسبق في المنتقي + تحقق خادمي
- **FR-002**: تصحيح slipDate: ≥ أحدث voucherDate عضو و≤ اليوم
- **FR-003**: add-voucher/remove-voucher في Draft فقط (الإزالة بـ reason?)
- **FR-004**: الاعتماد بـ confirm — الأثر: 47 ترحيل إيراد، 48 نقل شيكات
- **FR-005**: منع الاعتماد بلا أعضاء
- **FR-006**: الكشف الشهري: منتقي (year/month/fundId) → summary ستّي + vouchers + clearings

### Data Contract

**DepositSlipDto** ✓ (L29632):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| slipNumber | str | ✓ | DSL-{D6} عند الإنشاء |
| slipDate | date | ✓ | ≥ أحدث عضو، ≤ اليوم |
| formType | enum FormType | ✓ | **Form47/Form48** (L31509) |
| formTypeName | str | — | عرض |
| status | enum DepositSlipStatus | ✓ | **Draft/Approved** |
| statusName | str | — | عرض |
| totalAmount | dec | ✓ | خادمي |
| approvedById / approvedAt | int? / dt? | ○ | — |
| receiptVouchers[] | array | — | كائنات كاملة (وليس معرفات) |
| rowVersion + audit | — | ✓ | — |

**CreateDepositSlipCommand** ✓ (L27269): `{slipDate, formType, voucherIds[]}`
**AddVoucherToSlipCommand** ✓ (L20877): `{slipId, voucherId, rowVersion}`
**RemoveVoucherFromSlipCommand** ✓ (L36125): `{slipId, voucherId, reason?, rowVersion}`
**ApproveDepositSlipCommand** ✓ (L21468): `{id, reason?, rowVersion}`

**MonthlyStatementDto** ✓ (L33412): year, month, fundId, fundName, generatedAt, summary{totalCashCollections, totalCheckCollections, totalDeposited, totalUnderCollection, totalCleared, totalBounced}, vouchers[], clearings[]
**CheckClearingDto** ✓ (L24994): checkNumber, bankName, clearedAt, amount

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Draft | add-voucher | Draft | تجانس + تاريخ |
| Draft | remove-voucher | Draft | reason? |
| Draft | approve | Approved | ≥1 عضو + تأكيد · 47→ترحيل، 48→شيكات UnderCollection |
| Approved | — | (نهاية) | بلا إلغاء/حذف في العقد |

### Permissions

| Code | Status |
|---|---|
| Revenue.DepositSlips.View/Create/Approve | GAP-ADD |
| Revenue.DepositSlips.Update (عمليات الأعضاء) | GAP-ADD — أو ضمن Create/Approve (تحقق) |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Revenue/DepositSlips |
| GET | /{id} · /monthly-statement |
| POST | /{id}/add-voucher · /{id}/remove-voucher · /{id}/approve |

### Business Rules
- **BR-1** التجانس قبل أي إضافة (فلتر مسبق + خادمي)
- **BR-2** تصحيح التاريخ إلزامي
- **BR-3** الاعتماد نقطة الاعتراف: 47 إيراد فوري، 48 تحت التحصيل
- **BR-4** الأعضاء يعرضون ككائنات كاملة (رقم/تاريخ/مبلغ/جهة)

### UI/UX
- `/treasury/deposit-slips`: جدول (رقم/تاريخ/نوع/حالة/إجمالي) + فلاتر (نوع/حالة/فترة)
- `/treasury/deposit-slips/create`: نوع (47/48 radios) + تاريخ + منتقي أعضاء (مفلتر تلقائياً بالتجانس والحالة Approved وبلا بطاقة)
- `/treasury/deposit-slips/{id}`: رأس + جدول أعضاء (إزالة بسبب في Draft) + إجمالي + اعتماد (تأكيد) + لوحات 022
- `/treasury/monthly-statement`: منتقي (شهر/سنة/صندوق) + 6 بطاقات ملخص + جدولا vouchers/clearings
- الحالات: CC-2

### Success Criteria
- **SC-001**: تجميع سندات يوم كامل <5 دقائق
- **SC-002**: صفر بطاقة مختلطة النوع
- **SC-003**: كل اعتماد 47 يقترن بأثر ترحيل مرئي (رابط الحدث/القيد)

### Tests
- **T1** (FR-001): التجانس — منتقي 47 بلا شيكات + رفض خادمي للمخالف
- **T2** (FR-002): تصحيح التاريخ
- **T3** (FR-003): إضافة/إزالة Draft فقط + الإزالة بسبب
- **T4** (FR-004/US3): اعتماد 47 و48 بأثرهما
- **T5** (FR-005): بلا أعضاء → منع
- **T6** (FR-006/US4): الكشف بملخصه الست + الفارغ أصفار

### Open Questions
- **OQ-N1** (Engineering): هل الإزالة بعد الاعتماد مرفوضة خادمياً برسالة (تحقق نص الرسالة)؟
- **OQ-N2** (User): البطاقة للصندوق النقدي الواحد أم لكل صندوق (fundId في الكشف فقط — من أين يُستنتج صندوق البطاقة)؟

### Out of Scope
السندات (TRE-01) · تصفية الشيكات (TRE-03)

---

## SPEC TRE-03 — الشيكات والكشف الشهري (checks-statements)

> **CONTRACT NOTE**: كما في TRE-01 — العقد أدناه ملزم حرفياً.

```yaml
id: TRE-03
slug: checks-statements
branch: tre03-checks-statements
group: treasury
priority: P1
depends_on: [TRE-01 (مصدر الشيكات), TRE-02 (48 تنقلها + الكشف)]
blocked_by: []
consumers: [RPT-02, MonthlyStatement]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
contract_gap: "لا endpoint قائمة شيكات مستقل — القائمة تُبنى من سندات الفترة بطريقة Check"
```

### User Stories

**US1 (P1) — قائمة تحت التحصيل.** الكاشير يعرف ماذا ينتظر البنوك.
- **Given** سندات Check للفترة، **When** يفتح قائمة الشيكات، **Then** كل شيك UnderCollection (رقم/بنك/تاريخ/مبلغ/سند رابط)
- **Given** شيكاً Cleared، **When** يفلتر تحت التحصيل، **Then** غير مدرج

**US2 (P1) — التصفية.** لحظة الاعتراف بالإيراد للشيكات.
- **Given** شيكاً UnderCollection، **When** يصفّيه (تأكيد + clearedAt)، **Then** Cleared + ترحيل
- **Given** شيكاً Bounced، **When** يحاول تصفيته، **Then** رفض خادمي حرفي

**US3 (P1) — الارتداد والاستبدال.** المرتد يعيد فتح السند للاستبدال — بلا خسارة تحصيل.
- **Given** شيكاً UnderCollection، **When** يرتّده (bouncedAt + reason?)، **Then** Bounced والسند قابل للاستبدال
- **Given** شيكاً Bounced، **When** يستبدله بـ Cash، **Then** سند بديل مرتبط (replacementVoucherId)
- **Given** شيكاً Bounced، **When** يستبدله بـ Check، **Then** checkDetails (CreateCheckDto) مطلوبة
- **Given** شيكاً مستبدلاً (replacementVoucherId موجود)، **When** يحاول استبدالاً ثانياً، **Then** رفض

### Edge Cases
تصفية بعد ارتداد (رفض) · ارتداد بعد تصفية (رفض) · استبدال مزدوج (رفض) · السند المرتبط أُلغي · تعارض RowVersion · شهر بلا تصفيات (فارغ صريح) · شيك بلا سند ظاهر (تكامل بيانات)

### Functional Requirements
- **FR-001**: قائمة تحت التحصيل تُبنى من سندات الفترة بطريقة Check (checks[] ضمنها) — **لا endpoint مستقل** (موثق)
- **FR-002**: clear بـ `{id, clearedAt, rowVersion}` + تأكيد
- **FR-003**: bounce بـ `{id, bouncedAt, reason?, rowVersion}` + تأكيد
- **FR-004**: replace بـ `{checkId, paymentMethod, checkDetails?, rowVersion}` — حوار الطريقة + حقول شيك جديد عند Check
- **FR-005**: منع الاستبدال المزدوج (replacementVoucherId ⇒ محجوب)
- **FR-006**: التحقق من الحالة قبل كل فعل (refetch عند الشك) — الحالة لا تكذب

### Data Contract

**CheckDto** ✓ (L25075):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| receiptVoucherId | int | ✓ | السند الأب |
| bankName / checkNumber / checkDate / amount | str / str / date / dec | ✓ | — |
| status | enum CheckStatus | ✓ | **UnderCollection/Cleared/Bounced** (L25167) |
| statusName | str | — | عرض |
| clearedAt / bouncedAt | dt? | ○ | — |
| replacementVoucherId | int? | ○ | وجوده ⇒ استُبدل |
| created | dt | ✓ | — |

**ClearCheckCommand** ✓ (L25173): `{id, clearedAt, rowVersion}`
**BounceCheckCommand** ✓ (L23327): `{id, bouncedAt, reason?, rowVersion}`
**ReplaceCheckCommand** ✓ (L36293): `{checkId, paymentMethod, checkDetails?: CreateCheckDto, rowVersion}`
**CheckClearingDto** ✓ (L24994): checkNumber, bankName, clearedAt, amount (للكشف)
**CheckDetailDto** ✓ (L25063): checkId, bankName, checkNumber, checkDate, amount, status, clearedAt? (مصدره — OQ-N1)

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| UnderCollection | clear | Cleared | clearedAt · ترحيل |
| UnderCollection | bounce | Bounced | bouncedAt + reason? · السند يُعاد فتحه |
| Bounced | replace | (شيك/سند جديد) | replacementVoucherId يُسجل · بلا استبدال ثانٍ |
| Cleared | — | (نهاية) | bounce ممنوع |

### Permissions

| Code | Status |
|---|---|
| Revenue.Checks.View/Clear/Bounce/Replace | GAP-ADD |

### API

| Method | Path |
|---|---|
| POST | /api/Revenue/Checks/{id}/clear |
| POST | /api/Revenue/Checks/{id}/bounce |
| POST | /api/Revenue/Checks/{id}/replace |
| GET | /api/Revenue/DepositSlips/monthly-statement (للكشف — TRE-02) |

### Business Rules
- **BR-1** التصفية = اعتراف بالإيراد (ترحيل)
- **BR-2** الارتداد = إعادة فتح السند للاستبدال
- **BR-3** المستبدل لا يُستبدل مرتين
- **BR-4** كل انتقال بتحقق حالة مسبق

### UI/UX
- `/treasury/checks`: قائمة تحت التحصيل (مبنية من سندات الفترة) — أعمدة (رقم/بنك/تاريخ/مبلغ/سند) + أزرار صف: تصفية (تأكيد+تاريخ) / ارتداد (تأكيد+تاريخ+سبب) / استبدال (حوار: طريقة + حقول شيك عند Check)
- تبويب/صفحة الكشف الشهري (يعاد استخدام TRE-02)
- الحالات: CC-2 + حالة "مستبدل" (أزرار مخفية + رابط البديل)

### Success Criteria
- **SC-001**: كل انتقال مسجل بتاريخه (وسببه عند الارتداد)
- **SC-002**: صفر استبدال مزدوج ممكن
- **SC-003**: كل تصفية مقترنة بأثر ترحيل مرئي

### Tests
- **T1** (FR-001/US1): القائمة من السندات + الفلترة
- **T2** (FR-002/US2): التصفية + رفض تصفية Bounced
- **T3** (FR-003/US3): الارتداد بسبب + رفض ارتداد Cleared
- **T4** (FR-004/US3): الاستبدال بالنقدية وبشيك جديد
- **T5** (FR-005): منع الاستبدال المزدوج
- **T6** (CC-2): RowVersion + الفارغ الصريح

### Open Questions
- **OQ-N1** (Engineering): أي endpoint يعيد CheckDetailDto لشيك مفرد (يظهر في تفاصيل RPT-02)؟
- **OQ-N2** (User): الاستبدال النقدي — هل يتطلب حقولاً إضافية (مرجع إيداع)؟

### Out of Scope
السندات (TRE-01) · البطاقات (TRE-02) · الترحيل (خط الأنابيب)


---

# المجموعة: المدفوعات (PAY)

---

## SPEC PAY-01 — أوامر الدفع (payment-orders)

> **CONTRACT NOTE**: أسماء الحقول والمسارات وأكواد الصلاحيات وقيم الـ enums أدناه **متطلبات ملزمة** من العقد الخلفي الموثق — ليست تفاصيل تنفيذ. لا تُطرح ولا تُعاد صياغتها أثناء تحقق القوائم.

```yaml
id: PAY-01
slug: payment-orders
branch: pay01-payment-orders
group: payments
priority: P1
depends_on: ["022 (مورد/جهة)", "BGT (تفويض/بند/التزام)", "PAY-04 (حساب بنكي)", "024 (عملات)"]
blocked_by: []
consumers: [PAY-02, PAY-03, RPT-03, CTRL-03]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 3}
```

### User Stories

**US1 (P1) — إنشاء أمر كامل.** محاسب يصدر أمر الصرف الرسمي بمورده وبنوده واستقطاعاته ومستفيده.
- **Given** مورداً وتفويضاً وبنوداً، **When** ينشئ أمراً (رأس + lines[] + deductions[] + مستفيد)، **Then** Draft وpaymentOrderNumber (PO-{D6}) وamountGross/deductionAmount خادميان
- **Given** استقطاعاً isMandatory=true، **When** يحاول حذفه، **Then** محجوب
- **Given** أمراً بلا بنود، **When** يحاول الإرسال، **Then** رفض

**US2 (P1) — فحص الموازنة.** لا اعتماد بتجاوز دون قرار واعٍ.
- **Given** أمراً، **When** يُفحص، **Then** budgetCheckStatus (Pending/Passed/Failed/Overridden) بشارته
- **Given** Failed، **When** يحاول الاعتماد، **Then** حاجز يعرض الحالة والتفصيل
- **Given** Overridden، **When** يُعتمد، **Then** العبور موثق (صلاحية — OQ-N2)

**US3 (P1) — الدورة والخزينة.** المسار الرسمي: إرسال → اعتماد → خزينة → دفع.
- **Given** Draft، **When** يرسل، **Then** Submitted؛ فاعتماد (reason?) → Approved
- **Given** Approved، **When** يرسل للخزينة، **Then** SentToTreasury + treasuryStatus/treasuryReference/treasurySentAt
- **Given** Rejected، **When** يعرض التفاصيل، **Then** السبب في لوحات 022

**US4 (P1) — الإجماليات والدفع الجزئي.** أوامر كبيرة تُدفع دفعات.
- **Given** أمراً، **When** يعرض `/{id}/totals`، **Then** (amountGross/totalDeductions/netAmount/paidAmount/remainingAmount/isFullyPaid) خادمية
- **Given** دفعات جزئية (PAY-03)، **When** يُحدَّث، **Then** PartiallyPaid وremaining محدث

**US5 (P2) — الإبطال.** تصحيح معتمد غير مدفوع.
- **Given** Approved غير مدفوع، **When** يبطل (تأكيد)، **Then** Voided نهائي
- **Given** مدفوعاً جزئياً، **When** يحاول الإبطال، **Then** قاعدة خادمية تحسم (OQ-N1)

### Edge Cases
تعديل غير Draft (محجوب+خادمي) · إلغاء/إبطال بطلب صرف قائم (PAY-02 يقرأ الأمر — قاعدة خادمية) · استقطاع > الإجمالي · عملة ≠ الأساس (exchangeRate — OQ-N3) · تعارض RowVersion · أمر بلا appropriationId (رفض — مطلوب ✓)

### Functional Requirements
- **FR-001**: CRUD الأوامر: قائمة بفلاتر (status/budgetCheckStatus/فترة/مورد) + إنشاء + تعديل Draft فقط
- **FR-002**: بنود بـ5 أنواع (Invoice/Advance/Deduction/Adjustment/Other) — شبكة بنوع لكل سطر
- **FR-003**: استقطاعات بـ7 أنواع (Tax/WithholdingTax/Insurance/Penalty/AdvanceRecovery/LegalDeduction/Other) بأوضاع isMandatory/isTaxDeduction + taxAuthorityId
- **FR-004**: budgetCheckStatus بأحواله الأربعة — Failed يحجب الاعتماد
- **FR-005**: الدورة كاملة بأفعال مشروطة: submit/approve/reject/cancel/send-to-treasury/void
- **FR-006**: Totals من `/{id}/totals` حصراً — 6 أرقام + isFullyPaid
- **FR-007**: أثر الخزينة + journalEntryId (رابط القيد) معروضان
- **FR-008**: كل قرارات الاعتماد عبر لوحات 022 — لا أعمدة مضمنة (إصلاح 016)

### Data Contract

**PaymentOrderDto** ✓ (L34412):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| paymentOrderNumber | str | ✓ | PO-{D6} |
| paymentOrderDate | date | ✓ | — |
| dueDate | date | ○ | — |
| paymentOrderType | str | ✓ | قيم خادمية (OQ) |
| vendorId | int | ✓ | منتقي 022 (نوع Vendor — ترحيل 016) |
| fundId / fiscalYearId / appropriationId | int | ✓ | موازنة |
| budgetClassificationId / costCenterId / projectId | int | ○ | أبعاد |
| purchaseOrderId / encumbranceId | int | ○ | مشتريات/التزام |
| currencyId | int | ✓ | 024 |
| exchangeRate | dec | ○ | لغير الأساس |
| amountGross / deductionAmount | dec | ✓ | خادمية |
| paymentMethod | enum? | ○ | Cash/Check |
| paymentMethodName | str | — | عرض |
| bankAccountId | int | ○ | PAY-04 (نشط فقط) |
| beneficiaryName | str | ✓ | — |
| beneficiaryIban / beneficiaryAccountNumber / beneficiaryBankName | str | ○ | — |
| status | enum PaymentOrderStatus | ✓ | **Draft/Submitted/Approved/SentToTreasury/Paid/PartiallyPaid/Cancelled/Rejected/Voided** (L34712) |
| budgetCheckStatus | enum | ✓ | **Pending/Passed/Failed/Overridden** (L23452) |
| treasuryStatus / treasuryReference / treasurySentAt | str? / str? / dt? | ○ | أثر الخزينة |
| paidAt | dt? | ○ | — |
| journalEntryId | int? | ○ | رابط القيد |
| notes | str | ○ | — |
| lines[] / deductions[] | array | ✓ | — |

**PaymentOrderLineDto** ✓ (L34612): lineNumber, lineType (enum 5), description?, accountId*, amount*, taxAmount?, fundId?, appropriationId?, organizationUnitId?, costCenterId?, projectId?
**PaymentOrderDeductionDto** ✓ (L34320): lineNumber, deductionType (enum 7), deductionCode?, description?, accountId*, amount*, deductionPercent?, isMandatory, isTaxDeduction, taxAuthorityId?, referenceNumber?
**PaymentOrderTotalsDto** ✓ (L34724): id, amountGross, totalDeductions, netAmount, paidAmount, remainingAmount, isFullyPaid, status

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Draft | submit | Submitted | ≥1 بند |
| Draft | cancel | Cancelled | — |
| Submitted | approve | Approved | budgetCheck ≠ Failed |
| Submitted | reject | Rejected | reason (OQ-N1 إلزامية؟) |
| Approved | send-to-treasury | SentToTreasury | treasury* يسجل |
| SentToTreasury | (دفع PAY-03) | Paid / PartiallyPaid | خادمي |
| Approved/SentToTreasury | void | Voided | غير مدفوع (OQ-N1 للجزئي) |

### Permissions

| Code | Status |
|---|---|
| PaymentOrders.View/Create/Update/Submit/Approve/Reject/Cancel/SendToTreasury/Void | GAP-ADD — (Payments.Create/View موجودان لـ PAY-03؛ عائلة الأوامر تتحقق/تضاف) |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/PaymentOrders |
| GET | /{id} · /{id}/totals |
| POST | /{id}/submit · /approve · /reject · /cancel · /send-to-treasury · /void |

### Business Rules
- **BR-1** الصافي = gross − deductions (خادمي — Totals)
- **BR-2** تعديل Draft فقط
- **BR-3** Failed يحجب الاعتماد؛ Overridden يوثق عابره
- **BR-4** الاستقطاع mandatory غير قابل للحذف
- **BR-5** Void نهائي · كل قرار في ApprovalHistory

### UI/UX
- `/payments/payment-orders`: جدول (رقم/مورد/تاريخ/صافي/حالة/شارة فحص) + فلاتر
- `/create`: أقسام — رأس (مورد/صندوق/سنة/تفويض/تصنيف/أبعاد/عملة+سعر/استحقاق) · بنود (شبكة بنوع) · استقطاعات (شبكة بنوع + mandatory/tax + جهة ضريبية) · مستفيد (اسم/Iban/حساب/بنك) · معاينة إجمالي
- `/{id}`: رأس + بنود + استقطاعات + بطاقة Totals (6 أرقام) + شارة budgetCheckStatus + لوحات 022 + أثر خزينة + رابط قيد · حوارا send-to-treasury/void (تأكيد)
- الحالات: CC-2 + شارات فحص الموازنة الأربع بألوان

### Success Criteria
- **SC-001**: إنشاء أمر كامل (5+ بنود، 3+ استقطاعات) <5 دقائق
- **SC-002**: صفر اعتماد لأمر Failed دون عبور موثق
- **SC-003**: Totals مطابقة خادمياً 100% (بلا حساب عميل)

### Tests
- **T1** (FR-001..003/US1): إنشاء كامل بأنواع البنود والاستقطاعات + mandatory
- **T2** (FR-004/US2): شارات الفحص الأربع + حجب Failed
- **T3** (FR-005/US3): الدورة 9 حالات بأفعالها المشروطة
- **T4** (FR-006/US4): Totals الستة + PartiallyPaid
- **T5** (FR-007/US5): أثر الخزينة + void
- **T6** (FR-008): لوحات 022 تحوي كل قرار

### Open Questions
- **OQ-N1** (Engineering): حقول SendToTreasuryCommand (L37453) / VoidPaymentOrderCommand (L41057) — GAP-READ؛ وهل reject/cancel بسبب إلزامي؛ وقاعدة void للجزئي
- **OQ-N2** (User/Engineering): صلاحية Override لفحص Failed (دور؟)
- **OQ-N3** (Engineering): exchangeRate — متى يُطلب (عملة ≠ الأساس؟)

### Out of Scope
طلبات الصرف (PAY-02) · تسجيل الدفع (PAY-03) · الحسابات (PAY-04)

---

## SPEC PAY-02 — طلبات الصرف (disbursement-requests)

> **CONTRACT NOTE**: كما في PAY-01 — العقد أدناه ملزم حرفياً.

```yaml
id: PAY-02
slug: disbursement-requests
branch: pay02-disbursement-requests
group: payments
priority: P1
depends_on: [PAY-01 (أمر معتمد), 021 (توافر), SEC-02 (قواعد اعتماد محتملة)]
blocked_by: []
consumers: [PAY-03, RPT-03]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
```

### User Stories

**US1 (P1) — إنشاء طلب على أمر معتمد.** فصل القرار (الأمر) عن التنفيذ (الطلب).
- **Given** أمراً Approved بلا طلب، **When** ينشئ طلباً (paymentOrderId + notes?)، **Then** Draft وrequestNumber (DSB-{D6}) وrequestedAmount لقطة
- **Given** أمراً له طلب قائم، **When** يحاول طلباً ثانياً، **Then** رفض خادمي برسالة 1:1 حرفياً
- **Given** hasWarning=true، **When** يعرض الطلب، **Then** شارة تحذير التوافر

**US2 (P1) — التوقيع المزدوج.** مطلب رقابي قانوني.
- **Given** PendingApproval، **When** يعتمد المعتمد الأول (reason?)، **Then** approvals تسجل {step, approverName, role, decision, decisionAt}
- **Given** نفس المستخدم، **When** يحاول التوقيع الثاني، **Then** رفض خادمي
- **Given** الدور الثاني غير مؤهل (ليس AccountsManager/AuthorizingOfficer)، **When** يعتمد، **Then** رفض خادمي
- **Given** توقيعان صحيحان، **When** اكتمل، **Then** Approved + approvalDate

**US3 (P1) — الرفض والإلغاء.** مسارات خروج تحرر الموارد.
- **Given** PendingApproval، **When** يُرفض (reason)، **Then** Rejected + السبب ظاهر
- **Given** Approved غير مدفوع، **When** يُلغى، **Then** Cancelled + الأمر يعود لمنتقيات PAY-02 (طلب جديد ممكن)
- **Given** Disbursed، **When** يحاول الإلغاء، **Then** مرفوض

**US4 (P1) — بوابة التوافر.** الحاجز القانوني قبل الصرف.
- **Given** توافراً حاجزاً (Blocking)، **When** يرسل الطلب، **Then** رفض خادمي برسالة تفصيلية حرفية
- **Given** توافراً تحذيرياً، **When** يُنشأ/يُرسل، **Then** hasWarning=true يمرر مع الشارة

### Edge Cases
توافر تغيّر بين التحميل والإرسال (refetch + رسالة جديدة) · أمر أُلغي/أُبطل بعد التحميل (رفض خادمي) · مستخدمان مختلفان بدور واحد مؤهل (ASSUMED مسموح — تحقق) · تعارض RowVersion · Invalidated (حالة سابعة — متى؟ OQ-N2)

### Functional Requirements
- **FR-001**: فريد 1:1 مع الأمر (رفض الثاني برسالة الخادم حرفياً)
- **FR-002**: hasWarning بشارة تحذير (العقد منطقي — لا ثلاثي)
- **FR-003**: الإرسال عبر فحص التوافر — الرفض الحاجز يعرض تفصيله حرفياً
- **FR-004**: التوقيعات تُصور عبر approvals[] — مكون Stepper بخطوتين (خطوة/فاعل/دور/قرار/وقت)
- **FR-005**: PATCH lifecycle: submit/approve/reject/cancel مشروطة بالحالة
- **FR-006**: إلغاء Approved غير مدفوع يحرر الأمر (المنتقي يعيده)
- **FR-007**: الدور المؤهل للثاني (AccountsManager/AuthorizingOfficer) — تحقق خادمي، الرفض يعرض

### Data Contract

**DisbursementRequestDto** ✓ (L30153):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| requestNumber | str | ✓ | DSB-{D6} |
| paymentOrderId / paymentOrderNumber | int / str | ✓ | 1:1 فريد |
| requestedById / requestedByName | int / str | ✓ | — |
| requestDate | date | ✓ | — |
| status | enum DisbursementRequestStatus | ✓ | **Draft/PendingApproval/Approved/Rejected/Cancelled/Disbursed/Invalidated** (L30257) |
| hasWarning | bool | ✓ | تحذير التوافر |
| notes | str | ○ | — |
| requestedAmount | dec | ✓ | لقطة من الأمر |
| payeeName / fundName | str | ○ | عرض |
| approvalDate / paymentDate | dt? | ○ | — |

**DisbursementRequestDetailDto** ✓ (L30136): + `approvals: ApprovalStepDto[] {step, approverUserId, approverName, role, decision, decisionAt}`
**CreateDisbursementRequestRequest** ✓ (L27277): `{paymentOrderId!, notes!}` · **ApproveDisbursementRequestRequest** ✓ (L21476): `{reason!}`

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| Draft | submit | PendingApproval | توافر (Blocking ⇒ رفض) |
| PendingApproval | approve (1st) | PendingApproval | خطوة 1 تسجل |
| PendingApproval | approve (2nd) | Approved | مستخدم مختلف + دور مؤهل |
| PendingApproval | reject | Rejected | reason |
| Draft/PendingApproval | cancel | Cancelled | يحرر الأمر |
| Approved | cancel | Cancelled | غير مدفوع فقط · يحرر الأمر |
| Approved | (دفع PAY-03) | Disbursed | خادمي |
| — | — | Invalidated | شروطه خادمية (OQ-N2) |

### Permissions

| Code | Status |
|---|---|
| DisbursementRequests.View/Create/Submit/Approve/Reject/Cancel | GAP-ADD (لم تظهر في فحص PermissionCodes) |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/DisbursementRequests |
| GET | /{id} |
| PATCH | /{id}/submit · /approve · /reject · /cancel |

### Business Rules
- **BR-1** 1:1 صارم
- **BR-2** التوافر البوابة الوحيدة (Blocking/Warning) — التفصيل حرفي
- **BR-3** توقيع مزدوج: ≥2 مختلفان، ≥1 بدور مؤهل، نفس المستخدم ممنوع
- **BR-4** الإلغاء يحرر الأمر لطلب جديد

### UI/UX
- `/payments/disbursement-requests`: جدول (رقم/أمر/مبلغ/حالة/شارة تحذير) + فلاتر (حالة/أمر)
- `/create`: منتقي أوامر Approved بلا طلب + notes
- `/{id}`: رأس + **Stepper توقيعين** (من approvals) + شارة hasWarning + تفصيل الأمر المرتبط · أزرار PATCH مشروطة · حوارات approve/reject/cancel (reason)
- الحالات: CC-2 + حالة رفض التوافر (لوحة تفصيل حرفية)

### Success Criteria
- **SC-001**: صفر طلب ثانٍ على نفس الأمر
- **SC-002**: 100% من الاعتمادات بتوقيعين مختلفين وأدوار مطابقة
- **SC-003**: كل إلغاء يحرر أمره فوراً (مرئي في منتقي الإنشاء)

### Tests
- **T1** (FR-001/US1): رفض الطلب الثاني + الرسالة
- **T2** (FR-004/US2): Stepper بالتوقيعين + رفض نفس المستخدم/الدور
- **T3** (FR-003/US4): Blocking يرفض + التفصيل حرفي · Warning بشارة
- **T4** (FR-006/US3): الإلغاء يحرر الأمر
- **T5** (FR-005): الحالات السبع بأفعالها المشروطة

### Open Questions
- **OQ-N1** (Engineering): حقول Reject/Cancel commands (L35964/L24457 — GAP-READ)
- **OQ-N2** (Engineering): متى تُضبط Invalidated؟ (الحالة السابعة — للشارة والعرض)

### Out of Scope
الأمر (PAY-01) · الدفع (PAY-03) · خدمة التوافر نفسها (021)

---

## SPEC PAY-03 — تنفيذ الدفع (payment-execution)

> **CONTRACT NOTE**: كما في PAY-01 — العقد أدناه ملزم حرفياً.

```yaml
id: PAY-03
slug: payment-execution
branch: pay03-payment-execution
group: payments
priority: P1
depends_on: [PAY-02 (طلب معتمد), PAY-01 (يغلق)]
blocked_by: []
consumers: [RPT-03, ACC-05 (حدث الترحيل)]
permissions_status: "✓ مؤكدة (Payments.View/Create)"
oq: {blocking: 0, non_blocking: 2}
known_legal_gap: "Transfer/InKind في توكيل m26 قانونياً لكن enum العقد = Cash/Check — تسجل كفجوة، لا تُخترع"
```

### User Stories

**US1 (P1) — تسجيل الدفع.** أمين الصندوق ينفذ الطلب المعتمد — إغلاق الثلاثية.
- **Given** طلباً Approved، **When** يسجل دفعاً (paymentMethod + referenceNumber? + notes?؛ amount لقطة readonly)، **Then** Completed وpaymentNumber (PAY-{D6})
- **Given** النجاح، **When** يفتح الطلب، **Then** Disbursed + paymentDate؛ والأمر Paid (أو PartiallyPaid)
- **Given** طلباً غير معتمد، **When** يحاول التسجيل، **Then** رفض خادمي حرفي

**US2 (P1) — الفشل المرئي.** لا فشل صامت.
- **Given** دفعاً Failed، **When** يعرضه، **Then** رسالة الخادم حرفياً + مسار إعادة المحاولة

**US3 (P2) — قائمة المدفوعات.** مراجعة التدفقات الفعلية.
- **Given** مدفوعات فترة، **When** يفلتر (فترة/طريقة/حالة)، **Then** القائمة تتبع

### Edge Cases
دفع مزدوج متزامن (قيد خادمي — الرسالة تعرض) · Check بلا referenceNumber (OQ-N1) · طلب أُلغي بعد تحميل الحوار · فشل ترحيل بعد تسجيل ناجح (الحالة تعكس — ACC-05 يظهر الحدث) · amount ≠ requestedAmount (مستحيل — لقطة)

### Functional Requirements
- **FR-001**: التسجيل من طلب Approved حصراً (منتقي/سياق)
- **FR-002**: الطرق: **Cash/Check فقط** (enum L34315) — الفجوة القانونية تسجل ولا تُخترع
- **FR-003**: amount لقطة من الطلب — readonly في الحوار
- **FR-004**: النجاح يغلق الثلاثية (طلب Disbursed + أمر Paid/PartiallyPaid) — يُعرض في شاشة النجاح
- **FR-005**: status: Completed/Failed — الفشل بسببه حرفياً + إعادة محاولة
- **FR-006**: لا PUT/DELETE — تسجيل مرة واحدة (immutable)
- **FR-007**: حدث الترحيل يرفع آلياً — رابط نتيجته (ACC-05) عند الفشل

### Data Contract

**PaymentDto** ✓ (L34295):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | ✓ | — |
| paymentNumber | str | ✓ | PAY-{D6} |
| disbursementRequestId / disbursementRequestNumber | int / str | ✓ | — |
| paymentOrderId / paymentOrderNumber | int / str | ✓ | — |
| paymentMethod | enum | ✓ | Cash/Check |
| amount | dec | ✓ | لقطة الطلب |
| paidById / paidByName / paidAt | int / str / dt | ✓ | خادمية |
| referenceNumber | str | ○ | رقم شيك/مرجع (OQ-N1) |
| notes | str | ○ | — |
| status | enum PaymentStatus | ✓ | **Completed/Failed** (L34800) |
| payeeName | str | ○ | عرض |

**RecordPaymentRequest** ✓ (L35672): `{disbursementRequestId!, paymentMethod!, referenceNumber?, notes?}` — **بلا حقل amount** (اللقطة خادمية)

### Lifecycle
تسجيل لمرة واحدة: → Completed / Failed. لا تحولات لاحقة (immutable). الأثر على الطلب/الأمر خادمي.

### Permissions

| Code | Status |
|---|---|
| Payments.View | ✓ مؤكدة |
| Payments.Create | ✓ مؤكدة |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/Payments |
| GET | /api/Payments/Payments/{id} |

### Business Rules
- **BR-1** Approved شرط التسجيل
- **BR-2** النجاح يرفع AccountingEvent → ترحيل (خط الأنابيب)
- **BR-3** القيد الخادمي يمنع الدفع المزدوج المتزامن
- **BR-4** الدفعة immutable — التصحيح بدفعة أخرى/إجراء خادمي (OQ إن وجد)

### UI/UX
- `/payments/payments`: جدول (رقم/طلب/أمر/طريقة/مبلغ/حالة/دافع/تاريخ) + فلاتر (فترة/طريقة/حالة)
- حوار تسجيل (من تفاصيل PAY-02): لقطة المبلغ readonly + select طريقة + referenceNumber + notes + تأكيد
- شاشة نجاح: الثلاثية المغلقة (طلب/أمر/دفعة) بأرقامها · حالة فشل: سبب حرفي + زر إعادة
- الحالات: CC-2

### Success Criteria
- **SC-001**: تسجيل دفع <30 ثانية
- **SC-002**: 100% من النجاحات تغلق الثلاثية (اختبار)
- **SC-003**: كل فشل بسبب حرفي

### Tests
- **T1** (FR-001/US1): شرط Approved + رفض غيره
- **T2** (FR-003/FR-004): اللقطة readonly + إغلاق الثلاثية
- **T3** (FR-002): الطريقتان فقط في المنتقي
- **T4** (FR-005/US2): Failed بالسبب + إعادة محاولة
- **T5** (FR-006/US3): القائمة والفلاتر + بلا أزرار تحرير

### Open Questions
- **OQ-N1** (Engineering): referenceNumber — مطلوب شرطاً عند Check؟
- **OQ-N2** (User): الفجوة القانونية Transfer/InKind (m26) — تُرفع كطلب عقد خلفي منفصل؟ (لا تُخترع في الواجهة)

### Out of Scope
دورة الأوامر/الطلبات (PAY-01/02) · الترحيل (خط الأنابيب/ACC-05)

---

## SPEC PAY-04 — الحسابات البنكية (bank-accounts)

> **CONTRACT NOTE**: كما في PAY-01 — العقد أدناه ملزم حرفياً. **تنبيه مصحح**: bankName/openingBalance/currentBalance **موجودة فعلياً** في العقد الحالي (عكس ما أشيع عن حذفها بتفويض m26) — الكود هو الحقيقة (AGENTS.md).

```yaml
id: PAY-04
slug: bank-accounts
branch: pay04-bank-accounts
group: payments
priority: P2
depends_on: ["024 (عملات)", "BGT (صناديق)", "ACC (حساب GL)"]
blocked_by: []
consumers: [PAY-01 (bankAccountId), BANK-01]
permissions_status: "✓ مؤكدة (5 أكواد)"
oq: {blocking: 0, non_blocking: 3}
```

### User Stories

**US1 (P1) — سجل الحسابات.** مرجع البنوك للأوامر والكشوف.
- **Given** حسابات، **When** يفتح السجل، **Then** أعمدة (name/bankName/accountNumber/عملة/isDefault/حدود/isActive/lastReconciliationDate)
- **Given** حساباً معطلاً، **When** يفتح منتقي PAY-01، **Then** غير مدرج

**US2 (P1) — إنشاء/تعديل بالملاحق الكاملة.** رقابة البنوك.
- **Given** بيانات كاملة، **When** ينشئ (name/bankName/accountNumber + iban/swift/فرع + عملة/صندوق/GL + حدود/mزدوج/openingBalance)، **Then** يُحفظ ويعرض كاملاً
- **Given** accountNumber مكرراً، **When** ينشئ، **Then** رفض خادمي

**US3 (P2) — تفعيل/تعطيل.** إيقاف بلا حذف تاريخ.
- **Given** حساباً نشطاً، **When** يعطله (تأكيد)، **Then** isActive=false + استبعاد من المنتقيات
- **Given** حساباً بكشوف نشطة، **When** يعطله، **Then** قاعدة خادمية تحسم (OQ-N2)

### Edge Cases
isDefault متعدد (سلوك التبديل — OQ-N1) · تعديل glAccountId بعد استخدامه في ترحيل (قاعدة خادمية) · currentBalance يعرض ولا يُحرر (يُحدث آلياً — OQ-N3) · تعارض RowVersion · حدود (maxDaily/maxTransaction) تتجاوزها دفعة (أثرها في PAY-03؟ — يعرض كتحذير)

### Functional Requirements
- **FR-001**: CRUD كامل بكل حقول العقد الحرفية (17 حقلاً)
- **FR-002**: isDefault بادرة واحدة (عرض + تحكم — OQ-N1)
- **FR-003**: الحدود + requiresDualApproval معروضة (شارة تؤثر على PAY-02/03 كتوثيق)
- **FR-004**: activate/deactivate بـ `{id, rowVersion}` + تأكيد
- **FR-005**: openingBalance يُدخل عند الإنشاء · currentBalance/lastReconciliationDate عرض فقط (BANK-01 يغذيها)
- **FR-006**: المعطل مستبعد من كل المنتقيات الجديدة

### Data Contract

**BankAccountDto** ✓ (L22908):

| Field | Type | Req | Rule |
|---|---|---|---|
| id | int | — | — |
| name | str | ✓ | — |
| bankName | str | ✓ | موجود فعلياً (تصحيح) |
| accountNumber | str | ✓ | فريد |
| iban / swiftCode | str | ○ | — |
| branchName / branchCode | str | ○ | — |
| currencyId | int | ✓ | 024 |
| fundId | int | ○ | صندوق |
| glAccountId | int | ○ | ربط الترحيل |
| isDefault | bool | ✓ | واحد افتراضياً |
| maxDailyLimit / maxTransactionLimit | dec | ○ | حدود |
| requiresDualApproval | bool | ✓ | شارة |
| lastReconciliationDate | dt? | ○ | عرض فقط (BANK-01) |
| openingBalance / currentBalance | dec | ○ | opening يُدخل · current عرض |
| isActive | bool | ✓ | — |

**CreateBankAccountCommand** ✓ (L26313): كل ما سبق مطروح id/lastReconciliationDate/currentBalance/isActive
**Activate/DeactivateBankAccountCommand** ✓ (L20509/L29362): `{id, rowVersion}` (تحقق — GAP-READ جزئي)

### Lifecycle
isActive عبر activate/deactivate — لا دورة حالة.

### Permissions

| Code | Status |
|---|---|
| BankAccounts.View/Create/Update/Activate/Deactivate | ✓ مؤكدة الخمس |

### API

| Method | Path |
|---|---|
| GET / POST | /api/Payments/BankAccounts |
| GET / PUT | /{id} |
| POST | /{id}/activate · /{id}/deactivate |

### Business Rules
- **BR-1** accountNumber فريد
- **BR-2** isDefault واحد (آلية التبديل — OQ-N1)
- **BR-3** التعطيل لا يحذف التاريخ — الكشوف القديمة تبقى مرتبطة
- **BR-4** الأرصدة الجارية لا تُحرر يدوياً

### UI/UX
- `/payments/bank-accounts`: جدول + فلاتر (نشط/عملة/صندوق)
- نافذة إنشاء/تعديل بأقسام: الهوية (name/bank/account) · الفرع (iban/swift/branch) · المالية (عملة/صندوق/GL/opening) · الضوابط (حدود/mزدوج/افتراضي)
- التفاصيل: كل الملاحق + lastReconciliationDate + أزرار تفعيل/تعطيل (تأكيد)
- الحالات: CC-2

### Success Criteria
- **SC-001**: إنشاء حساب كامل <دقيقتين
- **SC-002**: المعطل غائب 100% عن المنتقيات
- **SC-003**: كل تفعيل/تعطيل بتأكيد

### Tests
- **T1** (FR-001/US2): CRUD بالملاحق + تكرار مرفوض
- **T2** (FR-004/FR-006/US3): تعطيل + استبعاد من منتقي PAY-01
- **T3** (FR-005): currentBalance/lastReconciliation للقراءة فقط
- **T4** (FR-003): شارة المزدوج والحدود

### Open Questions
- **OQ-N1** (User): سلوك isDefault عند تعيين ثانٍ (تبديل تلقائي أم رفض؟)
- **OQ-N2** (Engineering): قاعدة التعطيل بكشوف/تسويات نشطة
- **OQ-N3** (Engineering): هل currentBalance يُحدث آلياً من BANK-01؟

### Out of Scope
الكشوف والتسويات (BANK-01) · الدفعات (PAY-03)


---

# المجموعة: الرقابة المالية (CTRL)

---

## SPEC CTRL-01 — لوحة توافر الموازنة (availability-dashboard)

> **CONTRACT NOTE**: أسماء الحقول والمسارات وأكواد الصلاحيات أدناه **متطلبات ملزمة** من العقد الخلفي الموثق — ليست تفاصيل تنفيذ.

```yaml
id: CTRL-01
slug: availability-dashboard
branch: ctrl01-availability-dashboard
group: financial-control
priority: P1
depends_on: ["021 (شجرة البنود — نقطة الدخول)", "BGT (تفويضات)", "TRE/PAY (حركات)"]
blocked_by: []
consumers: [RPT-04 (لقطة أوسع)]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
contract_note: "endpoint واحد لبند واحد — لا قائمة شاملة (تُبنى من 021/RPT-04)"
```

### User Stories

**US1 (P1) — تفكيك توافر بند.** الكنترولر يرى من أين جاء التفويض وأين ذهب — بالأبعاد.
- **Given** بنداً بحركات عبر صناديق، **When** يفتح تفصيله، **Then** سطر لكل (صندوق × برنامج × مشروع) بأربعة أرقام (appropriation/encumbered/paid/available) + totals
- **Given** بنداً بلا أبعاد، **When** يفتح، **Then** سطر واحد (program/project فارغان)
- **Given** available سالباً، **When** يُعرض، **Then** تنبيه تجاوز (أحمر + أيقونة)

**US2 (P2) — التنقل من الشجرة.** نقطة الدخول الطبيعية.
- **Given** شجرة 021، **When** ينقر إجراء التوافر لبند، **Then** هذه اللوحة تفتح للبند نفسه

### Edge Cases
بند بلا تفويض (أصفار) · available سالب · سنة قديمة (فلاتر) · بند بلا حركات (فارغ صريح) · تحديث بعد حركة (invalidation من TRE/PAY/BGT)

### Functional Requirements
- **FR-001**: تفكيك fund→program→project لكل بند
- **FR-002**: الأرقام الأربعة + totals خادمية حصراً (CC-3)
- **FR-003**: إبراز السالب (تنبيه تجاوز)
- **FR-004**: invalidation سليم عند حركات الموازنة
- **FR-005**: رأس يعرض budgetItemCode + fiscalYearName

### Data Contract

**AvailabilityBreakdownResponse** ✓ (L21977):

| Field | Type | Req | Rule |
|---|---|---|---|
| budgetItemId / budgetItemCode | int / str | ✓ | — |
| fiscalYearId / fiscalYearName | int / str | ✓ | — |
| breakdown[] | Line[] | ✓ | — |
| totals | Total | ✓ | GAP-READ (L22039) |

**AvailabilityBreakdownLineResponse** ✓ (L21888): fundId, fundCode, fundName, programId?, programCode?, programName?, projectId?, projectCode?, projectName?, budgetItemId, itemCode, appropriationAmount, encumberedAmount, paidAmount, availableAmount

### Lifecycle
قراءة فقط — لا حالة.

### Permissions

| Code | Status |
|---|---|
| FinancialControl.Availability.View (أو ما يعادلها) | GAP-ADD — عائلة FinancialControl غائبة عن الفحص؛ المرجع BudgetAvailabilityService (src/Application/Budgeting/Common) |

### API

| Method | Path |
|---|---|
| GET | /api/FinancialControl/Availability/{budgetItemId} |

### Business Rules
- **BR-1** available = appropriation − encumbered − paid (خادمي)
- **BR-2** الأبعاد الفارغة تعرض "—" لا صفراً مضللاً
- **BR-3** لا قائمة شاملة هنا — التكامل مع 021/RPT-04

### UI/UX
- `/financial-control/availability/{itemId}`: رأس (بند + سنة) + جدول مكسور (صندوق/برنامج/مشروع × 4 أرقام) + صف totals + شارة سالب
- زر رجوع للشجرة (021) · الحالات: CC-2
- يعيد استخدام BudgetItemTree/AvailabilityIndicator من 021

### Success Criteria
- **SC-001**: توافر بند كامل <3 ثوان
- **SC-002**: صفر حساب عميل
- **SC-003**: كل سالب بتنبيه

### Tests
- **T1** (FR-001/US1): سطور الأبعاد + totals
- **T2** (FR-003): السالب بتنبيهه
- **T3** (CC-2): فارغ + loading skeleton
- **T4** (FR-005/US2): رأس البند + العودة للشجرة

### Open Questions
- **OQ-N1** (Engineering): حقول AvailabilityBreakdownTotalResponse (L22039 — GAP-READ)
- **OQ-N2** (Engineering): فلاتر إضافية للـ query (فترة؟) — تحقق من signature الـ handler

### Out of Scope
قائمة شاملة (021/RPT-04) · التحرير · اللقطة الزمنية (RPT-04)

---

## SPEC CTRL-02 — إغلاق/إعادة السنة المالية (year-closing)

> **CONTRACT NOTE**: كما في CTRL-01 — العقد أدناه ملزم حرفياً.

```yaml
id: CTRL-02
slug: year-closing
branch: ctrl02-year-closing
group: financial-control
priority: P1
depends_on: ["FS (FiscalYear + حالته)", "ACC-05 (فترات)", "BGT (تفويضات/التزامات)"]
blocked_by: []
consumers: [CTRL-03 (شرط محتمل — OQ)]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
```

### User Stories

**US1 (P1) — حالة السنوات.** يعرف أي سنة مفتوحة وأي مُسقطة.
- **Given** سنوات مالية، **When** يفتح الشاشة، **Then** بطاقة لكل سنة (اسم + حالة: مفتوحة/Lapsed)

**US2 (P1) — الإسقاط (Lapse).** إجراء قانوني يسقط بقايا التفويضات والالتزامات — موثق بـ run.
- **Given** سنة مفتوحة منتهية، **When** يسقطها (تأكيد مزدوج + تحذير FR-020: "القيود العكسية تصبح محظورة لهذه السنة")، **Then** نتيجة: lapsedAppropriationTotal + lapsedEncumbranceTotal + yearClosingRunId
- **Given** سنة Lapsed، **When** يحاول إسقاطها مجدداً، **Then** الزر مخفي + قاعدة خادمية

**US3 (P1) — الاستعادة (Reopen).** تصحيح إسقاط خاطئ بأرقام موثقة.
- **Given** سنة Lapsed، **When** يعيد فتحها (تأكيد)، **Then** restoredAppropriationTotal + restoredEncumbranceTotal + runId
- **Given** سنة مفتوحة، **When** يحاول إعادة فتحها، **Then** مرفوض

### Edge Cases
سنة بالتزامات مفتوحة (الأرقام تعكس المُسقط — لا منع افتراضي) · إسقاط قبل توليد قيود الإقفال FS (التسلسل — OQ-N1) · فعل متزامن (RowVersion/خادمي) · سنة بلا تفويضات (أصفار)

### Functional Requirements
- **FR-001**: عرض حالة السنوات (من FiscalYear — FS)
- **FR-002**: Lapse بـ `{fiscalYearId}` فقط — **العقد بلا reason** (أي حقل سبب شاشةً لا يدخل الطلب)
- **FR-003**: تحذير FR-020 إلزامي نصاً في حوار الإسقاط
- **FR-004**: عرض أرقام النتيجة + runId بعد كل فعل (تدقيق)
- **FR-005**: منع التكرار (Lapsed لا تُسقط، مفتوحة لا تُستعاد)
- **FR-006**: تأكيد مزدوج للإسقاط (حوار خطير)

### Data Contract

**LapseFiscalYearRequest** ✓ (L32793): `{fiscalYearId!}`
**LapseFiscalYearResponse** ✓ (L32841): `{yearClosingRunId, fiscalYearId, fiscalYearName, lapsedAppropriationTotal, lapsedEncumbranceTotal}`
**ReopenFiscalYearRequest** ✓ (L36134): `{fiscalYearId!}`
**ReopenFiscalYearResponse** ✓ (L36176 area): `{yearClosingRunId, fiscalYearId, restoredAppropriationTotal, restoredEncumbranceTotal}`

### Lifecycle
السنة: `Open → Lapsed → Open` (الحالة تُقرأ من FiscalYear — FS/024 مصدرها)

### Permissions

| Code | Status |
|---|---|
| FinancialControl.YearClosing.View/Lapse/Reopen | GAP-ADD |

### API

| Method | Path |
|---|---|
| POST | /api/FinancialControl/YearClosing/Lapse |
| POST | /api/FinancialControl/YearClosing/Reopen |

### Business Rules
- **BR-1** FR-020: السنة المُسقطة تحظر القيود العكسية — التحذير نصي إلزامي
- **BR-2** كل إسقاط/استعادة يسجل run (yearClosingRunId) — قابل للتدقيق
- **BR-3** الأرقام المُسقطة/المستعادة تعرض كما يعيدها الخادم (لا إعادة حساب)

### UI/UX
- `/financial-control/year-closing`: بطاقات السنوات (اسم/حالة) · زر Lapse (حوار خطير: تحذير FR-020 + تأكيد) · زر Reopen (تأكيد) · بطاقة نتيجة بعد الفعل (الأرقام + runId)
- الحالات: CC-2 + حالة النتيجة (نجاح بأرقام)

### Success Criteria
- **SC-001**: صفر إسقاط دون تحذير FR-020
- **SC-002**: كل فعل بأرقامه + runId معروضين
- **SC-003**: صفر تكرار ممكن

### Tests
- **T1** (FR-002/FR-003/US2): الإسقاط بالتحذير + الأرقام
- **T2** (FR-005): منع التكرار (Lapsed/مفتوحة)
- **T3** (FR-004/US3): الاستعادة بأرقامها
- **T4** (CC-2): confirm مزدوج + الحالة

### Open Questions
- **OQ-N1** (Engineering): هل Lapse يغلق فترات السنة آلياً (تفاعل ACC-05)؟ وهل يسبقه توليد قيود الإقفال (FS GenerateYearEndClosing)؟
- **OQ-N2** (Engineering): هل Lapse شرط لتوليد CTRL-03؟

### Out of Scope
إغلاق الفترات (ACC-05) · الحسابات النهائية (CTRL-03) · قيود الإقفال (FS/024)

---

## SPEC CTRL-03 — الحسابات النهائية (final-accounts)

> **CONTRACT NOTE**: كما في CTRL-01 — العقد أدناه ملزم حرفياً.

```yaml
id: CTRL-03
slug: final-accounts
branch: ctrl03-final-accounts
group: financial-control
priority: P1
depends_on: [CTRL-02 (إسقاط — OQ), ACC-05 (فترات), TRE (تحصيل), PAY (صرف)]
blocked_by: []
consumers: []
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
```

### User Stories

**US1 (P1) — التوليد والمعاينة.** مخرج الرقابة القانوني للسنة.
- **Given** سنة مالية، **When** يولّد حساباتها، **Then** Draft + finalAccountId + lineCount
- **Given** حسابات مولدة، **When** يعاينها، **Then** أقسام حسب dimension (Fund/Program/Project/Item) بأعمدة budgeted/actual/variance

**US2 (P2) — عبارات التغذية.** التحقق قبل الإصدار.
- **Given** عبارتي التحصيل/الصرف، **When** يفتحهما، **Then** السطور (fundCode/fundName/date/amount + source|payee) + grandTotal + شارة isClosed

**US3 (P1) — الإصدار النهائي.** تجميد قانوني.
- **Given** Draft، **When** يُصدر (تأكيد نهائي)، **Then** Issued + issuedAt
- **Given** Issued، **When** يحاول إعادة الإصدار، **Then** مرفوض
- **Given** إصدارات سابقة، **When** يفتح القائمة، **Then** read-only

### Edge Cases
توليد سنة غير مُسقطة (قاعدة خادمية — OQ-N2) · توليد مكرر لنفس السنة (Draft قائم — سلوك خادمي) · عبارات فارغة (أصفار صريحة) · variance سالب/موجب (تلوين) · سنة بلا بنود (lineCount=0 — حالة صريحة)

### Functional Requirements
- **FR-001**: Generate بـ `{fiscalYearId}` → `{finalAccountId, fiscalYearId, fiscalYearName, status, lineCount}`
- **FR-002**: البنود بأبعادها الأربعة (dimension enum) — أقسام ديناميكية
- **FR-003**: variance = actual − budgeted ملون (سالب أحمر/موجب أخضر — حسب DESIGN.md)
- **FR-004**: عبارتا Collection/Disbursement بـ grandTotal وisClosed
- **FR-005**: Issue نهائي (تأكيد) — Issued يمنع الإعادة
- **FR-006**: قائمة الإصدارات (سنة/حالة/تواريخ) read-only للصادر

### Data Contract

**FinalAccountResponse** ✓ (L31289): `{id, fiscalYearId, status (enum FinalAccountStatus: Draft/Issued — L31300), generatedAt, issuedAt?, lines[]}`
**FinalAccountLineResponse** ✓ (L31209): dimension (enum FinalAccountLineDimension: **Fund/Program/Project/Item**), dimensionId, dimensionCode, dimensionName, budgetedAmount, actualAmount, variance
**GenerateFinalAccountRequest** ✓ (L31923): `{fiscalYearId!}` → **Response:** `{finalAccountId, fiscalYearId, fiscalYearName, status, lineCount}`
**CollectionStatementResponse** ✓ (L25478): `{fiscalYearId, fiscalYearName, collections: CollectionLineResponse[] {fundCode, fundName, date, amount, source}, grandTotal, isClosed}`
**DisbursementStatementResponse** ✓ (L30348): `{fiscalYearId, fiscalYearName, disbursements: DisbursementLineResponse[] {fundCode, fundName, date, amount, payee}, grandTotal, isClosed}`

### Lifecycle

| From | Event | To | Guard |
|---|---|---|---|
| — | Generate | Draft | سنة (شرط الإسقاط — OQ-N2) |
| Draft | Issue | Issued | تأكيد نهائي |
| Issued | — | (نهاية) | read-only |

### Permissions

| Code | Status |
|---|---|
| FinancialControl.FinalAccounts.View/Generate/Issue | GAP-ADD |
| FinancialControl.Statements.View | GAP-ADD |

### API

| Method | Path |
|---|---|
| POST | /api/FinancialControl/FinalAccounts/Generate |
| POST | /api/FinancialControl/FinalAccounts/{id}/Issue |
| GET | /api/FinancialControl/FinalAccounts/{id} |
| GET | /api/FinancialControl/Statements/Collection · /Disbursement |

### Business Rules
- **BR-1** الإصدار نهائي قانونياً
- **BR-2** الأبعاد الأربعة بأقسامها — لا دمج
- **BR-3** isClosed في العبارات يعكس حالة السنة (CTRL-02)

### UI/UX
- `/financial-control/final-accounts`: قائمة الإصدارات + زر توليد (منتقي سنة)
- `/{id}`: جدول بأقسام dimension (كل قسم: code/name × budgeted/actual/variance ملون) + تبويبا العبارات (grandTotal + isClosed) + زر إصدار (تأكيد نهائي)
- الحالات: CC-2 + فارغ lineCount=0

### Success Criteria
- **SC-001**: توليد + عرض كامل <10 ثوان
- **SC-002**: صفر تعديل بعد Issued
- **SC-003**: كل variance بلونه الصحيح

### Tests
- **T1** (FR-001/US1): التوليد + lineCount
- **T2** (FR-002/FR-003): أقسام الأبعاد + التلوين
- **T3** (FR-005/US3): الإصدار النهائي + منع الإعادة
- **T4** (FR-004/US2): العبارتان بمجموعيهما وisClosed
- **T5** (CC-2): الفارغ الصريح

### Open Questions
- **OQ-N1** (Engineering): جسم Issue command (reason؟) — GAP-READ
- **OQ-N2** (Engineering): شرط الإسقاط (CTRL-02) قبل التوليد — ورسالة الرفض

### Out of Scope
إسقاط السنة (CTRL-02) · القوائم المالية (RPT-06) · ميزان المراجعة (RPT-05)


---

# المجموعة: التقارير (RPT)

## متطلبات التقارير المشتركة (Report Common — RC)

ترثها RPT-01..06 — لا تُكرر في كل spec:

- **RC-1** كل الأرقام خادمية (CC-3) — التقرير يعرض ما يصل فقط
- **RC-2** الفلاتر تُطبق على نفس query التصدير — **التصدير مطابق للشاشة** (نفس الفلاتر/الترتيب)
- **RC-3** حالات: loading skeleton · error+retry · **فارغ صريح** (رسام عربية + "لا توجد بيانات للفترة المحددة") · unauthorized
- **RC-4** pagination عند تجاوز العتبة (ASSUMED: 500 صف — يوثق في plan) · تصدير الفترة الجارية بتحذير "بيانات جزئية"
- **RC-5** RTL كامل في العرض والتصدير · أعمدة رقمية بمحاذاة بداية/نهاية منطقية
- **RC-6** كل تقرير مرتبط بصلاحية عرض + صلاحية تصدير منفصلتين (جدول لكل spec)

---

## SPEC RPT-01 — تقرير تنفيذ الموازنة (budget-execution-report)

> **CONTRACT NOTE**: العقد أدناه ملزم حرفياً (web-api-client.ts) — ليس تفاصيل تنفيذ.

```yaml
id: RPT-01
slug: budget-execution-report
branch: rpt01-budget-execution-report
group: reports
priority: P1
depends_on: [BGT (بنود), TRE/PAY (حركات)]
permissions_status: "✓ مؤكدة (Reporting.ViewBudgetExecution + ExportReports)"
oq: {blocking: 0, non_blocking: 1}
```

### User Stories
**US1 (P1) — التقرير العام.** متخذ القرار يرى تنفيذ كل بند: تفويض/التزام/صرف/متاح.
- **Given** سنة، **When** يفتح بفلاتر (سنة/صندوق/برنامج/مشروع)، **Then** lines[] بأربعة أرقام + totals
**US2 (P1) — تفصيل بند.** من السطر إلى حركاته.
- **Given** بنداً، **When** ينقره، **Then** encumbrances[] + payments[]
**US3 (P2) — التصدير.** مخرج ورقي/إلكتروني مطابق.

### Functional Requirements (deltas)
- **FR-001**: جدول بأعمدة (itemCode/itemName/fund/program/project + appropriated/encumbered/paid/available)
- **FR-002**: صف totals (4 مجاميع) ثابت أسفل الجدول
- **FR-003**: نسبة الاستخدام **محسوبة شاشةً** (paid/appropriated) كعمود إضافي موسوم "عرض" (ليست في العقد — لا تُصدر كحقل خادمي)
- **FR-004**: تفصيل البند بتبويبين (التزامات/مدفوعات)
- **FR-005**: RC-1..RC-6

### Data Contract
**BudgetExecutionReportDto** ✓ (L23956): `{fiscalYearId, fiscalYearName, lines[], totals}`
**BudgetExecutionLineDto** ✓ (L23878): budgetItemId, itemCode, itemName, fundId, fundNumber, fundName, programId?, programCode?, projectId?, projectCode?, appropriatedAmount, encumberedAmount, paidAmount, availableAmount
**BudgetExecutionTotalDto** ✓ (L24016): appropriatedAmount, encumberedAmount, paidAmount, availableAmount
**BudgetExecutionDetailDto** ✓ (L23785): budgetItemId, itemCode, itemName, fundId, fundNumber, encumbrances: EncumbranceDetailDto[], payments: PaymentDetailDto[]

### API
GET `/api/Reporting/BudgetExecutionReports` · GET `/{budgetItemId}/detail` · GET `/export`

### Permissions
| Code | Status |
|---|---|
| Reporting.ViewBudgetExecution | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria
- **SC-001**: تحميل سنة كاملة <3 ثوان
- **SC-002**: totals = Σlines (اختبار مقارنة)
- **SC-003**: التصدير مطابق للفلاتر

### Tests
T1 الفلاتر+الأعمدة · T2 totals · T3 التفصيل بالتبويبين · T4 التصدير · T5 الفارغ (RC-3)

### Open Questions
- **OQ-N1** (Engineering): فلاتر query الدقيقة (سنة إلزامية؟ صندوق/برنامج/مشروع اختيارية؟) — من signature الـ handler

### Out of Scope
تكسير الأبعاد (CTRL-01) · التحرير

---

## SPEC RPT-02 — تقرير تحصيل الإيرادات (revenue-collections-report)

> **CONTRACT NOTE**: كما في RPT-01.

```yaml
id: RPT-02
slug: revenue-collections-report
branch: rpt02-revenue-collections-report
group: reports
priority: P2
depends_on: [TRE-01..03]
permissions_status: "✓ مؤكدة"
oq: {blocking: 0, non_blocking: 1}
```

### User Stories
**US1 (P1) — التحصيلات.** متابعة سندات الفترة وحالة إيداعها.
- **Given** فترة، **When** يفلتر (طرف/طريقة/حالة)، **Then** سطور + حالة البطاقة
**US2 (P1) — تفصيل سند.** بنيته الكاملة.
- **Given** سنداً، **When** ينقره، **Then** lines (بحسابها) + checks + depositSlipNumber/Status
**US3 (P2) — التصدير.**

### Functional Requirements (deltas)
- **FR-001**: سطور (voucherNumber/voucherDate/partyName/totalAmount/paymentMethod/بطاقة+حالتها)
- **FR-002**: التفصيل ثلاثي: lines + checks + بطاقة
- **FR-003**: السند الملغي بشارته الواضحة (status من العقد)
- **FR-004**: RC-1..RC-6

### Data Contract
**RevenueCollectionsDetailDto** ✓ (L36636): receiptVoucherId, voucherNumber, voucherDate, partyName, totalAmount, paymentMethod, depositSlipNumber?, depositSlipStatus?, lines: RevenueCollectionsLineDetailDto[], checks: CheckDetailDto[]
**RevenueCollectionsLineDetailDto** ✓ (L36702): revenueAccountId, accountCode, accountName, amount
**RevenueCollectionsReportDto / LineDto / TotalDto** — GAP-READ (L36870/L36792/L36936): تُقرأ قبل التنفيذ (أعمدة القائمة الرئيسية)

### API
GET `/api/Reporting/RevenueCollectionsReports` · GET `/{receiptVoucherId}/detail` · GET `/export`

### Permissions
| Code | Status |
|---|---|
| Reporting.ViewRevenueCollections | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria
- **SC-001**: الفلاتر <2 ثوان
- **SC-002**: التفصيل الثلاثي كامل (بنود/شيكات/بطاقة)

### Tests
T1 الفلاتر · T2 التفصيل الثلاثي · T3 الملغي بشارته · T4 التصدير · T5 الفارغ

### Open Questions
- **OQ-N1** (Engineering): ReportDto/LineDto/TotalDto (GAP-READ) — أعمدة القائمة الرئيسية

### Out of Scope
تحرير (TRE) · الشيكات المفردة (TRE-03)

---

## SPEC RPT-03 — سجل الصرف (disbursement-register-report)

> **CONTRACT NOTE**: كما في RPT-01.

```yaml
id: RPT-03
slug: disbursement-register-report
branch: rpt03-disbursement-register-report
group: reports
priority: P2
depends_on: [PAY-01..03]
permissions_status: "✓ مؤكدة"
oq: {blocking: 0, non_blocking: 1}
```

### User Stories
**US1 (P1) — السجل وعدادات الحالات.** رقابة الصرف بلمحة.
- **Given** فترة، **When** يفتح السجل، **Then** totals (عدادات 5 حالات + مبالغ) + lines
**US2 (P1) — تفصيل أمر.** مدفوعاته الفعلية.
- **Given** أمراً، **When** ينقره، **Then** payments[] + approverName + paidAt
**US3 (P2) — التصدير.**

### Functional Requirements (deltas)
- **FR-001**: صف بطاقات العدادات (draft/submitted/approved/paid/rejected + totalRequests)
- **FR-002**: سطور (orderNumber/orderDate/payeeName/amount/status/fund/approver/paidAt)
- **FR-003**: التفصيل بمدفوعات الأمر
- **FR-004**: سبب الرفض **غير موجود في العقد** — لا يُعرض (لا اختراع)
- **FR-005**: RC-1..RC-6

### Data Contract
**DisbursementRegisterDto** ✓ (L29873): `{fiscalYearId, fiscalYearName, lines[], totals}`
**DisbursementRegisterLineDto** ✓ (L29957): paymentOrderId, orderNumber, orderDate, payeeName, amount, status, fundId, fundCode, fundName, approverId?, approverName?, paidAt?
**DisbursementRegisterTotalDto** ✓ (L30037): totalRequests, draftCount, submittedCount, approvedCount, paidCount, rejectedCount, totalAmount, paidAmount
**DisbursementRegisterDetailDto** ✓ (L29799): paymentOrderId, orderNumber, orderDate, payeeName, amount, status, fundCode, approverName?, paidAt?, payments: PaymentDetailDto[]

### API
GET `/api/Reporting/DisbursementRegisterReports` · GET `/{paymentOrderId}/detail` · GET `/export`

### Permissions
| Code | Status |
|---|---|
| Reporting.ViewDisbursementRegister | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria
- **SC-001**: العدادات مطابقة للسطور (اختبار مقارنة)
- **SC-002**: التفصيل بالمدفوعات

### Tests
T1 العدادات · T2 الفلاتر · T3 التفصيل · T4 التصدير · T5 الفارغ

### Open Questions
- **OQ-N1** (User): سبب الرفض مطلوب في السجل؟ ⇒ يتطلب إضافة عقدية خلفية (لا يُخترع شاشةً)

### Out of Scope
تحرير (PAY)

---

## SPEC RPT-04 — لقطة التوفر (availability-snapshot-report)

> **CONTRACT NOTE**: كما في RPT-01.

```yaml
id: RPT-04
slug: availability-snapshot-report
branch: rpt04-availability-snapshot-report
group: reports
priority: P2
depends_on: [BGT, TRE, PAY]
permissions_status: "✓ مؤكدة"
oq: {blocking: 0, non_blocking: 2}
```

### User Stories
**US1 (P1) — اللقطة بحالة الرقابة.** البنود بحالتها الرقابية بلمحة.
- **Given** سنة/وحدة، **When** يفتح اللقطة، **Then** أسطر بـ controlState بشارة ملونة + الأرقام
**US2 (P1) — تفصيل بند.** الثلاثي الكامل.
- **Given** بنداً، **When** ينقره، **Then** appropriations[] + encumbrances[] + payments[]
**US3 (P2) — التصدير.**

### Functional Requirements (deltas)
- **FR-001**: controlState بشارة ملونة (قيم خادمية — OQ-N1)
- **FR-002**: التفصيل ثلاثي المجموعات
- **FR-003**: breakdown/totals (GAP-READ — OQ-N2)
- **FR-004**: RC-1..RC-6

### Data Contract
**AvailabilitySnapshotDto** ✓ (L22211): budgetItemId, itemCode, itemName, fiscalYearId, fiscalYearName, **controlState**, breakdown: AvailabilitySnapshotLineDto[], totals
**AvailabilitySnapshotDetailDto** ✓ (L22129): budgetItemId, itemCode, itemName, appropriations: AppropriationDetailDto[], encumbrances: EncumbranceDetailDto[], payments: PaymentDetailDto[]
**Line/Total** — GAP-READ (L22296/L22363)

### API
GET `/api/Reporting/AvailabilitySnapshotReports` · GET `/{budgetItemId}/detail` · GET `/export`

### Permissions
| Code | Status |
|---|---|
| Reporting.ViewAvailabilitySnapshot | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria
- **SC-001**: controlState معروض 100%
- **SC-002**: التفصيل الثلاثي كامل

### Tests
T1 الحالة بشارتها · T2 الفلاتر · T3 التفصيل · T4 التصدير · T5 الفارغ

### Open Questions
- **OQ-N1** (Engineering): قيم controlState (حرّة string — وثقها للشارات/الألوان)
- **OQ-N2** (Engineering): Line/Total (GAP-READ)

### Out of Scope
التكسير اللحظي (CTRL-01)

---

## SPEC RPT-05 — ميزان المراجعة (trial-balance-report)

> **CONTRACT NOTE**: كما في RPT-01.

```yaml
id: RPT-05
slug: trial-balance-report
branch: rpt05-trial-balance-report
group: reports
priority: P1
depends_on: [ACC-05 (أرصدة), 023 (قيود)]
permissions_status: "✓ مؤكدة"
oq: {blocking: 0, non_blocking: 1}
```

### User Stories
**US1 (P1) — الميزان.** إثبات توازن الدفاتر.
- **Given** سنة/فترة، **When** يفتح الميزان، **Then** أسطر (accountCode/Name/Type + opening/debitTotal/creditTotal/closing) + totals (4 مجاميع)
- **Given** Σمدين = Σدائن، **When** يُعرض، **Then** شارة توازن (محسوبة شاشةً — موسومة)
- **Given** عدم توازن، **When** يُعرض، **Then** تنبيه أحمر (مؤشر خلل)
**US2 (P1) — حركة حساب.** تتبع مصدر الرصيد.
- **Given** حساباً، **When** يفتح حركته، **Then** entries + totals
**US3 (P2) — التصدير.**

### Functional Requirements (deltas)
- **FR-001**: الأعمدة الخمسة لكل حساب + 4 مجاميع
- **FR-002**: شارة التوازن محسوبة شاشةً (Σ totalDebits = Σ totalCredits) وموسومة "عرض" — **لا isBalanced في هذا العقد** (موجود في TrialBalanceDto القديم فقط)
- **FR-003**: حركة الحساب بمجاميعها
- **FR-004**: فترة غير مغلقة ⇒ تحذير "أرقام قابلة للتغير"
- **FR-005**: RC-1..RC-6

### Data Contract
**TrialBalanceReportDto** ✓ (L38162): `{fiscalYearId, fiscalYearName, fiscalPeriodId?, fiscalPeriodName?, lines[], totals}`
**TrialBalanceLineDto** ✓ (L38084): accountId, accountCode, accountName, accountType, openingBalance, debitTotal, creditTotal, closingBalance
**TrialBalanceTotalDto** ✓ (L38224): totalDebits, totalCredits, totalOpeningBalance, totalClosingBalance
**LedgerMovementDto** ✓ (L32967): accountId, accountCode, accountName, entries: LedgerMovementLineDto[], totals
**LedgerMovementLine/Total** — GAP-READ (L33037/L33097)

### API
GET `/api/Reporting/TrialBalanceReports` · GET `/{accountId}/ledger-movement` · GET `/export`

### Permissions
| Code | Status |
|---|---|
| Reporting.ViewTrialBalanceReport | ✓ |
| Reporting.ExportReports | ✓ |

### Success Criteria
- **SC-001**: الميزان <3 ثوان
- **SC-002**: شارة التوازن صحيحة 100% (اختبار على fixture)
- **SC-003**: حركة حساب <2 ثوان

### Tests
T1 الأعمدة+المجاميع · T2 التوازن/عدمه · T3 الحركة · T4 تحذير الفترة المفتوحة · T5 التصدير · T6 الفارغ

### Open Questions
- **OQ-N1** (Engineering): LedgerMovementLine/Total (GAP-READ)

### Out of Scope
القوائم المالية (RPT-06) · الإغلاق (ACC-05)

---

## SPEC RPT-06 — القوائم المالية (financial-statements)

> **CONTRACT NOTE**: كما في RPT-01. **ميزة مكتشفة**: مبنية على Reports.cs القديم (قوائم قانونية حقيقية — ميزانية/دخل/GL/تدفقات/ميزان قديم) وليست اختراعاً.

```yaml
id: RPT-06
slug: financial-statements
branch: rpt06-financial-statements
group: reports
priority: P1
depends_on: [023 (قيود), ACC-05 (أرصدة)]
permissions_status: "✓ Accounting.Reports (موجود) + ExportReports"
oq: {blocking: 0, non_blocking: 2}
```

### User Stories
**US1 (P1) — الميزانية العمومية.** القائمة القانونية الأولى.
- **Given** تاريخاً (asOfDate)، **When** يفتحها، **Then** assets/liabilities/equity (مجموعات بعناوين ثنائية title/titleAr) + liabilitiesAndEquity + شارة balanced
- **Given** balanced=false، **When** تُعرض، **Then** تنبيه أحمر (خلل بيانات)
**US2 (P1) — قائمة الدخل.** نتيجة الفترة.
- **Given** فترة (startDate/endDate)، **When** يفتحها، **Then** revenue + expenses + netIncome بارزاً
**US3 (P1) — دفتر الأستاذ العام.** السجل التفصيلي مقسّم صفحات.
- **Given** حساباً/فترة، **When** يتصفح GL، **Then** lines (documentDate/entryNumber/reference/narration/accountCode/Name/debit/credit/**runningBalance**) + page/pageSize/totalLines
**US4 (P2) — التدفقات النقدية.**
- **Given** التدفقات، **When** يفتحها، **Then** أقسام (title/titleAr) ببنود (description/amount) ومجاميع
**US5 (P2) — التصدير الموحد.** `/{reportType}/export` لأي قائمة.

### Functional Requirements (deltas)
- **FR-001**: الميزانية: 3 مجموعات + liabilitiesAndEquity + balanced (خادمي)
- **FR-002**: الدخل: فترة حرة + netIncome
- **FR-003**: GL **مقسّم صفحات** (لا حمل كامل) + runningBalance جارٍ
- **FR-004**: التدفقات بأقسامها **ثنائية اللغة** (titleAr يُعرض RTL)
- **FR-005**: الميزان القديم (sections + isBalanced + totalDebit/Credit)
- **FR-006**: تصدير موحد `/{reportType}/export`
- **FR-007**: RC-1..RC-6

### Data Contract
**BalanceSheetDto** ✓ (L22739): asOfDate, currency, assets/liabilities/equity: BalanceSheetGroup, liabilitiesAndEquity, balanced, generatedAt · **BalanceSheetGroup** (L22805): title, titleAr, items, total · **ReportLine** ✓ (L36356): accountCode, accountName, debit, credit, balance
**IncomeStatementDto** ✓ (L32217): startDate, endDate, currency, revenue/expenses: IncomeStatementGroup, netIncome, generatedAt
**GeneralLedgerDto** ✓ (L31737): currency, totalLines, page, pageSize, lines: GeneralLedgerLine[], totals, generatedAt · **Line**: documentDate, entryNumber, reference?, narration?, accountCode, accountName, debit, credit, runningBalance
**CashFlowStatementDto** — جزئي (L24926 GAP-READ): أقسام CashFlowSection {title, titleAr, items: CashFlowLineItem[] {description, amount}, total}
**TrialBalanceDto (قديم)** ✓ (L38006): fiscalYearId, fiscalYearName, fiscalPeriodId, periodName, sections: ReportSection[], totalDebit, totalCredit, isBalanced, currency, generatedAt

### API
GET `/api/Reports/balance-sheet` · `/income-statement` · `/general-ledger` · `/cash-flow` · `/trial-balance` · GET `/api/Reports/{reportType}/export`

### Permissions
| Code | Status |
|---|---|
| Accounting.Reports | ✓ (موجود) |
| Reporting.ExportReports | ✓ |

### Success Criteria
- **SC-001**: الميزانية بمجموعاتها وشارة توازنها <3 ثوان
- **SC-002**: GL يتصفح صفحات دون تجميد
- **SC-003**: كل titleAr يعرض RTL سليماً

### Tests
T1 الميزانية+balanced · T2 الدخل+netIncome · T3 GL pagination+runningBalance · T4 التدفقات بالأقسام · T5 الميزان القديم · T6 التصدير الموحد · T7 الفارغ

### Open Questions
- **OQ-N1** (Engineering): جسم CashFlowStatementDto كاملاً (GAP-READ)
- **OQ-N2** (Engineering): فلاتر query لكل قائمة (تاريخ/حساب/فترة) — من signatures

### Out of Scope
الميزان التشغيلي (RPT-05) · الحسابات النهائية (CTRL-03)


---

# المجموعة: الأمان (SEC)

---

## SPEC SEC-01 — الصلاحيات (permissions)

> **CONTRACT NOTE**: العقد أدناه ملزم حرفياً — ليس تفاصيل تنفيذ.

```yaml
id: SEC-01
slug: permissions
branch: sec01-permissions
group: security
priority: P3
depends_on: ["RBAC (features/security/rbac — موجود)"]
permissions_status: "✓ Permissions.View"
oq: {blocking: 0, non_blocking: 1}
```

### User Stories
**US1 (P1) — كتالوج الصلاحيات.** مدير الأمان يرى كل الأكواد بمستوياتها ونطاقاتها — مرجع تعريف الأدوار.
- **Given** صلاحيات مسجلة، **When** يفتح الكتالوج بفلاتر (module/action/isSensitive/isActive)، **Then** سطور (code/name/module/action/permissionLevel/dataScope + شارة isSensitive)
- **Given** الشاشة، **When** يبحث عن تحرير، **Then** لا يوجد (قراءة فقط حصراً)
**US2 (P2) — تفاصيل صلاحية.**
- **Given** صلاحية، **When** ينقرها، **Then** كل حقولها + description

### Edge Cases
كتالوج فارغ (نظام بلا صلاحيات — رسالة صريحة) · غير مصرح (مخفي) · module غريب (فلتر يعرضه)

### Functional Requirements
- **FR-001**: قائمة بفلاتر (module/action/sensitive/active)
- **FR-002**: شارة isSensitive + عرض permissionLevel/dataScope
- **FR-003**: قراءة فقط — صفر أزرار تحرير
- **FR-004**: الكود بصيغة {Module}.{Action} معروض كما هو

### Data Contract
**SecurityPermissionDto** ✓ (L37302): id, module, action, code, name, description?, permissionLevel, isSensitive (bool), dataScope, isActive

### Lifecycle
isActive فقط — لا تحولات (قراءة فقط شاشةً)

### Permissions
| Code | Status |
|---|---|
| Permissions.View | ✓ |

### API
GET `/api/Security/Permissions` · GET `/{id}`

### Business Rules
- **BR-1** الكتالوج يغذي RBAC (AssignRolePermissionCommand يستخدم permissionId)
- **BR-2** الحساسية ظاهرة للفرز البصري

### UI/UX
`/security/permissions`: جدول + فلاتر + تفاصيل (drawer/صفحة) · بلا أزرار تحرير · الحالات: CC-2

### Success Criteria
- **SC-001**: الكتالوج كامل <2 ثوان
- **SC-002**: صفر أزرار تحرير (اختبار)

### Tests
T1 الفلاتر · T2 الشارات (sensitive/level/scope) · T3 قراءة فقط · T4 الحجب

### Open Questions
- **OQ-N1** (Engineering): قيم permissionLevel/dataScope (حرّة string — وثقها للعرض)

### Out of Scope
إدارة الأدوار/التعيينات (RBAC موجود) · قواعد الاعتماد (SEC-02)

---

## SPEC SEC-02 — قواعد الاعتماد والتفويضات (approval-rules-delegations)

> **CONTRACT NOTE**: كما في SEC-01.

```yaml
id: SEC-02
slug: approval-rules-delegations
branch: sec02-approval-rules-delegations
group: security
priority: P1
depends_on: [RBAC (أدوار), 024 (عملات/صناديق), كل lifecycle (مستهلك), WF-01 (useApprovalRules)]
blocked_by: []
consumers: [PAY-02 (الدور المؤهل), WF-01/02, كل الموافقات]
permissions_status: "✓ مؤكدة (4)"
oq: {blocking: 0, non_blocking: 2}
```

### User Stories
**US1 (P1) — إدارة القواعد المتسلسلة.** القواعد تحكم من يعتمد ماذا ومتى.
- **Given** نوع وثيقة وسقفاً ودوراً، **When** ينشئ قاعدة (documentType/fundId?/amountThreshold?/currencyId?/approverRole/sequence)، **Then** تُحفظ بتسلسلها
- **Given** قاعدة نشطة، **When** يعطلها (PATCH deactivate)، **Then** تسقط من التقييم
- **Given** قاعدتين لنفس النوع، **When** تُقيمان، **Then** الترتيب حسب sequence
**US2 (P1) — طابور الموافقات المعلقة.** نقطة عمل واحدة لكل المنتظر.
- **Given** موافقات معلقة، **When** يفتح الطابور (GET /pending)، **Then** سطور (documentType/documentId/amount/submittedByUserId/submittedAt/requiredRole)
- **Given** بنداً، **When** ينقره، **Then** شاشة الوثيقة المرتبطة تفتح (خريطة documentType→route)
**US3 (P2) — التفويضات.** الغياب لا يوقف الموافقات.
- **Given** مفوِّضاً ومفوَّضاً وفترة، **When** ينشئ تفويضاً (entityType?/canReDelegate/reason?)، **Then** Active
- **Given** تفويضاً فعّالاً، **When** يلغيه (PATCH revoke)، **Then** Revoked
- **Given** تفويضاً تجاوز endDate، **When** يُعرض، **Then** Expired

### Edge Cases
تسلسل مكرر لنفس (نوع+صندوق+سقف) (قاعدة خادمية) · تفويض لنفس الشخص (رفض) · startDate > endDate (رفض تحقق) · قاعدة بلا amountThreshold (تلتقط الكل — تعرض كذلك) · وثيقة بالطابور بلا صلاحية رؤية (رسالة) · تفويض منتهٍ يُستخدم (خادمي)

### Functional Requirements
- **FR-001**: CRUD القواعد + deactivate (PATCH) — sequence يحدد الترتيب
- **FR-002**: الطابور الموحد GET /pending — يفتح الوثيقة المرتبطة
- **FR-003**: التفويضات: إنشاء (بفترة + canReDelegate + reason?) + revoke
- **FR-004**: حالات التفويض الثلاث (Active/Expired/Revoked) بشاراتها
- **FR-005**: منع تفويض النفس (تحقق نموذج + خادمي)

### Data Contract
**ApprovalRuleDto** ✓ (L21338): id, documentType, fundId?, amountThreshold?, currencyId?, approverRole?, approverRoleId?, sequence, isActive
**CreateApprovalRuleCommand** ✓ (L26218): documentType, fundId?, amountThreshold?, currencyId?, approverRole?, sequence — **بلا approverRoleId** (OQ-N1)
**ApprovalDelegationDto** ✓ (L21258): id, delegatorUserId, delegateUserId, entityType?, startDate, endDate, status (enum DelegationStatus: **Active/Expired/Revoked** — L29531), canReDelegate, reason?
**CreateApprovalDelegationCommand** ✓ (L26149): delegatorUserId, delegateUserId, entityType?, startDate, endDate, canReDelegate, reason?
**PendingApprovalDto** ✓ (L34862): documentType, documentId, amount, submittedByUserId, submittedAt, requiredRole

### Lifecycle
Rule: isActive (+ deactivate) · Delegation: `Active → Expired (زمنياً خادمياً) / Revoked (revoke)`

### Permissions
| Code | Status |
|---|---|
| ApprovalRules.View / ApprovalRules.Manage | ✓ |
| ApprovalDelegations.View / ApprovalDelegations.Manage | ✓ |

### API
GET/POST `/api/Security/ApprovalRules` · GET/PUT `/{id}` · PATCH `/{id}/deactivate` · GET `/pending` · GET/POST `/api/Security/ApprovalDelegations` · PATCH `/{id}/revoke`

### Business Rules
- **BR-1** التقييم: (documentType × fundId × amountThreshold × currencyId) → approverRole بترتيب sequence
- **BR-2** الطابور موحد لكل أنواع الوثائق — requiredRole يحدد من يرى (OQ-N2)
- **BR-3** التفويض المنتهي يتحول Expired خادمياً (لا إجراء شاشة)
- **BR-4** revoke نهائي

### UI/UX
- `/security/approval-rules`: جدول (نوع/صندوق/سقف/عملة/دور/تسلسل/نشط) + نافذة إنشاء/تعديل + زر تعطيل
- `/security/pending-approvals`: جدول + عمود "فتح" (خريطة documentType→route)
- `/security/approval-delegations`: جدول (مفوِّض/مفوَّض/نوع/فترة/حالة/إعادة تفويض) + نافذة إنشاء (منتقيا مستخدمين + فترة + toggle + سبب) + زر إلغاء
- الحالات: CC-2 + شارات الحالات الثلاث

### Success Criteria
- **SC-001**: القواعد بمتسلسلاتها تُقيَّم فعلياً (اختبار تكاملي مع دورة اعتماد)
- **SC-002**: كل بند طابور يفتح وثيقته بنقرة
- **SC-003**: كل تفويض بحالته الصحيحة

### Tests
T1 قواعد CRUD+تعطيل+تسلسل · T2 الطابور+الفتح · T3 تفويض CRUD+الحالات الثلاث+revoke · T4 تفويض النفس مرفوض · T5 الحجب

### Open Questions
- **OQ-N1** (Engineering): approverRoleId في DTO وغائب عن Create — كيف يُستنتج؟
- **OQ-N2** (User/Engineering): من يرى الطابور — هل requiredRole يفلتر العرض خادمياً؟

### Out of Scope
تنفيذ الموافقات (كل lifecycle) · محرك التقييم الخادمي · أدوار RBAC

---

# المجموعة: سير العمل (WF)

---

## SPEC WF-01 — تعريفات سير العمل (workflow-definitions)

> **CONTRACT NOTE**: كما في SEC-01.

```yaml
id: WF-01
slug: workflow-definitions
branch: wf01-workflow-definitions
group: workflow
priority: P2
depends_on: [RBAC (أدوار الخطوات), SEC-02 (useApprovalRules)]
blocked_by: []
consumers: [WF-02]
permissions_status: GAP-ADD (Workflow family absent)
oq: {blocking: 0, non_blocking: 2}
```

### User Stories
**US1 (P1) — إدارة التعريفات.** كل كيان له مسار موافقة معرف بنسخة.
- **Given** كياناً (entityName)، **When** ينشئ تعريفاً (version=1 + description + خطوات)، **Then** Draft
- **Given** Draft بخطوات، **When** يفعّله، **Then** Active
- **Given** Active مستخدماً بحالات، **When** يحاول حذفه، **Then** رفض خادمي
- **Given** تعريفاً غير مستخدم، **When** يحذفه (تأكيد)، **Then** يُحذف
**US2 (P1) — محرر الخطوات.** الخطوات تحمل المنطق.
- **Given** خطوة، **When** يضيفها (stepOrder/stepType/name/assignedRoleId/useApprovalRules/conditionExpression/timeoutHours/escalateToStepId)، **Then** تُحفظ بترتيبها
- **Given** خطوة بـ useApprovalRules=true، **When** تُحفظ، **Then** ترتبط بقواعد SEC-02 (بدون assignedRoleId إلزامي)
- **Given** تصعيداً لخطوة سابقة (دائري)، **When** يحفظ، **Then** رفض

### Edge Cases
تفعيل بلا خطوات (رفض) · شرط غير صالح (تحقق — OQ-N2) · تصعيد دائري · حذف Active مستخدم · نسختان Active لنفس entityName (OQ-N1) · stepOrder مكرر (إعادة ترتيب)

### Functional Requirements
- **FR-001**: CRUD التعريفات + الحالات (Draft/Active/Inactive/Archived) + version
- **FR-002**: محرر خطوات: ترتيب (سحب/أرقام) + كل حقول الخطوة
- **FR-003**: منع التفعيل بلا خطوات
- **FR-004**: منع التصعيد الدائري (تحقق نموذج)
- **FR-005**: منع حذف Active مستخدم (خادمي — الرسالة تعرض)
- **FR-006**: useApprovalRules يبدل مصدر المكلف (قواعد SEC-02 بدل assignedRoleId)

### Data Contract
**WorkflowDefinitionDto** ✓ (L41145): id, entityName, version, status (enum: **Draft/Active/Inactive/Archived**), description?, isActive, created/createdBy/lastModified/lastModifiedBy, steps: WorkflowStepDto[]
**WorkflowStepDto** ✓ (L41423): id, definitionId, stepOrder, stepType, name, description?, assignedRoleId?, useApprovalRules (bool), conditionExpression?, timeoutHours?, escalateToStepId?, isActive
**CreateWorkflowDefinitionCommand** (L29233) / **UpdateWorkflowDefinitionCommand** (L40569) — تُقرأ للتأكيد (GAP-READ خفيف: الشكل = رأس + خطوات)

### Lifecycle
`Draft → Active → Inactive → Archived` (+ isActive toggle) — الحذف لغير المستخدم

### Permissions
| Code | Status |
|---|---|
| Workflow.Definitions.View/Create/Update/Delete | GAP-ADD (عائلة Workflow غائبة) |

### API
GET/POST `/api/Workflow/WorkflowDefinitions` · GET/PUT/DELETE `/{id}`

### Business Rules
- **BR-1** نسخة واحدة Active لكل entityName (ASSUMED — OQ-N1)
- **BR-2** الخطوات بترتيب stepOrder صارم
- **BR-3** التعديل على Active ينشئ نسخة؟ (سلوك خادمي — OQ-N1)

### UI/UX
- `/workflow/definitions`: جدول (كيان/نسخة/حالة/عدد خطوات/نشط) + فلاتر
- نافذة إنشاء/تعديل: رأس (entityName/description) + **محرر خطوات** (قائمة مرتبة: إضافة/حذف/سحب ترتيب + حقول كل خطوة + toggle useApprovalRules يبدل حقل الدور + select تصعيد من الخطوات الأخرى)
- أزرار حالة مشروطة (تفعيل/تعطيل/أرشفة/حذف)
- الحالات: CC-2 + تحقق التصعيد الدائري inline

### Success Criteria
- **SC-001**: تعريف بخطواته يُنشأ ويُفعّل <5 دقائق
- **SC-002**: صفر تفعيل بلا خطوات
- **SC-003**: كل منطق خطوة (شرط/مهلة/تصعيد) محفوظ كما أدخل

### Tests
T1 CRUD+الحالات · T2 محرر الخطوات بالترتيب · T3 رفض التفعيل بلا خطوات · T4 رفض التصعيد الدائري · T5 useApprovalRules يبدل الدور · T6 حذف الممنوع

### Open Questions
- **OQ-N1** (Engineering): قواعد النسخ (واحدة Active لكل كيان؟ تعديل Active ينشئ نسخة؟)
- **OQ-N2** (Engineering): صيغة conditionExpression وخطوة التحقق (شاشة/خادم) · قيم stepType

### Out of Scope
تشغيل الحالات (WF-02) · القواعد نفسها (SEC-02)

---

## SPEC WF-02 — الحالات والتاريخ (workflow-instances-history)

> **CONTRACT NOTE**: كما في SEC-01.

```yaml
id: WF-02
slug: workflow-instances-history
branch: wf02-workflow-instances-history
group: workflow
priority: P2
depends_on: [WF-01 (تعريف Active), SEC-02 (المكلف), الوثائق المرتبطة]
permissions_status: GAP-ADD
oq: {blocking: 0, non_blocking: 2}
```

### User Stories
**US1 (P1) — بدء حالة.** تشغيل المسار على وثيقة.
- **Given** تعريفاً Active ووثيقة، **When** يبدأ حالة (definitionId/entityName/entityId/reason?)، **Then** instance بـ currentStepId وstartedAt
- **Given** تعريفاً غير Active، **When** يحاول البدء، **Then** رفض
**US2 (P1) — قرارات الخطوات.** المكلف يعتمد/يرفض.
- **Given** حالة Running، **When** المكلف يعتمد الخطوة الحالية (reason?)، **Then** التاريخ يسجل {actorUserId, decision, reason, evaluationSnapshot?, timestamp} وتتقدم currentStepId
- **Given** نفس الحالة، **When** يرفض (reason?)، **Then** تنتهي مرفوضة
- **Given** خطوة غير حالية أو غير مكلف، **When** يحاول قراراً، **Then** رفض
**US3 (P2) — الإلغاء والمراقبة.**
- **Given** حالة جارية، **When** تلغى (reason?)، **Then** تنتهي + completedAt
- **Given** حالة، **When** يفتح تاريخها، **Then** كل الخطوات بقراراتها ولقطات تقييمها (قابلة للطي)

### Edge Cases
قرار خطوة غير حالية · غير مكلف · إلغاء مكتملة · وثيقة مرتبطة حُذفت (الرابط يعرض معرفاً خاماً) · evaluationSnapshot طويل (طي) · حالتان لنفس الوثيقة (قاعدة خادمية؟)

### Functional Requirements
- **FR-001**: بدء حالة (الأمر الكامل) + منع غير Active
- **FR-002**: اعتماد/رفض **الخطوة الحالية فقط** (POST /{id}/steps/{stepId}/approve|reject)
- **FR-003**: إلغاء (POST /{id}/cancel)
- **FR-004**: التاريخ الكامل (GET /WorkflowHistory/instances/{instanceId}/history) — append-only
- **FR-005**: مخطط خطوات بصري (currentStepId مميز) + evaluationSnapshot قابل للطي
- **FR-006**: فتح الوثيقة المرتبطة (entityName/entityId → خريطة route)

### Data Contract
**WorkflowInstanceDto** ✓ (L41330): id, definitionId, entityName, entityId, currentStepId?, status (حر — OQ-N1), startedAt, completedAt?, created/createdBy/lastModified/lastModifiedBy, history: WorkflowHistoryDto[]
**WorkflowHistoryDto** ✓ (L41231): id, workflowInstanceId, stepId, actorUserId, decision, reason?, evaluationSnapshot?, timestamp
**StartWorkflowInstanceCommand** ✓ (L37561): `{definitionId, entityName, entityId, reason?}`
**StepDecisionRequest** ✓ (L37611): `{reason?}` · **CancelWorkflowInstanceRequest** ✓ (L24683): `{reason?}`

### Lifecycle
status حر (قيم خادمية — OQ-N1). نمط متوقع: `Running → Completed / Rejected / Cancelled` — تُوثق القيم الفعلية قبل بناء الشارات

### Permissions
| Code | Status |
|---|---|
| Workflow.Instances.View/Start/Decide/Cancel | GAP-ADD |
| Workflow.History.View | GAP-ADD |

### API
GET `/api/Workflow/WorkflowInstances` · GET `/{id}` · POST (start) · POST `/{id}/steps/{stepId}/approve` · POST `/{id}/steps/{stepId}/reject` · POST `/{id}/cancel` · GET `/api/Workflow/WorkflowHistory/instances/{instanceId}/history`

### Business Rules
- **BR-1** القرار للخطوة الحالية وللمكلف فقط (تحقق خادمي — الرفض يعرض)
- **BR-2** التاريخ append-only — لا تحرير شاشة
- **BR-3** evaluationSnapshot يوثق تقييم الشروط (لماذا مرت الخطوة)
- **BR-4** reason اختياري بالعقد — إلزامية الرفض قرار شاشة (يوثق في plan)

### UI/UX
- `/workflow/instances`: جدول (كيان/وثيقة/حالة/خطوة حالية/startedAt) + فلاتر
- `/{id}`: رأس + **مخطط الخطوات** (الحالية مميزة + المكلف/الدور لكل خطوة) + أزرار اعتماد/رفض (حوار reason) + زر إلغاء (حوار reason) + رابط الوثيقة
- قسم التاريخ: خط زمني (خطوة/فاعل/قرار/سبب/وقت + لقطة تقييم قابلة للطي)
- الحالات: CC-2

### Success Criteria
- **SC-001**: كل حالة مرتبطة بوثيقتها قابلة للفتح
- **SC-002**: كل قرار بفاعله ووقته (+لقطة التقييم إن وجدت)
- **SC-003**: صفر قرار لغير الحالية/غير المكلف ينجح

### Tests
T1 بدء+منع غير Active · T2 اعتماد/رفض الحالية · T3 رفض غير المكلف/غير الحالية · T4 الإلغاء · T5 التاريخ بلقطاته · T6 رابط الوثيقة

### Open Questions
- **OQ-N1** (Engineering): قيم status الفعلية (للشارات والمخططات)
- **OQ-N2** (User): reason إلزامي للرفض شاشةً رغم اختياريته بالعقد؟

### Out of Scope
التعريفات (WF-01) · محرك التقييم (خادمي)


---

# المجموعة: اللجان (COM)

---

## SPEC COM-01 — اللجان (committees)

> **CONTRACT NOTE**: العقد أدناه ملزم حرفياً — ليس تفاصيل تنفيذ.

```yaml
id: COM-01
slug: committees
branch: com01-committees
group: committees
priority: P2
depends_on: ["المشتريات (purchaseOrderId — مرجع)", "022/HR (أعضاء — OQ-N3)"]
permissions_status: "✓ مؤكدة (6 لجان) + GAP-ADD (أعضاء/تكليفات)"
oq: {blocking: 0, non_blocking: 3}
```

### User Stories
**US1 (P1) — إدارة اللجان.** لجنة المشتريات قرار إداري موثق.
- **Given** قرار تشكيل، **When** ينشئ لجنة (committeeNumber/name/committeeType/formationDecisionNumber/formationDecisionDate/validFrom/validTo?)، **Then** تُحفظ وتظهر
- **Given** لجنة نشطة، **When** يفعّل/يعطّل، **Then** الحالة تنعكس
- **Given** لجنة، **When** يحلها (تأكيد نهائي)، **Then** محلولة — لا تكليف بعدها
**US2 (P1) — الأعضاء.**
- **Given** لجنة، **When** يضيف/يعدّل/يفعّل/يعطّل أعضاء، **Then** كل عملية تُحفظ (CommitteeMemberDto)
**US3 (P1) — التكليفات والتواقيع.** الامتثال الإجرائي (مناقصة/استلام/فحص).
- **Given** لجنة نشطة وأمراً شراء، **When** يكلّفها (assignmentType/purchaseOrderId/decisionNumber?/decisionDate?/requiredSignaturesCount)، **Then** تكليف بعدّاد (0/N)
- **Given** تكليفاً، **When** يسجل توقيعاً (POST /{id}/signature — واحد لكل استدعاء)، **Then** actualSignaturesCount يزيد
- **Given** اكتمال العدد، **When** يتحقق، **Then** Completed
- **Given** عدداً مكتملاً، **When** يحاول توقيعاً إضافياً، **Then** رفض خادمي
- **Given** لجنة خارج صلاحيتها (validTo تجاوز)، **When** يحاول تكليفها، **Then** رفض

### Edge Cases
توقيع فوق العدد · تكليف لجنة محلولة/منتهية · حل لجنة بتكليفات نشطة (قاعدة خادمية) · توقيع مكرر لنفس العضو (OQ-N2) · تكليف بلا purchaseOrderId (نوع Receiving/Inspection — مسموح؟ ○ في العقد) · تحديث حالة التكليف يدوياً (PUT — أي حالات؟)

### Functional Requirements
- **FR-001**: CRUD اللجان + activate/deactivate/dissolve (نهائي بتأكيد)
- **FR-002**: CRUD الأعضاء + activate/deactivate
- **FR-003**: التكليفات: إنشاء بأنواعها الثلاثة + PUT تحديث حالة + عداد التواقيع
- **FR-004**: RecordSignature واحد لكل استدعاء — العداد يرتفع حتى requiredSignaturesCount ثم Completed
- **FR-005**: منع: توقيع زائد، تكليف محلولة/منتهية
- **FR-006**: حالات التكليف الخمس بشاراتها (Draft/Assigned/InProgress/Completed/Cancelled)

### Data Contract
**CommitteeDto** — جزئي ✓ (L25680، GAP-READ للبقية): id, committeeNumber, name, committeeType (enum — GAP-READ), formationDecisionNumber, formationDecisionDate, validFrom, validTo?, …
**CommitteeMemberDto** — GAP-READ (L25768)
**CommitteeAssignmentDto** ✓ (L25567): id, committeeId, assignmentType (enum: **Tender/Receiving/Inspection**), purchaseOrderId?, assignmentDate, decisionNumber?, decisionDate?, status (enum: **Draft/Assigned/InProgress/Completed/Cancelled**), requiredSignaturesCount, actualSignaturesCount, notes?, created, createdBy
**CreateCommitteeAssignmentCommand** ✓ (L26933): committeeId, assignmentType, purchaseOrderId?, assignmentDate, decisionNumber?, decisionDate?, requiredSignaturesCount, notes?
**RecordSignatureCommand** ✓ (L35681): `{id}`

### Lifecycle
Committee: نشطة ⇄ معطلة → **محلولة (نهائي)** · Assignment: `Draft → Assigned → InProgress → Completed / Cancelled`

### Permissions
| Code | Status |
|---|---|
| Committees.View/Create/Update/Activate/Deactivate/Dissolve | ✓ مؤكدة الست |
| أكواد الأعضاء/التكليفات (Members/Assignments) | GAP-ADD — تحقق |

### API
CRUD `/api/Committees/Committees` + `/{id}/activate|deactivate|dissolve` · CRUD `/api/Committees/CommitteeMembers` + `/{id}/activate|deactivate` · GET/POST `/api/Committees/CommitteeAssignments` + PUT `/{id}` + POST `/{id}/signature`

### Business Rules
- **BR-1** التواقيع تتراكم حتى المطلوب — الاكتمال يحجب المزيد
- **BR-2** صلاحية اللجنة (validFrom/validTo) تحكم التكليف
- **BR-3** الحل نهائي — اللجنة المحلولة للقراءة

### UI/UX
- `/committees`: جدول (رقم/اسم/نوع/قرار تشكيل/صلاحية/حالة) + فلاتر
- `/{id}`: تبويبات — بيانات · أعضاء (شبكة CRUD) · تكليفات (شبكة: نوع/PO/قرار/تاريخ + **عداد تواقيع N/M** + زر توقيع مشروط + PUT حالة)
- حوار حل (تأكيد نهائي بخطورة) · الحالات: CC-2 + شارة العداد المكتمل

### Success Criteria
- **SC-001**: كل تكليف بعداده المطلوب والفعلي
- **SC-002**: صفر توقيع بعد الاكتمال
- **SC-003**: كل حل بتأكيد نهائي

### Tests
T1 لجان CRUD+الحل · T2 أعضاء CRUD · T3 تكليف بالأنواع الثلاثة · T4 عداد التواقيع حتى الاكتمال+المنع · T5 تكليف منتهية/محلولة مرفوض · T6 الحجب

### Open Questions
- **OQ-N1** (Engineering): CommitteeDto كاملة + CommitteeMemberDto + CommitteeType enum (GAP-READ)
- **OQ-N2** (User): التوقيع المكرر لنفس العضو — ممنوع؟
- **OQ-N3** (User): مصدر الأعضاء (Users؟ Parties؟ HR Employees؟)

### Out of Scope
المشتريات نفسها (purchaseOrderId مرجع خارجي)

---

# المجموعة: مصرفي (BANK)

---

## SPEC BANK-01 — الكشوف والتسويات البنكية (bank-statements-reconciliation)

> **CONTRACT NOTE**: العقد أدناه ملزم حرفياً — ليس تفاصيل تنفيذ.

```yaml
id: BANK-01
slug: bank-statements-reconciliation
branch: bank01-bank-statements-reconciliation
group: banking
priority: P2
depends_on: [PAY-04 (حسابات), 023 (قيود الربط), ACC-03 (journalId)]
permissions_status: "✓部分 (BankStatements 3) + GAP-ADD (Reconciliations)"
oq: {blocking: 0, non_blocking: 4}
```

### User Stories
**US1 (P1) — إنشاء/استيراد كشف.** مرجع البنك الحقيقي بأرصدته الثلاثة.
- **Given** حساباً بنكياً، **When** ينشئ/يستورد كشفاً (statementDate/balanceStart/balanceEnd + importSource عند الاستيراد)، **Then** balanceEndComputed معروض + **تنبيه عند عدم تطابق balanceEnd وbalanceEndComputed**
- **Given** كشف مستورداً، **When** يفتح بنوده، **Then** سطور (transactionDate/description/debit/credit/balance/reference/isReconciled)
**US2 (P1) — تسوية البنود مع القيود.** كل حركة بنك ترتبط بقيد.
- **Given** بنداً غير مسوّى، **When** يربطه بقيد يومية (journalEntryLineId)، **Then** isReconciled=true
- **Given** بنداً مسوّى، **When** يحاول إعادة الربط، **Then** محجوب (OQ-N4)
**US3 (P1) — التسوية البنكية الرسمية.** إثبات بأربعة أرصدة.
- **Given** كشف وحساباً، **When** ينشئ تسوية (reconciliationDate/bookBalance/statementBalance)، **Then** Pending + adjustedBalance/difference معروضان
- **Given** Pending، **When** يعتمدها المراجع، **Then** Approved (+approvedById)
- **Given** Approved، **When** يكملها، **Then** Completed · وعند الرفض (reason)، **Then** Rejected
**US4 (P2) — بنود التسوية + إلغاء كشف.**
- **Given** تسوية، **When** يضيف بنداً يدوياً، **Then** يُحفظ (BankReconciliationLineDto — GAP-READ)
- **Given** كشف بمسودة، **When** يلغيه (POST /{id}/cancel)، **Then** حالة الإلغاء واضحة

### Edge Cases
استيراد برصيد غير متطابق (تنبيه — شدته OQ-N3) · تسوية difference ≠ 0 عند الإكمال (قاعدة — OQ-N4) · رفض ثم إعادة إنشاء · كشف ملغي ببنود مسوّاة (أثرها — OQ) · تعارض RowVersion · بند بلا قيد مطابق (يبقى isReconciled=false)

### Functional Requirements
- **FR-001**: كشوف: قائمة/إنشاء/استيراد (importSource)/إلغاء + بنودها (GET/POST lines)
- **FR-002**: تحقق الرصيد: balanceEnd مقابل balanceEndComputed — تنبيه عدم التطابق
- **FR-003**: تسوية البنود: ربط journalEntryLineId + isReconciled
- **FR-004**: تسويات: قائمة/إنشاء/بنود + دورة approve→complete / reject(reason)
- **FR-005**: difference بارز (adjustedBalance − statementBalance أو كما يحسبه الخادم — يعرض كما يصل)
- **FR-006**: الكشف يرتبط بـ bankAccountId وjournalId (من ACC-03)

### Data Contract
**BankStatementDto** ✓ (L23172): id, name, bankAccountId, bankAccountName?, journalId?, statementDate, balanceStart, balanceEnd, **balanceEndComputed**, importSource?, status (قيم — OQ-N3)
**BankStatementLineDto** ✓ (L23260): id, statementId, lineNumber, transactionDate, description?, debit, credit, balance?, reference?, **isReconciled**, **journalEntryLineId?**
**BankReconciliationDto** ✓ (L23007): id, bankAccountId, bankAccountName?, statementId, reconciliationDate, bookBalance, statementBalance, adjustedBalance, difference?, status, preparedById, approvedById?
**CreateBankReconciliationCommand** ✓ (L26333): bankAccountId, statementId, reconciliationDate, bookBalance, statementBalance, preparedById
**BankReconciliationLineDto** — GAP-READ (L23087) · **ImportBankStatementCommand** — GAP-READ (L32151: صيغة الاستيراد)
**RejectBankReconciliationCommand** ✓ (L35870): `{id, reason?}`

### Lifecycle
Statement: قيم status خادمية (OQ-N3) — نمط: Draft/Imported → Reconciled / Cancelled
Reconciliation: `Pending → Approved → Completed` · `Pending/Approved → Rejected (reason)`

### Permissions
| Code | Status |
|---|---|
| BankStatements.View/Create/Import | ✓ مؤكدة |
| BankReconciliations.View/Create/Approve/Complete/Reject | GAP-ADD |

### API
GET/POST `/api/Banking/BankStatements` · GET `/{id}` · POST `/import` · POST `/{id}/reconcile|cancel` · GET/POST `/{id}/lines` · GET/POST `/api/Banking/BankReconciliations` · GET `/{id}` · POST `/{id}/approve|complete|reject` · GET/POST `/{id}/lines`

### Business Rules
- **BR-1** عدم تطابق الرصيد المعلن/المحسوب = تنبيه بارز قبل أي تسوية
- **BR-2** البند المسوّى لا يُعاد ربطه (ASSUMED — OQ-N4)
- **BR-3** التسوية الرسمية بأربعة أرصدة ودورة اعتماد
- **BR-4** الإلغاء لا يحذف البنود المسوّاة (تاريخ)

### UI/UX
- `/banking/statements`: جدول (اسم/حساب/تاريخ/balanceStart/End/Computed+شارة تطابق/حالة) + إنشاء/استيراد (حوار) + إلغاء
- `/{id}`: بنود (مدين/دائن/رصيد/مرجع + toggle isReconciled + منتقي قيد للربط) + إضافة بند
- `/banking/reconciliations`: جدول (حساب/كشف/تاريخ/book/statement/adjusted/difference بارز/حالة) + إنشاء (حوار بالحقول الست) + بنود + أزرار الدورة (تأكيدات + reason للرفض)
- الحالات: CC-2 + شارة تطابق الرصيد + فرق ملون

### Success Criteria
- **SC-001**: كل استيراد بأرصدته الثلاثة وشارة التطابق
- **SC-002**: كل بند مسوّى برابط قيده
- **SC-003**: كل تسوية بفرقها ودورتها المكتملة

### Tests
T1 إنشاء/استيراد+تحقق الرصيد · T2 تسوية بنود (ربط+toggle) · T3 الدورة (approve/complete/reject+reason) · T4 الأرصدة والفرق · T5 إلغاء كشف · T6 الفارغ

### Open Questions
- **OQ-N1** (Engineering): ImportBankStatementCommand (L32151 — صيغة الاستيراد: ملف؟ لصق؟)
- **OQ-N2** (Engineering): BankReconciliationLineDto (L23087)
- **OQ-N3** (Engineering): قيم status للكشف والتسوية
- **OQ-N4** (User/Engineering): شرط difference=0 عند الإكمال؟ وإعادة ربط بند مسوّى؟

### Out of Scope
الحسابات (PAY-04) · الترحيل · تسوية أرصدة المحاسبة (ACC-05 reconcile — مختلفة)

---

# المجموعة: نظم (SYS)

---

## SPEC SYS-01 — لوحة التحكم (dashboard)

> **CONTRACT NOTE**: لا DTO جديد — يعيد استخدام عقود RPT-01/RPT-03/SYS-02 حرفياً.

```yaml
id: SYS-01
slug: dashboard
branch: sys01-dashboard
group: system
priority: P2
depends_on: [RPT-01, RPT-03, SYS-02]
blocked_by: ["OQ-B1 (قرار المستخدم — آلية البناء)"]
permissions_status: "مركّبة من صلاحيات المصادر"
oq: {blocking: 1, non_blocking: 0}
contract_gap: "لا endpoint /api/Dashboard — يؤكد القرار B1"
```

### User Stories
**US1 (P1) — بطاقات المؤشرات.** الشاشة الأولى تجمع الحالة دون تنقل.
- **Given** بيانات الفترة الجارية، **When** يفتح الرئيسية، **Then** بطاقات: إجمالي التفويضات/الالتزامات/المتاح (من RPT-01 totals) + عدادات حالات الصرف الخمسة وtotalAmount/paidAmount (من RPT-03 totals)
- **Given** بطاقة، **When** ينقرها، **Then** يفتح تقريرها المصدر بفلاترها
**US2 (P2) — الصمود الجزئي.**
- **Given** فشل استعلام بطاقة، **When** تُعرض اللوحة، **Then** البطاقة تعرض خطأها+retry والبقية تعمل
**US3 (P2) — غير المصرح.**
- **Given** مستخدماً بلا صلاحيات التقارير، **When** يفتح اللوحة، **Then** فارغ صريح (لا أخطاء حمراء)

### Edge Cases
فشل استعلام واحد (عزل) · مستخدم بلا صلاحيات (فارغ) · بطء (skeletons مستقلة لكل بطاقة) · سنة جارية بلا بيانات (أصفار)

### Functional Requirements
- **FR-001**: البطاقات تملأ من استعلامات RPT-01/RPT-03/SYS-02 الموجودة (بلا endpoint جديد — حتى قرار OQ-B1)
- **FR-002**: كل بطاقة رابط لتقريرها المصدر
- **FR-003**: التحميل المتوازي + الفشل الجزئي المعزول
- **FR-004**: الصلاحيات مركّبة: بطاقة تظهر فقط لصاحب صلاحية تقريرها

### Data Contract
يعيد استخدام: **BudgetExecutionTotalDto** (4 أرقام) · **DisbursementRegisterTotalDto** (عدادات) · **UnreadCountDto** (SYS-02) — بلا DTO جديد

### API
`/api/Reporting/BudgetExecutionReports` · `/api/Reporting/DisbursementRegisterReports` · عداد الإشعارات — بلا endpoint جديد

### UI/UX
`/dashboard`: شبكة بطاقات (قيمة بارزة + عنوان عربي + اتجاه رابط) + صف عدادات حالات الصرف + skeleton لكل بطاقة مستقلة · الحالات: CC-2 + الفشل الجزئي

### Success Criteria
- **SC-001**: البطاقات مطابقة لمصادرها 100% (اختبار مقارنة)
- **SC-002**: فشل بطاقة لا يُسقط اللوحة
- **SC-003**: غير المصرح يرى فارغاً صريحاً

### Tests
T1 البطاقات بأرقامها · T2 الروابط · T3 الفشل الجزئي · T4 الحجب المركب · T5 skeletons

### Open Questions
- **OQ-B1** (User — **BLOCKING**): آلية البناء — (أ) من استعلامات موجودة: بلا عمل خادمي، تحميل أعلى، أسرع تسليم · (ب) endpoint تجميعي خادمي: أرخص تشغيلياً، يتطلب عملاً خلفياً + عقداً جديداً. **القرار قبل /speckit.specify**

### Out of Scope
اشتراكات فورية (real-time) · widgets قابلة للتخصيص · endpoint تجميعي (حتى قرار B1)

---

## SPEC SYS-02 — الإشعارات (notifications)

> **CONTRACT NOTE**: العقد أدناه ملزم حرفياً.

```yaml
id: SYS-02
slug: notifications
branch: sys02-notifications
group: system
priority: P2
depends_on: [كل الميزات (مولدة), routes.tsx (خريطة الفتح)]
permissions_status: "بيانات المستخدم نفسه (userId)"
oq: {blocking: 0, non_blocking: 1}
```

### User Stories
**US1 (P1) — الجرس والعداد.** يوقظ المستخدم على ما ينتظره.
- **Given** إشعارات غير مقروءة، **When** يفتح الهيدر، **Then** جرس بعداد (UnreadCountDto)
- **Given** قراءة إشعار، **When** تتم، **Then** العداد ينقص فوراً (invalidation)
**US2 (P1) — القائمة والقراءة.**
- **Given** القائمة، **When** يتصفح، **Then** صفحات (PaginatedResultOfNotificationDto) بشارات (priority/notificationType) ونقطة غير مقروء
- **Given** إشعاراً، **When** ينقره، **Then** isRead=true + readAt + **يفتح وثيقته** (documentType/documentId → خريطة route)
- **Given** "تعليم الكل"، **When** يضغطه، **Then** العداد صفر
**US3 (P2) — الوثيقة بلا صلاحية.**
- **Given** إشعاراً بوثيقة لا يملك صلاحيتها، **When** ينقره، **Then** رسالة واضحة (لا كسر)

### Edge Cases
وثيقة محذوفة (رسالة) · documentType بلا route映射 (يعرض التفاصيل فقط) · قائمة فارغة صريحة · عداد >99 (‎99+) · إشعارات مستخدم آخر (ممنوع — userId)

### Functional Requirements
- **FR-001**: عداد حي (تحديث مع كل قراءة — invalidation)
- **FR-002**: قائمة صفحات بشارات priority/notificationType
- **FR-003**: mark-read فردي (readAt) + mark-all
- **FR-004**: خريطة documentType→route لفتح الوثيقة + احترام صلاحياتها
- **FR-005**: خصوصية صارمة (إشعارات userId الحالي فقط)

### Data Contract
**NotificationDto** ✓ (L33631): id, userId, notificationType, title, message, documentType?, documentId?, priority, isRead, readAt?, created
**UnreadCountDto** ✓ (L38327) · **PaginatedResultOfNotificationDto** ✓ (L33918)

### Lifecycle
isRead فقط (unread → read) — لا حذف في العقد

### Permissions
بيانات المستخدم نفسه — لا كود خاص (يتحقق: هل يوجد Notifications.View؟ — ضمن OQ-N1)

### API
notify endpoints الموجودة (features/notifications — store/notify) · عداد · mark-read/mark-all (من الواجهة الحالية — تُوثق في plan)

### Business Rules
- **BR-1** القراءة لا رجعة فيها
- **BR-2** الفتح يحترم صلاحية الوثيقة (رسالة عند الغياب)
- **BR-3** الأولوية/النوع شارات بصرية (قيم خادمية — OQ-N1)

### UI/UX
جرس بالهيدر + شارة عداد (‎99+ ceiling) · popover/صفحة `/notifications`: قائمة (أولوية/نوع/عنوان/رسالة/وقت/نقطة غير مقروء) + زر تعليم الكل + pagination · النقر: mark-read ثم navigate · الحالات: CC-2

### Success Criteria
- **SC-001**: العداد يتحدث فور كل قراءة
- **SC-002**: كل إشعار بوثيقة يفتحها بنقرة (أو رسالة صلاحية)
- **SC-003**: تعليم الكل دفعة واحدة

### Tests
T1 العداد+التحديث · T2 القراءة الفردية/الكلية · T3 خريطة الفتح (نوع→route) · T4 وثيقة بلا صلاحية/محذوفة · T5 الفارغ · T6 الخصوصية (userId)

### Open Questions
- **OQ-N1** (Engineering): قيم notificationType/priority (حرّة string — وثقها للشارات والخريطة)

### Out of Scope
توليد الإشعارات (خادمي) · قنوات (email/SMS) · تفضيلات المستخدم

---

## SPEC SYS-03 — المهام الخلفية وOutbox (background-outbox)

> **CONTRACT NOTE**: العقد أدناه ملزم حرفياً.

```yaml
id: SYS-03
slug: background-outbox
branch: sys03-background-outbox
group: system
priority: P3
depends_on: [ACC-04 (الدوري — OQ-N2), خط أنابيب الترحيل (Outbox)]
permissions_status: "✓部分 (BackgroundJobs 2) + GAP-ADD (Outbox)"
oq: {blocking: 0, non_blocking: 3}
```

### User Stories
**US1 (P1) — مراقبة الوظائف.** شفافية التشغيل الخلفي.
- **Given** وظائف مسجلة، **When** يفتح القائمة، **Then** سطور (displayName/scheduleType/scheduleValue/isActive/currentStatus/lastExecutedAt/nextScheduledAt/lastRetryCount/**lastError**)
- **Given** وظيفة بآخر خطأ، **When** تُعرض، **Then** lastError حرفي بارز
**US2 (P1) — التاريخ والمحاولات.** تشخيص الفشل.
- **Given** وظيفة، **When** يفتح تاريخها، **Then** instances (instanceId/scheduledAt/startedAt/completedAt/status/retryCount/triggeredBy) + attempts لكل instance (قابلة للطي)
**US3 (P1) — الإلغاء وصندوق الخروج.** التحكم التشغيلي.
- **Given** حالة جارية/مجدولة، **When** يلغيها (POST /{id}/cancel + تأكيد)، **Then** CancelBackgroundJobResponse
- **Given** Outbox، **When** يفتحه، **Then** pendingCount/failedCount/**oldestPendingAge** + جدول failedMessages (typeName/aggregateId/retryCount/errorMessage/createdAt) + وضع النظام (mode)
- **Given** رسالة فاشلة، **When** يعيد تعيينها (POST /{id}/reset)، **Then** تعود للمعالجة

### Edge Cases
وظيفة بلا تنفيذ أبداً (lastExecutedAt فارغ — عرض "—") · محاولات متعددة فاشلة (attempts مطوية) · reset أثناء معالجة (قاعدة خادمية) · Outbox سليم (أصفار صريحة + شارة صحة) · oldestPendingAge كبير (تنبيه شيخوخة)

### Functional Requirements
- **FR-001**: قائمة الوظائف بكل حقول العرض + lastError بارز
- **FR-002**: التاريخ: instances + attempts (طي/توسيع) + pagination (totalCount/limit/offset)
- **FR-003**: إلغاء حالة بتأكيد
- **FR-004**: Outbox: عدادان + oldestPendingAge + جدول الفاشل + mode
- **FR-005**: reset للفاشل فقط
- **FR-006**: قراءة فقط عدا cancel/reset

### Data Contract
**BackgroundJobDto** ✓ (L22444): id, typeName, displayName, scheduleType, scheduleValue, isActive, currentStatus, lastExecutedAt?, nextScheduledAt?, lastRetryCount?, lastError?
**BackgroundJobsResponse** ✓ (L22670): jobs[]
**BackgroundJobHistoryResponse** ✓ (L22525): jobId, displayName, instances: BackgroundJobInstanceDto[], totalCount, limit, offset
**BackgroundJobInstanceDto** ✓ (L22607): instanceId, scheduledAt, startedAt?, completedAt?, status, retryCount, triggeredBy, attempts: ExecutionAttemptDto[]
**ExecutionAttemptDto** — GAP-READ (L31064)
**CancelBackgroundJobResponse** ✓ (L24406)
**OutboxStatusResponse** ✓ (L33774): pendingCount, failedCount, oldestPendingAge?, failedMessages: FailedOutboxMessageDto[], mode?
**FailedOutboxMessageDto** ✓ (L31131): id, typeName, aggregateId?, retryCount, errorMessage?, createdAt

### Lifecycle
Job: currentStatus حر (OQ-N2) · Instance/Attempt: قيم خادمية · Outbox message: pending → processed / failed → (reset) → pending

### Permissions
| Code | Status |
|---|---|
| BackgroundJobs.View / BackgroundJobs.Manage | ✓ مؤكدة |
| Outbox.View / Outbox.Reset (أو ما يعادلها) | GAP-ADD |

### API
GET `/api/BackgroundJobs` · GET `/{id}/history` · POST `/{id}/cancel` · GET `/api/Outbox/status` · POST `/api/Outbox/{id}/reset`

### Business Rules
- **BR-1** الأحداث قراءة فقط عدا cancel/reset
- **BR-2** reset للفاشل حصراً
- **BR-3** oldestPendingAge مؤشر صحة الطابور (تنبيه عند الشيخوخة — عتبة ASSUMED في plan)

### UI/UX
- `/system/background-jobs`: جدول (اسم/جدولة/نشط/حالة/آخر تنفيذ/التالي/محاولات/خطأ) · تاريخ: خط زمني instances + attempts قابلة للطي · زر إلغاء (تأكيد)
- `/system/outbox`: بطاقتا عداد (معلق/فاشل) + عمر الأقدم (تنبيه شيخوخة) + mode + جدول الفاشل + زر reset لكل فاشل (تأكيد)
- الحالات: CC-2 + صحة الطابور (سليم/معلق/فاشل)

### Success Criteria
- **SC-001**: كل وظيفة بجدولتها وآخر خطأ ظاهرين
- **SC-002**: كل فشل بمحاولاته التفصيلية
- **SC-003**: reset يعيد الرسالة للمعالجة (يتحقق بالعدّاد)

### Tests
T1 القائمة+lastError · T2 التاريخ+attempts+pagination · T3 الإلغاء · T4 Outbox بالعدادات والعمر · T5 reset للفاشل فقط · T6 الأصفار الصريحة

### Open Questions
- **OQ-N1** (Engineering): ExecutionAttemptDto (L31064 — GAP-READ)
- **OQ-N2** (Engineering): قيم currentStatus/scheduleType/status (للشارات)
- **OQ-N3** (Engineering): هل reset يعيد ترتيب الرسالة في الطابور؟

### Out of Scope
إنشاء وظائف · تحرير الجدولة · مراقبة أحداث الترحيل (ACC-05 — مختلفة)

---

# سجل الفجوات العام (Global OQ Registry)

## B — حاجزة (تحسم قبل /speckit.specify)

| # | Spec | السؤال | المالك |
|---|---|---|---|
| B1 | SYS-01 | لوحة التحكم: (أ) استعلامات موجودة — بلا عمل خادمي، تسليم أسرع · (ب) endpoint تجميعي — أرخص تشغيلياً، يتطلب عملاً خلفياً | **User** |

## GAP-READ — قراءات مطلوبة قبل تنفيذ الـ spec المعني (~15 قراءة سريعة من web-api-client.ts)

| Spec | العنصر | السطر |
|---|---|---|
| ACC-05 | RebuildAccountBalancesCommand | L35206 |
| PAY-01 | SendToTreasuryCommand · VoidPaymentOrderCommand | L37453 · L41057 |
| PAY-02 | Reject/CancelDisbursementRequestRequest | L35964 · L24457 |
| PAY-04 | Activate/DeactivateBankAccountCommand (تأكيد) | L20509 · L29362 |
| CTRL-01 | AvailabilityBreakdownTotalResponse | L22039 |
| CTRL-03 | Issue command (FinalAccounts) | — |
| RPT-02 | RevenueCollectionsReportDto/LineDto/TotalDto | L36870 · L36792 · L36936 |
| RPT-04 | AvailabilitySnapshotLineDto/TotalDto | L22296 · L22363 |
| RPT-05 | LedgerMovementLineDto/TotalDto | L33037 · L33097 |
| RPT-06 | CashFlowStatementDto كاملاً | L24926 |
| WF-01 | Create/UpdateWorkflowDefinitionCommand (تأكيد الشكل) | L29233 · L40569 |
| COM-01 | CommitteeDto كاملة · CommitteeMemberDto · CommitteeType enum | L25680 · L25768 |
| BANK-01 | ImportBankStatementCommand · BankReconciliationLineDto | L32151 · L23087 |
| SYS-03 | ExecutionAttemptDto | L31064 |

## GAP-ADD — أكواد صلاحيات تُضاف وفق صيغة {Module}.{Action} (تسجيل الإضافة في نفس المهمة)

| العائلة | الميزات |
|---|---|
| Accounting.* (Journals/Templates/RecurringEntries/Balances/AccountingEvents/PostingRules) | ACC-03/04/05 |
| Revenue.* (ReceiptVouchers/DepositSlips/Checks) | TRE-01/02/03 |
| PaymentOrders.* · DisbursementRequests.* | PAY-01/02 |
| FinancialControl.* (Availability/YearClosing/FinalAccounts/Statements) | CTRL-01/02/03 |
| Workflow.* (Definitions/Instances/History) | WF-01/02 |
| Committees.* (Members/Assignments — إن غابت) | COM-01 |
| BankReconciliations.* · Outbox.* | BANK-01 · SYS-03 |

> **تنبيه AGENTS.md**: كل policies الحالية placeholder (`RequireAssertion(_ => true)`) — الواجهة تربط الأكواد استعداداً لتفعيل RBAC؛ لا تُزل الـ placeholders.

## N — غير حاجزة (56 موثقة داخل كل spec في قسم Open Questions)

تُعالج بافتراض موثق في Assumptions أثناء التنفيذ، وتُرشد عند الحاجة.

---

# نهاية الملف

**28 spec** · كلها بحقول حرفية من العقد الموثق · ملاحظة العقد مكررة داخل كل spec (ذاتية الاحتواء للصق الفردي) · المتطلبات المشتركة (CC/RC) في الأعلى لا تُكرر.

**الخطوة التالية:** سد OQ-B1 (قرار المستخدم) → إنشاء overrides القالبين → بدء الموجة W1 (ACC-03) عبر `/speckit.specify`.

