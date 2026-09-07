# جداول قاعدة البيانات — Database Tables Complete Reference

> **تاريخ آخر تحديث:** 2026-09-07
> **عدد الجداول:** 108 جدول

---

## أساس الكيانات — Entity Base Classes

| الكيان الأساسي | English | الأعمدة الموروثة |
|---|---|---|
| BaseEntity | BaseEntity | Id (int) |
| BaseAuditableEntity | BaseAuditableEntity | Id + Created + CreatedBy + LastModified + LastModifiedBy |
| BaseLongEntity | BaseLongEntity | Id (long) |
| BaseLongAuditableEntity | BaseLongAuditableEntity | Id (long) + Created + CreatedBy + LastModified + LastModifiedBy |

---

## 1. الإعدادات المالية — Financial Settings (6)

### 1.1 العملات — Currencies

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الرمز الحرفية | Symbol | string | |
| أماكن العشرية | DecimalPlaces | int | default 2 |
| دقة التقريب | RoundingPrecision | decimal(23,2) | default 0.01 |
| العملة الأساسية | IsBase | bool | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | concurrency |

### 1.2 أسعار الصرف — ExchangeRates

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف العملة الأساسية | BaseCurrencyId | int | FK→Currencies |
| معرف العملة | CurrencyId | int | FK→Currencies |
| تاريخ السعر | RateDate | DateOnly | |
| نوع السعر | RateType | enum (ExchangeRateType) | |
| السعر | Rate | decimal | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 1.3 السنة المالية — FiscalYears

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الاسم | Name | string | |
| رقم السنة | YearNumber | int | |
| تاريخ البدء | StartDate | DateOnly | |
| تاريخ الانتهاء | EndDate | DateOnly | |
| الحالة | Status | enum (FiscalYearStatus) | |
| مغلقة | IsClosed | bool | |
| معرف قيد الإغلاق | ClosingJournalEntryId | int? | nullable |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 1.4 الفترات المالية — FiscalPeriods

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| رقم الفترة | PeriodNumber | int | |
| الاسم | Name | string | |
| تاريخ البدء | StartDate | DateOnly | |
| تاريخ الانتهاء | EndDate | DateOnly | |
| مقفلة للترحيل | IsLockedForPosting | bool | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 1.5 تسلسل المستندات — DocumentSequences

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الاسم | Name | string | |
| نوع المستند | DocumentType | string | |
| معرف السنة المالية | FiscalYearId | int? | FK→FiscalYears |
| الرقم الحالي | CurrentNumber | int | default 1 |
| سياسة إعادة التعيين | ResetPolicy | enum (ResetPolicy) | default Yearly |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 1.6 قيود إغلاق السنة — YearEndClosingEntries

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم قيد الإغلاق | ClosingEntryNumber | string | |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| تاريخ الإغلاق | ClosingDate | DateOnly | |
| الوصف | Description | string? | |
| الحالة | Status | enum (ClosingEntryStatus) | |
| قيد إزالة | IsReversal | bool | |
| معرف القيد المُلغى | ReversalOfId | int? | FK→self |
| معرف قيد اليومية | JournalEntryId | int? | |
| معرف المُعتمِد | ApprovedById | string? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

---

## 2. الأمان والصلاحيات — Security & Permissions (18)

### 2.1 المستخدمون — Users

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم الدخول | Login | string | |
| نشط | IsActive | bool | default true |
| كلمة المرور المشفرة | PasswordHash | string? | |
| محاولات الدخول الفاشلة | FailedLoginAttempts | int | |
| معرف الحظر | LockedUntil | DateTimeOffset? | |
| تاريخ تغيير كلمة المرور | PasswordChangedAt | DateTimeOffset? | |
| رمز إعادة تعيين كلمة المرور | PasswordResetToken | string? | |
| انتهاء رمز إعادة التعيين | PasswordResetTokenExpiry | DateTimeOffset? | |
| آخر دخول | LastLoginAt | DateTimeOffset? | |
| المصادقة الثنائية | MfaEnabled | bool | |
| طريقة المصادقة الثنائية | MfaMethod | string? | |
| يجب تغيير كلمة المرور | MustChangePassword | bool | |
| نوع الحساب | AccountType | enum (AccountType) | |
| معرف القسم | DepartmentId | int? | |
| معرف الدور | RoleId | int | FK→SecurityRoles |
| نسخة الصف | RowVersion | byte[] | |

### 2.2 جلسات المستخدم — UserSessions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف المستخدم | UserId | int | FK→Users |
| رمز الجلسة المشفر | SessionTokenHash | string | |
| رمز التحديث المشفر | RefreshTokenHash | string | |
| عنوان IP | IpAddress | string | |
| مُعرّف المتصفح | UserAgent | string? | |
| بصمة الجهاز | DeviceFingerprint | string? | |
| تاريخ الانتهاء | ExpiresAt | DateTimeOffset | |
| انتهاء رمز التحديث | RefreshTokenExpiresAt | DateTimeOffset | |
| الانتهاء المطلق | AbsoluteExpiryAt | DateTimeOffset | |
| آخر نشاط | LastActivityAt | DateTimeOffset? | |
| مُلغية | IsRevoked | bool | |
| سبب تسجيل الخروج | LogoutReason | string? | |
| تاريخ الإنشاء | CreatedAt | DateTimeOffset | |
| نسخة الصف | RowVersion | byte[] | |

### 2.3 الأدوار — SecurityRoles

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| مستوى الدور | RoleLevel | enum (RoleLevel) | |
| حصري بشكل متبادل | IsMutuallyExclusive | bool | |
| معرف الدور المتبادل | ExclusiveWithRoleId | int? | FK→self |
| يتطلب مصادقة ثنائية | RequiresMfa | bool | |
| مدة الجلسة القصوى | MaxSessionDuration | int? | |
| نظام | IsSystem | bool | |
| مشرف | IsAdmin | bool | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 2.4 الصلاحيات — SecurityPermissions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الوحدة | Module | string | |
| الإجراء | Action | string | |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| مستوى الصلاحية | PermissionLevel | enum (PermissionLevel) | |
| حساس | IsSensitive | bool | |
| نطاق البيانات | DataScope | enum (DataScope) | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 2.5 صلاحيات المستخدم — UserPermissions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف المستخدم | UserId | int | FK→Users |
| معرف الصلاحية | PermissionId | int | FK→SecurityPermissions |
| ممنوحة | IsGranted | bool | default true |
| سارية من | EffectiveFrom | DateTimeOffset? | |
| سارية حتى | EffectiveTo | DateTimeOffset? | |
| السبب | Reason | string? | |
| معرف المعتمِد | ApprovedById | int? | |
| نسخة الصف | RowVersion | byte[] | |

### 2.6 صلاحيات الأدوار — RolePermissions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الدور | RoleId | int | FK→SecurityRoles |
| معرف الصلاحية | PermissionId | int | FK→SecurityPermissions |
| نسخة الصف | RowVersion | byte[] | |

### 2.7 تفويضات الاعتماد — ApprovalDelegations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف المُفوِّض | DelegatorUserId | int | FK→Users |
| معرف المُفوَّض إليه | DelegateUserId | int | FK→Users |
| نوع الكيان | EntityType | string? | |
| تاريخ البدء | StartDate | DateOnly | |
| تاريخ الانتهاء | EndDate | DateOnly | |
| الحالة | Status | enum (DelegationStatus) | |
| يُسمح بإعادة التفويض | CanReDelegate | bool | |
| السبب | Reason | string? | |
| نسخة الصف | RowVersion | byte[] | |

### 2.8 مصفوفة فصل الصلاحيات — SoDMatrix

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الصلاحية أ | PermissionAId | int | FK→SecurityPermissions |
| معرف الصلاحية ب | PermissionBId | int | FK→SecurityPermissions |
| مستوى الخطورة | RiskLevel | enum (RiskLevel) | |
| الإجراء عند المخالفة | ActionOnViolation | enum (ActionOnViolation) | |
| الوصف | Description | string? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 2.9 سجلات التدقيق الأمنية — SecurityAuditLogs

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | **long** | PK |
| فئة الحدث | EventCategory | string | |
| الإجراء | Action | string | |
| معرف المستخدم | UserId | int | FK→Users |
| اسم الكيان | EntityName | string? | |
| معرف الكيان | EntityId | int? | |
| عنوان IP | IpAddress | string? | |
| معلومات الجهاز | DeviceInfo | string? | |
| معرف الجلسة | SessionId | string? | |
| نجاح | Success | bool | |
| سبب الفشل | FailureReason | string? | |
| القيم القديمة | OldValues | string? | |
| القيم الجديدة | NewValues | string? | |
| الوقت | Timestamp | DateTimeOffset | |

### 2.10 قواعد الاعتماد — ApprovalRules

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| نوع المستند | DocumentType | string | |
| معرف الصندوق | FundId | int? | |
| حد المبلغ | AmountThreshold | decimal? | |
| معرف العملة | CurrencyId | int? | |
| معرف دور المعتمِد | ApproverRoleId | int? | |
| دور المعتمِد | ApproverRole | string? | |
| التسلسل | Sequence | int | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 2.11 سجل الاعتمادات — ApprovalHistory

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| نوع المستند | DocumentType | string | |
| معرف المستند | DocumentId | int | |
| خطوة الاعتماد | ApprovalStep | int | default 1 |
| الإجراء | Action | enum (ApprovalAction) | |
| معرف المعتمِد | ApproverUserId | int | FK→Users |
| الدور المطلوب | RequiredRole | string | |
| القرار | Decision | string | |
| وقت القرار | DecisionAt | DateTimeOffset | |
| السبب | Reason | string? | |
| لقطة التقييم | EvaluationSnapshot | string? | |
| نسخة الصف | RowVersion | byte[] | |

### 2.12 المرفقات — Attachments

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم الكيان | EntityName | string | |
| معرف المستند | DocumentId | int | |
| نوع المستند | DocumentType | string | |
| رمز نوع المرفق | AttachmentTypeCode | string | |
| مطلوب | IsRequired | bool | |
| اسم الملف | FileName | string | |
| نوع MIME | MimeType | string | |
| مسار التخزين | StoragePath | string | |
| الحجم بالبايت | SizeBytes | int | |
| التجزئة | FileHash | string? | |
| معرف الرافع | UploadedById | int | FK→Users |
| تاريخ الإنشاء | CreatedAt | DateTimeOffset | |
| نسخة الصف | RowVersion | byte[] | |

### 2.13 سجلات حالة المستندات — DocumentStatusLogs

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم الكيان | EntityName | string | |
| معرف المستند | DocumentId | int | |
| الحالة السابقة | FromStatus | string | |
| الحالة الجديدة | ToStatus | string | |
| معرف المُغيِّر | ChangedById | int | FK→Users |
| وقت التغيير | ChangedAt | DateTimeOffset | |
| السبب | Reason | string? | |

### 2.14 متطلبات المرفقات — DocumentAttachmentRequirements

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| نوع المستند | DocumentType | string | |
| رمز نوع المرفق | AttachmentTypeCode | string | |
| العنوان بالعربية | TitleAr | string | |
| إلزامي | IsMandatory | bool | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 2.15 الإشعارات — Notifications

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | **long** | PK |
| معرف المستخدم | UserId | int | FK→Users |
| نوع الإشعار | NotificationType | enum (NotificationType) | |
| العنوان | Title | string | |
| الرسالة | Message | string | |
| نوع المستند | DocumentType | string? | |
| معرف المستند | DocumentId | int? | |
| الأولوية | Priority | enum (NotificationPriority) | |
| مقروء | IsRead | bool | |
| وقت القراءة | ReadAt | DateTimeOffset? | |

### 2.16 آثار التدقيق — AuditTrails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | **long** | PK |
| فئة الحدث | EventCategory | string | |
| نوع المستند | DocumentType | string? | |
| معرف المستند | DocumentId | int? | |
| الإجراء | Action | enum (AuditAction) | |
| معرف المستخدم | UserId | int? | |
| نجاح | Success | bool | default true |
| سبب الفشل | FailureReason | string? | |
| معلومات الجهاز | DeviceInfo | string? | |
| الوقت | Timestamp | DateTimeOffset | |
| عنوان IP | IpAddress | string? | |
| معرف الجلسة | SessionId | string? | |
| ملخص التغييرات | ChangeSummary | string? | |
| تغييرات الحقول | FieldChanges | string? | |
| القيم القديمة | OldValues | string? | |
| القيم الجديدة | NewValues | string? | |

---

## 3. سير العمل — Workflow (4)

### 3.1 تعريفات سير العمل — WorkflowDefinitions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم الكيان | EntityName | string | |
| الإصدار | Version | int | |
| الحالة | Status | string | |
| الوصف | Description | string? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 3.2 خطوات سير العمل — WorkflowSteps

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف التعريف | DefinitionId | int | FK→WorkflowDefinitions |
| ترتيب الخطوة | StepOrder | int | |
| نوع الخطوة | StepType | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| معرف الدور المعين | AssignedRoleId | int? | |
| استخدام قواعد الاعتماد | UseApprovalRules | bool | |
| تعبير الشرط | ConditionExpression | string? | |
| مهلة بالساعات | TimeoutHours | int? | |
| معرف خطوة التصعيد | EscalateToStepId | int? | FK→self |
| نشط | IsActive | bool | default true |

### 3.3 مثيلات سير العمل — WorkflowInstances

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف التعريف | DefinitionId | int | FK→WorkflowDefinitions |
| اسم الكيان | EntityName | string | |
| معرف الكيان | EntityId | int | |
| معرف الخطوة الحالية | CurrentStepId | int? | FK→WorkflowSteps |
| الحالة | Status | string | |
| وقت البدء | StartedAt | DateTimeOffset | |
| وقت الانتهاء | CompletedAt | DateTimeOffset? | |
| نسخة الصف | RowVersion | byte[] | |

### 3.4 سجل سير العمل — WorkflowHistory

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف مثيل سير العمل | WorkflowInstanceId | int | FK→WorkflowInstances |
| معرف الخطوة | StepId | int | FK→WorkflowSteps |
| معرف المستخدم الفاعل | ActorUserId | int | |
| القرار | Decision | string | |
| السبب | Reason | string? | |
| لقطة التقييم | EvaluationSnapshot | string? | |
| الوقت | Timestamp | DateTimeOffset | |

---

## 4. التنظيم — Organization (5)

### 4.1 الوحدات التنظيمية — OrganizationalUnits

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| معرف الوحدة الأب | ParentId | int? | FK→self |
| مسار الأصل | ParentPath | string? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 4.2 مراكز التكلفة — CostCenters

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| معرف الوحدة التنظيمية | OrganizationUnitId | int? | FK→OrganizationalUnits |
| حد الموازنة | BudgetLimit | decimal? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 4.3 حسابات مراكز التكلفة — CostCenterAccounts

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| معرف مركز التكلفة | CostCenterId | int | FK→CostCenters (composite PK) |
| معرف الحساب | AccountId | int | FK→Accounts (composite PK) |

### 4.4 المشاريع — Projects

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| معرف الصندوق | FundId | int? | FK→Funds |
| معرف مركز التكلفة | CostCenterId | int? | FK→CostCenters |
| تاريخ البدء | StartDate | DateOnly? | |
| تاريخ الانتهاء | EndDate | DateOnly? | |
| مبلغ الموازنة | BudgetAmount | decimal? | |
| الحالة | Status | enum (ProjectStatus) | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 4.5 الموظفون — Employees

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الموظف | EmployeeNumber | string | |
| الاسم | Name | string | |
| معرف المستخدم | UserId | int? | FK→Users |
| معرف الوحدة التنظيمية | OrganizationalUnitId | int | FK→OrganizationalUnits |
| المسمى الوظيفي | JobTitle | string | |
| الدرجة الوظيفية | JobGrade | string? | |
| تاريخ التعيين | HireDate | DateOnly | |
| حالة التوظيف | EmploymentStatus | enum (EmploymentStatus) | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

---

## 5. المحاسبة — Accounting (14)

### 5.1 مجموعات الحسابات — AccountGroups

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| النوع | Type | enum (AccountGroupType) | |
| الرصيد الطبيعي | NormalBalance | enum (NormalBalanceType) | |
| الوصف | Description | string? | |
| معرف المجموعة الأب | ParentId | int? | FK→self |
| المستوى | Level | byte | default 1 |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 5.2 شجرة الحسابات — Accounts

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| معرف مجموعة الحسابات | AccountGroupId | int | FK→AccountGroups |
| معرف الحساب الأب | ParentId | int? | FK→self |
| المستوى | Level | byte | |
| الرصيد الطبيعي | NormalBalance | enum (NormalBalanceType) | |
| قابل للترحيل | IsPostable | bool | default true |
| قابل للتدقيق | IsReconcilable | bool | |
| معرف العملة | CurrencyId | int? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 5.3 القيود اليومية — Journals

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| النوع | Type | enum (JournalType) | |
| معرف الحساب | AccountId | int? | FK→Accounts |
| معرف حساب المستودع | SuspenseAccountId | int? | FK→Accounts |
| يسمح بالعملة الأجنبية | AllowForeignCurrency | bool | |
| معرف التسلسل | SequenceId | int? | |
| يتطلب اعتماد قبل الترحيل | RequireApprovalBeforePosting | bool | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 5.4 قيود اليومية — JournalEntries

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم القيد | EntryNumber | string | |
| مرجع | Ref | string? | |
| تاريخ المستند | DocumentDate | DateOnly | |
| تاريخ الترحيل | PostingDate | DateOnly? | |
| نوع القيد | EntryType | enum (MoveEntryType)? | |
| حالة القيد | EntryStatus | enum (EntryStatus) | default Draft |
| معرف اليومية | JournalId | int? | FK→Journals |
| معرف الفترة | PeriodId | int | FK→FiscalPeriods |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| البيان | Narration | string? | |
| معرف الحدث المصدر | SourceEventId | int? | FK→AccountingEvents |
| معرف القيد المُلغى | ReversalOfId | int? | FK→self |
| سبب الإلغاء | ReversalReason | string? | |
| معرف المُرحِّل | PostedById | int? | FK→Users |
| وقت الترحيل | PostedAt | DateTimeOffset? | |
| معرف المُلغِي | CancelledById | int? | FK→Users |
| وقت الإلغاء | CancelledAt | DateTimeOffset? | |
| مُولَّد من النظام | IsSystemGenerated | bool | |
| نسخة الصف | RowVersion | byte[] | |

### 5.5 أسطر قيود اليومية — JournalEntryLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | **long** | PK |
| معرف قيد اليومية | JournalEntryId | int | FK→JournalEntries |
| التسلسل | Sequence | int | |
| معرف الحساب | AccountId | int | FK→Accounts |
| الوصف | Description | string? | |
| معرف العملة | CurrencyId | int | FK→Currencies |
| سعر الصرف | ExchangeRate | decimal | default 1 |
| المدين | Debit | decimal | CHECK: Debit>0 XOR Credit>0 |
| الدائن | Credit | decimal | CHECK: Debit>0 XOR Credit>0 |
| معرف مركز التكلفة | CostCenterId | int? | FK→CostCenters |
| معرف أمر الدفع | PaymentOrderId | int? | FK→PaymentOrders |
| نسخة الصف | RowVersion | byte[] | |

### 5.6 أرصدة الحسابات — AccountBalances

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الحساب | AccountId | int | FK→Accounts |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| معرف الفترة المالية | FiscalPeriodId | int | FK→FiscalPeriods |
| معرف العملة | CurrencyId | int | FK→Currencies |
| رصيد افتتاحي مدين | OpeningDebit | decimal | |
| رصيد افتتاحي دائن | OpeningCredit | decimal | |
| إجمالي المدين | Debit | decimal | |
| إجمالي الدائن | Credit | decimal | |
| رصيد ختامي مدين | ClosingDebit | decimal | |
| رصيد ختامي دائن | ClosingCredit | decimal | |
| مُ finalized | IsFinalized | bool | |
| وقت التfinalization | FinalizedAt | DateTimeOffset? | |
| نشط | IsActive | bool | default true |
| نسخة الصف | RowVersion | byte[] | |

### 5.7 الأحداث المحاسبية — AccountingEvents

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| نوع الحدث | EventType | enum (EventType) | |
| نوع المستند المصدر | SourceDocumentType | string | |
| معرف المستند المصدر | SourceDocumentId | int | |
| الحالة | Status | enum (EventStatus) | default Pending |
| معرف قيد اليومية | JournalEntryId | int? | FK→JournalEntries |
| فئة الحدث | EventCategory | enum (EventCategory) | |
| رسالة الخطأ | ErrorMessage | string? | |
| وقت المعالجة | ProcessedAt | DateTimeOffset? | |
| عدد المحاولات | RetryCount | int | |
| نسخة الصف | RowVersion | byte[] | |

### 5.8 قواعد الترحيل — PostingRules

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الاسم | Name | string | |
| نوع الحدث | EventType | string | |
| معرف اليومية | JournalId | int | FK→Journals |
| الأولوية | Priority | int | default 100 |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 5.9 أسطر قواعد الترحيل — PostingRuleLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف قاعدة الترحيل | PostingRuleId | int | FK→PostingRules |
| التسلسل | Sequence | int | |
| مصدر الحساب | AccountSource | enum (AccountSource) | |
| معرف الحساب الثابت | FixedAccountId | int? | FK→Accounts |
| مدين أو دائن | DebitOrCredit | enum (DebitOrCredit) | |
| مصدر المبلغ | AmountSource | enum (AmountSource) | |
| بُعد الصندوق مطلوب | FundDimensionRequired | bool | |
| بُعد مركز التكلفة مطلوب | CostCenterDimensionRequired | bool | |
| بُعد المشروع مطلوب | ProjectDimensionRequired | bool | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 5.10 قوالب قيود اليومية — JournalEntryTemplates

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم القالب | TemplateName | string | |
| الوصف | Description | string? | |
| معرف اليومية | JournalId | int | FK→Journals |
| نوع القالب | TemplateType | enum (JournalEntryTemplateType) | |
| قالب نظام | IsSystemTemplate | bool | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 5.11 أسطر القوالب — JournalEntryTemplateLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف القالب | TemplateId | int | FK→JournalEntryTemplates |
| التسلسل | Sequence | int | |
| معرف الحساب | AccountId | int | FK→Accounts |
| الوصف | Description | string? | |
| معرف العملة | CurrencyId | int | FK→Currencies |
| سعر الصرف | ExchangeRate | decimal | default 1 |
| المدين | Debit | decimal | |
| الدائن | Credit | decimal | |
| معرف مركز التكلفة | CostCenterId | int? | FK→CostCenters |
| نسخة صف | RowVersion | byte[] | |

### 5.12 القيود الدورية — RecurringEntries

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم القيد | EntryNumber | string | |
| معرف القالب | TemplateId | int? | FK→JournalEntryTemplates |
| معرف اليومية | JournalId | int | FK→Journals |
| الاسم | Name | string | |
| التكرار | Frequency | enum (RecurringFrequency) | |
| تاريخ البدء | StartDate | DateOnly | |
| تاريخ الانتهاء | EndDate | DateOnly? | |
| تاريخ التنفيذ التالي | NextExecutionDate | DateOnly | |
| آخر تنفيذ | LastExecutedAt | DateTime? | |
| المبلغ | Amount | decimal? | |
| معرف العملة | CurrencyId | int? | |
| معرف الصندوق | FundId | int? | FK→Funds |
| معرف مركز التكلفة | CostCenterId | int? | FK→CostCenters |
| معرف المشروع | ProjectId | int? | FK→Projects |
| قالب الوصف | DescriptionTemplate | string? | |
| الحالة | Status | enum (RecurringEntryStatus) | default Active |
| معرف القيد المُولَّد | GeneratedJournalEntryId | int? | FK→JournalEntries |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 5.13 سجلات التنفيذ — RecurringEntryExecutionLogs

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف القيد الدوري | RecurringEntryId | int | FK→RecurringEntries |
| تاريخ التنفيذ | ExecutionDate | DateOnly | |
| معرف القيد المُولَّد | GeneratedJournalEntryId | int? | FK→JournalEntries |
| الحالة | Status | enum (RecurringEntryExecutionStatus) | default Created |
| وقت البدء | StartedAt | DateTimeOffset | |
| وقت الانتهاء | CompletedAt | DateTimeOffset? | |
| رسالة الخطأ | ErrorMessage | string? | |
| المُشغِّل | TriggeredBy | string | default "Scheduler" |
| نسخة صف | RowVersion | byte[] | |

### 5.14 قواعد تعيين التدفق النقدي — CashFlowMappingRules

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف مجموعة الحسابات | AccountGroupId | int | FK→AccountGroups |
| القسم | Section | enum (CashFlowSectionType) | |
| الوصف | Description | string? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[]? | |

---

## 6. الموازنة — Budgeting (11)

### 6.1 التصنيفات المالية — BudgetClassifications

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| معرف التصنيف الأب | ParentId | int? | FK→self |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 6.2 الصناديق — Funds

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الصندوق | FundNumber | string | |
| اسم الصندوق | FundName | string | |
| نوع الصندوق | FundType | enum (FundType) | |
| فئة الصندوق | FundCategory | enum (FundCategory) | |
| معرف السنة المالية | FiscalYearId | int? | FK→FiscalYears |
| السلطة القانونية | LegalAuthority | string | |
| الوصف | Description | string? | |
| معرف حساب الإيراد الافتراضي | DefaultRevenueDebitAccountId | int? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 6.3 أنواع الموازنة — BudgetTypes

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| طريقة التحكم | ControlMethod | enum (BudgetControlMethod) | |
| يسمح بالتجاوز | AllowOverrun | bool | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 6.4 الموازنات — Budgets

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الموازنة | BudgetNumber | string | |
| اسم الموازنة | BudgetName | string | |
| معرف نوع الموازنة | BudgetTypeId | int | FK→BudgetTypes |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| معرف الصندوق | FundId | int | FK→Funds |
| المبلغ الإجمالي | TotalAmount | decimal | |
| الحالة | Status | enum (BudgetStatus) | default Draft |
| يسمح بالتجاوز | AllowOverrun | bool? | |
| سارية من | EffectiveFrom | DateOnly | |
| سارية حتى | EffectiveTo | DateOnly? | |
| الوصف | Description | string? | |
| نسخة صف | RowVersion | byte[] | |

### 6.5 بنود الموازنة — BudgetItems

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رمز البند | ItemCode | string | |
| اسم البند | ItemName | string | |
| معرف الموازنة | BudgetId | int | FK→Budgets |
| معرف البند الأب | ParentId | int? | FK→self |
| معرف الحساب | AccountId | int? | FK→Accounts |
| معرف الصندوق | FundId | int? | FK→Funds |
| معرف مركز التكلفة | CostCenterId | int? | FK→CostCenters |
| معرف التصنيف | BudgetClassificationId | int? | FK→BudgetClassifications |
| ملاحظات | Remarks | string? | |
| يسمح بالتجاوز | AllowOverrun | bool? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 6.6 الاعتمادات — Appropriations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الاعتماد | AppropriationNumber | string | |
| معرف الموازنة | BudgetId | int | FK→Budgets |
| معرف بند الموازنة | BudgetItemId | int | FK→BudgetItems |
| معرف البند المستهدف | TargetBudgetItemId | int? | FK→BudgetItems |
| نوع الاعتماد | AppropriationType | enum (AppropriationType) | |
| نوع المستند | DocumentType | string | |
| معرف المستند | DocumentId | int | |
| المبلغ | Amount | decimal | |
| الحالة | Status | enum (AppropriationStatus) | default Draft |
| نسخة صف | RowVersion | byte[] | |

### 6.7 الالتزامات — Encumbrances

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الالتزام | EncumbranceNumber | string | |
| نوع الالتزام | EncumbranceType | enum (EncumbranceType) | |
| معرف الاعتماد | AppropriationId | int | FK→Appropriations |
| معرف المورد | VendorId | int? | |
| معرف طرف المورد | VendorPartyId | int? | FK→Parties |
| معرف أمر الشراء | PurchaseOrderId | int? | |
| نوع المستند | DocumentType | string | |
| معرف المستند | DocumentId | int | |
| الوصف | Description | string? | |
| تاريخ الالتزام | EncumbranceDate | DateOnly | |
| المبلغ | Amount | decimal | |
| الحالة | Status | enum (EncumbranceStatus) | default Draft |
| معرف الالتزام المُلغى | ReversalOfId | int? | FK→self |
| سبب الإلغاء | ReversalReason | string? | |
| نسخة صف | RowVersion | byte[] | |

### 6.8 الخطط الشهرية — BudgetItemMonthlyPlans

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف بند الموازنة | BudgetItemId | int | FK→BudgetItems |
| الشهر | Month | int | UNIQUE(BudgetItemId, Month) |
| المبلغ المخطط | PlannedAmount | decimal(23,2) | ≥ 0 |
| نسخة صف | RowVersion | byte[] | |

### 6.9 عمليات إغلاق السنة — YearClosingRuns

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| وقت التشغيل | RunAt | DateTimeOffset | |
| معرف المشغِّل | RunById | int | FK→Users |
| نوع التشغيل | RunType | enum (YearClosingRunType) | |
| إجمالي الاعتمادات المنقضية | LapsedAppropriationTotal | decimal | default 0 |
| إجمالي الالتزامات المنقضية | LapsedEncumbranceTotal | decimal | default 0 |
| الحالة | Status | enum (YearClosingRunStatus) | default Completed |
| معرف المُلغِي | ReversedById | int? | FK→Users |
| وقت الإلغاء | ReversedAt | DateTimeOffset? | |
| نسخة صف | RowVersion | byte[] | |

### 6.10 الحسابات الختامية — FinalAccounts

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears, UNIQUE |
| وقت الإنشاء | GeneratedAt | DateTimeOffset | |
| معرف المُنشئ | GeneratedById | int | FK→Users |
| الحالة | Status | enum (FinalAccountStatus) | default Draft |
| وقت الإصدار | IssuedAt | DateTimeOffset? | |
| معرف المُصدر | IssuedById | int? | FK→Users |
| نسخة صف | RowVersion | byte[] | |

### 6.11 أسطر الحسابات الختامية — FinalAccountLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الحساب الختامي | FinalAccountId | int | FK→FinalAccounts |
| البُعد | Dimension | enum (FinalAccountLineDimension) | |
| معرف البُعد | DimensionId | int | |
| رمز البُعد | DimensionCode | string | |
| اسم البُعد | DimensionName | string | |
| المبلغ المُ موازَن | BudgetedAmount | decimal | |
| المبلغ الفعلي | ActualAmount | decimal | |
| الفرق | Variance | decimal | |
| نسخة صف | RowVersion | byte[] | |

---

## 7. الأطراف — Parties (1)

### 7.1 الأطراف — Parties

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رمز الطرف | PartyCode | string | UNIQUE |
| نوع الطرف | PartyType | enum (PartyType) | INDEX(PartyType, IsActive) |
| الاسم بالعربية | NameAr | string | INDEX(NameAr) |
| الاسم بالإنجليزية | NameEn | string? | |
| الرقم الضريبي | TaxNumber | string? | INDEX(TaxNumber) |
| الرقم الوطني | NationalId | string? | |
| الهاتف | Phone | string? | |
| البريد الإلكتروني | Email | string? | |
| العنوان | Address | string? | |
| ملاحظات | Notes | string? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

---

## 8. المشتريات — Procurement (8)

### 8.1 طلبات الشراء — PurchaseRequests

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الطلب | RequestNumber | string | |
| تاريخ الطلب | RequestDate | DateTime | |
| التاريخ المطلوب | RequiredDate | DateOnly? | |
| معرف القسم | DepartmentId | int? | |
| معرف مركز التكلفة | CostCenterId | int? | |
| معرف الطالب | RequesterId | int? | |
| الأولوية | Priority | string? | |
| نوع الطلب | RequestType | string | |
| الحالة | Status | string | |
| إجمالي البنود | TotalItems | int? | |
| الكمية الإجمالية | TotalQuantity | decimal? | |
| التكلفة التقديرية | EstimatedTotalCost | decimal? | |
| رمز العملة | CurrencyCode | string? | |
| ملاحظات | Notes | string? | |
| سبب الرفض | RejectionReason | string? | |
| معرف المُلغِي | CancelledById | int? | |
| وقت الإلغاء | CancelledAt | DateTime? | |
| معرف المعتمِد | ApprovedById | int? | |
| وقت الاعتماد | ApprovedAt | DateTime? | |
| معرف المعتمِد النهائي | FinalApprovedById | int? | |
| وقت الاعتماد النهائي | FinalApprovedAt | DateTime? | |
| نسخة صف | RowVersion | byte[] | |

### 8.2 تفاصيل طلبات الشراء — PurchaseRequestDetails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف طلب الشراء | PurchaseRequestId | int | FK→PurchaseRequests |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal? | |
| الكمية المطلوبة | RequestedQuantity | decimal | |
| الكمية المعتمدة | ApprovedQuantity | decimal? | |
| تكلفة الوحدة التقديرية | UnitCostEstimate | decimal? | |
| التكلفة الإجمالية التقديرية | TotalCostEstimate | decimal? | |
| ملاحظات | Notes | string? | |
| الحالة | Status | string? | |
| نسخة صف | RowVersion | byte[] | |

### 8.3 طلبات عروض الأسعار — RequestForQuotations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم طلب عرض السعر | RFQNumber | string | |
| تاريخ طلب عرض السعر | RFQDate | DateTime | |
| معرف طلب الشراء | PurchaseRequestId | int? | FK→PurchaseRequests |
| موعد التسليم | DeadlineDate | DateTime? | |
| رمز العملة | CurrencyCode | string? | |
| الشروط والأحكام | TermsAndConditions | string? | |
| الحالة | Status | string | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 8.4 موردو طلبات عروض الأسعار — RFQSuppliers

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف طلب عرض السعر | RFQId | int | FK→RequestForQuotations |
| معرف المورد | SupplierId | int | |
| معرف الطرف | PartyId | int? | FK→Parties |
| تاريخ الدعوة | InvitationDate | DateTime? | |
| تاريخ الرد | ResponseDate | DateTime? | |
| الحالة | Status | string | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 8.5 عروض الأسعار — Quotations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم عرض السعر | QuotationNumber | string | |
| معرف طلب عرض السعر | RFQId | int | FK→RequestForQuotations |
| معرف مورد طلب عرض السعر | RFQSupplierId | int | FK→RFQSuppliers |
| معرف المورد | SupplierId | int | |
| معرف الطرف | PartyId | int? | FK→Parties |
| تاريخ عرض السعر | QuotationDate | DateTime | |
| صالح حتى | ValidUntil | DateTime? | |
| رمز العملة | CurrencyCode | string? | |
| سعر الصرف | ExchangeRate | decimal? | |
| المجموع الفرعي | SubTotal | decimal? | |
| مبلغ الخصم | DiscountAmount | decimal? | |
| مبلغ الضريبة | TaxAmount | decimal? | |
| تكلفة الشحن | ShippingCost | decimal? | |
| رسوم أخرى | OtherCharges | decimal? | |
| المجموع الكلي | GrandTotal | decimal? | |
| شروط الدفع | PaymentTerms | string? | |
| شروط التسليم | DeliveryTerms | string? | |
| مدة التسليم (أيام) | LeadTimeDays | int? | |
| فترة الضمان (أشهر) | WarrantyPeriodMonths | int? | |
| الحالة | Status | string | |
| مختار | IsSelected | bool | |
| سبب الاختيار | SelectionReason | string? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 8.6 تفاصيل عروض الأسعار — QuotationDetails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف عرض السعر | QuotationId | int | FK→Quotations |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal? | |
| الكمية | Quantity | decimal | |
| سعر الوحدة | UnitPrice | decimal? | |
| نسبة الخصم | DiscountPercent | decimal? | |
| مبلغ الخصم | DiscountAmount | decimal? | |
| سعر الوحدة الصافي | NetUnitPrice | decimal? | |
| إجمالي البند | LineTotal | decimal? | |
| نسبة الضريبة | TaxPercent | decimal? | |
| مبلغ الضريبة | TaxAmount | decimal? | |
| إجمالي البند مع الضريبة | LineTotalWithTax | decimal? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 8.7 أوامر الشراء — PurchaseOrders

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم أمر الشراء | PONumber | string | |
| تاريخ أمر الشراء | PODate | DateTime | |
| معرف طلب الشراء | PurchaseRequestId | int? | FK→PurchaseRequests |
| معرف عرض السعر | QuotationId | int? | FK→Quotations |
| معرف المورد | SupplierId | int | |
| معرف طرف المورد | SupplierPartyId | int? | FK→Parties |
| معرف المستودع | WarehouseId | int? | FK→Warehouses |
| معرف الموقع | LocationId | int? | FK→Locations |
| رمز العملة | CurrencyCode | string? | |
| سعر الصرف | ExchangeRate | decimal? | |
| المجموع الفرعي | SubTotal | decimal? | |
| مبلغ الخصم | DiscountAmount | decimal? | |
| مبلغ الضريبة | TaxAmount | decimal? | |
| تكلفة الشحن | ShippingCost | decimal? | |
| رسوم أخرى | OtherCharges | decimal? | |
| المجموع الكلي | GrandTotal | decimal? | |
| شروط الدفع | PaymentTerms | string? | |
| شروط التسليم | DeliveryTerms | string? | |
| تاريخ التسليم المتوقع | ExpectedDeliveryDate | DateTime? | |
| الحالة | Status | string | |
| معرف المعتمِد | ApprovedById | int? | |
| وقت الاعتماد | ApprovedAt | DateTime? | |
| سبب الرفض | RejectionReason | string? | |
| معرف المُلغِي | CancelledById | int? | |
| وقت الإلغاء | CancelledAt | DateTime? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 8.8 تفاصيل أوامر الشراء — PurchaseOrderDetails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف أمر الشراء | PurchaseOrderId | int | FK→PurchaseOrders |
| معرف تفصيل طلب الشراء | PurchaseRequestDetailId | int? | FK→PurchaseRequestDetails |
| معرف تفصيل عرض السعر | QuotationDetailId | int? | FK→QuotationDetails |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal? | |
| الكمية المطلوبة | OrderedQuantity | decimal | |
| الكمية المستلمة | ReceivedQuantity | decimal? | |
| الكمية المتبقية | RemainingQuantity | decimal? | |
| سعر الوحدة | UnitPrice | decimal? | |
| نسبة الخصم | DiscountPercent | decimal? | |
| مبلغ الخصم | DiscountAmount | decimal? | |
| سعر الوحدة الصافي | NetUnitPrice | decimal? | |
| إجمالي البند | LineTotal | decimal? | |
| نسبة الضريبة | TaxPercent | decimal? | |
| مبلغ الضريبة | TaxAmount | decimal? | |
| إجمالي البند مع الضريبة | LineTotalWithTax | decimal? | |
| تاريخ التسليم المتوقع | ExpectedDeliveryDate | DateTime? | |
| الحالة | Status | string? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

---

## 9. المدفوعات — Payments (6)

### 9.1 الحسابات البنكية — BankAccounts

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الاسم | Name | string | |
| اسم البنك | BankName | string | |
| رقم الحساب | AccountNumber | string | |
| الآيبان | Iban | string? | |
| رمز السويفت | SwiftCode | string? | |
| اسم الفرع | BranchName | string? | |
| رمز الفرع | BranchCode | string? | |
| معرف العملة | CurrencyId | int | FK→Currencies |
| معرف الصندوق | FundId | int? | FK→Funds |
| معرف الحساب في دفتر الأستاذ | GlAccountId | int? | FK→Accounts |
| افتراضي | IsDefault | bool | |
| الحد اليومي الأقصى | MaxDailyLimit | decimal? | |
| حد المعاملة الأقصى | MaxTransactionLimit | decimal? | |
| يتطلب اعتماد مزدوج | RequiresDualApproval | bool | |
| آخر تاريخ تسوية | LastReconciliationDate | DateOnly? | |
| الرصيد الافتتاحي | OpeningBalance | decimal? | |
| الرصيد الحالي | CurrentBalance | decimal? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 9.2 أوامر الدفع — PaymentOrders

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم أمر الدفع | PaymentOrderNumber | string | |
| تاريخ أمر الدفع | PaymentOrderDate | DateOnly | |
| تاريخ الاستحقاق | DueDate | DateOnly? | |
| نوع أمر الدفع | PaymentOrderType | string | |
| معرف المورد | VendorId | int | |
| معرف طرف المورد | VendorPartyId | int? | FK→Parties |
| معرف الصندوق | FundId | int | FK→Funds |
| معرف السنة المالية | FiscalYearId | int | FK→FiscalYears |
| معرف الاعتماد | AppropriationId | int | FK→Appropriations |
| معرف التصنيف المالي | BudgetClassificationId | int? | FK→BudgetClassifications |
| معرف مركز التكلفة | CostCenterId | int? | FK→CostCenters |
| معرف المشروع | ProjectId | int? | FK→Projects |
| معرف أمر الشراء | PurchaseOrderId | int? | FK→PurchaseOrders |
| معرف الالتزام | EncumbranceId | int? | FK→Encumbrances |
| معرف العملة | CurrencyId | int | FK→Currencies |
| سعر الصرف | ExchangeRate | decimal? | |
| المبلغ الإجمالي | AmountGross | decimal | |
| مبلغ الخصومات | DeductionAmount | decimal | |
| طريقة الدفع | PaymentMethod | enum (PaymentMethod)? | |
| معرف الحساب البنكي | BankAccountId | int? | FK→BankAccounts |
| اسم المستفيد | BeneficiaryName | string | |
| آيبان المستفيد | BeneficiaryIban | string? | |
| رقم حساب المستفيد | BeneficiaryAccountNumber | string? | |
| بنك المستفيد | BeneficiaryBankName | string? | |
| الحالة | Status | enum (PaymentOrderStatus) | |
| حالة فحص الموازنة | BudgetCheckStatus | enum (BudgetCheckStatus) | |
| حالة الخزينة | TreasuryStatus | string? | |
| مرجع الخزينة | TreasuryReference | string? | |
| وقت الإرسال للخزينة | TreasurySentAt | DateTimeOffset? | |
| وقت الدفع | PaidAt | DateTimeOffset? | |
| معرف قيد اليومية | JournalEntryId | int? | |
| معرف الحدث المحاسبي | AccountingEventId | int? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 9.3 أسطر أمر الدفع — PaymentOrderLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف أمر الدفع | PaymentOrderId | int | FK→PaymentOrders |
| رقم البند | LineNumber | int | |
| نوع البند | LineType | enum (PaymentOrderLineType) | |
| الوصف | Description | string? | |
| معرف الحساب | AccountId | int | |
| المبلغ | Amount | decimal | |
| مبلغ الضريبة | TaxAmount | decimal? | |
| معرف العملة | CurrencyId | int? | |
| سعر الصرف | ExchangeRate | decimal? | |
| حالة التخصيص | AllocationStatus | string? | |
| معرف الصندوق | FundId | int? | |
| معرف الاعتماد | AppropriationId | int? | |
| معرف الوحدة التنظيمية | OrganizationUnitId | int? | |
| معرف مركز التكلفة | CostCenterId | int? | |
| معرف المشروع | ProjectId | int? | |
| نسخة صف | RowVersion | byte[] | |

### 9.4 خصومات أمر الدفع — PaymentOrderDeductions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف أمر الدفع | PaymentOrderId | int | FK→PaymentOrders |
| رقم البند | LineNumber | int | |
| نوع الخصم | DeductionType | enum (DeductionType) | |
| رمز الخصم | DeductionCode | string? | |
| الوصف | Description | string? | |
| معرف الحساب | AccountId | int | |
| المبلغ | Amount | decimal | |
| نسبة الخصم | DeductionPercent | decimal? | |
| إلزامي | IsMandatory | bool | |
| ضريبة | IsTaxDeduction | bool | |
| معرف جهة الضريبة | TaxAuthorityId | int? | |
| رقم المرجع | ReferenceNumber | string? | |
| نسخة صف | RowVersion | byte[] | |

### 9.5 طلبات الصرف — DisbursementRequests

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الطلب | RequestNumber | string | |
| معرف أمر الدفع | PaymentOrderId | int | FK→PaymentOrders |
| معرف الطالب | RequestedById | int | |
| تاريخ الطلب | RequestDate | DateOnly | |
| الحالة | Status | enum (DisbursementRequestStatus) | |
| يحتوي على تحذير | HasWarning | bool | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 9.6 المدفوعات — Payments

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الدفع | PaymentNumber | string | |
| معرف طلب الصرف | DisbursementRequestId | int | FK→DisbursementRequests |
| معرف أمر الدفع | PaymentOrderId | int | FK→PaymentOrders |
| طريقة الدفع | PaymentMethod | enum (PaymentMethod) | |
| المبلغ | Amount | decimal | |
| معرف الدافع | PaidById | int | |
| وقت الدفع | PaidAt | DateTimeOffset | |
| رقم المرجع | ReferenceNumber | string? | |
| ملاحظات | Notes | string? | |
| الحالة | Status | enum (PaymentStatus) | |
| نسخة صف | RowVersion | byte[] | |

---

## 10. اللجان — Committees (3)

### 10.1 اللجان — Committees

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم اللجنة | CommitteeNumber | string | |
| الاسم | Name | string | |
| نوع اللجنة | CommitteeType | enum (CommitteeType) | |
| رقم قرار التشكيل | FormationDecisionNumber | string | |
| تاريخ قرار التشكيل | FormationDecisionDate | DateOnly | |
| سارية من | ValidFrom | DateOnly | |
| سارية حتى | ValidTo | DateOnly? | |
| الحالة | Status | enum (CommitteeStatus) | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 10.2 أعضاء اللجان — CommitteeMembers

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف اللجنة | CommitteeId | int | FK→Committees |
| معرف الموظف | EmployeeId | int? | |
| اسم العضو | MemberName | string | |
| دور العضو | MemberRole | enum (CommitteeMemberRole) | |
| سارية من | EffectiveFrom | DateOnly | |
| سارية حتى | EffectiveTo | DateOnly? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 10.3 تكليفات اللجان — CommitteeAssignments

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف اللجنة | CommitteeId | int | FK→Committees |
| نوع التكليف | AssignmentType | enum (CommitteeAssignmentType) | |
| معرف أمر الشراء | PurchaseOrderId | int? | |
| تاريخ التكليف | AssignmentDate | DateOnly | |
| رقم القرار | DecisionNumber | string? | |
| تاريخ القرار | DecisionDate | DateOnly? | |
| الحالة | Status | enum (CommitteeAssignmentStatus) | |
| عدد التوقيعات المطلوبة | RequiredSignaturesCount | int | default 1 |
| عدد التوقيعات الفعلية | ActualSignaturesCount | int | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

---

## 11. الإيرادات — Revenue (6)

### 11.1 سندات الإيرادات — RevenueReceipts

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم السند | ReceiptNumber | string | |
| نوع السند | ReceiptType | enum (RevenueReceiptType) | |
| تاريخ السند | ReceiptDate | DateOnly | |
| اسم الدافع | PayerName | string | |
| رقم هوية الدافع | PayerNationalId | string? | |
| معرف الصندوق | FundId | int | |
| معرف التصنيف المالي | BudgetClassificationId | int? | |
| معرف العملة | CurrencyId | int | |
| المبلغ الإجمالي | AmountTotal | decimal | |
| طريقة الدفع | PaymentMethod | enum (PaymentMethod)? | |
| مرجع المعاملة الخارجية | ExternalTransactionRef | string? | |
| معرف قيد اليومية | JournalEntryId | int? | |
| الحالة | Status | enum (RevenueReceiptStatus) | |
| سبب الإلغاء | CancellationReason | string? | |
| نسخة صف | RowVersion | byte[] | |

### 11.2 أسطر سندات الإيرادات — RevenueReceiptLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف السند | ReceiptId | int | FK→RevenueReceipts |
| معرف الحساب | AccountId | int | |
| الوصف | Description | string? | |
| المبلغ | Amount | decimal | |
| نسخة صف | RowVersion | byte[] | |

### 11.3 سندات القبض — ReceiptVouchers

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم السند | VoucherNumber | string | |
| تاريخ السند | VoucherDate | DateOnly | |
| معرف الطرف | PartyId | int | FK→Parties |
| طريقة الدفع | PaymentMethod | enum (PaymentMethod) | |
| المُستلم من | ReceivedFrom | string | |
| ملاحظات | Notes | string? | |
| معرف الإيداع البنكي | DepositSlipId | int? | FK→DepositSlips |
| الحالة | Status | enum (ReceiptVoucherStatus) | |
| معرف المُقدِّم | SubmittedById | int? | |
| وقت التقديم | SubmittedAt | DateTimeOffset? | |
| معرف المُراجع | ReviewedById | int? | |
| وقت المراجعة | ReviewedAt | DateTimeOffset? | |
| سبب الإلغاء | CancellationReason | string? | |
| نسخة صف | RowVersion | byte[] | |

### 11.4 أسطر سندات القبض — ReceiptVoucherLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف سند القبض | ReceiptVoucherId | int | FK→ReceiptVouchers |
| معرف حساب الإيراد | RevenueAccountId | int | |
| المبلغ | Amount | decimal | |
| الوصف | Description | string? | |
| نسخة صف | RowVersion | byte[] | |

### 11.5 الشيكات — Checks

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف سند القبض | ReceiptVoucherId | int | FK→ReceiptVouchers |
| اسم البنك | BankName | string | |
| رقم الشيك | CheckNumber | string | |
| تاريخ الشيك | CheckDate | DateOnly | |
| المبلغ | Amount | decimal | |
| الحالة | Status | enum (CheckStatus) | |
| وقت التحصيل | ClearedAt | DateTimeOffset? | |
| وقت الارتجاع | BouncedAt | DateTimeOffset? | |
| معرف السند البديل | ReplacementVoucherId | int? | FK→ReceiptVouchers |
| نسخة صف | RowVersion | byte[] | |

### 11.6 إيصالات الإيداع — DepositSlips

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الإيصال | SlipNumber | string | |
| تاريخ الإيصال | SlipDate | DateOnly | |
| نوع النموذج | FormType | enum (FormType) | |
| الحالة | Status | enum (DepositSlipStatus) | |
| معرف المعتمِد | ApprovedById | int? | |
| وقت الاعتماد | ApprovedAt | DateTimeOffset? | |
| المبلغ الإجمالي | TotalAmount | decimal | |
| نسخة صف | RowVersion | byte[] | |

---

## 12. الأصول — Assets (9)

### 12.1 مجموعات الأصول — AssetGroups

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| معرف المجموعة الأب | ParentAssetGroupId | int? | FK→self |
| معرف حساب الأصل | AccountAssetId | int? | |
| معرف حساب الإهلاك | AccountDepreciationId | int? | |
| معرف حساب المصروف | AccountExpenseId | int? | |
| معرف حساب الإهراكات المتراكمة | AccountAccumulatedDepreciationId | int? | |
| معرف حساب التخلص | AccountDisposalId | int? | |
| معرف حساب إعادة التقييم | AccountRevaluationId | int? | |
| معرف حساب انخفاض القيمة | AccountImpairmentId | int? | |
| طريقة الإهلاك | DepreciationMethod | string | |
| معدل الإهلاك | DepreciationRate | decimal? | |
| العمر الإنتاجي الافتراضي (سنوات) | DefaultUsefulLifeYears | int? | |
| نسبة القيمة المتبقية | ResidualValuePercentage | decimal? | |
| قابل للإهلاك | IsDepreciable | bool | default true |
| فئة الأصل | AssetCategory | string | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 12.2 الأصول — Assets

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الوصف | Description | string? | |
| معرف مجموعة الأصل | AssetGroupId | int | FK→AssetGroups |
| معرف الموقع | LocationId | int? | |
| معرف الصندوق | FundId | int? | |
| معرف مركز التكلفة | CostCenterId | int? | |
| معرف المُستودِع | CustodianId | int? | |
| وسم الأصل | AssetTag | string? | |
| الباركود | Barcode | string? | |
| الرقم التسلسلي | SerialNumber | string? | |
| رابط الصورة | ImageUrl | string? | |
| رمز العملة | CurrencyCode | string? | |
| القيمة الأصلية | OriginalValue | decimal | |
| تكلفة الاستحواذ | AcquisitionCost | decimal? | |
| القيمة المتبقية | ResidualValue | decimal? | |
| قيمة التنازل | RelinquishmentValue | decimal? | |
| الإهلاك المتراكم | AccumulatedDepreciation | decimal | |
| القيمة الحالية | CurrentValue | decimal? | |
| تاريخ الشراء | PurchaseDate | DateOnly | |
| تاريخ التفعيل | ActivationDate | DateOnly? | |
| تاريخ بدء الإهلاك | DepreciationStartDate | DateOnly | |
| تاريخ آخر إهلاك | LastDepreciationDate | DateOnly? | |
| الحالة | Status | string | |
| نوع الاستحواذ | AcquisitionType | string | |
| العمر الإنتاجي (سنوات) | UsefulLifeYears | int? | |
| مُهلاك بالكامل | IsFullyDepreciated | bool | |
| ملاحظات | Notes | string? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 12.3 حركات الأصول — AssetMovements

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الحركة | MovementNumber | string | |
| معرف الأصل | AssetId | int | FK→Assets |
| نوع الحركة | MovementType | string | |
| تاريخ الحركة | MovementDate | DateTime | |
| نوع المرجع | ReferenceType | string? | |
| معرف المرجع | ReferenceId | int? | |
| معرف الموقع المصدر | FromLocationId | int? | |
| معرف الموقع الهدف | ToLocationId | int? | |
| معرف المُستودِع المصدر | FromCustodianId | int? | |
| معرف المُستودِع الهدف | ToCustodianId | int? | |
| معرف القسم المصدر | FromDepartmentId | int? | |
| معرف القسم الهدف | ToDepartmentId | int? | |
| القيمة القديمة | OldValue | decimal? | |
| القيمة الجديدة | NewValue | decimal? | |
| المبلغ | Amount | decimal? | |
| رمز العملة | CurrencyCode | string? | |
| معرف قيد اليومية | JournalEntryId | int? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 12.4 جداول الإهلاك — DepreciationSchedules

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الأصل | AssetId | int | FK→Assets |
| تاريخ الإهلاك | DepreciationDate | DateOnly | |
| معرف السنة المالية | FiscalYearId | int? | |
| معرف الفترة المالية | FiscalPeriodId | int? | |
| طريقة الإهلاك | DepreciationMethod | string? | |
| أساس الإهلاك | DepreciationBase | decimal? | |
| معدل الإهلاك | DepreciationRate | decimal? | |
| رقم الفترة | PeriodNumber | int? | |
| إجمالي الفترات | TotalPeriods | int? | |
| المبلغ | Amount | decimal | |
| الإهلاك المتراكم | AccumulatedDepreciation | decimal | |
| صافي القيمة الدفترية | NetBookValue | decimal | |
| الحالة | Status | string | |
| معرف قيد اليومية | JournalEntryId | int? | |
| مُلغى | IsReversed | bool | |
| معرف الإهلاك المُلغى | ReversalOfId | int? | FK→self |
| سبب الإلغاء | ReversalReason | string? | |
| تاريخ الإلغاء | ReversalDate | DateOnly? | |
| نسخة صف | RowVersion | byte[] | |

### 12.5 إعادة تقييم الأصول — AssetRevaluations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم إعادة التقييم | RevaluationNumber | string | |
| معرف الأصل | AssetId | int | FK→Assets |
| تاريخ إعادة التقييم | RevaluationDate | DateOnly | |
| طريقة إعادة التقييم | RevaluationMethod | string? | |
| القيمة الدفترية القديمة | OldBookValue | decimal | |
| القيمة الدفترية الجديدة | NewBookValue | decimal | |
| مبلغ إعادة التقييم | RevaluationAmount | decimal | |
| نوع إعادة التقييم | RevaluationType | string? | |
| المُقيِّم | Appraiser | string? | |
| رقم تقرير التقييم | AppraisalReportNumber | string? | |
| رمز العملة | CurrencyCode | string? | |
| معرف حساب إعادة التقييم | AccountRevaluationId | int? | |
| معرف قيد اليومية | JournalEntryId | int? | |
| مُرحَّل | IsPosted | bool | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 12.6 انخفاض قيمة الأصول — AssetImpairments

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم انخفاض القيمة | ImpairmentNumber | string | |
| معرف الأصل | AssetId | int | FK→Assets |
| تاريخ انخفاض القيمة | ImpairmentDate | DateOnly | |
| القيمة الدفترية | CarryingAmount | decimal | |
| المبلغ القابل للاسترداد | RecoverableAmount | decimal | |
| خسورة انخفاض القيمة | ImpairmentLoss | decimal | |
| سبب انخفاض القيمة | ImpairmentReason | string? | |
| وصف انخفاض القيمة | ImpairmentDescription | string? | |
| المُقيِّم | AssessedBy | string? | |
| رقم تقرير التقييم | AssessmentReportNumber | string? | |
| رمز العملة | CurrencyCode | string? | |
| معرف حساب انخفاض القيمة | AccountImpairmentId | int? | |
| معرف قيد اليومية | JournalEntryId | int? | |
| مُرحَّل | IsPosted | bool | |
| مُلغى | IsReversed | bool | |
| معرف ال انخفاض المُلغى | ReversalOfId | int? | FK→self |
| سبب الإلغاء | ReversalReason | string? | |
| تاريخ الإلغاء | ReversalDate | DateOnly? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 12.7 تخلص الأصول — AssetDisposals

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم التخلص | DisposalNumber | string | |
| معرف الأصل | AssetId | int | FK→Assets |
| تاريخ التخلص | DisposalDate | DateOnly | |
| طريقة التخلص | DisposalMethod | string | |
| القيمة الدفترية عند التخلص | BookValueAtDisposal | decimal | |
| الإهلاك المتراكم عند التخلص | AccumulatedDepreciationAtDisposal | decimal | |
| عائد البيع | SaleProceeds | decimal? | |
| تكلفة التخلص | DisposalCost | decimal? | |
| العائد الصافي | NetProceeds | decimal? | |
| الربح أو الخسارة | GainOrLoss | decimal? | |
| اسم المشتري | BuyerName | string? | |
| بيانات الاتصال بالمشتري | BuyerContact | string? | |
| رمز العملة | CurrencyCode | string? | |
| معرف حساب التخلص | AccountDisposalId | int? | |
| معرف قيد اليومية | JournalEntryId | int? | |
| مُرحَّل | IsPosted | bool | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 12.8 جرد الأصول — AssetPhysicalCounts

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الجرد | CountNumber | string | |
| تاريخ الجرد | CountDate | DateOnly | |
| معرف الموقع | LocationId | int? | |
| معرف القسم | DepartmentId | int? | |
| نوع الجرد | CountType | string | |
| الحالة | Status | string | |
| وقت البدء | StartedAt | DateTime? | |
| وقت الانتهاء | CompletedAt | DateTime? | |
| معرف الجازر | CountedById | int? | |
| معرف المُراجع | ReviewedById | int? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 12.9 تفاصيل جرد الأصول — AssetPhysicalCountDetails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف جرد الأصول | AssetPhysicalCountId | int | FK→AssetPhysicalCounts |
| معرف الأصل | AssetId | int | FK→Assets |
| معرف الموقع بالنظام | SystemLocationId | int? | |
| معرف الموقع الفعلي | PhysicalLocationId | int? | |
| معرف المُستودِع بالنظام | SystemCustodianId | int? | |
| معرف المُستودِع الفعلي | PhysicalCustodianId | int? | |
| الحالة بالنظام | SystemStatus | string? | |
| الحالة الفعلية | PhysicalStatus | string? | |
| مُوجود | IsFound | bool | |
| متطابق | IsMatch | bool | |
| ملاحظات الفروقات | DiscrepancyNotes | string? | |
| نسخة صف | RowVersion | byte[] | |

---

## 13. المخزون — Inventory (11)

### 13.1 المواقع — Locations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الباركود | Barcode | string? | |
| معرف الموقع الأب | ParentLocationId | int? | FK→self |
| المستوى | Level | int? | |
| المسار | Breadcrumb | string? | |
| المدينة | City | string? | |
| العنوان | Address | string? | |
| السعة | Capacity | decimal? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 13.2 المستودعات — Warehouses

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| معرف الموقع | LocationId | int? | FK→Locations |
| معرف المدير | ManagerId | int? | |
| العنوان | Address | string? | |
| المدينة | City | string? | |
| الهاتف | Phone | string? | |
| البريد الإلكتروني | Email | string? | |
| السعة الإجمالية | TotalCapacity | decimal? | |
| الحمولة الحالية | CurrentLoad | decimal? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 13.3 فئات الأصناف — ItemCategories

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الاسم بالإنجليزية | NameEn | string? | |
| الوصف | Description | string? | |
| معرف الفئة الأب | ParentItemCategoryId | int? | FK→self |
| المستوى | Level | int? | |
| المسار | Breadcrumb | string? | |
| معرف حساب المصروف | ExpenseAccountId | int? | |
| معرف حساب المخزون | InventoryAccountId | int? | |
| معرف حساب الضريبة | TaxAccountId | int? | |
| فئة الضريبة | TaxClass | string? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 13.4 الوحدات — Units

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الاسم بالعربية | NameAr | string? | |
| نوع الوحدة | UnitType | string? | |
| معرف الوحدة الأساسية | BaseUnitId | int? | FK→self |
| عامل التحويل للأساسية | ConversionToBase | decimal? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 13.5 الأصناف — Items

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الرمز | Code | string | |
| الاسم | Name | string | |
| الاسم بالإنجليزية | NameEn | string? | |
| الوصف | Description | string? | |
| معرف الفئة | CategoryId | int? | FK→ItemCategories |
| معرف الوحدة | UnitId | int | FK→Units |
| معرف المورد | SupplierId | int? | |
| الباركود | Barcode | string? | |
| نوع الصنف | ItemType | string | |
| المخزون الافتتاحي | OpeningStock | decimal? | |
| الكمية المتاحة | AvailableQuantity | decimal? | |
| الكمية المحجوزة | ReservedQuantity | decimal? | |
| التكلفة المتوسطة | AverageCost | decimal? | |
| الحد الأدنى للمخزون | MinimumStock | decimal? | |
| الحد الأقصى للمخزون | MaximumStock | decimal? | |
| مستوى إعادة الطلب | ReorderLevel | decimal? | |
| كمية إعادة الطلب | ReorderQuantity | decimal? | |
| مدة التسليم (أيام) | LeadTimeDays | int? | |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 13.6 وحدات الأصناف — ItemUnits

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal | |
| أساسية | IsBase | bool | |
| نسخة صف | RowVersion | byte[] | |

### 13.7 معاملات المخزون — StockTransactions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم المعاملة | TransactionNumber | string | |
| نوع المعاملة | TransactionType | string | |
| تاريخ المعاملة | TransactionDate | DateTime | |
| نوع المرجع | ReferenceType | string? | |
| معرف المرجع | ReferenceId | int? | |
| رقم المرجع | ReferenceNumber | string? | |
| معرف المستودع | WarehouseId | int | FK→Warehouses |
| معرف الموقع | LocationId | int | FK→Locations |
| معرف الخزّان | BinId | int? | |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الدفعة | LotId | int? | |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal? | |
| الكمية | Quantity | decimal | |
| تكلفة الوحدة | UnitCost | decimal? | |
| التكلفة الإجمالية | TotalCost | decimal? | |
| الكمية قبل | QuantityBefore | decimal? | |
| الكمية بعد | QuantityAfter | decimal? | |
| ملاحظات | Notes | string? | |
| مُلغى | IsReversed | bool | |
| معرف المعاملة المُلغاة | ReversalOfId | int? | FK→self |
| سبب الإلغاء | ReversalReason | string? | |
| تاريخ الإلغاء | ReversalDate | DateTime? | |
| نسخة صف | RowVersion | byte[] | |

### 13.8 إيصالات استلام البضائع — GoodsReceiptNotes

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الإيصال | GRNNumber | string | |
| تاريخ الإيصال | GRNDate | DateTime | |
| معرف المورد | SupplierId | int? | |
| معرف أمر الشراء | PurchaseOrderId | int | |
| رقم أمر الشراء | PurchaseOrderNumber | string? | |
| معرف المستودع | WarehouseId | int | FK→Warehouses |
| معرف الموقع | LocationId | int | FK→Locations |
| رقم الفاتورة | InvoiceNumber | string? | |
| تاريخ الفاتورة | InvoiceDate | DateOnly? | |
| الكمية الإجمالية | TotalQuantity | decimal? | |
| المبلغ الإجمالي | TotalAmount | decimal? | |
| الحالة | Status | string | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 13.9 تفاصيل إيصالات استلام البضائع — GoodsReceiptNoteDetails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الإيصال | GRNId | int | FK→GoodsReceiptNotes |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal? | |
| الكمية المطلوبة | OrderedQuantity | decimal | |
| الكمية المستلمة | ReceivedQuantity | decimal | |
| الكمية المقبولة | AcceptedQuantity | decimal? | |
| الكمية المرفوضة | RejectedQuantity | decimal? | |
| تكلفة الوحدة | UnitCost | decimal? | |
| التكلفة الإجمالية | TotalCost | decimal? | |
| رقم الدفعة | BatchNumber | string? | |
| تاريخ الانتهاء | ExpiryDate | DateOnly? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 13.10 جرد المخزون — StockTakes

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| رقم الجرد | StockTakeNumber | string | |
| تاريخ الجرد | StockTakeDate | DateOnly | |
| معرف المستودع | WarehouseId | int | FK→Warehouses |
| معرف الموقع | LocationId | int? | FK→Locations |
| نوع الجرد | StockTakeType | string | |
| الحالة | Status | string | |
| وقت البدء | StartedAt | DateTime? | |
| وقت الانتهاء | CompletedAt | DateTime? | |
| معرف الجازر | CountedById | int? | |
| معرف المُراجع | ReviewedById | int? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

### 13.11 تفاصيل جرد المخزون — StockTakeDetails

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف جرد المخزون | StockTakeId | int | FK→StockTakes |
| معرف الصنف | ItemId | int | FK→Items |
| معرف الوحدة | UnitId | int | FK→Units |
| عامل التحويل | ConversionFactor | decimal? | |
| كمية النظام | SystemQuantity | decimal | |
| الكمية المُجرَّدة | CountedQuantity | decimal | |
| فرق الكمية | QuantityDifference | decimal | |
| تكلفة الوحدة | UnitCost | decimal? | |
| مبلغ الفرق | DifferenceAmount | decimal? | |
| معرف الدفعة | LotId | int? | |
| معرف الخزّان | BinId | int? | |
| ملاحظات | Notes | string? | |
| نسخة صف | RowVersion | byte[] | |

---

## 14. البنوك — Banking (4)

### 14.1 كشوفات الحسابات البنكية — BankStatements

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| الاسم | Name | string | |
| معرف الحساب البنكي | BankAccountId | int | FK→BankAccounts |
| معرف اليومية | JournalId | int | |
| تاريخ الكشف | StatementDate | DateOnly | |
| الرصيد الافتتاحي | BalanceStart | decimal | |
| الرصيد الختامي | BalanceEnd | decimal | |
| الرصيد الختامي المحسوب | BalanceEndComputed | decimal | |
| مصدر الاستيراد | ImportSource | string? | |
| الحالة | Status | enum (BankStatementStatus) | |
| نسخة صف | RowVersion | byte[] | |

### 14.2 أسطر كشوفات الحسابات البنكية — BankStatementLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الكشف | StatementId | int | FK→BankStatements |
| رقم البند | LineNumber | int | |
| تاريخ المعاملة | TransactionDate | DateOnly | |
| الوصف | Description | string? | |
| المدين | Debit | decimal | |
| الدائن | Credit | decimal | |
| الرصيد | Balance | decimal? | |
| المرجع | Reference | string? | |
| مُسوَّى | IsReconciled | bool | |
| معرف سطر قيد اليومية | JournalEntryLineId | long? | |
| نسخة صف | RowVersion | byte[] | |

### 14.3 التسوية البنكية — BankReconciliations

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف الحساب البنكي | BankAccountId | int | FK→BankAccounts |
| معرف الكشف | StatementId | int | FK→BankStatements |
| تاريخ التسوية | ReconciliationDate | DateOnly | |
| رصيد الدفاتر | BookBalance | decimal | |
| رصيد الكشف | StatementBalance | decimal | |
| الرصيد المُعدَّل | AdjustedBalance | decimal | |
| الفرق | Difference | decimal? | |
| الحالة | Status | enum (ReconciliationStatus) | |
| معرف المُعِد | PreparedById | int | |
| معرف المعتمِد | ApprovedById | int? | |
| سبب الرفض | RejectionReason | string? | |
| نسخة صف | RowVersion | byte[] | |

### 14.4 أسطر التسوية البنكية — BankReconciliationLines

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف التسوية | ReconciliationId | int | FK→BankReconciliations |
| نوع البند | LineType | enum (ReconciliationLineType) | |
| معرف سطر الكشف | BankStatementLineId | int? | FK→BankStatementLines |
| معرف سطر قيد اليومية | JournalEntryLineId | long? | |
| المبلغ | Amount | decimal | |
| الوصف | Description | string? | |
| الحالة | Status | enum (ReconciliationLineStatus) | |
| نسخة صف | RowVersion | byte[] | |

---

## 15. المخرجات — Infrastructure (1)

### 15.1 رسائل صندوق الخارج — OutboxMessages

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم النوع | TypeName | string | |
| الحمولة | Payload | string | |
| معرف المجموعة | AggregateId | string? | |
| معرف الارتباط | CorrelationId | Guid | default NewGuid |
| الحالة | Status | enum (OutboxMessageStatus) | default Pending |
| وقت الإنشاء | CreatedAt | DateTimeOffset | default UtcNow |
| وقت المعالجة | ProcessedAt | DateTimeOffset? | |
| عدد المحاولات | RetryCount | int | |
| المحاولة التالية | NextRetryAt | DateTimeOffset? | |
| رسالة الخطأ | ErrorMessage | string? | |

---

## 16. المهام الخلفية — Background Jobs (3)

### 16.1 تعريفات المهام — BackgroundJobDefinitions

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| اسم النوع | TypeName | string | |
| الاسم المعروض | DisplayName | string | |
| نوع الجدولة | ScheduleType | enum (BackgroundJobScheduleType) | |
| قيمة الجدولة | ScheduleValue | string | |
| محاولات إعادة المحاولة القصوى | MaxRetries | int | default 5 |
| المهلة بالثواني | TimeoutSeconds | int | default 300 |
| نشط | IsActive | bool | default true |
| نسخة صف | RowVersion | byte[] | |

### 16.2 مثيلات المهام — BackgroundJobInstances

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف التعريف | JobDefinitionId | int | FK→BackgroundJobDefinitions |
| مجدول في | ScheduledAt | DateTimeOffset | |
| بدأ في | StartedAt | DateTimeOffset? | |
| انتهى في | CompletedAt | DateTimeOffset? | |
| الحالة | Status | enum (BackgroundJobStatus) | default Pending |
| عدد المحاولات | RetryCount | int | |
| معرف المُعادية | IdempotencyKey | string? | |
| رسالة الخطأ | ErrorMessage | string? | |
| المُشغِّل | TriggeredBy | string | default "Scheduler" |
| نسخة صف | RowVersion | byte[] | |

### 16.3 سجلات التنفيذ — BackgroundJobExecutionLogs

| العمود | Column | النوع | القيود |
|--------|--------|------|--------|
| المعرف | Id | int | PK |
| معرف المثيل | JobInstanceId | int | FK→BackgroundJobInstances |
| رقم المحاولة | AttemptNumber | int | |
| بدأ في | StartedAt | DateTimeOffset | |
| انتهى في | CompletedAt | DateTimeOffset? | |
| الحالة | Status | enum (BackgroundJobExecutionStatus) | default Running |
| رسالة الخطأ | ErrorMessage | string? | |
| تتبع المكدس | StackTrace | string? | |
| نسخة صف | RowVersion | byte[] | |

---

## ملخص الإحصائيات

| الوحدة | عدد الجداول |
|--------|------------|
| الإعدادات المالية | 6 |
| الأمان والصلاحيات | 18 |
| سير العمل | 4 |
| التنظيم | 5 |
| المحاسبة | 14 |
| الموازنة | 11 |
| الأطراف | 1 |
| المشتريات | 8 |
| المدفوعات | 6 |
| اللجان | 3 |
| الإيرادات | 6 |
| الأصول | 9 |
| المخزون | 11 |
| البنوك | 4 |
| المخرجات | 1 |
| المهام الخلفية | 3 |
| **المجموع** | **110** |
