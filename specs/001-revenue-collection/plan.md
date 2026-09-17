# خطة التنفيذ: نظام إدارة وتوريد الإيرادات الحكومية

## السياق الفني

### البنية الحالية
- **Framework**: .NET 10 / C# 13, EF Core + SQL Server, MediatR, FluentValidation
- **نمط الفصل**: Clean Architecture (Domain → Application → Infrastructure → Web)
- **Kيانات الإيرادات الحالية**: RevenueClaim, CollectionOrder, ReceiptVoucher, ReceiptVoucherLine, Check, DepositSlip47, DepositSlip48
- **الأحداث النطاقية**: 6 أحداث موجودة (CashReceiptApproved, CheckReceiptApproved, CheckCleared, CheckBounced, DepositSlip47Approved, DepositSlip48Approved)
- **معالجات الأحداث**: RevenueJournalEntryService تتولى إنشاء القيود المحاسبية
- **الحقول المعرفة**: BaseAuditableEntity (int PK) + byte[] RowVersion (concurrency)

### المطابقة مع المواصفة
الكيانات والأحداث الموجودة تتطابق 100% مع المواصفة. المطلوب:
1. التحقق من اكتمال جميع الحقول والعلاقات
2. التحقق من صحة سير الحالات (state transitions)
3. التحقق من دقة القيود المحاسبية في Event Handlers
4. التأكد من أن RevenueMetricsCalculator يحسب AvailableAmount بشكل صحيح

## فحص الدستور (Constitution Check)

| المبدأ | الحالة | ملاحظات |
|--------|--------|---------|
| بنية النظافة | ✅ متوافق | Domain/Application/Web منفصلة |
| MediatR + CQRS | ✅ متوافق | Command/Query مفصولة |
| FluentValidation | ✅ متوافق | Validators موجودة |
| IApplicationDbContext | ✅ متوافق | DbContext موحد |
| BaseAuditableEntity | ✅ متوافق | جميع الكيانات ترث BaseAuditableEntity |
| RowVersion | ✅ متوافق | جميع الكيانات لديها byte[] RowVersion |
| Result<T> | ✅ متوافق | Commands تُرجع Result/Result<T> |
| PermissionCodes | ✅ متوافق | جميع الـ endpoints تستخدم [Authorize] |
| IEndpointGroup | ✅ متوافق | Minimal APIs فقط |

**لا توجد انتهاكات** — المواصفة متوافقة بالكامل مع الدستور.

---

## المرحلة 0: البحث والتوضيح

### قرارات البحث المطلوبة

| القرار | البديل المختار | المبرر |
|--------|---------------|--------|
| استراتيجية قفل التعديل المتزامن | RowVersion (Optimistic) | موجود بالفعل في جميع الكيانات — لا عمل مطلوب |
| التكامل البنكي | يدوي | المستخدم يُدخل بيانات التصفية/الارتجاع يدويًا |
| عدد الشيكات لكل سند قبض | شيك واحد فقط | FR-04: "شيك واحد لكل سند قبض" |
| حجم البيانات | < 500 مطالبة/شهر | نظام متوسط الحجم |

---

## المرحلة 1: نموذج البيانات

### الكيانات وال relationships

**RevenueClaim** (المطالبة الإيرادية)
- ClaimNumber (string, فريد)
- ClaimDate (DateOnly)
- PartyId (int, FK → Party)
- TotalAmount (decimal)
- Status (ClaimStatus enum)
- **CollectionOrders** (1:N)

**CollectionOrder** (أمر التحصيل)
- RevenueClaimId (int, FK → RevenueClaim)
- OrderNumber (string, فريد)
- OrderDate (DateOnly)
- AuthorizedAmount (decimal)
- Status (CollectionOrderStatus enum)
- **ReceiptVouchers** (1:N)

**ReceiptVoucher** (سند القبض)
- CollectionOrderId (int, FK → CollectionOrder)
- VoucherNumber (string, فريد)
- VoucherDate (DateOnly)
- PartyId (int, FK → Party)
- PaymentMethod (PaymentMethod enum: Cash=1, Check=2)
- ReceivedFrom (string?)
- Status (ReceiptVoucherStatus enum)
- **ReceiptVoucherLines** (1:N)
- **Checks** (1:N) — لكن المواصفة تقول شيك واحد فقط

**ReceiptVoucherLine** (بند سند القبض)
- ReceiptVoucherId (int, FK → ReceiptVoucher)
- RevenueAccountId (int, FK → Account)
- Amount (decimal)
- Description (string?)

**Check** (الشيك)
- ReceiptVoucherId (int, FK → ReceiptVoucher)
- BankName (string)
- CheckNumber (string)
- CheckDate (DateOnly)
- Amount (decimal)
- Status (CheckStatus enum)
- DepositSlip48Id (int?, FK → DepositSlip48)
- ClearedAt (DateTimeOffset?)
- BouncedAt (DateTimeOffset?)

**DepositSlip47** (حافظة توريد نقدي)
- SlipNumber (string, فريد)
- SlipDate (DateOnly)
- TotalAmount (decimal)
- **ReceiptVouchers** (1:N)

**DepositSlip48** (حافظة إرسال شيكات)
- SlipNumber (string, فريد)
- SlipDate (DateOnly)
- TotalAmount (decimal)
- **Checks** (1:N)

### حالات الكيانات

| الكيان | الحالات |
|--------|---------|
| RevenueClaim | Draft(0) → PendingApproval(1) → Open(2) → PartiallySettled(3) → Settled(4) / WrittenOff(5) |
| CollectionOrder | Draft(0) → PendingApproval(1) → Approved(2) → PartiallyCollected(3) → Collected(4) / Cancelled(5) |
| ReceiptVoucher | Draft(0) → Approved(1) / Cancelled(2) |
| Check | Received(0) → UnderCollection(1) → Cleared(2) / Bounced(3) |
| DepositSlip47 | Draft(0) → Approved(1) |
| DepositSlip48 | Draft(0) → Approved(1) |

### قيود البيانات
- RevenueClaim.ClaimNumber: فريد، تسلسلي
- CollectionOrder.OrderNumber: فريد، تسلسلي
- ReceiptVoucher.VoucherNumber: فريد، تسلسلي
- Check.CheckNumber: فريد
- DepositSlip47.SlipNumber: فريد، تسلسلي
- DepositSlip48.SlipNumber: فريد، تسلسلي
- ReceiptVoucher.PaymentMethod: Cash أو Check فقط
- Check: شيك واحد لكل سند قبض (FR-04)

---

## المرحلة 2: القيود المحاسبية

### حساب المبلغ المتاح (FR-09)

```
CollectedAmount = Σ(Approved Cash Vouchers) + Σ(Cleared Checks)
UnderCollectionAmount = Σ(Checks with status Received or UnderCollection)
OutstandingAmount = ClaimAmount - CollectedAmount
AvailableAmount = OutstandingAmount - UnderCollectionAmount
```

### القيود المحاسبية لكل عملية

| العملية | الحساب المدين | الحساب الدائن | ملاحظات |
|---------|-------------|-------------|---------|
| اعتماد سند قبض نقدي | 1812 نقدية لدى أمين الصندوق | حساب الإيراد | اعتراف فعلي |
| اعتماد سند قبض بالشيك | 110201 شيكات برسم الإيداع | 210901 متحصلات شيكات معلقة | قيد وسيط |
| اعتماد حافظة 47 | 110102 حساب البنك | 1812 نقدية لدى أمين الصندوق | نقل أمانة |
| اعتماد حافظة 48 | 110202 شيكات تحت التحصيل | 110201 شيكات برسم الإيداع | تحويل |
| تصفية شيك | قيد 1: 110102 البنك / 110202 تحت التحصيل<br>قيد 2: 210901 متحصلات معلقة / حساب الإيراد | مزدوج | اعتراف فعلي |
| ارتجاع شيك | 210901 متحصلات شيكات معلقة | 110202 شيكات تحت التحصيل | عكس وسيط |

---

## المرحلة 3: بنية الأوامر والمعالجات

### الأوامر المطلوبة (مطابقة للحالية)

| الأمر | الكيان | الوظيفة |
|-------|--------|---------|
| CreateRevenueClaimCommand | RevenueClaim | إنشاء مطالبة |
| ApproveRevenueClaimCommand | RevenueClaim | اعتماد مطالبة |
| CreateCollectionOrderCommand | CollectionOrder | إنشاء أمر تحصيل |
| ApproveCollectionOrderCommand | CollectionOrder | اعتماد أمر تحصيل |
| CreateReceiptVoucherCommand | ReceiptVoucher | إنشاء سند قبض |
| ApproveReceiptVoucherCommand | ReceiptVoucher | اعتماد سند قبض |
| CreateDepositSlip47Command | DepositSlip47 | إنشاء حافظة 47 |
| ApproveDepositSlip47Command | DepositSlip47 | اعتماد حافظة 47 |
| CreateDepositSlip48Command | DepositSlip48 | إنشاء حافظة 48 |
| ApproveDepositSlip48Command | DepositSlip48 | اعتماد حافظة 48 |
| ClearCheckCommand | Check | تصفية شيك |
| BounceCheckCommand | Check | ارتجاع شيك |

### الاستعلامات المطلوبة (مطابقة للحالية)

| الاستعلام | المخرجات |
|-----------|----------|
| GetRevenueClaimsQuery | List<RevenueClaimDto> |
| GetCollectionOrdersQuery | List<CollectionOrderDto> |
| GetReceiptVouchersQuery | List<ReceiptVoucherDto> |

### معالجات الأحداث (مطابقة للحالية)

| المعالج | الحدث | القيود |
|---------|-------|-------|
| CashReceiptApprovedEventHandler | CashReceiptApprovedEvent | نقدية صندوق → إيراد |
| CheckReceiptApprovedEventHandler | CheckReceiptApprovedEvent | شيكات برسم الإيداع → متحصلات معلقة |
| CheckClearedEventHandler | CheckClearedEvent | مزدوج: بنك + إيراد |
| CheckBouncedEventHandler | CheckBouncedEvent | عكس وسيط |
| DepositSlip47ApprovedEventHandler | DepositSlip47ApprovedEvent | بنك → نقدية صندوق |
| DepositSlip48ApprovedEventHandler | DepositSlip48ApprovedEvent | شيكات تحت التحصيل → شيكات برسم الإيداع |

---

## المرحلة 4: واجهات API

### الـ Endpoints المطلوبة

| المجموعة | Routes |
|----------|--------|
| RevenueClaims | GET `/`, POST `/`, POST `/{id}/approve` |
| CollectionOrders | GET `/`, POST `/`, POST `/{id}/approve` |
| ReceiptVouchers | GET `/`, POST `/`, POST `/{id}/approve` |
| Checks | POST `/{id}/clear`, POST `/{id}/bounce` |
| DepositSlips | POST `/slip47`, POST `/slip47/{id}/approve`, POST `/slip48`, POST `/slip48/{id}/approve` |

---

## تقدير حجم العمل

| المرحلة | المحتوى | الحالة |
|---------|---------|--------|
| المرحلة 0 | البحث | ✅ مكتمل |
| المرحلة 1 | نموذج البيانات | ✅ مطابق للحالي |
| المرحلة 2 | القيود المحاسبية | ✅ مطابق للحالي |
| المرحلة 3 | الأوامر والمعالجات | ✅ مطابق للحالي |
| المرحلة 4 | واجهات API | ✅ مطابقة للحالية |

**التقييم**: الكيانات والأحداث والمعالجات الموجودة مطابقة بالكامل للمواصفة. لا يوجد عمل تنفيذي مطلوب — فقط التحقق من الاكتمال.
