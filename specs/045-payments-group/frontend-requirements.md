# Frontend Requirements: Payments Group (PAY-01..04)

**Branch**: `045-payments-group` | **Date**: 2026-09-09 (amended per request-first workflow)

**Purpose**: Detailed screen, field, and behavior specifications for all payments UI screens. References [spec.md](./spec.md) for functional requirements (FR-*) and user scenarios (US-*).

**Confirmed requirements** are marked ✓. **Design proposals** are marked [DESIGN].

---

## Table of Contents

- [PAY-02: Disbursement Requests](#pay-02-disbursement-requests)
- [PAY-01: Payment Orders](#pay-01-payment-orders)
- [PAY-03: Payment Execution](#pay-03-payment-execution)
- [PAY-04: Bank Accounts](#pay-04-bank-accounts)
- [Cross-cutting UI Rules](#cross-cutting-ui-rules)

---

## PAY-02: Disbursement Requests

> Request-first workflow: request → approval (dual signature) → order generated → full PAY-01 lifecycle.

### PAY-02-LIST: Disbursement Requests List Page

**Route**: `/payments/disbursement-requests`

**Purpose**: Show all disbursement requests with status, amount, and requester information. Allow filtering by status and requester. Quick access to create new request or view details.

**Authorized users**: All users with `DisbursementRequests.View` permission.

**Data source**: `GET /api/Payments/DisbursementRequests` (query parameters for filters).

#### Fields

| Field | Column header (Arabic) | Source | Editable | Visible | Format |
|---|---|---|---|---|---|
| `requestNumber` | رقم الطلب | server | No | Always | DSB-{D6} |
| `requestedByName` | مقدم الطلب | server | No | Always | text |
| `beneficiaryName` | المستفيد | server | No | Always | text |
| `requestedAmount` | المبلغ المطلوب | server | No | Always | decimal(23,2) + currency symbol |
| `approvedAmount` | المبلغ المعتمد | ApprovalHistory | No | After first approval | decimal(23,2) + currency symbol |
| `status` | الحالة | server | No | Always | color-coded badge |
| `requestDate` | تاريخ الطلب | server | No | Always | date (YYYY-MM-DD) |
| `purpose` | الغرض | server | No | Always | text (truncated) |

#### Status Badges (CC-2)

| Status | Arabic label | Color |
|---|---|---|
| Draft | مسودة | gray |
| PendingApproval | قيد الموافقة | blue |
| Approved | معتمد | green |
| Rejected | مرفوض | red |
| Cancelled | ملغي | gray |
| Disbursed | تم الصرف | green (filled) |
| Invalidated | ملغي تلقائياً | red (outline) |

#### Filters

| Filter | Field | Type | Options |
|---|---|---|---|
| الحالة | `status` | multi-select | All + 7 status options |
| مقدم الطلب | `requestedByName` | text search | autocomplete |

#### Actions

| Button | Label (Arabic) | Condition | Permission |
|---|---|---|---|
| New request | طلب صرف جديد | Always visible | `DisbursementRequests.Create` |

#### Loading / Empty / Error

- **Loading**: skeleton rows (8 rows × 6 columns).
- **Empty**: "لا توجد طلبات صرف" with icon.
- **Error**: server message rendered verbatim (CC-2) with retry button.

---

### PAY-02-CREATE: Create Disbursement Request Page

**Route**: `/payments/disbursement-requests/create`

**Purpose**: Allow a user to create an independent disbursement request (no existing payment order required). Server allocates request number (DSB-{D6}) at creation.

**Authorized users**: Users with `DisbursementRequests.Create` permission.

**Data source**: `POST /api/Payments/DisbursementRequests` with body: `{beneficiaryName!, requestedAmount!, currencyId!, purpose!, financialYearId!, notes?}`.

#### Fields

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `beneficiaryName` | اسم المستفيد | user input | Yes | ✓ | nonblank, server-validated |
| `requestedAmount` | المبلغ المطلوب | user input | Yes | ✓ | positive (> 0), decimal(23,2) |
| `currencyId` | العملة | dropdown (024 currencies) | Yes | ✓ | active currency only |
| `purpose` | الغرض | user input | Yes | ✓ | nonblank |
| `financialYearId` | السنة المالية | dropdown (fiscal years) | Yes | ✓ | active year only |
| `notes` | ملاحظات | user input | Yes | No | free text |

#### Default Values

- `currencyId`: user's default currency (if available).
- `financialYearId`: current active fiscal year.

#### Validation

| Rule | Trigger | Message (Arabic) |
|---|---|---|
| Required fields | submit | "هذا الحقل مطلوب" per field |
| `requestedAmount > 0` | submit | "المبلغ المطلوب يجب أن يكون أكبر من صفر" |
| `beneficiaryName` nonblank | submit | "اسم المستفيد مطلوب" |

#### Actions

| Button | Label (Arabic) | Action |
|---|---|---|
| Save draft | حفظ مسودة | `POST /api/Payments/DisbursementRequests` → redirect to detail |
| Cancel | إلغاء | return to list |

#### Loading / Empty / Error

- **Loading**: form skeleton (6 field skeletons).
- **Empty**: N/A (always has form fields).
- **Error**: inline field errors from server (4xx) + toast for 5xx (CC-2).

---

### PAY-02-DETAIL: Disbursement Request Detail Page

**Route**: `/payments/disbursement-requests/:id`

**Purpose**: Show complete request information, approval history (dual-signature Stepper), linked order (after approval), and lifecycle action buttons. This is the primary work screen for reviewers.

**Authorized users**: Users with `DisbursementRequests.View` permission.

**Data source**: `GET /api/Payments/DisbursementRequests/{id}` with `approvals[]` from ApprovalHistory.

#### Header Section

| Field | Label (Arabic) | Source | Editable | Format |
|---|---|---|---|---|
| `requestNumber` | رقم الطلب | server | No | DSB-{D6} |
| `requestedByName` | مقدم الطلب | server | No | text |
| `beneficiaryName` | المستفيد | server | No | text |
| `requestedAmount` | المبلغ المطلوب | server | No | decimal(23,2) + currency |
| `approvedAmount` | المبلغ المعتمد | ApprovalHistory | No | decimal(23,2) + currency (visible after approval) |
| `currency` | العملة | server | No | text |
| `purpose` | الغرض | server | No | text |
| `financialYearId` | السنة المالية | server | No | text |
| `requestDate` | تاريخ الطلب | server | No | date |
| `status` | الحالة | server | No | color-coded badge |
| `notes` | ملاحظات | server | No | text (if present) |

#### Dual-Signature Stepper (FR-013, ✓ confirmed)

**Purpose**: Visual representation of the two-signature approval process. Each step shows the signer's identity, role, decision, time, approved amount, and issuing authority.

**Component**: Stepper with 2 steps.

**Step data source**: `approvals[]` from `DisbursementRequestDetailDto` — ApprovalHistory rows (DocumentType = "DisbursementRequest").

| Step field | Source | Format |
|---|---|---|
| step | `approvals[i].step` | 1 or 2 |
| approverName | `approvals[i].approverName` | text |
| role | `approvals[i].role` | Arabic role name |
| decision | `approvals[i].decision` | Approve/Reject badge |
| decisionAt | `approvals[i].decisionAt` | datetime |
| approvedAmount | `approvals[i].approvedAmount` | decimal(23,2) + currency |
| issuingAuthorityName | `approvals[i].issuingAuthorityName` | text (approval only) |
| issuingAuthorityCapacity | `approvals[i].issuingAuthorityCapacity` | "المدير العام" or "المدير المالي" |

**Step states**:

| State | Badge | Description (Arabic) |
|---|---|---|
| Completed (approved) | ✓ أُعجب | Green check |
| Completed (rejected) | ✗ مرفوض | Red X |
| Pending | ○ في الانتظار | Gray circle |
| Not reached | ○ لم يصل | Gray outline |

#### Linked Order Section (after approval)

**Visible when**: `status = Approved` and order exists.

| Field | Source | Format |
|---|---|---|
| `paymentOrderNumber` | order | PO-{D6} |
| `amountGross` | order | decimal(23,2) + currency |
| `status` | order | color-coded badge |

#### Lifecycle Action Buttons

| Button | Label (Arabic) | Condition | Permission | Dialog |
|---|---|---|---|---|
| Submit | إرسال | `status = Draft` | `DisbursementRequests.Submit` | Confirmation dialog |
| Approve (step 1/2) | موافقة | `status = PendingApproval` + user not yet signed + qualified role | `DisbursementRequests.Approve` | Approve dialog (see below) |
| Reject | رفض | `status = PendingApproval` + user not yet signed + qualified role | `DisbursementRequests.Reject` | Reject dialog (see below) |
| Cancel | إلغاء | `status = Draft` or `status = PendingApproval` or (`status = Approved` + order not paid) | `DisbursementRequests.Cancel` | Cancel dialog (reason required) |

#### Approve Dialog

**Purpose**: Allow a qualified reviewer to approve the request with an amount and issuing authority.

**Fields**:

| Field | Label (Arabic) | Source | Required | Validation |
|---|---|---|---|---|
| `approvedAmount` | المبلغ المعتمد | user input | ✓ | > 0, ≤ requestedAmount |
| `issuingAuthorityName` | اسم جهة الأمر | user input | ✓ | nonblank |
| `issuingAuthorityCapacity` | صلاحية جهة الأمر | dropdown | ✓ | "المدير العام" or "المدير المالي" |

**Validation**:

| Rule | Message (Arabic) |
|---|---|
| `approvedAmount > 0` | "المبلغ المعتمد يجب أن يكون أكبر من صفر" |
| `approvedAmount ≤ requestedAmount` | "المبلغ المعتمد لا يمكن أن يتجاوز المبلغ المطلوب" |
| Same amount as step 1 (step 2 only) | "يجب أن يتطابق المبلغ المعتمد مع الخطوة الأولى" |
| Different user (step 2) | "يجب أن يكون المُعتمد مختلفاً عن الخطوة الأولى" |

**Behavior**:
- Step 1: Records approval in ApprovalHistory, request stays PendingApproval.
- Step 2: Records approval, generates linked order atomically, request becomes Approved.

#### Reject Dialog

**Purpose**: Allow a reviewer to reject the request with a reason.

**Fields**:

| Field | Label (Arabic) | Required | Validation |
|---|---|---|---|
| `reason` | سبب الرفض | ✓ | nonblank |

**Validation**: "سبب الرفض مطلوب" if blank.

#### Cancel Dialog

**Purpose**: Allow a user to cancel the request.

**Fields**:

| Field | Label (Arabic) | Required | Validation |
|---|---|---|---|
| `reason` | سبب الإلغاء | ✓ | nonblank |

**Behavior**: For Approved requests with an unpaid order, the order is released.

#### Loading / Empty / Error

- **Loading**: skeleton header + skeleton Stepper (2 steps).
- **Empty**: "الطلب غير موجود" with back button.
- **Error**: server message verbatim (CC-2) with retry.

---

## PAY-01: Payment Orders

> Two creation paths: (1) order-first (direct creation by accountant), (2) request-first (auto-generated from approved PAY-02 request). Both share the same lifecycle.

### PAY-01-LIST: Payment Orders List Page

**Route**: `/payments/payment-orders`

**Purpose**: Show all payment orders with status, amount, and budget check status. Allow filtering by status, budget check status, and period.

**Authorized users**: Users with `PaymentOrders.View` permission.

**Data source**: `GET /api/Payments/PaymentOrders` (query parameters for filters).

#### Fields

| Field | Column header (Arabic) | Source | Editable | Visible | Format |
|---|---|---|---|---|---|
| `paymentOrderNumber` | رقم أمر الصرف | server | No | Always | PO-{D6} |
| `beneficiaryName` | المستفيد | server | No | Always | text |
| `paymentOrderDate` | التاريخ | server | No | Always | date |
| `amountGross` | المبلغ الإجمالي | server | No | Always | decimal(23,2) + currency |
| `netAmount` | الصافي | server (Totals) | No | Always | decimal(23,2) + currency |
| `status` | الحالة | server | No | Always | color-coded badge |
| `budgetCheckStatus` | فحص الموازنة | server | No | Always | color-coded badge |

#### Status Badges (CC-2)

| Status | Arabic label | Color |
|---|---|---|
| Draft | مسودة | gray |
| Submitted | مُرسل | blue |
| Approved | معتمد | green |
| SentToTreasury | أُرسل للخزينة | purple |
| Paid | مدفوع | green (filled) |
| Cancelled | ملغي | gray |
| Rejected | مرفوض | red |
| Voided | مُلغى | red (outline) |

#### Budget Check Badges

| Status | Arabic label | Color |
|---|---|---|
| Pending | قيد الفحص | gray |
| Passed | ناجح | green |
| Failed | فاشل | red |
| Overridden | متجاوز | orange |

#### Filters

| Filter | Field | Type | Options |
|---|---|---|---|
| الحالة | `status` | multi-select | All + 8 status options |
| فحص الموازنة | `budgetCheckStatus` | multi-select | All + 4 check options |
| الفترة | `period` | date range | start/end date picker |

#### Actions

| Button | Label (Arabic) | Condition | Permission |
|---|---|---|---|
| New order (order-first) | أمر صرف جديد | Always visible | `PaymentOrders.Create` |

#### Loading / Empty / Error

- **Loading**: skeleton rows (8 rows × 7 columns).
- **Empty**: "لا توجد أوامر صرف" with icon.
- **Error**: server message rendered verbatim (CC-2) with retry button.

---

### PAY-01-CREATE: Create Payment Order Page (Order-First Path)

**Route**: `/payments/payment-orders/create`

**Purpose**: Allow an accountant to create a payment order directly (order-first path). The order starts as Draft with a server-issued PO-{D6} number.

**Authorized users**: Users with `PaymentOrders.Create` permission.

**Data source**: `POST /api/Payments/PaymentOrders` with body.

#### Header Fields

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `fundId` | صندوق الموازنة | dropdown (funds) | Yes | ✓ | active fund |
| `fiscalYearId` | السنة المالية | dropdown (fiscal years) | Yes | ✓ | active year |
| `appropriationId` | الصرفية | dropdown (appropriations) | Yes | ✓ | active appropriation under fund |
| `currencyId` | العملة | dropdown (024 currencies) | Yes | ✓ | active currency |
| `exchangeRate` | سعر الصرف | user input | Yes | conditional | required when currency ≠ base |
| `dueDate` | تاريخ الاستحقاق | date picker | Yes | No | future date |
| `paymentOrderType` | نوع أمر الصرف | dropdown | Yes | ✓ | server values |
| `accountId` | الحساب العام | dropdown (GL accounts) | Yes | No (Draft) / ✓ (Submit) | GL account |
| `bankAccountId` | الحساب البنكي | dropdown (PAY-04 active) | Yes | No | active bank account |

#### Deductions Section

**Purpose**: Allow adding up to 7 deduction types with amount/percent, mandatory/tax flags.

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `deductionType` | نوع الاستقطاع | dropdown (7 types) | Yes | ✓ | enum |
| `description` | الوصف | user input | Yes | No | text |
| `accountId` | الحساب | dropdown (GL accounts) | Yes | ✓ | GL account |
| `amount` | المبلغ | user input | Yes | ✓ | decimal(23,2), > 0 |
| `deductionPercent` | النسبة | user input | Yes | No | decimal |
| `isMandatory` | إلزامي | checkbox | Yes | — | boolean |
| `isTaxDeduction` | ضريبة | checkbox | Yes | — | boolean |
| `taxAuthority` | جهة الضرائب | dropdown (if isTaxDuction) | conditional | conditional | active TaxAuthority |
| `referenceNumber` | رقم المرجع | user input | Yes | No | text |

**Deduction rules**:
- `isMandatory = true`: deduction cannot be deleted.
- `isTaxDeduction = true`: `taxAuthorityId` required.
- Total deductions ≤ amountGross (server-validated at submit).

#### Beneficiary Section

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `beneficiaryName` | اسم المستفيد | user input | Yes | ✓ | nonblank |
| `beneficiaryIban` | الآيبان | user input | Yes | No | text |
| `beneficiaryAccountNumber` | رقم الحساب | user input | Yes | No | text |
| `beneficiaryBankName` | اسم البنك | user input | Yes | No | text |

#### Totals Preview (server-issued, CC-1)

| Value | Source | Format |
|---|---|---|
| amountGross | header | decimal(23,2) + currency |
| totalDeductions | computed (sum of deductions) | decimal(23,2) + currency |
| netAmount | server: gross − deductions | decimal(23,2) + currency |

#### Actions

| Button | Label (Arabic) | Action |
|---|---|---|
| Save draft | حفظ مسودة | `POST /api/Payments/PaymentOrders` → redirect to detail |
| Submit | إرسال | (not available on create — must save first) |
| Cancel | إلغاء | return to list |

#### Validation

| Rule | Trigger | Message (Arabic) |
|---|---|---|
| Required fields | submit | "هذا الحقل مطلوب" per field |
| Deduction total ≤ gross | submit | "إجمالي الاستقطاعات لا يمكن أن يتجاوز المبلغ الإجمالي" |
| At least one deduction | submit | "يجب إضافة استقطاع واحد على الأقل" |

#### Loading / Empty / Error

- **Loading**: form skeleton (header + deductions grid skeleton).
- **Empty**: N/A (always has form fields).
- **Error**: inline field errors from server (4xx) + toast for 5xx (CC-2).

---

### PAY-01-DETAIL: Payment Order Detail Page

**Route**: `/payments/payment-orders/:id`

**Purpose**: Show complete order information, deductions, totals, budget check status, approval history, treasury trace, journal link, and lifecycle action buttons. For request-first orders, also shows issuing authority and source request.

**Authorized users**: Users with `PaymentOrders.View` permission.

**Data source**: `GET /api/Payments/PaymentOrders/{id}` + `GET /api/Payments/PaymentOrders/{id}/totals`.

#### Header Section

| Field | Label (Arabic) | Source | Editable | Format |
|---|---|---|---|---|
| `paymentOrderNumber` | رقم أمر الصرف | server | No | PO-{D6} |
| `paymentOrderDate` | التاريخ | server | No | date |
| `dueDate` | تاريخ الاستحقاق | server | No | date (if present) |
| `beneficiaryName` | المستفيد | server | No | text |
| `fund` | صندوق الموازنة | server | No | text |
| `appropriation` | الصرفية | server | No | text |
| `currency` | العملة | server | No | text + exchange rate |
| `accountId` | الحساب العام | server | No | text (if present) |
| `status` | الحالة | server | No | color-coded badge |
| `notes` | ملاحظات | server | No | text (if present) |

#### Issuing Authority Section (request-first orders only)

**Visible when**: order has `issuingAuthorityName` (request-first path).

| Field | Label (Arabic) | Source | Format |
|---|---|---|---|
| `issuingAuthorityName` | جهة الأمر | ApprovalHistory | text |
| `issuingAuthorityCapacity` | صلاحية جهة الأمر | ApprovalHistory | "المدير العام" or "المدير المالي" |
| `sourceRequestNumber` | رقم الطلب الأصلي | order.disbursementRequestId | DSB-{D6} (linked) |

#### Budget Check Section

| Field | Label (Arabic) | Source | Format |
|---|---|---|---|
| `budgetCheckStatus` | حالة فحص الموازنة | server | color-coded badge |

#### Deductions Section

| Field | Label (Arabic) | Source | Format |
|---|---|---|---|
| deductionType | نوع الاستقطاع | server | enum text |
| description | الوصف | server | text |
| amount | المبلغ | server | decimal(23,2) |
| isMandatory | إلزامي | server | badge |
| isTaxDeduction | ضريبة | server | badge |

#### Totals Card (server-issued, CC-1)

| Value | Label (Arabic) | Source | Format |
|---|---|---|---|
| amountGross | المبلغ الإجمالي | /{id}/totals | decimal(23,2) + currency |
| totalDeductions | إجمالي الاستقطاعات | /{id}/totals | decimal(23,2) + currency |
| netAmount | الصافي | /{id}/totals | decimal(23,2) + currency |
| isFullyPaid | مدفوع بالكامل | /{id}/totals | boolean badge |

#### Treasury Trace Section

**Visible when**: `status = SentToTreasury` or later.

| Field | Label (Arabic) | Source | Format |
|---|---|---|---|
| `treasuryStatus` | حالة الخزينة | server | text |
| `treasuryReference` | مرجع الخزينة | server | text |
| `treasurySentAt` | تاريخ الإرسال | server | datetime |
| `journalEntryId` | قيد اليومية | server | link to journal entry |

#### Approval History (022 Panels, CC-3)

**Purpose**: Show all approval decisions recorded in ApprovalHistory.

| Field | Source | Format |
|---|---|---|
| actor | ApprovalHistory.approverName | text |
| decision | ApprovalHistory.decision | badge (approve/reject) |
| time | ApprovalHistory.decisionAt | datetime |
| reason | ApprovalHistory.reason | text (if present) |

#### Lifecycle Action Buttons

| Button | Label (Arabic) | Condition | Permission | Dialog |
|---|---|---|---|---|
| Edit | تعديل | `status = Draft` + order-first path | `PaymentOrders.Update` | inline form |
| Submit | إرسال | `status = Draft` + fundId + appropriationId + accountId present | `PaymentOrders.Submit` | Confirmation dialog |
| Approve | موافقة | `status = Submitted` + budgetCheck ≠ Failed | `PaymentOrders.Approve` | Approve dialog |
| Approve (override) | موافقة (تجاوز) | `status = Submitted` + budgetCheck = Failed | `PaymentOrders.Approve` + `PaymentOrders.OverrideBudgetCheck` | Approve dialog with override flag |
| Reject | رفض | `status = Submitted` | `PaymentOrders.Reject` | Reject dialog (reason required) |
| Cancel | إلغاء | `status = Draft` or `status = Submitted` | `PaymentOrders.Cancel` | Cancel dialog (reason required) |
| Send to treasury | إرسال للخزينة | `status = Approved` | `PaymentOrders.SendToTreasury` | Send dialog (treasuryReference required) |
| Void | إلغاء أمر | `status = Approved` or `status = SentToTreasury` + unpaid | `PaymentOrders.Void` | Void dialog (confirmation) |

#### Approve Dialog

**Fields**:

| Field | Label (Arabic) | Required | Condition |
|---|---|---|---|
| `reason` | ملاحظات | No | optional |
| `overrideFailedBudgetCheck` | تجاوز فشل فحص الموازنة | — | visible only when budgetCheck = Failed |

**Behavior**:
- budgetCheck = Failed + no override → 400 verbatim message.
- budgetCheck = Failed + override + no permission → 403.
- budgetCheck = Failed + override + permission → Approved, BudgetCheckStatus = Overridden.

#### Loading / Empty / Error

- **Loading**: skeleton header + skeleton deductions + skeleton totals.
- **Empty**: "أمر الصرف غير موجود" with back button.
- **Error**: server message verbatim (CC-2) with retry.

---

## PAY-03: Payment Execution

### PAY-03-LIST: Payments List Page

**Route**: `/payments/payments`

**Purpose**: Show all completed payments with method, amount, and linked order. Allow filtering by period, method, and status.

**Authorized users**: Users with `Payments.View` permission.

**Data source**: `GET /api/Payments/Payments` (query parameters for filters).

#### Fields

| Field | Column header (Arabic) | Source | Editable | Visible | Format |
|---|---|---|---|---|---|
| `paymentNumber` | رقم الدفع | server | No | Always | PAY-{D6} |
| `disbursementRequestNumber` | طلب الصرف | server (derived) | No | Always | DSB-{D6} |
| `paymentOrderNumber` | أمر الصرف | server | No | Always | PO-{D6} |
| `paymentMethod` | طريقة الدفع | server | No | Always | "نقدي" or "شيك" |
| `amount` | المبلغ | server | No | Always | decimal(23,2) + currency |
| `status` | الحالة | server | No | Always | color-coded badge |
| `paidByName` | الدافع | server | No | Always | text |
| `paidAt` | التاريخ | server | No | Always | datetime |

#### Status Badges

| Status | Arabic label | Color |
|---|---|---|
| Completed | مكتمل | green |
| Failed | فاشل | red |

#### Filters

| Filter | Field | Type | Options |
|---|---|---|---|
| الفترة | `period` | date range | start/end date picker |
| طريقة الدفع | `paymentMethod` | select | All / نقدي / شيك |
| الحالة | `status` | select | All / مكتمل / فاشل |

#### Loading / Empty / Error

- **Loading**: skeleton rows (8 rows × 8 columns).
- **Empty**: "لا توجد مدفوعات" with icon.
- **Error**: server message rendered verbatim (CC-2) with retry button.

---

### PAY-03-RECORD: Record Payment Dialog

**Purpose**: Allow a cashier to record a single payment on an Approved order. This closes the request-order-payment triad. Accessed from PAY-02 detail page (after approval) or PAY-01 detail page (after approval + treasury).

**Authorized users**: Users with `Payments.Create` permission.

**Data source**: `POST /api/Payments/Payments` with body: `{paymentOrderId!, paymentMethod!, referenceNumber?, notes?}`.

**Precondition**: Order must be in `Approved` or `SentToTreasury` status. Amount is server-computed (readonly snapshot of order net amount).

#### Fields

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `paymentOrderId` | أمر الصرف | context (order ID) | No | ✓ | — |
| `paymentOrderNumber` | رقم أمر الصرف | order | No | ✓ | PO-{D6} (readonly) |
| `amount` | المبلغ | server snapshot (order netAmount) | No | ✓ | readonly, decimal(23,2) + currency |
| `paymentMethod` | طريقة الدفع | dropdown | Yes | ✓ | Cash or Check only |
| `referenceNumber` | رقم المرجع | user input | Yes | No | text (optional, code-verified) |
| `notes` | ملاحظات | user input | Yes | No | free text |

#### Validation

| Rule | Message (Arabic) |
|---|---|
| `paymentMethod` required | "طريقة الدفع مطلوبة" |
| Order not Approved | "لا يمكن التسجيل — أمر الصرف غير معتمد" (verbatim server message) |
| Double payment | "تم تسجيل الدفع مسبقاً لهذا الأمر" (verbatim server message) |

#### Actions

| Button | Label (Arabic) | Action |
|---|---|---|
| Record payment | تسجيل الدفع | `POST /api/Payments/Payments` |
| Cancel | إلغاء | close dialog |

#### Success State

**Purpose**: Show the closed triad (request → order → payment) with all numbers.

| Element | Label (Arabic) | Format |
|---|---|---|
| requestNumber | طلب الصرف | DSB-{D6} |
| paymentOrderNumber | أمر الصرف | PO-{D6} |
| paymentNumber | رقم الدفع | PAY-{D6} |
| amount | المبلغ المدفوع | decimal(23,2) + currency |
| message | "تم تسجيل الدفع بنجاح" | green success banner |

#### Failure State

**Purpose**: Show the server error message with retry option.

| Element | Format |
|---|---|
| error message | verbatim server message (CC-2) |
| retry button | "إعادة المحاولة" |

#### Loading / Empty / Error

- **Loading**: dialog skeleton (6 field skeletons).
- **Empty**: N/A (dialog always has fields).
- **Error**: inline from server (4xx) + toast for 5xx (CC-2).

---

## PAY-04: Bank Accounts

### PAY-04-LIST: Bank Accounts List Page

**Route**: `/payments/bank-accounts`

**Purpose**: Show all bank accounts with balance, default status, and active status. Allow filtering by active/currency/fund.

**Authorized users**: Users with `BankAccounts.View` permission.

**Data source**: `GET /api/Payments/BankAccounts` (query parameters for filters).

#### Fields

| Field | Column header (Arabic) | Source | Editable | Visible | Format |
|---|---|---|---|---|---|
| `name` | اسم الحساب | server | No | Always | text |
| `bankName` | البنك | server | No | Always | text |
| `accountNumber` | رقم الحساب | server | No | Always | text |
| `currency` | العملة | server | No | Always | text |
| `isDefault` | افتراضي | server | No | Always | badge (✓/—) |
| `maxDailyLimit` | الحد اليومي | server | No | Always | decimal(23,2) |
| `isActive` | نشط | server | No | Always | badge (✓/—) |
| `lastReconciliationDate` | آخر تسوية | server | No | Always | date (if present) |

#### Filters

| Filter | Field | Type | Options |
|---|---|---|---|
| الحالة | `isActive` | select | All / نشط / غير نشط |
| العملة | `currencyId` | select | All + active currencies |
| الصندوق | `fundId` | select | All + active funds |

#### Actions

| Button | Label (Arabic) | Condition | Permission |
|---|---|---|---|
| New account | حساب بنكي جديد | Always visible | `BankAccounts.Create` |

#### Loading / Empty / Error

- **Loading**: skeleton rows (8 rows × 8 columns).
- **Empty**: "لا توجد حسابات بنكية" with icon.
- **Error**: server message rendered verbatim (CC-2) with retry button.

---

### PAY-04-CREATE: Create Bank Account Page

**Route**: `/payments/bank-accounts/create`

**Purpose**: Allow a user to create a bank account with full details (17 fields). AccountNumber must be unique. IsDefault=true auto-demotes previous default.

**Authorized users**: Users with `BankAccounts.Create` permission.

**Data source**: `POST /api/Payments/BankAccounts` with body.

#### Section: Identity

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `name` | اسم الحساب | user input | Yes | ✓ | nonblank |
| `bankName` | اسم البنك | user input | Yes | ✓ | nonblank |
| `accountNumber` | رقم الحساب | user input | Yes | ✓ | nonblank, unique (server) |

#### Section: Branch

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `iban` | الآيبان | user input | Yes | No | text |
| `swiftCode` | سويفت كود | user input | Yes | No | text |
| `branchName` | اسم الفرع | user input | Yes | No | text |
| `branchCode` | كود الفرع | user input | Yes | No | text |

#### Section: Financial

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `currencyId` | العملة | dropdown (024 currencies) | Yes | ✓ | active currency |
| `fundId` | الصندوق | dropdown (funds) | Yes | No | active fund |
| `glAccountId` | الحساب العام | dropdown (GL accounts) | Yes | No | GL account |
| `openingBalance` | الرصيد الافتتاحي | user input | Yes | No | decimal(23,2) |

#### Section: Controls

| Field | Label (Arabic) | Source | Editable | Required | Validation |
|---|---|---|---|---|---|
| `maxDailyLimit` | الحد اليومي الأقصى | user input | Yes | No | decimal(23,2), > 0 |
| `maxTransactionLimit` | الحد الأقصى للمعاملة | user input | Yes | No | decimal(23,2), > 0 |
| `requiresDualApproval` | يتطلب موافقة مزدوجة | checkbox | Yes | — | boolean |
| `isDefault` | افتراضي | checkbox | Yes | — | boolean — auto-demotes previous |

#### Validation

| Rule | Message (Arabic) |
|---|---|
| Required fields | "هذا الحقل مطلوب" per field |
| `accountNumber` unique | "رقم الحساب مسجل مسبقاً" |
| `isDefault` auto-switch | — (server handles in same transaction) |

#### Actions

| Button | Label (Arabic) | Action |
|---|---|---|
| Save | حفظ | `POST /api/Payments/BankAccounts` → redirect to detail |
| Cancel | إلغاء | return to list |

#### Loading / Empty / Error

- **Loading**: form skeleton (4 sections × field skeletons).
- **Empty**: N/A (always has form fields).
- **Error**: inline field errors from server (4xx) + toast for 5xx (CC-2).

---

### PAY-04-DETAIL: Bank Account Detail Page

**Route**: `/payments/bank-accounts/:id`

**Purpose**: Show all account details, balance, reconciliation date, and activate/deactivate buttons.

**Authorized users**: Users with `BankAccounts.View` permission.

**Data source**: `GET /api/Payments/BankAccounts/{id}`.

#### All Fields

| Field | Label (Arabic) | Source | Editable | Format |
|---|---|---|---|---|
| `name` | اسم الحساب | server | No | text |
| `bankName` | البنك | server | No | text |
| `accountNumber` | رقم الحساب | server | No | text |
| `iban` | الآيبان | server | No | text (if present) |
| `swiftCode` | سويفت كود | server | No | text (if present) |
| `branchName` | اسم الفرع | server | No | text (if present) |
| `branchCode` | كود الفرع | server | No | text (if present) |
| `currency` | العملة | server | No | text |
| `fund` | الصندوق | server | No | text (if present) |
| `glAccount` | الحساب العام | server | No | text (if present) |
| `isDefault` | افتراضي | server | No | badge |
| `maxDailyLimit` | الحد اليومي | server | No | decimal(23,2) (if present) |
| `maxTransactionLimit` | الحد الأقصى للمعاملة | server | No | decimal(23,2) (if present) |
| `requiresDualApproval` | موافقة مزدوجة | server | No | badge |
| `lastReconciliationDate` | آخر تسوية | server (BANK-01) | No | date (if present) |
| `openingBalance` | الرصيد الافتتاحي | server | No | decimal(23,2) |
| `currentBalance` | الرصيد الحالي | server (BANK-01) | No | decimal(23,2) |
| `isActive` | نشط | server | No | badge |

#### Actions

| Button | Label (Arabic) | Condition | Permission | Dialog |
|---|---|---|---|---|
| Edit | تعديل | `isActive = true` | `BankAccounts.Update` | redirect to edit page |
| Activate | تنشيط | `isActive = false` | `BankAccounts.Activate` | Confirmation dialog |
| Deactivate | إلغاء التنشيط | `isActive = true` | `BankAccounts.Deactivate` | Confirmation dialog |

#### Confirmation Dialogs

**Activate**: "هل أنت متأكد من تنشيط هذا الحساب البنكي؟"
**Deactivate**: "هل أنت متأكد من إلغاء تنشيط هذا الحساب البنكي؟ سيتم استبعاده من جميع القوائم."

#### Loading / Empty / Error

- **Loading**: skeleton detail (all field skeletons).
- **Empty**: "الحساب البنكي غير موجود" with back button.
- **Error**: server message verbatim (CC-2) with retry.

---

## Cross-cutting UI Rules

### Shared Rules (all screens, ✓ confirmed)

1. **CC-1 (Server numbers)**: Every financial amount, status, and computed total displayed or exported comes from the server; the frontend never recomputes financial values.
2. **CC-2 (State UI)**: Every status renders as a color-coded badge; every blocking server refusal renders its server message verbatim plus actionable detail — never silently swallowed.
3. **CC-3 (Approvals via 022 panels)**: Every approval/reject decision is recorded through the 022 ApprovalHistory panels — no inline approval columns.
4. **CC-4 (Concurrency)**: All mutating endpoints accept and honor `rowVersion`; conflicts surface as a server message.
5. **CC-5 (Lifecycle actions are conditional)**: Action buttons appear only in the states their lifecycle table lists; server re-validates independently.

### RTL Rules (all screens, ✓ confirmed)

- Logical CSS properties only: `ms-/me-`, `ps-/pe-`, `start/end`. Physical properties (`margin-left`, `padding-right`) prohibited.
- `<html dir="rtl" lang="ar">` fixed at boot.
- All strings hardcoded in Arabic. No `t()` calls, no locale files.

### Loading States (all screens, ✓ confirmed)

Every query has `isPending` skeleton + `isError` retry UI. No spinners-as-default.

### Error Handling (all screens, ✓ confirmed)

- 4xx → inline field error (via Zod refine).
- 5xx → toast.
- Server business-rule refusals → verbatim message in the UI (CC-2).
- Never swallow errors.

### Status Badge Colors (all screens, ✓ confirmed)

| Category | Colors |
|---|---|
| Terminal (success) | green (filled) |
| Terminal (failure) | red |
| Active/in-progress | blue |
| Warning/override | orange |
| Inactive/draft | gray |

---

## Confirmed vs Design Proposals Summary

### Confirmed by Stakeholder ✓

- Independent request creation without existing payment order (FR-001)
- Dual-signature approval with same amount, distinct qualified users (FR-005)
- Issuing authority = officeholder name + capacity (General Manager or Finance Director) (FR-003)
- Approved amount ≤ requested amount; exceeding prohibited (FR-011)
- Atomic order generation on final approval (FR-006)
- Single payment per order; no partial payment (FR-008)
- Void/cancel propagates invalidation to requests (FR-016)
- Budget check at order submit only; no check at request submit (FR-012)
- BeneficiaryName string only; no Party linkage in v1 (ADR-001 D-1)
- Approval data in ApprovalHistory only; no duplicate columns (ADR-001 D-5)
- PaymentOrderId UNIQUE on Payment; one payment per order (ADR-001 D-6)
- PaymentOrderLine removed; header-only model with AccountId (ADR-001 D-4)

### Design Proposals [DESIGN]

- Stepper component with 2 steps for dual-signature visualization (FR-013)
- Specific badge colors for each status (design system choice)
- Filter UI patterns (multi-select vs. dropdown)
- Form section layout and grouping (4 sections for bank accounts)
- Success/failure screen designs
- Confirmation dialog text and patterns
