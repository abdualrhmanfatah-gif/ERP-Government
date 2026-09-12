# Feature Specification: Budget Preparation — BudgetItemAllocations Model

**Feature Branch**: `046-budget-ledger-redesign`

**Created**: 2026-09-10

**Status**: Draft (Amended — supersedes prior Budget Ledger-Like Model)

**Input**: User description: "عدّل مواصفات الميزات القائمة المتأثرة دون إنشاء مواصفات مكررة، لدعم إعداد الموازنة عبر جدول «مخصصات بنود الموازنة» BudgetItemAllocations المرتبط بـ Budgets، ويحتوي على BudgetItemId وProposedAmount وApprovedAmount وRemarks وحقول التدقيق وRowVersion، مع منع تكرار البند داخل الموازنة. دورة إعداد الموازنة: مسودة ← قيد المراجعة، مع إمكانية الإعادة بالملاحظات، ← معتمدة ← مرحّلة، وتُثبّت المبالغ عند الاعتماد. اربط أوامر الصرف بمخصصات البنود في المستوى الذي يحمل بند الصرف فعليًا وفق النموذج الحالي. احسب المتبقي من المخصص بالمعادلة: المبلغ المخصص المعتمد للبند − ما صُرف فعليًا، ويُستخرج المصروف الفعلي من سطور القيود اليومية المرحّلة عبر الحساب المرتبط ببند الموازنة، مع مراعاة السنة المالية وصافي الأثر المدين والدائن والإلغاءات والقيود العكسية وفق طبيعة الحساب وقواعد الترحيل القائمة. لا تستخدم صافي BudgetTransactions أو مبالغ أوامر الصرف بوصفها مصدرًا للمصروف الفعلي، ولا تخزّن الرصيد المحسوب. تحقّق من علاقة البنود بالحسابات، وعالج اشتراك عدة بنود في حساب واحد إن كان مسموحًا بما يضمن إسناد المصروف إلى البند الصحيح دون تكرار احتسابه. اجعل كل سجل في BudgetTransactions خاصًا بمخصص بند واحد ومرتبطًا به عبر BudgetItemAllocationId، ويحتوي على نوع العملية وتاريخها ومبلغها واتجاهها وحالتها ومرجعها، واستغنِ عن BudgetTransactionLines. حدّد أثر العمليات التي تغيّر المبلغ المخصص على القيمة المعتمدة في BudgetItemAllocations بصورة ذرّية وقابلة للتدقيق، دون احتساب العملية مرتين أو السماح بتعديل المبلغ المعتمد خارج دورة عمل معتمدة. ألغِ المناقلة بين البنود بالكامل ونوع Transfer وواجهاته ومنطقه، دون إعادة ترقيم قيم أنواع العمليات الأخرى المخزنة. استخدم ApprovalHistory وDocumentStatusLog لتسجيل الاعتماد وانتقالات الحالة، وصحّح العمليات المرحّلة بالعكس المرتبط بالأصل دون تعديلها أو حذفها. حافظ على ضوابط الارتباط والصرف القائمة مع التمييز بين المتبقي بعد الصرف الفعلي والمتاح بعد الالتزامات، ومنع خصم المصروف نفسه مرتين. حدّث واجهات إعداد الموازنة والمخصصات والمعاملات وأوامر الصرف بالعربية وRTL والعقود والتوثيق المتأثر. راجع الكيانات والمواصفات الحالية وروابط بنود الموازنة بالحسابات والقيود اليومية قبل تحديد تفاصيل التنفيذ، وحدّد ترحيلًا يحفظ البيانات والمراجع والأثر التدقيق للعمليات وأوامر الصرف السابقة."

## Clarifications

### Session 2026-09-10

- Q: What prevents duplicate BudgetItemAllocations for the same item within a budget? → A: UNIQUE constraint on (BudgetId, BudgetItemId) in the database. Server-side validation before insert. Error message: "هذا البند مسجل بالفعل في الموازنة".
- Q: When is the ApprovedAmount in BudgetItemAllocations frozen? → A: At the moment the budget reaches Approved status. No further modifications to ProposedAmount or ApprovedAmount are permitted outside the Approved workflow. Draft can be edited; UnderReview allows resubmission with notes; Approved freezes amounts.
- Q: How does the system handle multiple BudgetItemAllocations sharing the same GL account? → A: Shared accounts within the same budget/fiscal year are a setup conflict prevented at allocation creation/update time. Each GL account is linked to at most one BudgetItem with an allocation within the same budget and fiscal year. When separation is needed, independent detailed accounts must be used.
- Q: Should BudgetTransactions retain the TransactionType enum values (InitialAppropriation=0, Supplement=1, etc.) or redefine them? → A: Keep existing enum values and their stored int codes. Remove Transfer=3 from the enum but do NOT renumber remaining values. Add new values at the end if needed. This preserves existing database rows.
- Q: What is the exact lifecycle for BudgetItemAllocations? → A: Managed through the Budget lifecycle. Budget status Draft → Submitted (Under Review) → Approved → Active (Posted). In Draft, allocations can be added/edited/deleted. On submit, allocations are reviewed. On approve, ApprovedAmount = ProposedAmount for each allocation, amounts are frozen. On post (activate), allocations become active and budget transactions can reference them.
- Q: When a BudgetItemAllocation has linked BudgetTransactions, should deletion be blocked even if the budget is in Draft status? → A: Yes — block deletion if any BudgetTransaction references the allocation (BudgetItemAllocationId), regardless of budget status. The user must reverse or delete linked transactions before deleting the allocation.
- Q: When a JournalEntryLine lacks a BudgetItemId and multiple allocations share the same GL account, how should the system handle the unattributed expenditure? → A: Expenditure is attributed via the GL account linked to the BudgetItem — BudgetItemId on JournalEntryLine is not required. ActualExpenditure is computed from net posted JournalEntryLines on the account within the fiscal year. Sharing the same GL account across multiple allocations within the same budget/fiscal year IS a setup conflict that must be prevented at allocation creation/update time. When separation is needed, independent detailed accounts must be used.
- Q: Can new BudgetItemAllocations be added to a budget after it reaches Active (Posted) status? → A: Yes — new allocations can be added via a Supplement BudgetTransaction. The Supplement follows the standard lifecycle and is subject to budget control checks. On posting, the new allocation's ApprovedAmount is set to the Supplement amount.
- Q: What happens to BudgetItemAllocations when a Budget is cancelled or closed? → A: Close vs Cancel are distinguished. Close: allocations and expenditure records are preserved, new operations are blocked, no automatic reversal, posted transactions remain. Cancel: allowed ONLY if no posted transactions or outstanding commitments exist; otherwise blocked until settled via approved procedures. Queries and reports remain accessible in both states.
- Q: When a BudgetTransaction with Direction=Decrease is posted, should the system check that the allocation's ApprovedAmount does not go negative? → A: Yes — block posting if decrease would drive ApprovedAmount below zero. ApprovedAmount must always be >= 0.

## User Scenarios & Testing

### User Story 1 - Create Budget with Item Allocations (Priority: P1)

A budget officer creates a Budget for a fiscal year, adds budget items with proposed amounts through the BudgetItemAllocations table, and progresses the budget through its preparation lifecycle. Each allocation records a proposed amount for a specific budget item, and duplicates within the same budget are prevented.

**Why this priority**: This is the foundational capability. Without allocations, no budget amounts exist and no downstream transactions (appropriations, encumbrances, payments) can be gated.

**Independent Test**: Create a Budget in Draft, add BudgetItemAllocations with proposed amounts, verify uniqueness constraint, submit for review, approve (amounts frozen), and verify that post-approval edits are rejected.

**Acceptance Scenarios**:

1. **Given** a Budget in Draft status with BudgetItems defined, **When** a budget officer creates a BudgetItemAllocation for BudgetItem "ITM-001" with ProposedAmount 500,000, **Then** the allocation is saved with ProposedAmount 500,000 and ApprovedAmount null.
2. **Given** a BudgetItemAllocation for BudgetItem "ITM-001" already exists, **When** the officer attempts to create another allocation for the same item, **Then** the system rejects with "هذا البند مسجل بالفعل في الموازنة".
3. **Given** a Budget in Draft with allocations, **When** the officer edits ProposedAmount on an allocation, **Then** the change is permitted and ApprovedAmount remains null.
4. **Given** a Budget in Draft with allocations, **When** the officer deletes an allocation, **Then** the allocation is removed.
5. **Given** a Budget in Draft, **When** submitted, **Then** status transitions to Submitted (Under Review) and allocations become read-only.
6. **Given** a Budget in Submitted, **When** an approver approves it, **Then** status transitions to Approved, for each allocation ApprovedAmount = ProposedAmount, and amounts are frozen. Approval is recorded in ApprovalHistory and DocumentStatusLog.
7. **Given** a Budget in Submitted, **When** an approver rejects with notes, **Then** status transitions back to Draft with the rejection reason and notes visible, and the officer can edit allocations and resubmit.

---

### User Story 2 - Link Disbursement Orders to Allocations (Priority: P1)

A payment order is linked to a specific BudgetItemAllocation at the expenditure level. The system computes remaining budget per allocation as ApprovedAmount minus actual expenditure from posted journal entries.

**Why this priority**: Budget control requires knowing how much of each allocation has been spent. Without the link, budget availability cannot be enforced at the allocation level.

**Independent Test**: Create a payment order linked to a BudgetItemAllocation, post it (generating journal entries), and verify that the remaining amount decreases by the net expenditure amount.

**Acceptance Scenarios**:

1. **Given** a BudgetItemAllocation with ApprovedAmount 500,000, **When** a payment order is created and linked to this allocation, **Then** the allocation reference is stored on the payment order.
2. **Given** a payment order linked to allocation A (ApprovedAmount 500,000) with Posted journal entries totaling 200,000 net debit, **When** remaining amount is computed, **Then** RemainingAmount = 500,000 − 200,000 = 300,000.
3. **Given** a BudgetItemAllocation, **When** multiple payment orders are linked and their journal entries are posted, **Then** the remaining amount equals ApprovedAmount minus the sum of all net expenditures across all linked posted journal entries.
4. **Given** a payment order linked to an allocation, **When** the payment order is cancelled or voided, **Then** the journal entries are reversed and the remaining amount is restored.

---

### User Story 3 - Compute Actual Expenditure from Journal Entries (Priority: P1)

Actual expenditure for a BudgetItemAllocation is derived from posted JournalEntryLines through the GL account linked to the BudgetItem. The computation considers fiscal year, net debit/credit effects, cancellations, and reversals.

**Why this priority**: This is the core financial integrity mechanism. Actual expenditure must be accurate and traceable to the ledger — never from stored snapshots or payment order amounts.

**Independent Test**: Create journal entries with debit/credit lines against the linked account, post them, reverse some, cancel some, and verify the actual expenditure computation matches the expected net.

**Acceptance Scenarios**:

1. **Given** a BudgetItemAllocation linked to BudgetItem with AccountId=100, **When** a posted JournalEntry exists with debit 50,000 on AccountId=100 in the same fiscal year, **Then** actual expenditure for this allocation is 50,000.
2. **Given** the same allocation, **When** another posted JournalEntry has credit 10,000 on AccountId=100, **Then** actual expenditure is 50,000 − 10,000 = 40,000.
3. **Given** a posted JournalEntry with debit 50,000, **When** it is reversed (new reversing JournalEntry posted), **Then** the reversal's debit/credit effects cancel the original, and actual expenditure returns to the pre-reversal amount.
4. **Given** a posted JournalEntry with debit 50,000, **When** it is cancelled, **Then** it is excluded from the actual expenditure computation entirely.
5. **Given** two BudgetItemAllocations sharing the same GL account (AccountId=100), **When** a JournalEntryLine is posted against AccountId=100, **Then** the expenditure is attributed to the allocation whose BudgetItem has the matching AccountId, and is NOT double-counted across allocations.
6. **Given** a BudgetItemAllocation with no linked BudgetItem AccountId, **When** actual expenditure is queried, **Then** the system returns 0 with a note that no GL account is linked.

---

### User Story 4 - BudgetTransaction Per Allocation (Priority: P1)

Each BudgetTransaction record is linked to exactly one BudgetItemAllocation via BudgetItemAllocationId. The transaction records the operation type, date, amount, direction, status, and reference. BudgetTransactionLines are removed.

**Why this priority**: Simplifies the transaction model — each transaction affects exactly one allocation, eliminating multi-line complexity and ensuring atomic impact on the allocation's ApprovedAmount.

**Independent Test**: Create a BudgetTransaction linked to an allocation, verify it affects only that allocation's ApprovedAmount, and verify that BudgetTransactionLines table is not used.

**Acceptance Scenarios**:

1. **Given** a BudgetItemAllocation with ApprovedAmount 500,000, **When** a BudgetTransaction of type Supplement with Amount 100,000 and direction Increase is posted against this allocation, **Then** the allocation's ApprovedAmount becomes 600,000 atomically.
2. **Given** the same allocation, **When** a BudgetTransaction of type Reduction with Amount 50,000 and direction Decrease is posted, **Then** the allocation's ApprovedAmount becomes 550,000.
3. **Given** a BudgetTransaction linked to allocation A, **When** the transaction is posted, **Then** only allocation A's ApprovedAmount is affected; other allocations remain unchanged.
4. **Given** a BudgetTransaction, **When** queried, **Then** it carries BudgetItemAllocationId, TransactionType, TransactionDate, Amount, Direction, Status, and DocumentType/DocumentId reference.
5. **Given** the BudgetTransactionType enum, **When** Transfer=3 is encountered in existing data, **Then** the system preserves the stored value but no new Transfer transactions can be created. The UI does not expose Transfer as an option.

---

### User Story 5 - Reverse Posted Transactions (Priority: P2)

A budget officer reverses a previously posted BudgetTransaction. The reversal creates a new transaction of type Reversal with inverted direction and amount. The original transaction's status changes to Reversed. The original is never modified or deleted.

**Why this priority**: Corrections to posted transactions must follow financial integrity principles — reversing entries, not mutations.

**Independent Test**: Post a transaction, reverse it, verify the allocation's ApprovedAmount is restored and the original transaction is untouched.

**Acceptance Scenarios**:

1. **Given** a Posted BudgetTransaction with Amount 100,000 Increase on allocation A, **When** a reversal is requested with reason, **Then** a new BudgetTransaction of type Reversal is created with Amount 100,000 Decrease, linked to the same allocation A, and referencing the original via ReversalOfId.
2. **Given** the reversal is posted, **Then** the original transaction's status becomes Reversed, and the allocation's ApprovedAmount reflects the net effect.
3. **Given** a Reversed transaction, **When** a reversal of the reversal is attempted, **Then** it is rejected — only original Posted transactions can be reversed.
4. **Given** a reversal BudgetTransaction, **When** its ApprovalHistory is queried, **Then** the reversal decision is recorded with actor, timestamp, and reason.

---

### User Story 6 - Budget Preparation Workflow (Priority: P1)

The budget preparation lifecycle governs when allocations can be edited, when amounts are frozen, and when transactions can reference allocations. The workflow is: Draft → Submitted (Under Review) → Approved → Active (Posted). Rejection from Submitted returns to Draft with notes.

**Why this priority**: The workflow ensures proper authorization and prevents unauthorized changes to budget amounts.

**Independent Test**: Walk a budget through every lifecycle state; verify allocation editability, amount freezing, and transaction eligibility at each state.

**Acceptance Scenarios**:

1. **Given** a Budget in Draft, **When** allocations are added/edited/deleted, **Then** changes are permitted and ApprovedAmount remains null.
2. **Given** a Budget in Draft, **When** submitted, **Then** status becomes Submitted, allocations are read-only, and the submission is recorded in ApprovalHistory.
3. **Given** a Budget in Submitted, **When** approved, **Then** status becomes Approved, for each allocation ApprovedAmount = ProposedAmount, amounts are frozen, and the approval is recorded in ApprovalHistory and DocumentStatusLog.
4. **Given** a Budget in Submitted, **When** rejected with notes, **Then** status returns to Draft, the rejection reason and notes are visible, and allocations become editable again for resubmission.
5. **Given** a Budget in Approved, **When** activated (posted), **Then** status becomes Active, and BudgetTransactions can reference the allocations.
6. **Given** a Budget in Active, **When** a BudgetTransaction is created against an allocation, **Then** the allocation must exist and be active.

---

### User Story 7 - Existing Controls Preserved (Priority: P2)

Budget control (None/Warning/Blocking), AllowOverrun inheritance, encumbrance linkage, and availability computation are preserved with the new allocation model. The distinction between remaining after actual expenditure and available after commitments is maintained.

**Why this priority**: Existing budget control mechanisms must continue to work with the new allocation-based model.

**Independent Test**: Configure Blocking control, attempt to create a transaction that exceeds availability, verify rejection. Configure Warning, verify override is logged. Verify encumbrance availability uses allocation amounts.

**Acceptance Scenarios**:

1. **Given** a BudgetItemAllocation with ApprovedAmount 500,000 and actual expenditure 450,000, **When** a BudgetTransaction of type Supplement with Amount 100,000 is attempted, **Then** the system computes remaining as 500,000 − 450,000 = 50,000 and blocks the supplement if ControlMethod is Blocking.
2. **Given** ControlMethod Warning, **When** the same supplement is attempted, **Then** it is allowed with a warning logged to AuditTrail.
3. **Given** an encumbrance linked to a BudgetItem, **When** availability is computed, **Then** the system uses the allocation's ApprovedAmount as the basis, minus actual expenditure, minus outstanding encumbrances.
4. **Given** AllowOverrun inheritance chain (BudgetItem → Budget → BudgetType), **When** availability is checked, **Then** the first non-null value in the chain is used.
5. **Given** a BudgetItemAllocation, **When** remaining amount is queried, **Then** RemainingAmount = ApprovedAmount − ActualExpenditure (from journal entries).
6. **Given** the same allocation, **When** available amount is queried, **Then** AvailableAmount = RemainingAmount − OutstandingEncumbrance.

---

### Edge Cases

- What happens when a BudgetItemAllocation references a BudgetItem that has been deactivated? → The system MUST reject the allocation with "البند غير نشط".
- What happens when a Budget is closed? → Allocations and their expenditure records are preserved (historical). New operations (BudgetTransactions, PaymentOrders) referencing those allocations are blocked. No automatic reversal is triggered. Posted transactions remain as-is — closing does not cancel actual expenditure. Pending operations (Draft/Submitted transactions) must be resolved before closing. Queries and reports remain accessible.
- What happens when a Budget is cancelled? → Cancellation is permitted ONLY if no posted BudgetTransactions exist against its allocations AND no outstanding encumbrances are linked. If transactions or commitments exist, cancellation is blocked with "لا يمكن إلغاء الموازنة لوجود حركات مرحّلة أو التزامات قائمة". The user must settle or reverse them through approved procedures first. Queries and reports remain accessible.
- What happens when two allocations share the same GL account and a journal entry is posted against that account? → This is a setup conflict. System MUST prevent linking the same GL account to multiple budget items with independent allocations within the same budget and fiscal year. If separation is needed, independent detailed accounts must be used. The system rejects the allocation with "الحساب مرتبط ببند آخر في هذه الموازنة".
- What happens when a Budget is rejected from Submitted and resubmitted? → Allocations become editable in Draft, the officer can adjust ProposedAmount, and resubmission triggers a new review cycle.
- What happens when a Posted BudgetTransaction's original is reversed? → The reversal creates a new transaction; the original is never modified; the allocation's ApprovedAmount is restored atomically.
- What happens when a payment order is linked to an allocation but the allocation's BudgetItem has no AccountId? → The system MUST reject the linkage with "البند لا يرتبط بحساب في الدفتر العام".
- What happens when a BudgetTransaction is created against an allocation in a Budget that is not yet Active? → The system MUST reject with "الموازنة غير مفعلة".
- What happens when a cancellation of a journal entry would drive the allocation's remaining amount negative? → The system MUST allow the cancellation (corrections are always permitted) and the remaining amount may go negative temporarily; the ControlMethod check applies only to new transactions, not corrections.

## Requirements

### Functional Requirements

#### BudgetItemAllocations Entity

- **FR-001**: System MUST provide a BudgetItemAllocations table with fields: Id (int PK), BudgetId (int FK → Budgets, required), BudgetItemId (int FK → BudgetItems, required), ProposedAmount (decimal(23,2), required), ApprovedAmount (decimal(23,2), nullable — set at approval), Remarks (string(1000), nullable), audit fields (CreatedAt, CreatedBy, LastModifiedAt, LastModifiedBy), RowVersion (byte[], concurrency token).
- **FR-002**: System MUST enforce a UNIQUE constraint on (BudgetId, BudgetItemId) to prevent duplicate budget items within the same budget.
- **FR-003**: System MUST validate that BudgetItem belongs to the specified Budget before creating an allocation.
- **FR-004**: System MUST validate that BudgetItem.IsActive is true before creating or updating an allocation.
- **FR-005**: In Draft status, ProposedAmount and Remarks MUST be editable. ApprovedAmount MUST be null.
- **FR-006**: On budget approval, ApprovedAmount MUST be set to ProposedAmount for each allocation atomically. No further edits to ProposedAmount or ApprovedAmount are permitted after approval.
- **FR-007**: BudgetItemAllocations MUST be deletable only while the budget is in Draft status AND no BudgetTransactions reference the allocation (BudgetItemAllocationId). If any linked transactions exist, deletion MUST be rejected with "لا يمكن حذف المخصص لوجود معاملات مرتبطة به". The user must reverse or delete linked transactions first.
- **FR-007a**: New BudgetItemAllocations CAN be added to a budget in Active status via a Supplement BudgetTransaction. The Supplement transaction follows the standard lifecycle (Draft → Submitted → Approved → Posted) and is subject to budget control checks (None/Warning/Blocking). On posting, the new allocation's ApprovedAmount is set to the Supplement amount, and it becomes active for expenditure tracking.

#### Budget Preparation Workflow

- **FR-008**: Budget lifecycle MUST follow: Draft → Submitted (Under Review) → Approved → Active (Posted). Submitted → Rejected returns to Draft with notes.
- **FR-009**: Every lifecycle transition MUST be recorded in ApprovalHistory with actor, decision, timestamp, and evaluation snapshot (Constitution Principle VIII).
- **FR-010**: Every lifecycle transition MUST be recorded in DocumentStatusLog with from-status, to-status, actor, and timestamp.
- **FR-011**: When a budget is rejected from Submitted, the rejection reason and any reviewer notes MUST be stored and visible when the budget returns to Draft.
- **FR-012**: Resubmission from Draft after rejection MUST create a new Submitted entry in the approval history, preserving the full audit trail.

#### BudgetTransactions (Per-Allocation Model)

- **FR-013**: System MUST modify BudgetTransaction to include BudgetItemAllocationId (int FK → BudgetItemAllocations, required) instead of using BudgetTransactionLines.
- **FR-014**: Each BudgetTransaction MUST be linked to exactly one BudgetItemAllocation. The transaction contains: TransactionNumber (BTR-NNNNNN), BudgetItemAllocationId, TransactionType, TransactionDate, Amount (decimal(23,2)), Direction (Increase/Decrease), Status (Draft → Submitted → Approved → Posted), DocumentType, DocumentId, Description, ReversalOfId, ReversalReason, audit fields, RowVersion.
- **FR-015**: System MUST remove BudgetTransactionLines table entirely. BudgetTransactionLines data MUST be migrated to BudgetTransaction records during the schema migration.
- **FR-016**: On posting a BudgetTransaction, the linked BudgetItemAllocation's ApprovedAmount MUST be updated atomically: increase for Direction=Increase, decrease for Direction=Decrease. The update MUST be part of the same transaction as the status change. System MUST reject posting if the decrease would drive ApprovedAmount below zero, with "المبلغ المعتمد لا يمكن أن يصبح سالبًا".
- **FR-017**: System MUST prevent double-counting: a BudgetTransaction can only be posted once. Idempotent posting attempts MUST be rejected.
- **FR-018**: Reversal BudgetTransactions MUST be linked to the original via ReversalOfId and MUST NOT modify the original transaction. The reversal inverts Direction and Amount.

#### Transfer Removal

- **FR-019**: System MUST remove Transfer (value=3) from the BudgetTransactionType enum as a createable option. The stored int value 3 MUST be preserved in existing database rows to avoid renumbering. No new Transfer transactions can be created through the API or UI.
- **FR-020**: The UI MUST NOT present Transfer as an available operation type. Existing Transfer transactions in the database MUST remain queryable and displayable.
- **FR-021**: Fund movement between budget items that was previously handled by Transfer MUST use a combination of Reduction (decrease source) and Supplement (increase target) transactions against separate allocations.

#### Actual Expenditure Computation

- **FR-022**: System MUST compute ActualExpenditure for a BudgetItemAllocation as: SUM(JournalEntryLine.Debit) − SUM(JournalEntryLine.Credit) WHERE JournalEntryLine.AccountId = BudgetItem.AccountId AND JournalEntry.FiscalYearId = Budget.FiscalYearId AND JournalEntry.EntryStatus = Posted AND JournalEntry.ReversalOfId IS NULL. Cancelled entries (EntryStatus = Cancelled) and reversed entries (ReversalOfId IS NOT NULL) are excluded.
- **FR-023**: System MUST NOT use BudgetTransaction net amounts or PaymentOrder amounts as the source for ActualExpenditure. Actual expenditure comes exclusively from posted journal entry lines.
- **FR-024**: System MUST NOT store a computed balance for ActualExpenditure or RemainingAmount. These MUST be computed at query time from current journal entry data.
- **FR-025**: System MUST prevent linking the same GL account (BudgetItem.AccountId) to multiple budget items that have independent allocations within the same budget and fiscal year. ActualExpenditure is computed from net posted JournalEntryLines on the account linked to the BudgetItem. When separation of expenditure tracking is needed, independent detailed accounts MUST be used. Shared accounts within the same budget/fiscal year are a setup conflict that MUST be prevented at allocation creation/update time, not handled by runtime attribution logic.
- **FR-026**: Reversed JournalEntries (where ReversalOfId is set) MUST be excluded from the expenditure computation. Cancelled JournalEntries (EntryStatus = Cancelled) MUST also be excluded.

#### Remaining and Available Amount

- **FR-027**: System MUST compute RemainingAmount for a BudgetItemAllocation as: ApprovedAmount − ActualExpenditure.
- **FR-028**: System MUST compute AvailableAmount for a BudgetItemAllocation as: RemainingAmount − OutstandingEncumbrance.
- **FR-029**: OutstandingEncumbrance MUST be computed as: SUM(EncumbranceLine.Amount − EncumbranceLine.LiquidatedAmount − EncumbranceLine.CancelledAmount) WHERE EncumbranceLine.BudgetItemId = allocation.BudgetItemId AND Encumbrance.Status IN (Active, PartiallyReleased, PartiallyLiquidated) AND Encumbrance.ReversalOfId IS NULL.
- **FR-030**: Budget control (None/Warning/Blocking) MUST be evaluated against AvailableAmount, not RemainingAmount. ControlMethod is resolved from BudgetItem.ControlMethod ?? Budget.ControlMethod ?? BudgetType.ControlMethod.

#### PaymentOrder Linkage

- **FR-031**: System MUST add BudgetItemAllocationId (int FK → BudgetItemAllocations, nullable) to PaymentOrder. This replaces the existing BudgetItemId direct link for budget-gated payments.
- **FR-032**: When a PaymentOrder is linked to a BudgetItemAllocation, the system MUST validate that the allocation exists, is active, and belongs to an Active budget.
- **FR-033**: On payment completion (journal entry posted), the system MUST verify that the allocation's AvailableAmount is sufficient. If ControlMethod is Blocking, the payment MUST be rejected.
- **FR-034**: The existing BudgetItemId on PaymentOrder MUST be retained for backward compatibility but the primary budget control path goes through BudgetItemAllocationId.

#### Approval and Audit

- **FR-035**: All BudgetItemAllocation lifecycle changes (create, update, delete) MUST be recorded in the audit trail with actor, timestamp, and before/after values.
- **FR-036**: ApprovalHistory MUST record: DocumentType "BudgetItemAllocation" or "Budget", DocumentId, ActorId, Decision (Approved/Rejected/Submitted), DecisionAt, Notes (for rejections).
- **FR-037**: DocumentStatusLog MUST record: DocumentType, DocumentId, FromStatus, ToStatus, ActorId, TransitionAt, Notes.

#### Reversals and Corrections

- **FR-038**: Posted BudgetTransactions MUST only be corrected by reversal. The original transaction MUST NOT be modified or deleted.
- **FR-039**: Reversal transactions MUST carry ReversalOfId pointing to the original and ReversalReason.
- **FR-040**: Posted JournalEntries MUST only be corrected by reversing entries. The original journal entry MUST NOT be modified or deleted (Constitution Principle IV).

#### Frontend

- **FR-041**: Budget preparation screens MUST display: budget list with status, allocation grid (item code, item name, proposed amount, approved amount, remarks, remaining, available), lifecycle action buttons (Submit, Approve, Reject, Activate) enabled per status, approval history panel, document status log.
- **FR-042**: Allocation grid in Draft MUST allow inline editing of ProposedAmount and Remarks. In Submitted+, fields MUST be read-only.
- **FR-043**: All screens MUST render in Arabic-first RTL with logical CSS properties (ms-/me-, ps-/pe-), design-token-only styling, dark mode support, and loading/empty/error states.
- **FR-044**: Budget transaction screens MUST show: transaction list with filters (type, status, date range, allocation), create form (type, amount, direction, allocation picker), lifecycle actions, linked allocation details.
- **FR-045**: Payment order screens MUST show the linked BudgetItemAllocation reference and remaining amount when viewing an order.

### Key Entities

- **BudgetItemAllocations** (NEW): Represents the budgeted amount for a specific budget item within a budget. Fields: BudgetId, BudgetItemId, ProposedAmount, ApprovedAmount (frozen at approval), Remarks, audit, RowVersion. UNIQUE(BudgetId, BudgetItemId). The central entity for budget preparation and control.
- **Budget**: Fiscal year budget container. Lifecycle governs allocation editability. Fields: BudgetNumber, BudgetName, BudgetTypeId, FiscalYearId, FundId, Status, EffectiveFrom, EffectiveTo, AllowOverrun, Description, audit, RowVersion.
- **BudgetItem**: Line item within a budget, forming a tree. Fields: ItemCode, ItemName, BudgetId, ParentId, AccountId (GL link), CostCenterId, BudgetClassificationId, ControlMethod (per-item override), AllowOverrun, IsActive, Remarks, audit, RowVersion.
- **BudgetTransaction**: Independent ledger entry per allocation. Fields: TransactionNumber, BudgetItemAllocationId, TransactionType, TransactionDate, Amount, Direction, Status, DocumentType, DocumentId, ReversalOfId, ReversalReason, audit, RowVersion. Replaces BudgetTransaction + BudgetTransactionLine.
- **BudgetTransactionLines** (REMOVED): Entirely removed. Migrated to BudgetTransaction records.
- **Encumbrance**: Fund reservation against BudgetItems. Fields: EncumbranceNumber, EncumbranceType, VendorPartyId, PurchaseOrderId, EncumbranceDate, TotalAmount, Status, ReversalOfId, audit, RowVersion.
- **EncumbranceLine**: Per-item encumbrance line. Fields: EncumbranceId, BudgetItemId, Amount, LiquidatedAmount, CancelledAmount, Description, RowVersion.
- **PaymentOrder**: Disbursement order. New field: BudgetItemAllocationId (FK → BudgetItemAllocations). Retains BudgetItemId for backward compatibility.
- **BudgetType**: Defines control regime. Fields: Code, Name, ControlMethod, AllowOverrun, IsActive.
- **Fund**: Financial fund. Fields: FundNumber, FundName, FundType, FundCategory, LegalAuthority, DefaultRevenueAccountId, CurrencyId, IsActive.
- **BudgetClassification**: Hierarchical classification. Fields: Code, Name, ParentId, ClassificationLevel, IsActive.

## Success Criteria

### Measurable Outcomes

- **SC-001**: Budget officers can create a budget with 10+ allocations in under 5 minutes.
- **SC-002**: Duplicate allocations within the same budget are rejected 100% of the time.
- **SC-003**: Amount freezing on approval is atomic — all allocations in a budget receive ApprovedAmount = ProposedAmount in a single transaction.
- **SC-004**: Actual expenditure computation from journal entries matches expected values to two decimal places for all test scenarios including reversals and cancellations.
- **SC-005**: Remaining amount = ApprovedAmount − ActualExpenditure is computed at query time with no stored balance.
- **SC-006**: Budget control (Blocking) rejects transactions that would exceed AvailableAmount within 2 seconds.
- **SC-007**: Transfer is not available as a createable transaction type; existing Transfer rows remain queryable.
- **SC-008**: Every lifecycle transition is recorded in both ApprovalHistory and DocumentStatusLog with complete audit metadata.
- **SC-009**: Reversal transactions correctly invert original direction and amount, and the original is never modified.
- **SC-010**: All budget preparation, allocation, transaction, and payment screens render correctly in Arabic RTL with design tokens.
- **SC-011**: Linking the same GL account to multiple budget items with independent allocations within the same budget/fiscal year is rejected 100% of the time with a clear error message.
- **SC-012**: Migration preserves existing BudgetTransaction data, BudgetItemAllocation data is created from migrated BudgetTransactionLines, and no historical references are lost.

## Assumptions

- BudgetItemAllocations is a new table added to the existing budgeting schema; it does not replace any existing table.
- Existing BudgetTransaction and BudgetTransactionLines data will be migrated: each BudgetTransactionLine becomes a BudgetTransaction record with the appropriate BudgetItemAllocationId resolved from BudgetItemId.
- The BudgetItem.AccountId link to the GL Account is already established and is the primary mechanism for journal entry attribution.
- Each GL account (BudgetItem.AccountId) is linked to at most one BudgetItem with an allocation within the same budget and fiscal year. Shared accounts across multiple allocations within the same budget/fiscal year are a setup conflict prevented by the system.
- The existing approval workflow engine handles budget approval routing; this spec defines the allocation lifecycle but not the approval rule evaluation details.
- Frontend language remains Arabic-only with RTL layout; no localization files or language switchers are introduced.
- Existing budget data is preserved through migration; this is not a clean-slate redesign for BudgetItemAllocations.
- Document numbering continues to use BTR for BudgetTransactions, ENC for Encumbrances, PO for PaymentOrders via the document sequence service.
- The Transfer enum value (3) is preserved in the database but no longer usable for new transactions; no renumbering occurs.
- JournalEntryLine does not currently carry a BudgetItemId; attribution will be based on AccountId matching plus any available dimensional data, or a new BudgetItemId FK may be added to JournalEntryLine in a future spec.
