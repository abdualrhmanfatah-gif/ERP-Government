# نموذج البيانات: نظام إدارة وتوريد الإيرادات الحكومية

## مخطط الكيانات (ER Diagram)

```
RevenueClaim 1 ──── N CollectionOrder
CollectionOrder 1 ──── N ReceiptVoucher
ReceiptVoucher 1 ──── N ReceiptVoucherLine
ReceiptVoucher 1 ──── 1 Check (اختياري — فقط عند PaymentMethod=Check)
DepositSlip47 1 ──── N ReceiptVoucher
DepositSlip48 1 ──── N Check
```

---

## الكيانات التفصيلية

### RevenueClaim (المطالبة الإيرادية)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| ClaimNumber | string | UNIQUE, NOT NULL | رقم تسلسلي تلقائي |
| ClaimDate | DateOnly | NOT NULL | تاريخ المطالبة |
| PartyId | int | FK → Party | الجهة المطالبة |
| TotalAmount | decimal(18,2) | NOT NULL, >= 0 | إجمالي المبلغ |
| Status | ClaimStatus | NOT NULL | Draft=0, PendingApproval=1, Open=2, PartiallySettled=3, Settled=4, WrittenOff=5 |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency |乐观锁 |

**Relationships**:
- RevenueClaim 1:N CollectionOrder

---

### CollectionOrder (أمر التحصيل)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| RevenueClaimId | int | FK → RevenueClaim, NOT NULL | المطالبة المرتبطة |
| OrderNumber | string | UNIQUE, NOT NULL | رقم تسلسلي تلقائي |
| OrderDate | DateOnly | NOT NULL | تاريخ الأمر |
| AuthorizedAmount | decimal(18,2) | NOT NULL, >= 0 | المبلغ المفوض |
| Status | CollectionOrderStatus | NOT NULL | Draft=0, PendingApproval=1, Approved=2, PartiallyCollected=3, Collected=4, Cancelled=5 |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency | |

**Relationships**:
- CollectionOrder N:1 RevenueClaim
- CollectionOrder 1:N ReceiptVoucher

**Validation**:
- AuthorizedAmount <= RevenueClaim.TotalAmount

---

### ReceiptVoucher (سند القبض)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| CollectionOrderId | int | FK → CollectionOrder, NOT NULL | أمر التحصيل المرتبط |
| VoucherNumber | string | UNIQUE, NOT NULL | رقم تسلسلي تلقائي |
| VoucherDate | DateOnly | NOT NULL | تاريخ السند |
| PartyId | int | FK → Party | الجهة المحصلة |
| PaymentMethod | PaymentMethod | NOT NULL | Cash=1, Check=2 |
| ReceivedFrom | string? | | اسم المحصّل |
| Status | ReceiptVoucherStatus | NOT NULL | Draft=0, Approved=1, Cancelled=2 |
| ApprovedById | int? | | |
| ApprovedAt | DateTimeOffset? | | |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency | |

**Relationships**:
- ReceiptVoucher N:1 CollectionOrder
- ReceiptVoucher 1:N ReceiptVoucherLine
- ReceiptVoucher 1:1 Check (اختياري — فقط عند PaymentMethod=Check)
- ReceiptVoucher N:1 DepositSlip47 (اختياري)

---

### ReceiptVoucherLine (بند سند القبض)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| ReceiptVoucherId | int | FK → ReceiptVoucher, NOT NULL | سند القبض المرتبط |
| RevenueAccountId | int | FK → Account, NOT NULL | حساب الإيراد |
| Amount | decimal(18,2) | NOT NULL, > 0 | مبلغ البند |
| Description | string? | | وصف البند |

**Relationships**:
- ReceiptVoucherLine N:1 ReceiptVoucher

---

### Check (الشيك)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| ReceiptVoucherId | int | FK → ReceiptVoucher, NOT NULL | سند القبض المرتبط |
| CheckNumber | string | NOT NULL | رقم الشيك |
| BankName | string | NOT NULL | اسم البنك |
| CheckDate | DateOnly | NOT NULL | تاريخ الشيك |
| Amount | decimal(18,2) | NOT NULL, > 0 | مبلغ الشيك |
| Status | CheckStatus | NOT NULL | Received=0, UnderCollection=1, Cleared=2, Bounced=3 |
| DepositSlip48Id | int? | FK → DepositSlip48 | حافظة الإرسال المرتبطة |
| ClearedAt | DateTimeOffset? | | تاريخ التصفية |
| BouncedAt | DateTimeOffset? | | تاريخ الارتجاع |
| ReplacementVoucherId | int? | FK → ReceiptVoucher | سند القبض البديل (عند الارتجاع) |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency | |

**Relationships**:
- Check N:1 ReceiptVoucher
- Check N:1 DepositSlip48 (اختياري)

**Constraint**: شيك واحد فقط لكل سند قبض (FR-04)

---

### DepositSlip47 (حافظة توريد نقدي)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| SlipNumber | string | UNIQUE, NOT NULL | رقم تسلسلي تلقائي |
| SlipDate | DateOnly | NOT NULL | تاريخ الحافظة |
| TotalAmount | decimal(18,2) | NOT NULL, >= 0 | إجمالي المبالغ |
| ApprovedById | int? | | |
| ApprovedAt | DateTimeOffset? | | |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency | |

**Relationships**:
- DepositSlip47 1:N ReceiptVoucher

---

### DepositSlip48 (حافظة إرسال شيكات)

| الحقل | النوع | القيد | ملاحظات |
|-------|-------|-------|---------|
| Id | int | PK | BaseAuditableEntity |
| SlipNumber | string | UNIQUE, NOT NULL | رقم تسلسلي تلقائي |
| SlipDate | DateOnly | NOT NULL | تاريخ الحافظة |
| TotalAmount | decimal(18,2) | NOT NULL, >= 0 | إجمالي المبالغ |
| ApprovedById | int? | | |
| ApprovedAt | DateTimeOffset? | | |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency | |

**Relationships**:
- DepositSlip48 1:N Check

---

## حالات الكيانات (State Machines)

### RevenueClaim

```
Draft ──→ PendingApproval ──→ Open ──→ PartiallySettled ──→ Settled
  │                                          │
  └──────────────────────────────────────────┴──→ WrittenOff
```

### CollectionOrder

```
Draft ──→ PendingApproval ──→ Approved ──→ PartiallyCollected ──→ Collected
  │                                                          │
  └──────────────────────────────────────────────────────────┴──→ Cancelled
```

### ReceiptVoucher

```
Draft ──→ Approved
  │
  └──→ Cancelled
```

### Check

```
Received ──→ UnderCollection ──→ Cleared
                                  │
                                  └──→ Bounced
```

### DepositSlip47 / DepositSlip48

```
Draft ──→ Approved
```

---

## الحسابات المحاسبية المطلوبة

| الرمز | اسم الحساب | النوع | رصيد طبيعي |
|-------|-----------|-------|-----------|
| 181 | نقدية في الصندوق | أصول | مدين |
| 1811 | الصندوق المركزي الرئيسي | أصول | مدين |
| 1812 | نقدية لدى أمين الصندوق | أصول | مدين |
| 110102 | حساب البنك | أصول | مدين |
| 110201 | شيكات برسم الإيداع | أصول | مدين |
| 110202 | شيكات تحت التحصيل | أصول | مدين |
| 210901 | متحصلات شيكات معلقة | خصوم | دائن |
| (حساب الإيراد) | حساب الإيراد المختص | إيرادات | دائن |
