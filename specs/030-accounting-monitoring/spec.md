# Feature Specification: ACC-05 — مراقبة المحاسبة (Accounting Monitoring)

> **RETIRED (DEP-026, spec 041)**: AccountBalance entity/endpoints removed. Balances computed live from JournalEntryLines. Remaining sections below document PostingRules/AccountingEvents as they stood; AccountingEvents retired separately (spec 042).

**Feature Branch**: `030-accounting-monitoring`

**Created**: 2026-09-07

**Status**: Draft

**Input**: User description: "ACC-05 — مراقبة المحاسبة (accounting-monitoring) — Period closing/opening, balance reconciliation/rebuild, accounting event queue (read-only), posting rule CRUD"

## User Scenarios & Testing

### US1 — الأرصدة وإغلاق الفترة (Priority: P1)

المستخدم (كنترولر/محاسب) يحتاج رؤية أرصدة الحسابات لكل (سنة، فترة،عملة) مع حالة الإغلاق — أساس FR-020.

**Why this priority**: الأرصدة هي الأساس للقرارات المالية وإغلاق الفترة آلية حماية للبيانات.

**Independent Test**: إغلاق فترة → `isFinalized` يتحول `true` ويحظر العكس؛ فتح يعيد `false`.

**Acceptance Scenarios**:

1. **Given** أرصدة فترة محددة، **When** أتصفح بفلاتر (سنة/فترة/حساب)، **Then** 6 أعمدة (افتتاحي/دوران/ختامي × مدين/دائن) + اتجاه + شارة `isFinalized`
2. **Given** فترة مفتوحة، **When** أغلقها (تأكيد مع تحذير FR-020)، **Then** `isFinalized=true` و `finalizedAt` يُحدَّث
3. **Given** فترة مغلقة، **When** أعيد فتحها، **Then** `isFinalized=false`

---

### US2 — التوافق وإعادة البناء (Priority: P2)

ثقة البيانات تتطلب كشف الانحراف بين المُجَمَّع والمحسوب — إعادة بناء الأرصدة من سطور القيود.

**Why this priority**: بدون توافق لا يمكن الوثوق بالأرقام المالية.

**Independent Test**: فحص توافق → جدول فروق لكل عملة أو رسالة "متوازن".

**Acceptance Scenarios**:

1. **Given** انحرافاً في حساب، **When** أشغّل التوافق، **Then** سطر بالفروق (مدين/دائن × مُجمع/محسوب) لكل عملة
2. **Given** أرصدة خاطئة، **When** أشغل إعادة البناء (تأكيد خطير)، **Then** إعادة حساب من سطور القيود المؤشرة

---

### US3 — طابور الأحداث (Priority: P2)

شفافية خط أنابيب الترحيل — رؤية الأحداث الفاشلة وسببها.

**Why this priority**: بدون رؤية الأحداث الفاشلة لا يمكن اكتشاف مشاكل الترحيل.

**Independent Test**: صفحة الأحداث تعرض كل الأحداث بحالتها وسبب الفشل — بلا أزرار تحرير.

**Acceptance Scenarios**:

1. **Given** حدثاً فاشلاً، **When** أفتح الطابور، **Then** `errorMessage` + `retryCount` + رابط `journalEntryId` إن وجد
2. **Given** أحداث متعددة، **When** أتصفح الطابور، **Then** ترتيب بالحدث الأحدث أولاً مع فلاتر (نوع الحدث/الحالة)

---

### US4 — قواعد الترحيل (Priority: P2)

قواعد الترحيل الآلي تُدار لا تُخترع — إنشاء/تعديل/حذف قواعد برؤوسها وبنودها.

**Why this priority**: القواعد تحدد كيف تتحول الأحداث إلى قيود يومية.

**Independent Test**: إنشاء قاعدة ببنودها (مصدر حساب/اتجاه/أبعاد) → تُحفظ كاملة.

**Acceptance Scenarios**:

1. **Given** قاعدة ببنودها، **When** أنشئها، **Then** تُحفظ كاملة مع `_Line` مرتبة بـ `sequence`
2. **Given** قاعدة موجودة، **When** أعدلها (رؤوس + بنود)، **Then** التغييرات تُطبَّق متكاملة
3. **Given** قاعدة بـ `isActive=false`، **When** أحاول حذفها (تأكيد)، **Then** تُحذف مع بنودها

---

### Edge Cases

- إغلاق فترة بها أحداث `pending` → رفض الإغلاق (FR-002)
- فروق متعددة العملات في التوافق → فروق منفصلة لكل عملة
- حذف قاعدة ترحيل مستخدمة في أحداث معالجة → رفض الحذف
- بند قاعدة بلا `fixedAccountId` و `accountSource` غير صالح → رفض الحفظ
- إعادة بناء متزامنة (عمليتان在同一) → آمنة بفضل RowVersion
- أرصدة لعملات مختلفة في نفس الحساب → أعمدة منفصلة لكل عملة

## Requirements

### Functional Requirements

- **FR-001**: النظام MUST يعرض الأرصدة بـ 6 أعمدة (افتتاحي/دوران/ختامي × مدين/دائن) + اتجاه الرصيد + حالة الإغلاق — خادمية حصراً، لا حسابات عميل
- **FR-002**: النظام MUST يرفض إغلاق فترة إذا كانت بها أحداث `pending` — رسالة خطأ واضحة: "هناك أحداث معلقة، أكمل الترحيل أولاً". إغلاق الفترة يتطلب تحذير FR-020 إلزامي
- **FR-003**: النظام MUST يقدم فحص توافق (نتيجة: متوازن/فروق) وإعادة بناء أرصدة (بتأكيد خطير)
- **FR-004**: النظام MUST يعرض طابور الأحداث للقراءة فقط — بدون أزرار تعديل
- **FR-005**: النظام MUST يدير قواعد الترحيل برؤوسها (name, eventType, journalId, priority كترتيب تنفيذ أعلى أولاً, isActive) وبنودها (sequence, accountSource, fixedAccountId, debitOrCredit, amountSource, أبعاد مطلوبة)
- **FR-006**: النظام MUST يسجل `finalizedAt` عند إغلاق الفترة ويعيد فتحها عند Unfinalize
- **FR-007**: أحداث الفشل MUST تظهر `errorMessage` + `retryCount` + رابط `journalEntryId` إن وجد
- **FR-008**: حذف قاعدة ترحيل MUST يتحقق من عدم استخدامها في أحداث معالجة

### Key Entities

- **AccountBalance**: رصيد حساب لكل (سنة، فترة،عملة) — 6 قيم ماليّة + اتجاه + حالة إغلاق
- **AccountingEvent**: حدث ترحيل — حالة (pending/processing/processed/failed) + سبب الفشل + عدد المحاولات
- **PostingRule**: قاعدة ترحيل — رأس (نوع حدث/urnal/أولوية كترتيب تنفيذ/نشط) + بنود (مصدر حساب/اتجاه/مبلغ/أبعاد مطلوبة)

## Success Criteria

### Measurable Outcomes

- **SC-001**: صفر حساب عميل للأرصدة — كل الأرصدة محسوبة خادمية من سطور القيود
- **SC-002**: كل إغلاق/فتح فترة مقرون بتحذير FR-020 — المستخدم يُجبر على التأكيد
- **SC-003**: 100% من أحداث الفشل تظهر سببها حرفياً في `errorMessage`
- **SC-004**: فحص التوافق يكشف كل فروق متعددة العملات
- **SC-005**: إعادة البناء تُعيد حساب الأرصدة من القيود المؤشرة في < 30 ثانية لفترة عادية

## Clarifications

### Session 2026-09-07

- Q: هل يجب تمييز الأدوار (كنترولر vs محاسب) في صلاحيات العرض؟ → A: كل المستخدمون بنفس الصلاحيات، لا تمييز أدوار
- Q: ماذا يحدث عند إغلاق فترة بها أحداث pending — هل نرفض الإغلاق بالكامل أم نسمح مع تحذير؟ → A: رفض الإغلاق مع رسالة خطأ واضحة
- Q: هل priority في PostingRule تحدد ترتيب التنفيذ (الأعلى أولاً) أم أنها فقط للتصنيف؟ → A: ترتيب تنفيذ فعلي (الأعلى أولاً)

## Assumptions

- أحداث الترحيل يمكن أن تكون بأي حالة نصية حرة (OQ1) — يُستخدم نموذج: pending, processing, processed, failed
- قيم `accountSource`/`amountSource`/`debitOrCredit` are server-side enums (OQ2)
- `RebuildAccountBalancesCommand` يأخذ `fiscalYearId` + `fiscalPeriodId` (OQ3) — إعادة بناء لفترة محددة
- الفوترس والـ Authorization تتوافق مع النماذج الموجودة في المشروع
- الأحداث الفاشلة يمكن إعادة محاولة يدوياً عبر إعادة الترحيل من المصدر
- لا يوجد حذف للأحداث — فقط تحديث حالتها
- لا تمييز بين الأدوار — كل المستخدمون بنفس الصلاحيات
