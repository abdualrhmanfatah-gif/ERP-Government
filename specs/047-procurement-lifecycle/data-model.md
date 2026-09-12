# Data Model: Procurement Lifecycle Rebuild

**Branch**: `047-procurement-lifecycle` | **Date**: 2026-09-10

## Entity Relationship Overview

```
PurchaseRequest 1──* PurchaseRequestDetail
       │
       │ 1
RequestForQuotation *──1 PurchaseRequest (required FK)
       │
       │ 1
  RFQSupplier *──1 RequestForQuotation
       │
       │ 1
    Quotation *──1 RequestForQuotation
    Quotation *──1 RFQSupplier
       │
       │ 1
QuotationDetail *──1 Quotation
QuotationDetail *──1 PurchaseRequestDetail
       │
       │ 0..1
PurchaseOrder *──0..1 PurchaseRequest
PurchaseOrder *──0..1 Quotation
       │
       │ 1
PurchaseOrderDetail *──1 PurchaseOrder
PurchaseOrderDetail *──1 PurchaseRequestDetail
PurchaseOrderDetail *──0..1 QuotationDetail
       │
       │ 1
GoodsReceiptNote *──1 PurchaseOrder
       │
       │ 1
GoodsReceiptNoteDetail *──1 GoodsReceiptNote
GoodsReceiptNoteDetail *──1 PurchaseOrderDetail
       │
       │ 1
SupplierInvoice *──1 PurchaseOrder
       │
       │ 1
SupplierInvoiceDetail *──1 SupplierInvoice
SupplierInvoiceDetail *──1 PurchaseOrderDetail
SupplierInvoiceDetail *──0..1 GoodsReceiptNoteDetail
       │
       │ 1
PaymentOrder *──0..1 PurchaseOrder
PaymentOrder *──0..1 Encumbrance

Encumbrance *──1 PurchaseOrder (via PurchaseOrderId)
EncumbranceLine *──1 Encumbrance
EncumbranceLine *──1 BudgetItem

CommitteeAssignment *──0..1 RequestForQuotation (via RfqId)
```

## Entities

### PurchaseRequest (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, Identity | Inherited from BaseAuditableEntity |
| RequestNumber | string(20) | Required, Unique | PRQ-{D6}, generated at creation |
| RequestDate | DateTime | Required | Defaults to UTC now |
| RequiredDate | DateOnly? | Nullable | When goods are needed |
| DepartmentId | int? | FK → OrganizationalUnits | Requesting department |
| CostCenterId | int? | FK → CostCenters | Cost center |
| RequesterId | int? | FK → Users | Who created the request |
| Priority | enum | Required | Low=0, Normal=1, High=2, Urgent=3 |
| Status | enum | Required | Draft=0, Submitted=1, Approved=2, UnderProcurement=3, Rejected=4, Cancelled=5, Expired=6 |
| TotalEstimatedCost | decimal(23,2) | Computed | SUM(Detail.TotalCostEstimate) |
| Notes | string(2000)? | Nullable | Free text |
| Created | DateTimeOffset | Audit | |
| CreatedBy | string? | Audit | |
| LastModified | DateTimeOffset | Audit | |
| LastModifiedBy | string? | Audit | |
| RowVersion | byte[] | Concurrency token | |

**Removed fields**: RequestType (string), TotalItems, TotalQuantity, CurrencyCode, RejectionReason, CancelledById/At, ApprovedById/At, FinalApprovedById/At — all replaced by ApprovalHistory/DocumentStatusLog.

### PurchaseRequestDetail (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| PurchaseRequestId | int | FK → PurchaseRequest, Required, Restrict | |
| ItemId | int | FK → Items, Required, Restrict | |
| UnitId | int | FK → Units, Required, Restrict | |
| RequestedQuantity | decimal(18,4) | Required, >0 | |
| ApprovedQuantity | decimal(18,4)? | Nullable | Set at approval, defaults to RequestedQuantity |
| UnitCostEstimate | decimal(23,2)? | Nullable | Estimated unit price |
| TotalCostEstimate | decimal(23,2)? | Nullable | Computed: RequestedQuantity × UnitCostEstimate |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

### RequestForQuotation (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| RFQNumber | string(20) | Required, Unique | RFQ-{D6} |
| RFQDate | DateTime | Required | |
| PurchaseRequestId | int | FK → PurchaseRequest, Required, Restrict | **Changed from nullable to required** |
| DeadlineDate | DateTime? | Nullable | Submission deadline |
| CurrencyCode | string(3)? | Nullable | ISO 4217 |
| TermsAndConditions | string(4000)? | Nullable | |
| Status | enum | Required | Draft=0, Published=1, CollectingResponses=2, Closed=3, Cancelled=4, NoResponse=5 |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

### RFQSupplier (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| RFQId | int | FK → RequestForQuotation, Required, Restrict | |
| SupplierPartyId | int | FK → Parties (PartyType=Supplier), Required, Restrict | **Replaces SupplierId + PartyId** |
| InvitationDate | DateTime? | Nullable | When invited |
| ResponseDate | DateTime? | Nullable | When responded |
| Status | enum | Required | Invited=0, Responded=1, Expired=2, Declined=3 |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**Removed fields**: SupplierId, PartyId — replaced by SupplierPartyId.

### Quotation (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| QuotationNumber | string(20) | Required, Unique | QT-{D6} |
| RFQId | int | FK → RequestForQuotation, Required, Restrict | |
| RFQSupplierId | int | FK → RFQSupplier, Required, Restrict | |
| SupplierPartyId | int | FK → Parties, Required, Restrict | **Replaces SupplierId + PartyId** |
| QuotationDate | DateTime | Required | |
| ValidUntil | DateTime? | Nullable | Quotation expiry |
| CurrencyCode | string(3)? | Nullable | |
| ExchangeRate | decimal(18,6)? | Nullable | |
| SubTotal | decimal(23,2)? | Nullable | Sum of line totals before discount/tax |
| DiscountAmount | decimal(23,2)? | Nullable | |
| TaxAmount | decimal(23,2)? | Nullable | |
| ShippingCost | decimal(23,2)? | Nullable | |
| OtherCharges | decimal(23,2)? | Nullable | |
| GrandTotal | decimal(23,2)? | Nullable | Computed |
| PaymentTerms | string(2000)? | Nullable | |
| DeliveryTerms | string(2000)? | Nullable | |
| LeadTimeDays | int? | Nullable | |
| WarrantyPeriodMonths | int? | Nullable | |
| Status | enum | Required | Draft=0, Submitted=1, UnderEvaluation=2, Evaluated=3, Selected=4, Awarded=5, Rejected=6, Expired=7 |
| TechnicalScore | decimal(5,2)? | Nullable | Committee evaluation result |
| FinancialScore | decimal(5,2)? | Nullable | Committee evaluation result |
| SelectionReason | string(2000)? | Nullable | Why this offer was selected |
| RejectionReason | string(2000)? | Nullable | Why this offer was rejected/excluded |
| Created/Modified/RowVersion | | Audit | |

**Removed fields**: SupplierId, PartyId — replaced by SupplierPartyId.
**Added fields**: TechnicalScore, FinancialScore (from evaluation committee).

### QuotationDetail (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| QuotationId | int | FK → Quotation, Required, Restrict | |
| PurchaseRequestDetailId | int | FK → PurchaseRequestDetail, Required, Restrict | **Links to original PR line** |
| ItemId | int | FK → Items, Required, Restrict | |
| UnitId | int | FK → Units, Required, Restrict | |
| Quantity | decimal(18,4) | Required, >0 | Supplier's offered quantity |
| UnitPrice | decimal(23,2) | Required, ≥0 | |
| DiscountPercent | decimal(5,2)? | Nullable | |
| DiscountAmount | decimal(23,2)? | Nullable | Computed: Quantity × UnitPrice × DiscountPercent / 100 |
| NetUnitPrice | decimal(23,2)? | Nullable | Computed: UnitPrice - DiscountAmount/Quantity |
| TaxPercent | decimal(5,2)? | Nullable | |
| TaxAmount | decimal(23,2)? | Nullable | Computed |
| LineTotal | decimal(23,2)? | Nullable | Computed: Quantity × NetUnitPrice |
| LineTotalWithTax | decimal(23,2)? | Nullable | Computed: LineTotal + TaxAmount |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

### PurchaseOrder (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| PONumber | string(20) | Required, Unique | PO-{D6} |
| PODate | DateTime | Required | |
| PurchaseRequestId | int? | FK → PurchaseRequest, Nullable, Restrict | Null for direct purchase |
| QuotationId | int? | FK → Quotation, Nullable, Restrict | Null for direct purchase |
| SupplierPartyId | int | FK → Parties, Required, Restrict | **Replaces SupplierId** |
| WarehouseId | int? | FK → Warehouses, Nullable, Restrict | Delivery warehouse |
| DeliveryLocationId | int? | FK → Locations, Nullable, Restrict | |
| CurrencyCode | string(3)? | Nullable | |
| ExchangeRate | decimal(18,6)? | Nullable | |
| SubTotal | decimal(23,2)? | Nullable | |
| DiscountAmount | decimal(23,2)? | Nullable | |
| TaxAmount | decimal(23,2)? | Nullable | |
| ShippingCost | decimal(23,2)? | Nullable | |
| OtherCharges | decimal(23,2)? | Nullable | |
| GrandTotal | decimal(23,2)? | Nullable | |
| PaymentTerms | string(2000)? | Nullable | |
| DeliveryTerms | string(2000)? | Nullable | |
| ExpectedDeliveryDate | DateTime? | Nullable | |
| Status | enum | Required | Draft=0, Submitted=1, Approved=2, Issued=3, PartiallyReceived=4, Received=5, Closed=6, Cancelled=7 |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**Removed fields**: SupplierId, ApprovedById/At, RejectionReason, CancelledById/At — replaced by SupplierPartyId and ApprovalHistory.

### PurchaseOrderDetail (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| PurchaseOrderId | int | FK → PurchaseOrder, Required, Restrict | |
| PurchaseRequestDetailId | int | FK → PurchaseRequestDetail, Required, Restrict | Originating PR line |
| QuotationDetailId | int? | FK → QuotationDetail, Nullable, Restrict | Null for direct purchase |
| ItemId | int | FK → Items, Required, Restrict | |
| UnitId | int | FK → Units, Required, Restrict | |
| OrderedQuantity | decimal(18,4) | Required, >0 | |
| ReceivedQuantity | decimal(18,4) | Default 0 | Updated atomically on each receipt |
| RemainingQuantity | decimal(18,4) | Computed | OrderedQuantity - ReceivedQuantity |
| UnitPrice | decimal(23,2) | Required, ≥0 | Agreed price from quotation |
| DiscountPercent | decimal(5,2)? | Nullable | |
| DiscountAmount | decimal(23,2)? | Nullable | |
| NetUnitPrice | decimal(23,2)? | Nullable | |
| TaxPercent | decimal(5,2)? | Nullable | |
| TaxAmount | decimal(23,2)? | Nullable | |
| LineTotal | decimal(23,2)? | Nullable | |
| LineTotalWithTax | decimal(23,2)? | Nullable | |
| ExpectedDeliveryDate | DateTime? | Nullable | |
| Status | enum | Required | Pending=0, PartiallyReceived=1, Received=2, Closed=3 |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**Added fields**: PurchaseRequestDetailId, QuotationDetailId, RemainingQuantity, Status (typed enum).

### GoodsReceiptNote (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| GRNNumber | string(20) | Required, Unique | GRN-{D6} |
| GRNDate | DateTime | Required | |
| SupplierPartyId | int? | FK → Parties, Nullable, Restrict | **Replaces SupplierId** |
| PurchaseOrderId | int | FK → PurchaseOrder, Required, Restrict | |
| WarehouseId | int | FK → Warehouses, Required, Restrict | |
| LocationId | int | FK → Locations, Required, Restrict | |
| ReceivedBy | int? | FK → Users, Nullable | Who received the goods |
| Status | enum | Required | Draft=0, Confirmed=1, Rejected=2 |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**Removed fields**: SupplierId, PurchaseOrderNumber (denormalized), InvoiceNumber, InvoiceDate, TotalQuantity, TotalAmount — InvoiceNumber/Date move to SupplierInvoice.

### GoodsReceiptNoteDetail (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| GRNId | int | FK → GoodsReceiptNote, Required, Restrict | |
| PurchaseOrderDetailId | int | FK → PurchaseOrderDetail, Required, Restrict | **NEW — links to specific PO line** |
| ItemId | int | FK → Items, Required, Restrict | |
| UnitId | int | FK → Units, Required, Restrict | |
| OrderedQuantity | decimal(18,4) | Required | Copied from PO line at creation |
| ReceivedQuantity | decimal(18,4) | Required, ≥0 | What was physically received |
| AcceptedQuantity | decimal(18,4)? | Nullable | After inspection |
| RejectedQuantity | decimal(18,4)? | Nullable | After inspection |
| RemainingQuantity | decimal(18,4) | Computed | OrderedQuantity - AcceptedQuantity |
| UnitCost | decimal(23,2)? | Nullable | |
| TotalCost | decimal(23,2)? | Nullable | |
| BatchNumber | string(100)? | Nullable | |
| ExpiryDate | DateOnly? | Nullable | |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**Added fields**: PurchaseOrderDetailId (required FK), RemainingQuantity (computed).

### SupplierInvoice (NEW)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| InvoiceNumber | string(20) | Required, Unique | SINV-{D6}, system-generated |
| SupplierInvoiceNumber | string(100) | Required | Supplier's own invoice number |
| InvoiceDate | DateOnly | Required | Supplier's invoice date |
| PurchaseOrderId | int | FK → PurchaseOrder, Required, Restrict | |
| SupplierPartyId | int | FK → Parties, Required, Restrict | |
| CurrencyCode | string(3)? | Nullable | |
| ExchangeRate | decimal(18,6)? | Nullable | |
| SubTotal | decimal(23,2)? | Nullable | |
| DiscountAmount | decimal(23,2)? | Nullable | |
| TaxAmount | decimal(23,2)? | Nullable | |
| ShippingCost | decimal(23,2)? | Nullable | |
| OtherCharges | decimal(23,2)? | Nullable | |
| GrandTotal | decimal(23,2)? | Nullable | |
| DueDate | DateOnly? | Nullable | Payment due date |
| Status | enum | Required | Draft=0, Submitted=1, Matched=2, PartiallyPaid=3, Paid=4, Disputed=5, Cancelled=6 |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**UNIQUE constraint**: (SupplierPartyId, SupplierInvoiceNumber)

### SupplierInvoiceDetail (NEW)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| SupplierInvoiceId | int | FK → SupplierInvoice, Required, Restrict | |
| PurchaseOrderDetailId | int | FK → PurchaseOrderDetail, Required, Restrict | |
| GoodsReceiptNoteDetailId | int? | FK → GoodsReceiptNoteDetail, Nullable, Restrict | |
| ItemId | int | FK → Items, Required, Restrict | |
| Quantity | decimal(18,4) | Required, >0 | |
| UnitPrice | decimal(23,2) | Required, ≥0 | |
| DiscountAmount | decimal(23,2)? | Nullable | |
| TaxAmount | decimal(23,2)? | Nullable | |
| LineTotal | decimal(23,2)? | Nullable | Computed |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

### CommitteeAssignment (existing — modified)

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK | |
| CommitteeId | int | FK → Committees, Required, Restrict | |
| AssignmentType | enum | Required | Tender=0, Receiving=1, Inspection=2 |
| PurchaseOrderId | int? | FK → PurchaseOrder, Nullable, Restrict | For Receiving/Inspection committees |
| **RfqId** | **int?** | **FK → RequestForQuotation, Nullable, Restrict** | **NEW — for evaluation committees** |
| AssignmentDate | DateOnly | Required | |
| DecisionNumber | string(100)? | Nullable | |
| DecisionDate | DateOnly? | Nullable | |
| Status | enum | Required | Draft=0, Assigned=1, InProgress=2, Completed=3, Cancelled=4 |
| RequiredSignaturesCount | int | Default 1 | |
| ActualSignaturesCount | int | Default 0 | |
| Notes | string(2000)? | Nullable | |
| Created/Modified/RowVersion | | Audit | |

**Added field**: RfqId (int?, nullable FK to RequestForQuotation).

## Status Enums (NEW — replacing string statuses)

### PurchaseRequestStatus
```
Draft = 0
Submitted = 1
Approved = 2
UnderProcurement = 3
Rejected = 4
Cancelled = 5
Expired = 6
```

### RFQStatus
```
Draft = 0
Published = 1
CollectingResponses = 2
Closed = 3
Cancelled = 4
NoResponse = 5
```

### RFQSupplierStatus
```
Invited = 0
Responded = 1
Expired = 2
Declined = 3
```

### QuotationStatus
```
Draft = 0
Submitted = 1
UnderEvaluation = 2
Evaluated = 3
Selected = 4
Awarded = 5
Rejected = 6
Expired = 7
```

### PurchaseOrderStatus
```
Draft = 0
Submitted = 1
Approved = 2
Issued = 3
PartiallyReceived = 4
Received = 5
Closed = 6
Cancelled = 7
```

### PurchaseOrderDetailStatus
```
Pending = 0
PartiallyReceived = 1
Received = 2
Closed = 3
```

### GRNStatus
```
Draft = 0
Confirmed = 1
Rejected = 2
```

### SupplierInvoiceStatus
```
Draft = 0
Submitted = 1
Matched = 2
PartiallyPaid = 3
Paid = 4
Disputed = 5
Cancelled = 6
```

### PurchaseRequestPriority
```
Low = 0
Normal = 1
High = 2
Urgent = 3
```

## State Transition Diagrams

### PurchaseRequest
```
Draft → Submitted (submit)
Submitted → Approved (approve)
Submitted → Rejected (reject)
Approved → UnderProcurement (RFQ or PO created)
Approved → Cancelled (cancel)
```

### RequestForQuotation
```
Draft → Published (publish)
Published → CollectingResponses (first response)
CollectingResponses → Closed (all responded/expired)
any → Cancelled (cancel)
```

### Quotation
```
Draft → Submitted (submit)
Submitted → UnderEvaluation (start evaluation)
UnderEvaluation → Evaluated (evaluation complete)
Evaluated → Selected (select winner)
Selected → Awarded (award approved)
Selected → Rejected (award rejected)
UnderEvaluation → Rejected (exclude)
```

### PurchaseOrder
```
Draft → Submitted (submit)
Submitted → Approved (approve — creates encumbrance)
Approved → Issued (issue)
Issued → PartiallyReceived (first partial receipt)
PartiallyReceived → Received (all lines received)
Received → Closed (close)
PartiallyReceived → Closed (manual close)
any → Cancelled (cancel — reverses encumbrance)
```

### GoodsReceiptNote
```
Draft → Confirmed (confirm)
Draft → Reject (reject)
```

### SupplierInvoice
```
Draft → Submitted (submit)
Submitted → Matched (match confirmed)
Matched → PartiallyPaid (partial payment)
PartiallyPaid → Paid (full payment)
any → Cancelled (cancel)
any → Disputed (dispute)
```

### Encumbrance (existing — unchanged)
```
Draft → PendingApproval → Approved → Active → PartiallyReleased → PartiallyLiquidated → FullyLiquidated → Closed
any → Cancelled | Reversed | Suspended
```

### PaymentOrder (existing — unchanged)
```
Draft → Submitted → Approved → SentToTreasury → Paid
any → Cancelled | Rejected | Voided
```
