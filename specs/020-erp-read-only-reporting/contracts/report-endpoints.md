# API Contract: Report Endpoints

**Feature**: 020-erp-read-only-reporting
**Base Path**: `/api`
**Date**: 2026-09-06

## Overview

All endpoints are GET-only (read-only). Query parameters use `[AsParameters]` binding. Responses are JSON. Export endpoints return file streams.

---

## Budget Execution Reports

### GET /api/BudgetExecutionReports

**Purpose**: List budget execution summary (appropriations vs encumbrances vs payments)

**Authorization**: `Reporting.ViewBudgetExecution`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year to report on |
| FiscalPeriodId | int | No | Specific period (else full year) |
| FundId | int | No | Filter by fund |
| ProgramId | int | No | Filter by program |
| ProjectId | int | No | Filter by project |
| BudgetItemId | int | No | Filter by specific budget item |

**Response**: `200 OK` → `BudgetExecutionReportDto`

---

### GET /api/BudgetExecutionReports/{budgetItemId}/detail

**Purpose**: Drill-down into individual encumbrances and payments for a budget line

**Authorization**: `Reporting.ViewBudgetExecution`

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| budgetItemId | int | Budget item to drill into |

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FundId | int | No | Filter by fund |

**Response**: `200 OK` → `BudgetExecutionDetailDto`

---

### GET /api/BudgetExecutionReports/export

**Purpose**: Export budget execution report to Excel or PDF

**Authorization**: `Reporting.ExportReports`

**Query Parameters**: Same as GET /api/BudgetExecutionReports + `format` (string: "excel" | "pdf")

**Response**: `200 OK` → File stream (xlsx or pdf)

---

## Revenue Collections Reports

### GET /api/RevenueCollectionsReports

**Purpose**: List receipt vouchers with deposit-slip and check-clearing status

**Authorization**: `Reporting.ViewRevenueCollections`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FiscalPeriodId | int | No | Specific period |
| FundId | int | No | Filter by fund |
| RevenueAccountId | int | No | Filter by revenue account |
| PartyId | int | No | Filter by party |
| PaymentMethod | string | No | Filter by method (Cash/Check/Transfer) |

**Response**: `200 OK` → `RevenueCollectionsReportDto`

---

### GET /api/RevenueCollectionsReports/{receiptVoucherId}/detail

**Purpose**: Drill-down into a specific receipt voucher

**Authorization**: `Reporting.ViewRevenueCollections`

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| receiptVoucherId | int | Receipt voucher to view |

**Response**: `200 OK` → `RevenueCollectionsDetailDto`

---

### GET /api/RevenueCollectionsReports/export

**Purpose**: Export revenue collections report

**Authorization**: `Reporting.ExportReports`

**Query Parameters**: Same as GET /api/RevenueCollectionsReports + `format`

**Response**: `200 OK` → File stream

---

## Disbursement Register Reports

### GET /api/DisbursementRegisterReports

**Purpose**: List disbursement requests and payments by status

**Authorization**: `Reporting.ViewDisbursementRegister`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FiscalPeriodId | int | No | Specific period |
| FundId | int | No | Filter by fund |
| Status | string | No | Filter by status |
| ApproverId | int | No | Filter by approver |

**Response**: `200 OK` → `DisbursementRegisterDto`

---

### GET /api/DisbursementRegisterReports/{paymentOrderId}/detail

**Purpose**: Drill-down into a specific disbursement

**Authorization**: `Reporting.ViewDisbursementRegister`

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| paymentOrderId | int | Payment order to view |

**Response**: `200 OK` → `DisbursementRegisterDetailDto`

---

### GET /api/DisbursementRegisterReports/export

**Purpose**: Export disbursement register

**Authorization**: `Reporting.ExportReports`

**Query Parameters**: Same as GET /api/DisbursementRegisterReports + `format`

**Response**: `200 OK` → File stream

---

## Availability Snapshot Reports

### GET /api/AvailabilitySnapshotReports

**Purpose**: List budget lines with availability breakdown

**Authorization**: `Reporting.ViewAvailabilitySnapshot`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FundId | int | No | Filter by fund |
| ProgramId | int | No | Filter by program |
| ProjectId | int | No | Filter by project |
| BudgetItemId | int | No | Filter by specific budget item |

**Response**: `200 OK` → `AvailabilitySnapshotDto`

---

### GET /api/AvailabilitySnapshotReports/{budgetItemId}/detail

**Purpose**: Drill-down into individual appropriations, encumbrances, and payments

**Authorization**: `Reporting.ViewAvailabilitySnapshot`

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| budgetItemId | int | Budget item to drill into |

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FundId | int | No | Filter by fund |

**Response**: `200 OK` → `AvailabilitySnapshotDetailDto`

---

### GET /api/AvailabilitySnapshotReports/export

**Purpose**: Export availability snapshot

**Authorization**: `Reporting.ExportReports`

**Query Parameters**: Same as GET /api/AvailabilitySnapshotReports + `format`

**Response**: `200 OK` → File stream

---

## Trial Balance Reports

### GET /api/TrialBalanceReports

**Purpose**: Trial balance with opening/closing balances per account

**Authorization**: `Reporting.ViewTrialBalanceReport`

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FiscalPeriodId | int | No | Specific period |
| FundId | int | No | Filter by fund |
| ProjectId | int | No | Filter by project |

**Response**: `200 OK` → `TrialBalanceReportDto`

---

### GET /api/TrialBalanceReports/{accountId}/ledger-movement

**Purpose**: Ledger movement for a specific account

**Authorization**: `Reporting.ViewTrialBalanceReport`

**Path Parameters**:
| Parameter | Type | Description |
|-----------|------|-------------|
| accountId | int | Account to view movements for |

**Query Parameters**:
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| FiscalYearId | int | Yes | Fiscal year |
| FundId | int | No | Filter by fund |

**Response**: `200 OK` → `LedgerMovementDto`

---

### GET /api/TrialBalanceReports/export

**Purpose**: Export trial balance

**Authorization**: `Reporting.ExportReports`

**Query Parameters**: Same as GET /api/TrialBalanceReports + `format`

**Response**: `200 OK` → File stream

---

## Error Responses

All endpoints follow the standard problem-details contract:

| Status | Meaning |
|--------|---------|
| 400 | Invalid filter parameters |
| 401 | Unauthenticated |
| 403 | Forbidden (insufficient permission) |
| 404 | Resource not found (drill-down target) |
| 500 | Internal server error |

## Notes

- All amounts are in base currency (decimal(23,2)).
- Arabic-first RTL rendering applies to all responses.
- Export file naming: `{ReportName}-{YYYYMMDD}.{xlsx|pdf}`
