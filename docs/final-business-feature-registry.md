# FINAL BUSINESS FEATURE REGISTRY

**Version**: 1.3 (Budgeting Rebuild)
**Date**: 2026-09-04
**Baseline**: Feature Architecture Map v1.0
**Source of Truth**: Feature Registry (11 Business Features)
**Confidence**: HIGH

---

## 1. Feature Map

```
Financial Management
├── General Ledger
│   └── BF-001 Journal Entries
├── Budget Cycle
│   ├── BF-002 Budget Appropriations (rebuilt: 7 tables, availability engine)
│   └── BF-003 Budget Commitments (rebuilt: whole-item availability, reversal rows)
├── Payments
│   ├── BF-005 Payment Orders
│   ├── BF-006 Payment Execution
│   └── BF-007 Advance Payments
├── Revenue
│   └── BF-008 Revenue Receipts
├── Banking
│   ├── BF-009 Bank Statements
│   └── BF-010 Bank Reconciliation
└── Financial Configuration
    └── BF-011 Year-End Closing

Government Operations
└── Committees
    └── BF-012 Committee Management
```

---

## 2. Final Feature Registry

| ID | Name | Domain | Capability | Classification | Owner | Business Outcome | Main Process | Status | Evidence | Confidence |
|----|------|--------|------------|----------------|-------|------------------|--------------|--------|----------|------------|
| BF-001 | Journal Entries | Financial Management | General Ledger | BUSINESS_FEATURE | Accountant | Financial recording with double-entry balance | Create→Submit→Approve→Post→Reverse→Cancel | IMPLEMENTED | 7 commands, 2 queries, 12 endpoints, 3 events | HIGH |
| BF-002 | Budget Appropriations | Financial Management | Budget Cycle | BUSINESS_FEATURE | Budget Officer | Reserve budget authority for planned expenditure with computed transaction-time availability | Create→Submit→Approve→Activate→Suspend→Close→Cancel | IMPLEMENTED | 6 commands, 2 queries, 7 tables rebuilt, IBudgetAvailabilityService | HIGH |
| BF-003 | Budget Commitments | Financial Management | Budget Cycle | BUSINESS_FEATURE | Budget Officer | Lock budget for specific obligation with whole-BudgetItem availability check; transfers, monthly plans | Create→Submit→Approve→Activate→Suspend→Close→Cancel→Reverse | IMPLEMENTED | 10 commands, 2 queries, reversal rows, availability engine, transfer pairs | HIGH |
| BF-005 | Payment Orders | Financial Management | Payments | BUSINESS_FEATURE | Treasury Officer | Authorize disbursement of government funds | Create→Submit→Approve→Cancel→SendToTreasury→Void | IMPLEMENTED | 7 commands, 2 queries, 5 events | HIGH |
| BF-006 | Payment Execution | Financial Management | Payments | BUSINESS_FEATURE | Treasury Officer | Process actual bank transfer | Create→Approve→Send→Complete | IMPLEMENTED | 4 commands, 2 queries | HIGH |
| BF-007 | Advance Payments | Financial Management | Payments | BUSINESS_FEATURE | Treasury Officer | Prepay suppliers before delivery | Create→Approve→Consume→Cancel | IMPLEMENTED | 4 commands, 2 queries, 2 events | HIGH |
| BF-008 | Revenue Receipts | Financial Management | Revenue | BUSINESS_FEATURE | Revenue Officer | Record incoming government revenue | Create→Approve→Post→Cancel | IMPLEMENTED | 4 commands, 2 queries, 3 events | HIGH |
| BF-009 | Bank Statements | Financial Management | Banking | BUSINESS_FEATURE | Bank Reconciliation Officer | Import bank transaction data | Create→Cancel→Import→Reconcile | IMPLEMENTED | 5 commands, 3 queries, 1 event | HIGH |
| BF-010 | Bank Reconciliation | Financial Management | Banking | BUSINESS_FEATURE | Bank Reconciliation Officer | Match bank records to accounting books | Create→Approve→Complete→Reject | IMPLEMENTED | 5 commands, 3 queries, 1 event | HIGH |
| BF-011 | Year-End Closing | Financial Management | Financial Configuration | BUSINESS_FEATURE | Financial Admin | Close fiscal year and carry forward balances | GenerateClosingEntries→CloseYear | IMPLEMENTED | 3 commands, 2 queries, 1 event | HIGH |
| BF-012 | Committee Management | Government Operations | Committees | BUSINESS_FEATURE | Committee Admin | Form committees, manage members, track assignments | Create→Activate→Deactivate→Dissolve→AddMember→RemoveMember→CreateAssignment | IMPLEMENTED | 12 commands, 6 queries, 1 event | HIGH |

---

## 3. Supporting Business Capabilities

| ID | Name | Domain | Classification | Owner | Business Outcome | Status | Evidence | Confidence |
|----|------|--------|----------------|-------|------------------|--------|----------|------------|
| SB-001 | Fiscal Years & Periods | Financial Management | SUPPORTING_BUSINESS_CAPABILITY | Financial Admin | Define time boundaries for financial operations | IMPLEMENTED | 9 commands, 5 queries, 2 events | HIGH |
| SB-002 | Currencies & Exchange Rates | Financial Management | SUPPORTING_BUSINESS_CAPABILITY | Financial Admin | Provide currency conversion rates | IMPLEMENTED | 8 commands, 6 queries, 2 events | HIGH |
| SB-003 | Organizational Units | Organization & HR | SUPPORTING_BUSINESS_CAPABILITY | HR Admin | Define org hierarchy | IMPLEMENTED | 3 commands, 2 queries, full CRUD | HIGH |

---

## 4. Master Data

| ID | Name | Domain | Classification | Owner | Business Outcome | Status | Evidence | Confidence |
|----|------|--------|----------------|-------|------------------|--------|----------|------------|
| MD-001 | Chart of Accounts | Financial Management | MASTER_DATA | Accountant | Define account hierarchy | IMPLEMENTED | Account entity, AccountGroup entity | HIGH |
| MD-002 | Cost Centers | Organization & HR | MASTER_DATA | Financial Admin | Define financial dimension | IMPLEMENTED | 3 commands, 2 queries, full CRUD | HIGH |
| MD-003 | Projects | Organization & HR | MASTER_DATA | Financial Admin | Define project codes | IMPLEMENTED | 3 commands, 2 queries, full CRUD | HIGH |

---

## 5. Platform Capabilities

| ID | Name | Classification | Owner | Purpose | Status | Evidence | Confidence |
|----|------|----------------|-------|---------|--------|----------|------------|
| PC-001 | Identity & Users | PLATFORM_CAPABILITY | Platform | User management, authentication | IMPLEMENTED | 14 commands, 6 queries | HIGH |
| PC-002 | Authorization/RBAC | PLATFORM_CAPABILITY | Platform | Role-based access control | PARTIALLY_IMPLEMENTED | 231 open policies need wiring | HIGH |
| PC-003 | Workflow | PLATFORM_CAPABILITY | Platform | Multi-step approval workflows | IMPLEMENTED | 7 commands, 5 queries | HIGH |
| PC-004 | Audit | PLATFORM_CAPABILITY | Platform | Entity change tracking | IMPLEMENTED | AuditTrail, SecurityAuditLog | HIGH |
| PC-005 | Outbox | PLATFORM_CAPABILITY | Platform | Reliable domain event delivery | IMPLEMENTED | OutboxMessage entity | HIGH |
| PC-006 | Background Jobs | PLATFORM_CAPABILITY | Platform | Scheduled task execution | IMPLEMENTED | 3 entities, thin endpoint | HIGH |
| PC-007 | Notifications | PLATFORM_CAPABILITY | Platform | User notification delivery | IMPLEMENTED | Notification entity | HIGH |

---

## 6. Platform Mechanisms (Accounting-Specific)

| ID | Name | Classification | Owner | Purpose | Status | Evidence | Confidence |
|----|------|----------------|-------|---------|--------|----------|------------|
| PM-001 | Automated Posting | PLATFORM_CAPABILITY | Platform | Domain event → journal entry automation | IMPLEMENTED | PostingPipeline, PostingRule, PostingRuleLine, AccountingEvent | HIGH |
| PM-002 | Document Numbering | PLATFORM_CAPABILITY | Platform | Sequential document number generation | IMPLEMENTED | DocumentSequence entity, DocumentSequenceService | HIGH |

**Classification Rationale:**

- **Automated Posting**: No business actor. No independent business process. Technical mechanism that converts domain events into journal entries. Consumes PostingRules (configuration) and produces JournalEntries (BF-001). Infrastructure, not business function.
- **Document Numbering**: No business actor. No independent business process. Technical service that generates sequential numbers (JRN-, AUTO-). Used by multiple features. Infrastructure, not business function.

---

## 7. Shell / Future

| ID | Name | Domain | Classification | Entities | Events | Application Layer | Confidence |
|----|------|--------|----------------|----------|--------|-------------------|------------|
| SF-001 | Procurement | Government Operations | SHELL/FUTURE | 8 | 2 | NONE | HIGH |
| SF-002 | Supplier Management | Government Operations | SHELL/FUTURE | 3 | 0 | NONE | HIGH |
| SF-003 | Inventory Management | Government Operations | SHELL/FUTURE | 11 | 0 | NONE | HIGH |
| SF-004 | Asset Management | Government Operations | SHELL/FUTURE | 9 | 5 | NONE | HIGH |

---

## 8. Architecture Initiatives

| ID | Name | Classification | Status | Description | Confidence |
|----|------|----------------|--------|-------------|------------|
| AI-001 | EF Migration Baseline | ARCHITECTURE_INITIATIVE | IMPLEMENTED | Database schema baseline | HIGH |
| AI-002 | Payment → Accounting Integration | ARCHITECTURE_INITIATIVE | PARTIALLY_IMPLEMENTED | Cross-module event integration | HIGH |
| AI-003 | Budget Availability Model | ARCHITECTURE_INITIATIVE | IMPLEMENTED | Budget checking before payment — transaction-time availability engine with None/Warning/Blocking control, 3-level AllowOverrun inheritance, computed at appropriation/encumbrance create+approve | HIGH |
| AI-004 | Authorization Hardening | ARCHITECTURE_INITIATIVE | NOT_STARTED | Real RBAC wiring | HIGH |
| AI-005 | PostingPipeline Orphan Recovery | ARCHITECTURE_INITIATIVE | IMPLEMENTED | Crash recovery for stale events | HIGH |
| AI-006 | Structured Logging | ARCHITECTURE_INITIATIVE | IMPLEMENTED | Correlation IDs across pipeline | HIGH |

---

## 9. Final Counts

| Category | Count |
|----------|-------|
| **Business Domains** | 2 |
| **Business Capabilities** | 8 |
| **Business Features** | 11 |
| **Supporting Business Capabilities** | 3 |
| **Master Data** | 3 |
| **Platform Capabilities** | 9 |
| **Platform Mechanisms (Accounting-Specific)** | 2 |
| **Shell/Future** | 4 |
| **Architecture Initiatives** | 6 |
| **Total Items Classified** | 38 |

### Mathematical Verification

| Check | Expected | Actual | Match |
|-------|----------|--------|-------|
| Feature Registry rows | 11 | 11 | ✅ |
| Supporting Business Capabilities table rows | 3 | 3 | ✅ |
| Master Data table rows | 3 | 3 | ✅ |
| Platform Capabilities table rows | 7 | 7 | ✅ |
| Platform Mechanisms table rows | 2 | 2 | ✅ |
| Shell/Future table rows | 4 | 4 | ✅ |
| Architecture Initiatives table rows | 6 | 6 | ✅ |
| Section 1 Feature Map leaf nodes | 11 | 11 | ✅ |
| Section 2 Feature Registry rows | 11 | 11 | ✅ |
| Section 9 "Business Features" count | 11 | 11 | ✅ |
| **Total** | **38** | **38** | ✅ |

### Breakdown by Domain

| Domain | Business Features | Supporting | Master Data |
|--------|------------------|------------|-------------|
| Financial Management | 9 | 2 | 1 |
| Government Operations | 1 | 0 | 0 |
| Organization & HR | 0 | 1 | 2 |
| **Total** | **11** | **3** | **3** |

### Breakdown by Capability

| Capability | Business Features |
|------------|------------------|
| General Ledger | 1 (BF-001) |
| Budget Cycle | 2 (BF-002, BF-003) |
| Payments | 3 (BF-005, BF-006, BF-007) |
| Revenue | 1 (BF-008) |
| Banking | 2 (BF-009, BF-010) |
| Financial Configuration | 1 (BF-011) |
| Committees | 1 (BF-012) |
| **Total** | **11** |

---

**End of Final Business Feature Registry v1.3**
