# API Contracts: Procurement Lifecycle

**Branch**: `047-procurement-lifecycle` | **Date**: 2026-09-10

All endpoints follow the existing minimal API pattern: `IEndpointGroup` with static `Map`, request DTOs with `ToCommand()`, response DTOs with `ToResponse()`.

Base path: `/api/Procurement`

---

## Purchase Requests

### GET /api/Procurement/PurchaseRequests
**Query params**: `status?`, `priority?`, `search?`, `page?`, `pageSize?`
**Response**: `200` — `PurchaseRequestListResponse[]`

### GET /api/Procurement/PurchaseRequests/{id}
**Response**: `200` — `PurchaseRequestDetailResponse` | `404`

### POST /api/Procurement/PurchaseRequests
**Request**: `CreatePurchaseRequestRequest`
**Response**: `201` — `int` (new ID)

### PUT /api/Procurement/PurchaseRequests/{id}
**Request**: `UpdatePurchaseRequestRequest`
**Response**: `200` | `400` (validation) | `409` (concurrency)

### PATCH /api/Procurement/PurchaseRequests/{id}/submit
**Response**: `200` | `400` (invalid transition)

### PATCH /api/Procurement/PurchaseRequests/{id}/approve
**Request**: `ApprovePurchaseRequestRequest` (notes?)
**Response**: `200` | `400`

### PATCH /api/Procurement/PurchaseRequests/{id}/reject
**Request**: `RejectPurchaseRequestRequest` (reason required)
**Response**: `200` | `400`

### PATCH /api/Procurement/PurchaseRequests/{id}/cancel
**Request**: `CancelPurchaseRequestRequest` (reason required)
**Response**: `200` | `400`

### GET /api/Procurement/PurchaseRequests/{id}/approval-history
**Response**: `200` — `ApprovalHistoryEntry[]`

### GET /api/Procurement/PurchaseRequests/{id}/status-log
**Response**: `200` — `DocumentStatusLogEntry[]`

---

## Request for Quotations

### GET /api/Procurement/RequestForQuotations
**Query params**: `status?`, `purchaseRequestId?`, `search?`
**Response**: `200` — `RFQListResponse[]`

### GET /api/Procurement/RequestForQuotations/{id}
**Response**: `200` — `RFQDetailResponse` (includes suppliers list)

### POST /api/Procurement/RequestForQuotations
**Request**: `CreateRFQRequest` (purchaseRequestId, deadlineDate, currencyCode, terms, supplierPartyIds[])
**Response**: `201` — `int`

### PUT /api/Procurement/RequestForQuotations/{id}
**Request**: `UpdateRFQRequest`
**Response**: `200` | `400`

### PATCH /api/Procurement/RequestForQuotations/{id}/publish
**Response**: `200` | `400`

### PATCH /api/Procurement/RequestForQuotations/{id}/close
**Response**: `200` | `400`

### PATCH /api/Procurement/RequestForQuotations/{id}/cancel
**Response**: `200` | `400`

---

## Quotations

### GET /api/Procurement/Quotations
**Query params**: `rfqId?`, `supplierPartyId?`, `status?`, `search?`
**Response**: `200` — `QuotationListResponse[]`

### GET /api/Procurement/Quotations/{id}
**Response**: `200` — `QuotationDetailResponse` (includes lines)

### POST /api/Procurement/Quotations
**Request**: `CreateQuotationRequest` (rfqId, rfqSupplierId, supplierPartyId, lines[], terms)
**Response**: `201` — `int`

### PUT /api/Procurement/Quotations/{id}
**Request**: `UpdateQuotationRequest`
**Response**: `200` | `400`

### PATCH /api/Procurement/Quotations/{id}/submit
**Response**: `200` | `400`

### PATCH /api/Procurement/Quotations/{id}/start-evaluation
**Request**: `StartEvaluationRequest` (committeeAssignmentId)
**Response**: `200` | `400`

### PATCH /api/Procurement/Quotations/{id}/complete-evaluation
**Request**: `CompleteEvaluationRequest` (technicalScore, financialScore)
**Response**: `200` | `400`

### PATCH /api/Procurement/Quotations/{id}/select
**Request**: `SelectQuotationRequest` (selectionReason)
**Response**: `200` | `400`

### PATCH /api/Procurement/Quotations/{id}/award
**Response**: `200` | `400`

### PATCH /api/Procurement/Quotations/{id}/reject
**Request**: `RejectQuotationRequest` (rejectionReason)
**Response**: `200` | `400`

---

## Purchase Orders

### GET /api/Procurement/PurchaseOrders
**Query params**: `status?`, `supplierPartyId?`, `purchaseRequestId?`, `quotationId?`, `search?` (PONumber), `expectedDeliveryDateFrom?`, `expectedDeliveryDateTo?`, `page?`, `pageSize?`
**Response**: `200` — `PurchaseOrderListResponse[]` (includes resolved SupplierName from Parties join)

### GET /api/Procurement/PurchaseOrders/{id}
**Response**: `200` — `PurchaseOrderDetailResponse` (includes lines, approval history, status log, navigation links to PR, Quotation, GRNs, SupplierInvoices, PaymentOrders) | `404`

### POST /api/Procurement/PurchaseOrders
**Request**: `CreatePurchaseOrderRequest` (purchaseRequestId?, quotationId?, supplierPartyId, lines[], terms)
**Response**: `201` — `int`

### PUT /api/Procurement/PurchaseOrders/{id}
**Request**: `UpdatePurchaseOrderRequest`
**Response**: `200` | `400`

### PATCH /api/Procurement/PurchaseOrders/{id}/submit
**Response**: `200` | `400`

### PATCH /api/Procurement/PurchaseOrders/{id}/approve
**Response**: `200` | `400` (budget check fails)

### PATCH /api/Procurement/PurchaseOrders/{id}/issue
**Response**: `200` | `400`

### PATCH /api/Procurement/PurchaseOrders/{id}/cancel
**Request**: `CancelPurchaseOrderRequest` (reason required — string, max 2000 chars)
**Response**: `200` | `400`

### PATCH /api/Procurement/PurchaseOrders/{id}/close
**Request**: `ClosePurchaseOrderRequest` (reason optional — string, max 2000 chars)
**Response**: `200` | `400`

---

## Goods Receipt Notes

### GET /api/GoodsReceiptNotes
**Query params**: `purchaseOrderId?`, `status?`, `search?`, `page?` (default 1), `pageSize?` (default 20)
**Response**: `200` — `{ items: GRNListResponse[], totalCount: number }` (paginated)

### GET /api/GoodsReceiptNotes/{id}
**Response**: `200` — `GRNDetailResponse` (includes lines, warehouse, location, receivedBy)

### POST /api/GoodsReceiptNotes
**Request**: `CreateGRNCommand` (purchaseOrderId, warehouseId, locationId, receivedBy?, notes?, lines[])
**Response**: `201` — `int`

### PATCH /api/GoodsReceiptNotes/{id}/confirm
**Response**: `200` | `400` (quantity validation)

### PATCH /api/GoodsReceiptNotes/{id}/reject
**Request**: `{ notes?: string }`
**Response**: `200` | `400`

---

## Supplier Invoices

> **Backend gap (2026-09-12)**: The `accept-with-notes` endpoint is designed but NOT wired in the endpoint group. The `match` handler changes status only (no three-way matching verification). SupplierName is returned as null in both list and detail responses. Endpoint group lacks `.RequireAuthorization()` on routes. See spec G1–G6 for full gap list.

### GET /api/SupplierInvoices
**Query params**: `purchaseOrderId?`, `status?`, `search?`, `page?`, `pageSize?`
**Response**: `200` — `PaginatedList<SupplierInvoiceListItem>` (items, totalCount, page, pageSize, totalPages)

### GET /api/SupplierInvoices/{id}
**Response**: `200` — `SupplierInvoiceDetailResponse` (includes lines; SupplierName may be null)

### POST /api/SupplierInvoices
**Request**: `CreateSupplierInvoiceCommand` (purchaseOrderId, supplierInvoiceNumber, invoiceDate, currencyCode?, exchangeRate?, subTotal?, discountAmount?, taxAmount?, shippingCost?, otherCharges?, grandTotal?, dueDate?, notes?, details[])
**Response**: `201` — `int` (new invoice ID)

### PATCH /api/SupplierInvoices/{id}/submit
**Response**: `200` | `400`

### PATCH /api/SupplierInvoices/{id}/match
**Response**: `200` | `400` (status-transition-only; no three-way matching verification)

### PATCH /api/SupplierInvoices/{id}/cancel
**Request**: `CancelSupplierInvoiceCommand` (notes?)
**Response**: `200` | `400`

---

## Procurement Dashboard

### GET /api/Procurement/Dashboard
**Response**: `200` — `ProcurementDashboardResponse`
```json
{
  "pendingApprovals": 5,
  "openPurchaseOrders": 12,
  "pendingReceipts": 3,
  "pendingInvoices": 7,
  "encumbranceSummary": {
    "totalEncumbered": 15000000,
    "totalLiquidated": 8000000,
    "totalOutstanding": 7000000
  }
}
```

---

## Common DTOs

```csharp
// Request DTOs
record CreatePurchaseRequestRequest(
    DateOnly? RequiredDate,
    int? DepartmentId,
    int? CostCenterId,
    PurchaseRequestPriority Priority,
    string? Notes,
    List<PurchaseRequestDetailRequest> Lines);

record PurchaseRequestDetailRequest(
    int ItemId,
    int UnitId,
    decimal RequestedQuantity,
    decimal? UnitCostEstimate,
    string? Notes);

// Response DTOs
record PurchaseRequestListResponse(
    int Id, string RequestNumber, DateTime RequestDate,
    DateOnly? RequiredDate, string? Priority, string Status,
    decimal? TotalEstimatedCost, string? Notes);

record PurchaseRequestDetailResponse(
    int Id, string RequestNumber, DateTime RequestDate,
    DateOnly? RequiredDate, int? DepartmentId, int? CostCenterId,
    int? RequesterId, string Priority, string Status,
    decimal? TotalEstimatedCost, string? Notes,
    List<PurchaseRequestDetailLineResponse> Lines,
    List<ApprovalHistoryEntry> ApprovalHistory,
    List<DocumentStatusLogEntry> StatusLog);

record PurchaseRequestDetailLineResponse(
    int Id, int ItemId, string? ItemName, int UnitId, string? UnitName,
    decimal RequestedQuantity, decimal? ApprovedQuantity,
    decimal? UnitCostEstimate, decimal? TotalCostEstimate, string? Notes);
```
