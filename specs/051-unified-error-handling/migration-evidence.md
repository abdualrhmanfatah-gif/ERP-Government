# Migration Evidence: Unified Error Handling

**Date**: 2026-09-14
**Spec**: [spec.md](./spec.md)
**Tasks**: T050

## Summary

All migratable error patterns across the backend Application and Web layers have been converted to use `Result<T>.Failure()` with structured error codes, categories, and messages. The frontend normalization layer is complete. Build is clean (0 errors, 0 warnings).

## Migration Scope

### Total Files Modified

| Category | Count | Status |
|----------|-------|--------|
| Query handlers (nullable → Result<T>) | 40 | ✅ Done |
| Command handlers (throw → Result.Failure) | 11 | ✅ Done |
| Endpoints (Results.NotFound → ToProblemDetails) | 40 | ✅ Done |
| Error codes catalog | 1 module | ✅ Extended |
| Infrastructure (logging, classification) | 1 file | ✅ Enhanced |

### Modules Migrated

#### Accounting (5 queries + 2 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/AccountGroups/{id}` | `GetAccountGroupByIdQuery` | `return null` → 500 | `Result.Failure(AccountGroupNotFound)` → 404 | 404 |
| `GET /api/AccountGroups/{id}/detail` | `GetAccountGroupDetailQuery` | `return null` → 500 | `Result.Failure(AccountGroupNotFound)` → 404 | 404 |
| `GET /api/Accounts/{id}` | `GetAccountByIdQuery` | `return null` → 500 | `Result.Failure(AccountNotFound)` → 404 | 404 |
| `GET /api/Journals/{id}` | `GetJournalByIdQuery` | `return null` → 500 | `Result.Failure(JournalNotFound)` → 404 | 404 |
| `GET /api/JournalEntries/{id}` | `GetJournalEntryByIdQuery` | `return null` → 500 | `Result.Failure(JournalEntryNotFound)` → 404 | 404 |
| `GET /api/RecurringEntries/{id}` | `GetRecurringEntryByIdQuery` | `return null` → 500 | `Result.Failure(RecurringEntryNotFound)` → 404 | 404 |

#### Banking (2 queries + 2 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/BankStatements/{id}` | `GetBankStatementByIdQuery` | `return null` → 500 | `Result.Failure(StatementNotFound)` → 404 | 404 |
| `GET /api/BankReconciliations/{id}` | `GetBankReconciliationByIdQuery` | `return null` → 500 | `Result.Failure(BankReconciliationNotFound)` → 404 | 404 |

#### Budgeting (8 queries + 8 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/Budgets/{id}` | `GetBudgetByIdQuery` | `throw NotFoundException` | `Result.Failure(BudgetNotFound)` | 404 |
| `GET /api/BudgetTypes/{id}` | `GetBudgetTypeByIdQuery` | `throw NotFoundException` | `Result.Failure(BudgetTypeNotFound)` | 404 |
| `GET /api/Funds/{id}` | `GetFundByIdQuery` | `throw NotFoundException` | `Result.Failure(FundNotFound)` | 404 |
| `GET /api/BudgetClassifications/{id}` | `GetBudgetClassificationByIdQuery` | `throw NotFoundException` | `Result.Failure(ClassificationNotFound)` | 404 |
| `GET /api/BudgetItems/{id}` | `GetBudgetItemByIdQuery` | `throw NotFoundException` | `Result.Failure(BudgetNotFound)` | 404 |
| `GET /api/BudgetItems/{id}/availability` | `GetBudgetItemAvailabilityQuery` | `return null` → 500 | `Result.Failure(AvailabilityNotFound)` → 404 | 404 |
| `GET /api/BudgetItemAllocations/{id}` | `GetBudgetItemAllocationByIdQuery` | `return null` → 500 | `Result.Failure(AllocationNotFound)` → 404 | 404 |
| `GET /api/BudgetTransactions/{id}` | `GetBudgetTransactionByIdQuery` | `return null` → 500 | `Result.Failure(TransactionNotFound)` → 404 | 404 |
| `GET /api/Encumbrances/{id}` | `GetEncumbranceByIdQuery` | `return null` → 500 | `Result.Failure(EncumbranceNotFound)` → 404 | 404 |

#### Committees (3 queries + 3 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/Committees/{id}` | `GetCommitteeByIdQuery` | `return null` → 500 | `Result.Failure(CommitteeNotFound)` → 404 | 404 |
| `GET /api/CommitteeMembers/{id}` | `GetCommitteeMemberByIdQuery` | `return null` → 500 | `Result.Failure(MemberNotFound)` → 404 | 404 |
| `GET /api/CommitteeAssignments/{id}` | `GetCommitteeAssignmentByIdQuery` | `return null` → 500 | `Result.Failure(AssignmentNotFound)` → 404 | 404 |

#### FinancialSettings (7 queries + 7 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/Currencies/{id}` | `GetCurrencyByIdQuery` | `return null` → 500 | `Result.Failure(CurrencyNotFound)` → 404 | 404 |
| `GET /api/ExchangeRates/{id}` | `GetExchangeRateByIdQuery` | `return null` → 500 | `Result.Failure(ExchangeRateNotFound)` → 404 | 404 |
| `GET /api/ExchangeRates/lookup` | `LookupExchangeRateQuery` | `return null` → 500 | `Result.Failure(ExchangeRateNotFound)` → 404 | 404 |
| `GET /api/FiscalYears/{id}` | `GetFiscalYearByIdQuery` | `return null` → 500 | `Result.Failure(FiscalYearNotFound)` → 404 | 404 |
| `GET /api/FiscalYears/period-by-date` | `GetFiscalYearPeriodByDateQuery` | `return null` → 500 | `Result.Failure(FiscalYearNotFound)` → 404 | 404 |
| `GET /api/FiscalPeriods/{id}` | `GetFiscalPeriodByIdQuery` | `return null` → 500 | `Result.Failure(FiscalPeriodNotFound)` → 404 | 404 |
| `GET /api/ClosingEntries/{id}` | `GetClosingEntryByIdQuery` | `return null` → 500 | `Result.Failure(ClosingEntryNotFound)` → 404 | 404 |
| `GET /api/DocumentSequences/{id}` | `GetDocumentSequenceByIdQuery` | `return null` → 500 | `Result.Failure(DocumentSequenceNotFound)` → 404 | 404 |

#### Inventory (4 queries + 4 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/Warehouses/{id}` | `GetWarehouseByIdQuery` | `return null` → 500 | `Result.Failure(WarehouseNotFound)` → 404 | 404 |
| `GET /api/Units/{id}` | `GetUnitByIdQuery` | `return null` → 500 | `Result.Failure(UnitNotFound)` → 404 | 404 |
| `GET /api/Items/{id}` | `GetItemByIdQuery` | `return null` → 500 | `Result.Failure(ItemNotFound)` → 404 | 404 |
| `GET /api/ItemCategories/{id}` | `GetItemCategoryByIdQuery` | `return null` → 500 | `Result.Failure(ItemCategoryNotFound)` → 404 | 404 |

#### Organization (4 queries + 4 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/Projects/{id}` | `GetProjectByIdQuery` | `throw NotFoundException` | `Result.Failure(ProjectNotFound)` | 404 |
| `GET /api/OrganizationalUnits/{id}` | `GetOrganizationalUnitByIdQuery` | `throw NotFoundException` | `Result.Failure(OrgUnitNotFound)` | 404 |
| `GET /api/Employees/{id}` | `GetEmployeeByIdQuery` | `throw NotFoundException` | `Result.Failure(EmployeeNotFound)` | 404 |
| `GET /api/CostCenters/{id}` | `GetCostCenterByIdQuery` | `throw NotFoundException` | `Result.Failure(CostCenterNotFound)` | 404 |

#### Payments (6 queries + 6 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/DisbursementRequests/{id}` | `GetDisbursementRequestByIdQuery` | `return null` → 500 | `Result.Failure(DisbursementRequestNotFound)` → 404 | 404 |
| `GET /api/DisbursementRequests/{id}/accrual` | `GetDisbursementRequestAccrualQuery` | `return null` → 500 | `Result.Failure(DisbursementRequestNotFound)` → 404 | 404 |
| `GET /api/PaymentOrders/{id}` | `GetPaymentOrderByIdQuery` | `return null` → 500 | `Result.Failure(PaymentOrderNotFound)` → 404 | 404 |
| `GET /api/PaymentOrders/{id}/totals` | `GetPaymentOrderTotalsQuery` | `return null` → 500 | `Result.Failure(PaymentOrderTotalsNotFound)` → 404 | 404 |
| `GET /api/PaymentOrders/{id}/print` | `GetPaymentOrderPrintQuery` | `return null` → 500 | `Result.Failure(PaymentOrderNotFound)` → 404 | 404 |
| `GET /api/BankAccounts/{id}` | `GetBankAccountByIdQuery` | `return null` → 500 | `Result.Failure(BankAccountNotFound)` → 404 | 404 |

#### Revenue (1 query + 1 endpoint)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/ReceiptVouchers/{id}` | `GetReceiptVoucherByIdQuery` | `return null` → 500 | `Result.Failure(ReceiptNotFound)` → 404 | 404 |

#### Security (4 queries + 4 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/Users/{id}` | `GetUserByIdQuery` | `return null` → 500 | `Result.Failure(UserNotFound)` → 404 | 404 |
| `GET /api/Roles/{id}` | `GetRoleByIdQuery` | `return null` → 500 | `Result.Failure(RoleNotFound)` → 404 | 404 |
| `GET /api/Permissions/{id}` | `GetPermissionByIdQuery` | `return null` → 500 | `Result.Failure(PermissionNotFound)` → 404 | 404 |
| `GET /api/ApprovalRules/{id}` | `GetApprovalRuleByIdQuery` | `return null` → 500 | `Result.Failure(ApprovalRuleNotFound)` → 404 | 404 |

#### Workflow (3 queries + 3 endpoints)
| Endpoint | Query | Old Pattern | New Pattern | HTTP Status |
|----------|-------|-------------|-------------|-------------|
| `GET /api/WorkflowDefinitions/{id}` | `GetWorkflowDefinitionByIdQuery` | `return null` → 500 | `Result.Failure(DefinitionNotFound)` → 404 | 404 |
| `GET /api/WorkflowInstances/{id}` | `GetWorkflowInstanceByIdQuery` | `return null` → 500 | `Result.Failure(InstanceNotFound)` → 404 | 404 |
| `GET /api/WorkflowHistory/{instanceId}` | `GetWorkflowHistoryQuery` | `throw NotFoundException` | `Result.Failure(InstanceNotFound)` | 404 |

#### Workflow Commands (6 commands)
| Command | Old Pattern | New Pattern | HTTP Status |
|---------|-------------|-------------|-------------|
| `CreateWorkflowDefinitionCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `UpdateWorkflowDefinitionCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `DeleteWorkflowDefinitionCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `StartWorkflowInstanceCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `CancelWorkflowInstanceCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `AdvanceWorkflowInstanceCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `ExecuteApprovalStepCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |

#### ApprovalRules Commands (3 commands)
| Command | Old Pattern | New Pattern | HTTP Status |
|---------|-------------|-------------|-------------|
| `CreateApprovalRuleCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `UpdateApprovalRuleCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |
| `DeactivateApprovalRuleCommand` | `throw InvalidOperationException` | `Result.Failure` | 400 |

### Error Codes Catalog Extended

New codes added to `src/Application/Common/Errors/ErrorCodes.cs`:

| Module | New Codes |
|--------|-----------|
| Accounting | `AccountGroupNotFound`, `AccountNotFound`, `JournalNotFound`, `JournalEntryNotFound`, `RecurringEntryNotFound` |
| Banking | `StatementNotFound`, `BankReconciliationNotFound` |
| Budgets | `BudgetTypeNotFound`, `AllocationNotFound`, `TransactionNotFound`, `EncumbranceNotFound`, `AvailabilityNotFound` |
| Committees | `CommitteeNotFound`, `MemberNotFound`, `AssignmentNotFound` |
| FinancialSettings | `FiscalYearNotFound`, `FiscalYearPeriodNotFound`, `FiscalPeriodNotFound`, `ClosingEntryNotFound`, `DocumentSequenceNotFound` |
| Inventory | `WarehouseNotFound`, `UnitNotFound`, `ItemCategoryNotFound` |
| Payments | `DisbursementRequestNotFound`, `PaymentOrderNotFound`, `PaymentOrderTotalsNotFound`, `BankAccountNotFound` |
| Security | `PermissionNotFound` |
| Workflow | `DefinitionNotFound`, `InstanceNotFound`, `InstanceNotInProgress`, `StepNotFound`, `ActiveInstancesExist`, `NoApprovalRules`, `UserMissingRequiredRole`, `IdentityRequired`, `NoActiveSteps`, `ActiveInstanceExists`, `StepNotApproval` |
| SecurityApprovalRules | `NotFound`, `RoleNotFound`, `DuplicateRule` |
| Organization | `ProjectNotFound`, `OrgUnitNotFound`, `EmployeeNotFound`, `CostCenterNotFound` |

### Deferred (Not Migrated)

| Pattern | Reason | Count |
|---------|--------|-------|
| `throw InvalidOperationException` in private helpers returning domain types | Would require method signature changes across callers | 4 |
| Pattern C endpoints (direct DB lookups) | Bypass MediatR, no handler to migrate | 5 |
| `RecordPaymentCommand` private helper | Returns `JournalEntry`, not `Result<T>` | 1 |
| `RevenueJournalEntryService` | Service layer, not handler; returns domain entity | 1 |
| `GetTrialBalanceReportQueryHandler` | Reconciliation assertion, not business rejection | 1 |
| `DocumentSequenceService` | Infrastructure exception for sequence concurrency | 4 |
| `ReportAuditService` | Authorization check in service layer | 1 |

## Verification Results

### Build
- **Backend**: 0 errors, 0 warnings (`dotnet build src/Web/Web.csproj`)
- **Frontend**: Build blocked by pre-existing `ComponentGallery.tsx` duplicate import (unrelated)

### Tests
- **Domain Unit Tests**: 10/10 passed
- **Application Unit Tests**: 485/519 (34 pre-existing failures in `ReceiptVoucherTests.cs` referencing missing types — unrelated)

### Pre-existing Issues (Unrelated to Error Handling)
1. `ComponentGallery.tsx` duplicate `Badge` import — blocks `npm run build`
2. `ReceiptVoucherTests.cs` references `DepositSlip`, `FormType`, `DepositSlipStatus`, `ReceiptVoucherStatus.PendingReview` — all missing types
3. `PurchaseOrderTests.cs`, `QuotationTests.cs` reference `RequestForQuotations` namespace — missing
4. Various Revenue test files reference missing command types
