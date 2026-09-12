# Data Model: RFQ & Quotation Management

**Feature**: 050-rfq-quotation-ui
**Date**: 2026-09-11

## Existing Entities (No Schema Changes)

All entities already exist in `src/Domain/Procurement/Entities/`. No new entities or fields are added.

### RequestForQuotation

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | From BaseAuditableEntity |
| RFQNumber | string | required, unique | Format: RFQ-{D6} |
| RFQDate | DateTime | required | Auto-set on creation |
| PurchaseRequestId | int | FK → PurchaseRequest, required | Must reference Approved PR |
| DeadlineDate | DateTime? | nullable | Must be ≥ RFQDate |
| CurrencyCode | string? | nullable | ISO 4217 code |
| TermsAndConditions | string? | nullable | Free text |
| Status | RFQStatus | required, default Draft | Enum: Draft=0, Published=1, CollectingResponses=2, Closed=3, Cancelled=4, NoResponse=5 |
| Notes | string? | nullable | Free text |
| RowVersion | byte[] | concurrency token | Optimistic concurrency |

**Navigation**: PurchaseRequest (many→one), Suppliers (one→many RFQSupplier)

### RFQSupplier

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | From BaseAuditableEntity |
| RFQId | int | FK → RequestForQuotation, required | |
| SupplierPartyId | int | FK → Parties (PartyType=Supplier), required | |
| InvitationDate | DateTime | required | Set when supplier is invited |
| ResponseDate | DateTime? | nullable | Set when supplier responds |
| Status | RFQSupplierStatus | required, default Invited | Enum: Invited=0, Responded=1, Expired=2, Declined=3 |
| Notes | string? | nullable | |
| RowVersion | byte[] | concurrency token | |

### Quotation

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | From BaseAuditableEntity |
| QuotationNumber | string | required, unique | Format: QT-{D6} |
| RFQId | int | FK → RequestForQuotation, required | |
| RFQSupplierId | int | FK → RFQSupplier, required | |
| SupplierPartyId | int | FK → Parties, required | |
| QuotationDate | DateTime | required | |
| ValidUntil | DateTime? | nullable | |
| CurrencyCode | string? | nullable | |
| ExchangeRate | decimal? | nullable | |
| SubTotal | decimal? | nullable | Computed: sum of line LineTotal |
| DiscountAmount | decimal? | nullable | Computed: sum of line DiscountAmount |
| TaxAmount | decimal? | nullable | Computed: sum of line TaxAmount |
| ShippingCost | decimal? | nullable | |
| OtherCharges | decimal? | nullable | |
| GrandTotal | decimal? | nullable | Computed: SubTotal - DiscountAmount + TaxAmount + ShippingCost + OtherCharges |
| PaymentTerms | string? | nullable | |
| DeliveryTerms | string? | nullable | |
| LeadTimeDays | int? | nullable | |
| WarrantyPeriodMonths | int? | nullable | |
| Status | QuotationStatus | required, default Draft | Enum: Draft=0, Submitted=1, UnderEvaluation=2, Evaluated=3, Selected=4, Awarded=5, Rejected=6, Expired=7 |
| TechnicalScore | decimal? | nullable | Set during evaluation |
| FinancialScore | decimal? | nullable | Set during evaluation |
| SelectionReason | string? | nullable | Required on Select |
| RejectionReason | string? | nullable | Required on Reject |
| Notes | string? | nullable | |
| RowVersion | byte[] | concurrency token | |

**Navigation**: RequestForQuotation (many→one), RFQSupplier (many→one), Details (one→many QuotationDetail)

### QuotationDetail

| Field | Type | Constraints | Notes |
|-------|------|-------------|-------|
| Id | int | PK, auto-increment | From BaseAuditableEntity |
| QuotationId | int | FK → Quotation, required | |
| PurchaseRequestDetailId | int | FK → PurchaseRequestDetail, required | Links to PR line |
| ItemId | int | FK → Item, required | |
| UnitId | int | FK → Unit, required | |
| Quantity | decimal | required, > 0 | |
| UnitPrice | decimal? | nullable, ≥ 0 | |
| DiscountPercent | decimal? | nullable, 0-100 | |
| DiscountAmount | decimal? | nullable | Computed: Quantity × UnitPrice × DiscountPercent / 100 |
| NetUnitPrice | decimal? | nullable | Computed: UnitPrice - DiscountAmount/Quantity |
| TaxPercent | decimal? | nullable, 0-100 | |
| TaxAmount | decimal? | nullable | Computed: (Quantity × UnitPrice - DiscountAmount) × TaxPercent / 100 |
| LineTotal | decimal? | nullable | Computed: Quantity × UnitPrice - DiscountAmount |
| LineTotalWithTax | decimal? | nullable | Computed: LineTotal + TaxAmount |
| Notes | string? | nullable | |
| RowVersion | byte[] | concurrency token | |

## Status Transitions

### RFQ Status Machine

```
Draft ──publish──→ Published ──first response──→ CollectingResponses ──close──→ Closed
  │                                                                                 
  └─────────────────────── any non-Closed/Cancelled ──cancel──→ Cancelled          
```

### Quotation Status Machine

```
Draft ──submit──→ Submitted ──start-evaluation──→ UnderEvaluation ──complete-evaluation──→ Evaluated
  │                                          │                                              │
  │                                          └──reject──→ Rejected                         │
  │                                                                                         │
  └──────────────────────────────────────────────────────────────────── select ──→ Selected ──award──→ Awarded
```

## Relationships

```
PurchaseRequest 1──→ N RequestForQuotation 1──→ N RFQSupplier
                                  │                      │
                                  │                      └──→ Parties (SupplierPartyId)
                                  │
                                  └──→ N Quotation 1──→ N QuotationDetail
                                           │                    │
                                           └──→ RFQSupplier    ├──→ PurchaseRequestDetail
                                                │              ├──→ Item
                                                └──→ Parties   └──→ Unit
```
