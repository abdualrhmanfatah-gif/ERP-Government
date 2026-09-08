# ERP Government — Final Business Feature Architecture Map v1.0

**Date**: 2026-09-02
**Baseline**: v3.0
**Execution Map**: v1.1
**Confidence**: HIGH (all classifications based on verified codebase evidence)

---

## 1. Feature Map

### Financial Management (FM)

```
Financial Management
├── General Ledger
│   └── Journal Entries (FM-001) — FEATURE
├── Budget Cycle
│   ├── Budget Appropriations (FM-002) — FEATURE
│   ├── Budget Commitments / Encumbrances (FM-003) — FEATURE
│   └── Budget Liquidations (FM-004) — REMOVED
├── Payments
│   ├── Payment Orders (FM-005) — FEATURE
│   ├── Payment Execution (FM-006) — FEATURE
│   └── Advance Payments (FM-007) — FEATURE
├── Revenue
│   └── Revenue Receipts (FM-008) — FEATURE
├── Banking
│   ├── Bank Statements (FM-009) — FEATURE
│   └── Bank Reconciliation (FM-010) — FEATURE
└── Financial Configuration
    ├── Fiscal Years & Periods (FM-011) — FEATURE
    ├── Currencies & Exchange Rates (FM-012) — FEATURE
    └── Year-End Closing (FM-013) — FEATURE
```

### Organization & HR (OH)

```
Organization & HR
├── Organization Structure
│   └── Organizational Units (OH-001) — FEATURE
├── Workforce
│   └── Employee Management (OH-002) — FEATURE
└── Financial Dimensions
    ├── Cost Centers (OH-003) — FEATURE
    └── Projects (OH-004) — FEATURE
```

### Government Operations (GO)

```
Government Operations
├── Committees
│   └── Committee Management (GO-001) — FEATURE
├── Procurement
│   └── [SHELL] — Domain entities only, no Application layer
├── Suppliers
│   └── [SHELL] — Domain entities only, no Application layer
├── Inventory
│   └── [SHELL] — Domain entities only, no Application layer
└── Assets
    └── [SHELL] — Domain entities only, no Application layer
```

---

## 2. Feature Classifications

### FEATURE — Real Business Features

| Feature ID | Feature Name | Classification | Rationale |
|------------|--------------|----------------|-----------|
| FM-001 | Journal Entries | FEATURE | Core financial function. Users create, submit, approve, post, reverse entries. Clear business outcome, actors, rules. 7 commands, 2 queries, 12 endpoints. |
| FM-002 | Budget Appropriations | FEATURE | Budget cycle entry point. Users create, submit, approve appropriations. 5 commands, 2 queries, event-driven. |
| FM-003 | Budget Commitments | FEATURE | Encumbrance management. Users create, approve, release, reverse encumbrances. 5 commands, 2 queries, event-driven. |
| FM-004 | Budget Liquidations | FEATURE | Payment commitment tracking. Users create, approve, mark paid, reverse liquidations. 5 commands, 2 queries, event-driven. |
| FM-005 | Payment Orders | FEATURE | Payment initiation. Users create, submit, approve, cancel, send to treasury, void. 7 commands, 2 queries, 5 domain events. |
| FM-006 | Payment Execution | FEATURE | Payment processing. Users create, approve, send, complete executions. 4 commands, 2 queries. |
| FM-007 | Advance Payments | FEATURE | Pre-payment management. Users create, approve, consume, cancel. 4 commands, 2 queries, 2 domain events. |
| FM-008 | Revenue Receipts | FEATURE | Revenue collection. Users create, approve, post, cancel receipts. 4 commands, 2 queries, 3 domain events. |
| FM-009 | Bank Statements | FEATURE | Bank transaction import. Users create, cancel, import, reconcile statements. 5 commands, 3 queries. |
| FM-010 | Bank Reconciliation | FEATURE | Bank reconciliation. Users create, approve, complete, reject reconciliations. 5 commands, 3 queries. |
| FM-011 | Fiscal Years & Periods | FEATURE | Financial period management. Users create, open, close, lock periods. 9 commands, 5 queries, 2 domain events. |
| FM-012 | Currencies & Exchange Rates | FEATURE | Currency management. Users create, activate, deactivate currencies and rates. 8 commands, 6 queries, 2 domain events. |
| FM-013 | Year-End Closing | FEATURE | Annual closing process. Users generate closing entries, close fiscal years. 3 commands, 2 queries, 1 domain event. |
| OH-001 | Organizational Units | FEATURE | Org structure management. Full CRUD (GET+POST+PUT+DELETE). 3 commands, 2 queries. |
| OH-002 | Employee Management | FEATURE | Workforce management. Full CRUD. 3 commands, 2 queries. |
| OH-003 | Cost Centers | FEATURE | Financial dimension. Full CRUD. 3 commands, 2 queries. |
| OH-004 | Projects | FEATURE | Project tracking. Full CRUD. 3 commands, 2 queries. |
| GO-001 | Committee Management | FEATURE | Government committee management. Committees, members, assignments. 12 commands, 6 queries, 1 domain event. |

### PLATFORM — Not Business Features

| Item | Classification | Rationale |
|------|----------------|-----------|
| Automated Accounting Posting | PLATFORM | Architecture mechanism. PostingPipeline creates journal entries from domain events. No business owner. Infrastructure concern. |
| Document Numbering | PLATFORM | Shared service. Generates sequential document numbers (JRN-, AUTO-). Used by multiple features. |
| Identity & Authorization | PLATFORM | Platform capability. Users, Roles, Permissions, RBAC. Shared across all features. |
| Workflow Engine | PLATFORM | Platform capability. Workflow definitions, instances, steps. Used by multiple features. |
| Audit Trail | PLATFORM | Platform capability. Automatic entity change tracking. INSERT-only. |
| Outbox Pattern | PLATFORM | Infrastructure. Reliable event delivery. Background processing. |
| Background Jobs | PLATFORM | Infrastructure. Scheduled task execution. |
| Notifications | PLATFORM | Platform capability. User notifications. |

### SHELL / FUTURE — Domain Only, No Application Layer

| Item | Classification | Rationale | Entities | Events |
|------|----------------|-----------|----------|--------|
| Procurement | SHELL/FUTURE | 8 entities, 2 events, zero Application commands/queries/endpoints | PurchaseOrder, PurchaseRequest, Quotation, RFQ | 2 |
| Supplier Management | SHELL/FUTURE | 3 entities, zero Application layer, zero events | Supplier, SupplierStatus, SupplierType | 0 |
| Inventory Management | SHELL/FUTURE | 11 entities (largest domain-only module), zero Application layer | Item, Warehouse, StockTake, StockTransaction, etc. | 0 |
| Asset Management | SHELL/FUTURE | 9 entities, 5 events, zero Application layer | Asset, DepreciationSchedule, AssetDisposal, etc. | 5 |

### ARCHITECTURE INITIATIVES

| Item | Classification | Rationale |
|------|----------------|-----------|
| EF Migration Baseline | ARCHITECTURE INITIATIVE | Database schema baseline. Not a business feature. |
| Payment → Accounting Integration | ARCHITECTURE INITIATIVE | Cross-module event integration. PostingPipeline + domain events. |
| Budget Availability Model | ARCHITECTURE INITIATIVE | Budget checking mechanism. Not yet implemented. |
| Authorization Hardening | ARCHITECTURE INITIATIVE | Real RBAC wiring. Currently 231 open policies. |
| PostingPipeline Orphan Recovery | ARCHITECTURE INITIATIVE | Crash recovery for background processing. |

### NOT A FEATURE

| Item | Classification | Rationale |
|------|----------------|-----------|
| Chart of Accounts | NOT A FEATURE | Master Data within General Ledger. Configuration data, not operational function. Used by Journal Entries. |
| Budget Management | NOT A FEATURE | Budget entity is configuration data for Budget Appropriations. Not standalone. |
| Budget Items | SUB-FEATURE | Line-level detail within Budget Appropriations. Not standalone. |
| Budget Classifications | SUB-FEATURE | Classification taxonomy for budgets. Reference data. |
| Budget Types | SUB-FEATURE | Type taxonomy for budgets. Reference data. |
| Funds | SUB-FEATURE | Fund classification for budgets. Reference data. |
| Account Groups | SUB-FEATURE | Hierarchy taxonomy for accounts. Reference data. |
| Journal Entry Templates | SUB-FEATURE | Template for recurring entries. Not standalone. |

---

## 3. Platform Capabilities

| Platform Capability | Module | Entities | Purpose |
|---------------------|--------|----------|---------|
| Identity & Users | Security | User, UserSession, UserRole, UserPermission | User management, authentication |
| Authorization / RBAC | Security | SecurityRole, SecurityPermission, RolePermission, SoDMatrix, FieldSecurityPolicy, RecordRole | Role-based access control, separation of duties |
| Approval Rules | Security | ApprovalRule, ApprovalHistory, ApprovalDelegation | Approval workflow configuration |
| Audit Trail | Security | AuditTrail, SecurityAuditLog | Entity change tracking, security event logging |
| Workflow Engine | Workflow | WorkflowDefinition, WorkflowInstance, WorkflowStep, WorkflowHistory | Multi-step approval workflows |
| Outbox Pattern | Infrastructure | OutboxMessage | Reliable domain event delivery |
| Background Jobs | BackgroundJobs | BackgroundJobDefinition, BackgroundJobInstance, BackgroundJobExecutionLog | Scheduled task execution |
| Notifications | Security | Notification | User notification delivery |
| Automated Posting | Accounting | PostingRule, PostingRuleLine | Domain event → journal entry automation (AccountingEvent removed, DEP-026) |
| Document Numbering | FinancialSettings | DocumentSequence | Sequential document number generation |

---

## 4. Architecture Initiatives

| Initiative | Status | Description |
|------------|--------|-------------|
| EF Migration Baseline | IMPLEMENTED | Database schema baseline (SPEC-001 ef-migration-baseline) |
| Payment → Accounting Integration | PARTIALLY_IMPLEMENTED | PostingPipeline creates journal entries from PaymentOrder events. MoveLine generation deferred (FEATURE-027). |
| Budget Availability Model | NOT_STARTED | Budget checking before payment approval. Constitution Principle V requires this. |
| Authorization Hardening | NOT_STARTED | 231 open auth policies need real RBAC wiring. Constitution Principle VII requires this. |
| PostingPipeline Orphan Recovery | REMOVED (DEP-026) | AccountingEvents staging removed — pipeline writes JournalEntries directly. |
| Structured Logging | IMPLEMENTED | Correlation IDs across OutboxMessage → JournalEntry. |

---

## 5. Feature Dependency Map

```
Revenue Receipts
├── Journal Entries [REQUIRED]
├── Fiscal Years & Periods [REQUIRED]
├── Currencies & Exchange Rates [REQUIRED]
└── Document Numbering [REQUIRED]

Payment Orders
├── Budget Appropriations [REQUIRED]
├── Budget Commitments [REQUIRED]
├── Supplier Management [REFERENCE — Shell]
├── Fiscal Years & Periods [REQUIRED]
├── Currencies & Exchange Rates [REQUIRED]
└── Document Numbering [REQUIRED]

Payment Execution
├── Payment Orders [REQUIRED]
├── Bank Accounts [REFERENCE]
├── Fiscal Years & Periods [REQUIRED]
└── Currencies & Exchange Rates [REQUIRED]

Advance Payments
├── Payment Orders [REFERENCE]
├── Fiscal Years & Periods [REQUIRED]
└── Currencies & Exchange Rates [REQUIRED]

Bank Statements
├── Bank Accounts [REFERENCE]
└── Currencies & Exchange Rates [REQUIRED]

Bank Reconciliation
├── Bank Statements [REQUIRED]
├── Journal Entries [PROVIDER — creates entries]
└── Fiscal Years & Periods [REQUIRED]

Budget Appropriations
├── Funds [REFERENCE]
├── Budget Classifications [REFERENCE]
├── Fiscal Years & Periods [REQUIRED]
└── Currencies & Exchange Rates [REQUIRED]

Budget Commitments
├── Budget Appropriations [REQUIRED]
├── Funds [REFERENCE]
└── Fiscal Years & Periods [REQUIRED]

Budget Liquidations
├── Budget Commitments [REQUIRED]
├── Payment Orders [CONSUMER]
└── Fiscal Years & Periods [REQUIRED]

Journal Entries
├── Fiscal Years & Periods [REQUIRED]
├── Currencies & Exchange Rates [REQUIRED]
├── Document Numbering [REQUIRED]
└── Automated Posting [CONSUMER — creates entries]

Year-End Closing
├── Journal Entries [REQUIRED]
├── Fiscal Years & Periods [REQUIRED]
└── Account Balances [REQUIRED]

Automated Posting (Platform)
├── Journal Entries [PROVIDER — creates entries]
├── Posting Rules [CONFIGURATION]
└── Outbox Pattern [INFRASTRUCTURE]
```

### Dependency Types

| Type | Meaning |
|------|---------|
| REQUIRED | Must exist for feature to function |
| OPTIONAL | Enhances feature but not required |
| REFERENCE | Read-only reference data |
| CONSUMER | Feature consumes events/data from another |
| PROVIDER | Feature provides events/data to another |
| CONFIGURATION | Feature requires configuration from another |

---

## 6. Feature Ownership Matrix

| Feature | Owner | Write Authority | Read Dependencies | External Consumers |
|---------|-------|-----------------|-------------------|-------------------|
| FM-001 Journal Entries | Accountant | Accountant, Finance Manager | Fiscal Years, Currencies, Document Numbering | Automated Posting, Bank Reconciliation, Year-End Closing |
| FM-002 Budget Appropriations | Budget Officer | Budget Officer, Approver | Funds, Budget Classifications, Fiscal Years | Budget Commitments, Budget Liquidations |
| FM-003 Budget Commitments | Budget Officer | Budget Officer, Approver | Budget Appropriations, Funds | Budget Liquidations, Payment Orders |
| FM-004 Budget Liquidations | Budget Officer | Budget Officer, Approver | Budget Commitments, Payment Orders | — |
| FM-005 Payment Orders | Treasury Officer | Treasury Officer, Approver | Budget Appropriations, Suppliers, Fiscal Years | Payment Execution, Budget Liquidations |
| FM-006 Payment Execution | Treasury Officer | Treasury Officer, Approver | Payment Orders, Bank Accounts | — |
| FM-007 Advance Payments | Treasury Officer | Treasury Officer, Approver | Payment Orders, Fiscal Years | — |
| FM-008 Revenue Receipts | Revenue Officer | Revenue Officer, Approver | Fiscal Years, Currencies | Journal Entries (via events) |
| FM-009 Bank Statements | Bank Reconciliation Officer | Bank Reconciliation Officer | Bank Accounts, Currencies | Bank Reconciliation |
| FM-010 Bank Reconciliation | Bank Reconciliation Officer | Bank Reconciliation Officer | Bank Statements, Journal Entries | — |
| FM-011 Fiscal Years & Periods | Financial Admin | Financial Admin | — | All financial features |
| FM-012 Currencies & Exchange Rates | Financial Admin | Financial Admin | — | All financial features |
| FM-013 Year-End Closing | Financial Admin | Financial Admin | Journal Entries, Fiscal Years | — |
| OH-001 Organizational Units | HR Admin | HR Admin | — | Cost Centers, Projects |
| OH-002 Employee Management | HR Admin | HR Admin | Organizational Units | Committees |
| OH-003 Cost Centers | Financial Admin | Financial Admin | Organizational Units | Journal Entries (dimension) |
| OH-004 Projects | Financial Admin | Financial Admin | Organizational Units | Journal Entries (dimension) |
| GO-001 Committee Management | Committee Admin | Committee Admin | Employees | — |

---

## 7. Feature Status Matrix

| Feature | Status | Evidence |
|---------|--------|----------|
| FM-001 Journal Entries | IMPLEMENTED | 7 commands, 2 queries, 12 endpoints, 3 domain events, full lifecycle |
| FM-002 Budget Appropriations | IMPLEMENTED | 5 commands, 2 queries, partial endpoints, 2 domain events |
| FM-003 Budget Commitments | IMPLEMENTED | 5 commands, 2 queries, partial endpoints, 2 domain events |
| FM-004 Budget Liquidations | IMPLEMENTED | 5 commands, 2 queries, partial endpoints, 2 domain events |
| FM-005 Payment Orders | IMPLEMENTED | 7 commands, 2 queries, partial endpoints, 5 domain events |
| FM-006 Payment Execution | IMPLEMENTED | 4 commands, 2 queries, partial endpoints |
| FM-007 Advance Payments | IMPLEMENTED | 4 commands, 2 queries, partial endpoints, 2 domain events |
| FM-008 Revenue Receipts | IMPLEMENTED | 4 commands, 2 queries, partial endpoints, 3 domain events |
| FM-009 Bank Statements | IMPLEMENTED | 5 commands, 3 queries, partial endpoints, 1 domain event |
| FM-010 Bank Reconciliation | IMPLEMENTED | 5 commands, 3 queries, partial endpoints, 1 domain event |
| FM-011 Fiscal Years & Periods | IMPLEMENTED | 9 commands, 5 queries, near-full endpoints, 2 domain events |
| FM-012 Currencies & Exchange Rates | IMPLEMENTED | 8 commands, 6 queries, near-full endpoints, 2 domain events |
| FM-013 Year-End Closing | IMPLEMENTED | 3 commands, 2 queries, partial endpoints, 1 domain event |
| OH-001 Organizational Units | IMPLEMENTED | 3 commands, 2 queries, full CRUD endpoints |
| OH-002 Employee Management | IMPLEMENTED | 3 commands, 2 queries, full CRUD endpoints |
| OH-003 Cost Centers | IMPLEMENTED | 3 commands, 2 queries, full CRUD endpoints |
| OH-004 Projects | IMPLEMENTED | 3 commands, 2 queries, full CRUD endpoints |
| GO-001 Committee Management | IMPLEMENTED | 12 commands, 6 queries, partial endpoints, 1 domain event |
| Procurement | SHELL | 8 entities, 2 events, zero Application layer |
| Supplier Management | SHELL | 3 entities, zero Application layer |
| Inventory Management | SHELL | 11 entities, zero Application layer |
| Asset Management | SHELL | 9 entities, 5 events, zero Application layer |

---

## 8. Final Counts

| Category | Count |
|----------|-------|
| **Business Domains** | 3 |
| **Business Capabilities** | 8 |
| **Business Features** | 18 |
| **Platform Capabilities** | 10 |
| **Shell/Future Features** | 4 |
| **Architecture Initiatives** | 6 |
| **Sub-Features (not counted)** | 6 |

### Breakdown by Domain

| Domain | Capabilities | Features |
|--------|-------------|----------|
| Financial Management | 4 | 13 |
| Organization & HR | 3 | 4 |
| Government Operations | 1 | 1 (+ 4 Shell) |

### Breakdown by Capability

| Capability | Features |
|------------|----------|
| General Ledger | 1 (Journal Entries) |
| Budget Cycle | 3 (Appropriations, Commitments, Liquidations) |
| Payments | 3 (Payment Orders, Payment Execution, Advance Payments) |
| Revenue | 1 (Revenue Receipts) |
| Banking | 2 (Bank Statements, Bank Reconciliation) |
| Financial Configuration | 3 (Fiscal Years, Currencies, Year-End Closing) |
| Organization Structure | 1 (Organizational Units) |
| Workforce | 1 (Employee Management) |
| Financial Dimensions | 2 (Cost Centers, Projects) |
| Committees | 1 (Committee Management) |

---

## 9. Feature Registry

| Feature ID | Feature | Domain | Capability | Owner | Status | Module | Main Use Cases | Dependencies |
|------------|---------|--------|------------|-------|--------|--------|----------------|-------------|
| FM-001 | Journal Entries | Financial Management | General Ledger | Accountant | IMPLEMENTED | Accounting | Create, Submit, Approve, Post, Reverse, Cancel | Fiscal Years, Currencies, Document Numbering |
| FM-002 | Budget Appropriations | Financial Management | Budget Cycle | Budget Officer | IMPLEMENTED | Budgeting | Create, Submit, Approve | Funds, Budget Classifications, Fiscal Years |
| FM-003 | Budget Commitments | Financial Management | Budget Cycle | Budget Officer | IMPLEMENTED | Budgeting | Create, Approve, Release, Reverse | Budget Appropriations, Funds |
| FM-004 | Budget Liquidations | Financial Management | Budget Cycle | Budget Officer | IMPLEMENTED | Budgeting | Create, Approve, MarkPaid, Reverse | Budget Commitments, Payment Orders |
| FM-005 | Payment Orders | Financial Management | Payments | Treasury Officer | IMPLEMENTED | Payments | Create, Submit, Approve, Cancel, SendToTreasury, Void | Budget Appropriations, Suppliers, Fiscal Years |
| FM-006 | Payment Execution | Financial Management | Payments | Treasury Officer | IMPLEMENTED | Payments | Create, Approve, Send, Complete | Payment Orders, Bank Accounts |
| FM-007 | Advance Payments | Financial Management | Payments | Treasury Officer | IMPLEMENTED | Payments | Create, Approve, Consume, Cancel | Payment Orders, Fiscal Years |
| FM-008 | Revenue Receipts | Financial Management | Revenue | Revenue Officer | IMPLEMENTED | Revenue | Create, Approve, Post, Cancel | Fiscal Years, Currencies |
| FM-009 | Bank Statements | Financial Management | Banking | Bank Reconciliation Officer | IMPLEMENTED | Banking | Create, Cancel, Import, Reconcile | Bank Accounts, Currencies |
| FM-010 | Bank Reconciliation | Financial Management | Banking | Bank Reconciliation Officer | IMPLEMENTED | Banking | Create, Approve, Complete, Reject | Bank Statements, Journal Entries |
| FM-011 | Fiscal Years & Periods | Financial Management | Financial Configuration | Financial Admin | IMPLEMENTED | FinancialSettings | Create, Open, Close, Lock, Unlock | — |
| FM-012 | Currencies & Exchange Rates | Financial Management | Financial Configuration | Financial Admin | IMPLEMENTED | FinancialSettings | Create, Activate, Deactivate | — |
| FM-013 | Year-End Closing | Financial Management | Financial Configuration | Financial Admin | IMPLEMENTED | FinancialSettings | GenerateClosingEntries, CloseYear | Journal Entries, Fiscal Years |
| OH-001 | Organizational Units | Organization & HR | Organization Structure | HR Admin | IMPLEMENTED | Organization | Create, Update, Delete | — |
| OH-002 | Employee Management | Organization & HR | Workforce | HR Admin | IMPLEMENTED | Organization | Create, Update, Delete | Organizational Units |
| OH-003 | Cost Centers | Organization & HR | Financial Dimensions | Financial Admin | IMPLEMENTED | Organization | Create, Update, Delete | Organizational Units |
| OH-004 | Projects | Organization & HR | Financial Dimensions | Financial Admin | IMPLEMENTED | Organization | Create, Update, Delete | Organizational Units |
| GO-001 | Committee Management | Government Operations | Committees | Committee Admin | IMPLEMENTED | Committees | Create, Update, Activate, Deactivate, Dissolve, AddMember, RemoveMember, CreateAssignment | Employees |
| — | Procurement | Government Operations | Procurement | — | SHELL | Procurement | — | — |
| — | Supplier Management | Government Operations | Suppliers | — | SHELL | Suppliers | — | — |
| — | Inventory Management | Government Operations | Inventory | — | SHELL | Inventory | — | — |
| — | Asset Management | Government Operations | Assets | — | SHELL | Assets | — | — |

---

## 10. Evidence Summary

| Evidence Type | Count | Source |
|---------------|-------|--------|
| Domain Entities | 105 | Verified in src/Domain/*/Entities/ |
| Application Commands | 165 | Verified in src/Application/*/Commands/ |
| Application Queries | 95 | Verified in src/Application/*/Queries/ |
| Web Endpoints | 53 files | Verified in src/Web/Endpoints/ |
| Domain Events | 32 | Verified in src/Domain/Events/ |
| Seed Data | 12 PostingRules | Verified in PostingRuleSeedData.cs |

### Confidence Levels

| Classification | Confidence | Basis |
|----------------|------------|-------|
| FEATURE (18 items) | HIGH | Full Application layer + Endpoints + Tests |
| PLATFORM (10 items) | HIGH | Verified infrastructure code |
| SHELL/FUTURE (4 items) | HIGH | Domain entities exist, no Application layer |
| ARCHITECTURE INITIATIVE (6 items) | HIGH | Verified in codebase or Constitution |

---

**End of Feature Architecture Map v1.0**
