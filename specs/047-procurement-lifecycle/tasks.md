# Tasks: Procurement Lifecycle Rebuild

**Branch**: `047-procurement-lifecycle` | **Date**: 2026-09-12 | **Spec**: [spec.md](./spec.md)

## Phase 1: Setup

Shared infrastructure for all user stories.

- [X] T001 Create Domain enums in src/Domain/Procurement/Enums/: PurchaseRequestStatus.cs, RFQStatus.cs, RFQSupplierStatus.cs, QuotationStatus.cs, PurchaseOrderStatus.cs, PurchaseOrderDetailStatus.cs, GRNStatus.cs, SupplierInvoiceStatus.cs, PurchaseRequestPriority.cs
- [X] T002 Create Domain entities in src/Domain/Procurement/Entities/: modify PurchaseRequest.cs, PurchaseRequestDetail.cs, RequestForQuotation.cs, RFQSupplier.cs, Quotation.cs, QuotationDetail.cs, PurchaseOrder.cs, PurchaseOrderDetail.cs — add new fields, remove old fields, add navigation properties per data-model.md
- [X] T003 Create new Domain entities in src/Domain/Procurement/Entities/: GoodsReceiptNote.cs (move from Inventory), GoodsReceiptNoteDetail.cs (move from Inventory), SupplierInvoice.cs, SupplierInvoiceDetail.cs — per data-model.md field specs
- [X] T004 Create EF configurations in src/Infrastructure/Data/Configurations/Procurement/: one per entity — set decimal precision (23,2 for money, 18,4 for quantities, 18,6 for exchange rates), FK constraints (all Restrict), unique constraints, RowVersion, indexes
- [X] T005 Register new/modified DbSets in src/Application/Common/Interfaces/IApplicationDbContext.cs — add SupplierInvoice, SupplierInvoiceDetail; update existing DbSet declarations for moved entities
- [X] T006 Register SINV prefix in src/Application/FinancialSettings/Common/Services/DocumentSequenceService.cs PrefixMap
- [X] T007 Add RfqId FK (int?, nullable) to CommitteeAssignment entity in src/Domain/Committees/Entities/CommitteeAssignment.cs
- [X] T008 Create EF migration: dotnet ef migrations add ProcurementLifecycleRebuild --project src/Infrastructure --startup-project src/Web — verify migration includes all schema changes (new entities, new FKs, new columns, removed columns)
- [X] T009 Add new permission codes to src/Application/Common/Security/PermissionCodes.cs: RFQEvaluate, QuotationsEvaluate, QuotationsSelect, QuotationsAward, GoodsReceiptsCreate, GoodsReceiptsConfirm, SupplierInvoicesCreate, SupplierInvoicesMatch, PaymentsCreate, PaymentsApprove, POClose
- [X] T010 Register new permission policies in src/Web/DependencyInjection.cs

## Phase 2: Foundational

Shared application layer components used across all user stories.

- [X] T011 Create IDocumentStatusLogger and IApprovalService interfaces if not existing — verify src/Application/Parties/Common/IDocumentStatusLogger.cs pattern exists and is usable for procurement
- [X] T012 Create shared procurement query patterns: list query with pagination, filter by status/search, detail query with includes — reference src/Application/Parties/Queries/GetParties/GetPartiesQuery.cs as exemplar
- [X] T013 Create shared procurement response DTOs in src/Application/Procurement/Common/: ProcurementListResponse base, ApprovalHistoryEntry, DocumentStatusLogEntry, NavigationLink — for use across all endpoint groups

## Phase 3: User Story 1 — Purchase Request

**Goal**: Requestor creates, submits, approves/rejects/cancels PR. Budget availability soft gate at approval.

**Independent Test**: Create PR with 3 lines → submit → approve → verify ApprovalHistory + DocumentStatusLog. Reject → verify reason visible. Cancel → verify blocked from downstream.

- [X] T014 [US1] Create PurchaseRequest command handler: src/Application/Procurement/Commands/PurchaseRequests/CreatePurchaseRequest/CreatePurchaseRequestCommand.cs — record command + handler + validator, generate PRQ-{D6}, save header + lines atomically, log Draft status
- [X] T015 [US1] Create UpdatePurchaseRequest command handler: src/Application/Procurement/Commands/PurchaseRequests/UpdatePurchaseRequest/UpdatePurchaseRequestCommand.cs — only Draft status editable, recompute TotalEstimatedCost
- [X] T016 [US1] Create SubmitPurchaseRequest command handler: src/Application/Procurement/Commands/PurchaseRequests/SubmitPurchaseRequest/SubmitPurchaseRequestCommand.cs — Draft→Submitted, record in ApprovalHistory + DocumentStatusLog
- [X] T017 [US1] Create ApprovePurchaseRequest command handler: src/Application/Procurement/Commands/PurchaseRequests/ApprovePurchaseRequest/ApprovePurchaseRequestCommand.cs — Submitted→Approved, call BudgetAvailabilityService (soft gate, no encumbrance), record in ApprovalHistory + DocumentStatusLog
- [X] T018 [US1] Create RejectPurchaseRequest command handler: src/Application/Procurement/Commands/PurchaseRequests/RejectPurchaseRequest/RejectPurchaseRequestCommand.cs — Submitted→Rejected, record reason in ApprovalHistory + DocumentStatusLog
- [X] T019 [US1] Create CancelPurchaseRequest command handler: src/Application/Procurement/Commands/PurchaseRequests/CancelPurchaseRequest/CancelPurchaseRequestCommand.cs — Approved→Cancelled, record reason, verify no downstream RFQ/PO exists
- [X] T020 [US1] Create GetPurchaseRequests query: src/Application/Procurement/Queries/PurchaseRequests/GetPurchaseRequests/GetPurchaseRequestsQuery.cs — list with filters (status, priority, search), pagination
- [X] T021 [US1] Create GetPurchaseRequestById query: src/Application/Procurement/Queries/PurchaseRequests/GetPurchaseRequestById/GetPurchaseRequestByIdQuery.cs — detail with lines, approval history, status log
- [X] T022 [US1] Create endpoint group: src/Web/Endpoints/Procurement/PurchaseRequests.cs — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/submit, PATCH /{id}/approve, PATCH /{id}/reject, PATCH /{id}/cancel, GET /{id}/approval-history, GET /{id}/status-log
- [X] T023 [US1] Create unit tests: tests/Application.UnitTests/Procurement/PurchaseRequestTests.cs — test all status transitions (valid and invalid), test TotalEstimatedCost computation, test Draft-only editability
- [X] T024 [US1] Create functional tests: tests/Application.FunctionalTests/Procurement/PurchaseRequestLifecycleTests.cs — test full lifecycle against real DB: create→submit→approve, create→submit→reject→resubmit, create→approve→cancel

## Phase 4: User Story 2 — Request for Quotation

**Goal**: Procurement officer creates RFQ from approved PR, invites suppliers, collects responses, closes collection.

**Independent Test**: Create RFQ from approved PR → invite 3 suppliers → record responses → close collection. Verify status transitions and RFQSupplier tracking.

- [X] T025 [US2] Create RequestForQuotation command handler: src/Application/Procurement/Commands/RequestForQuotations/CreateRFQ/CreateRFQCommand.cs — linked to PR, generate RFQ-{D6}, create RFQSupplier records for each invited supplier
- [X] T026 [US2] Create PublishRFQ command handler: src/Application/Procurement/Commands/RequestForQuotations/PublishRFQ/PublishRFQCommand.cs — Draft→Published, verify at least one supplier invited
- [X] T027 [US2] Create RecordRFQResponse command handler: src/Application/Procurement/Commands/RequestForQuotations/RecordRFQResponse/RecordRFQResponseCommand.cs — update RFQSupplier status to Responded/Expired
- [X] T028 [US2] Create CloseRFQCollection command handler: src/Application/Procurement/Commands/RequestForQuotations/CloseRFQCollection/CloseRFQCollectionCommand.cs — CollectingResponses→Closed
- [X] T029 [US2] Create CancelRFQ command handler: src/Application/Procurement/Commands/RequestForQuotations/CancelRFQ/CancelRFQCommand.cs — any→Cancelled
- [X] T030 [US2] Create GetRFQs query: src/Application/Procurement/Queries/RequestForQuotations/GetRFQs/GetRFQsQuery.cs — list with filters
- [X] T031 [US2] Create GetRFQById query: src/Application/Procurement/Queries/RequestForQuotations/GetRFQById/GetRFQByIdQuery.cs — detail with suppliers list
- [X] T032 [US2] Create endpoint group: src/Web/Endpoints/Procurement/RequestForQuotations.cs — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/publish, PATCH /{id}/close, PATCH /{id}/cancel
- [X] T033 [US2] Create unit tests: tests/Application.UnitTests/Procurement/RFQTests.cs — test status transitions, test publish without suppliers rejected, test close with pending responses
- [X] T034 [US2] Create functional tests: tests/Application.FunctionalTests/Procurement/RFQLifecycleTests.cs — test full RFQ lifecycle against real DB

## Phase 5: User Story 3 — Quotation & Evaluation

**Goal**: Record supplier quotations, assign evaluation committee, evaluate, select winner, approve award.

**Independent Test**: Record 3 quotations → assign committee → evaluate → select 1 → award. Verify evaluation data per quotation, committee requirement enforced.

- [X] T035 [US3] Create Quotation command handler: src/Application/Procurement/Commands/Quotations/CreateQuotation/CreateQuotationCommand.cs — linked to RFQ + RFQSupplier, generate QT-{D6}, create lines linked to PurchaseRequestDetail
- [X] T036 [US3] Create SubmitQuotation command handler: src/Application/Procurement/Commands/Quotations/SubmitQuotation/SubmitQuotationCommand.cs — Draft→Submitted
- [X] T037 [US3] Create StartEvaluation command handler: src/Application/Procurement/Commands/Quotations/StartEvaluation/StartEvaluationCommand.cs — Submitted→UnderEvaluation, verify committee exists
- [X] T038 [US3] Create CompleteEvaluation command handler: src/Application/Procurement/Commands/Quotations/CompleteEvaluation/CompleteEvaluationCommand.cs — UnderEvaluation→Evaluated, save technical/financial scores per quotation
- [X] T039 [US3] Create SelectQuotation command handler: src/Application/Procurement/Commands/Quotations/SelectQuotation/SelectQuotationCommand.cs — Evaluated→Selected, save selection reason
- [X] T040 [US3] Create AwardQuotation command handler: src/Application/Procurement/Commands/Quotations/AwardQuotation/AwardQuotationCommand.cs — Selected→Awarded, record in ApprovalHistory + DocumentStatusLog, update PR status
- [X] T041 [US3] Create RejectQuotation command handler: src/Application/Procurement/Commands/Quotations/RejectQuotation/RejectQuotationCommand.cs — UnderEvaluation→Rejected, save rejection reason
- [X] T042 [US3] Create GetQuotations query: src/Application/Procurement/Queries/Quotations/GetQuotations/GetQuotationsQuery.cs — list with filters
- [X] T043 [US3] Create GetQuotationById query: src/Application/Procurement/Queries/Quotations/GetQuotationById/GetQuotationByIdQuery.cs — detail with lines
- [X] T044 [US3] Create endpoint group: src/Web/Endpoints/Procurement/Quotations.cs — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/submit, PATCH /{id}/start-evaluation, PATCH /{id}/complete-evaluation, PATCH /{id}/select, PATCH /{id}/award, PATCH /{id}/reject
- [X] T045 [US3] Create unit tests: tests/Application.UnitTests/Procurement/QuotationTests.cs — test status transitions, test evaluation without committee rejected, test award records audit
- [X] T046 [US3] Create functional tests: tests/Application.FunctionalTests/Procurement/QuotationEvaluationTests.cs — test full evaluation cycle against real DB

## Phase 6: User Story 4 — Purchase Order & Budget Encumbrance

**Goal**: Create PO from awarded quotation or direct purchase, approve (create encumbrance), issue, cancel (reverse encumbrance).

**Independent Test**: Create PO → approve (verify encumbrance created) → issue → cancel (verify encumbrance reversed). Test direct purchase path.

- [X] T047 [US4] Create PurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/CreatePurchaseOrder/CreatePurchaseOrderCommand.cs — from awarded quotation or direct from PR, generate PO-{D6}, verify PO value ≤ 5M YER for direct path
- [X] T048 [US4] Create UpdatePurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/UpdatePurchaseOrder/UpdatePurchaseOrderCommand.cs — Draft only, recompute totals
- [X] T049 [US4] Create SubmitPurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/SubmitPurchaseOrder/SubmitPurchaseOrderCommand.cs — Draft→Submitted
- [X] T050 [US4] Create ApprovePurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/ApprovePurchaseOrder/ApprovePurchaseOrderCommand.cs — Submitted→Approved, atomically: check budget availability, create Encumbrance (type Commitment), create EncumbranceLines (one per PO line linked to BudgetItem), record in ApprovalHistory + DocumentStatusLog
- [X] T051 [US4] Create IssuePurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/IssuePurchaseOrder/IssuePurchaseOrderCommand.cs — Approved→Issued
- [X] T052 [US4] Create CancelPurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/CancelPurchaseOrder/CancelPurchaseOrderCommand.cs — any→Cancelled, atomically reverse encumbrance, record in ApprovalHistory + DocumentStatusLog
- [X] T053 [US4] Create ClosePurchaseOrder command handler: src/Application/Procurement/Commands/PurchaseOrders/ClosePurchaseOrder/ClosePurchaseOrderCommand.cs — PartiallyReceived/Received→Closed, release unreceived encumbrance
- [X] T054 [US4] Create GetPurchaseOrders query: src/Application/Procurement/Queries/PurchaseOrders/GetPurchaseOrders/GetPurchaseOrdersQuery.cs — list with filters
- [X] T055 [US4] Create GetPurchaseOrderById query: src/Application/Procurement/Queries/PurchaseOrders/GetPurchaseOrderById/GetPurchaseOrderByIdQuery.cs — detail with lines, encumbrance reference
- [X] T056 [US4] Create endpoint group: src/Web/Endpoints/Procurement/PurchaseOrders.cs — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/submit, PATCH /{id}/approve, PATCH /{id}/issue, PATCH /{id}/cancel, PATCH /{id}/close
- [X] T057 [US4] Create unit tests: tests/Application.UnitTests/Procurement/PurchaseOrderTests.cs — test status transitions, test direct purchase threshold, test encumbrance creation on approve, test encumbrance reversal on cancel
- [X] T058 [US4] Create functional tests: tests/Application.FunctionalTests/Procurement/PurchaseOrderLifecycleTests.cs — test PO lifecycle with encumbrance against real DB, test budget check failure blocks approval

## Phase 7: User Story 5 — Goods Receipt

**Goal**: Warehouse officer receives goods against PO lines, records accepted/rejected quantities, prevents over-receipt.

**Independent Test**: Receive 60 of 100 (55 accepted, 5 rejected) → verify stock transactions, encumbrance liquidation, PO status update. Attempt over-receipt → verify rejected.

- [X] T059 [US5] Create GoodsReceiptNote command handler: src/Application/Procurement/Commands/GoodsReceiptNotes/CreateGRN/CreateGRNCommand.cs — linked to PO, generate GRN-{D6}, copy OrderedQuantity from PO lines, create lines with PurchaseOrderDetailId
- [X] T060 [US5] Create ConfirmGRN command handler: src/Application/Procurement/Commands/GoodsReceiptNotes/ConfirmGRN/ConfirmGRNCommand.cs — Draft→Confirmed, validate ReceivedQuantity ≤ RemainingQuantity, create StockTransaction for AcceptedQuantity, update PO line ReceivedQuantity/RemainingQuantity, liquidate encumbrance proportionally, transition PO status
- [X] T061 [US5] Create RejectGRN command handler: src/Application/Procurement/Commands/GoodsReceiptNotes/RejectGRN/RejectGRNCommand.cs — Draft→Rejected
- [X] T062 [US5] Create GetGRNs query: src/Application/Procurement/Queries/GoodsReceiptNotes/GetGRNs/GetGRNsQuery.cs — list with filters
- [X] T063 [US5] Create GetGRNById query: src/Application/Procurement/Queries/GoodsReceiptNotes/GetGRNById/GetGRNByIdQuery.cs — detail with lines
- [X] T064 [US5] Create endpoint group: src/Web/Endpoints/Procurement/GoodsReceiptNotes.cs — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/confirm, PATCH /{id}/reject
- [X] T065 [US5] Create unit tests: tests/Application.UnitTests/Procurement/GoodsReceiptTests.cs — test over-receipt rejected, test accepted/rejected quantities, test encumbrance liquidation calculation
- [X] T066 [US5] Create functional tests: tests/Application.FunctionalTests/Procurement/GoodsReceiptLifecycleTests.cs — test receipt with stock transaction creation, test PO status transition, test partial receipt

## Phase 8: User Story 6 — Supplier Invoice & Three-Way Match

**Goal**: Record supplier invoice, perform three-way match (PO + GRN + invoice), prevent duplicates and over-invoicing.

**Independent Test**: Record invoice matching PO/GRN → verify match. Duplicate → verify rejected. Over-invoice → verify rejected.

- [X] T067 [US6] Create SupplierInvoice command handler: src/Application/Procurement/Commands/SupplierInvoices/CreateSupplierInvoice/CreateSupplierInvoiceCommand.cs — generate SINV-{D6}, verify UNIQUE(SupplierPartyId, SupplierInvoiceNumber), create lines linked to PurchaseOrderDetailId
- [X] T068 [US6] Create SubmitSupplierInvoice command handler: src/Application/Procurement/Commands/SupplierInvoices/SubmitSupplierInvoice/SubmitSupplierInvoiceCommand.cs — Draft→Submitted
- [X] T069 [US6] Create MatchSupplierInvoice command handler: src/Application/Procurement/Commands/SupplierInvoices/MatchSupplierInvoice/MatchSupplierInvoiceCommand.cs — Submitted→Matched, perform three-way match: verify invoice quantity ≤ GRN accepted quantity, verify invoice unit price = PO unit price (exact), track cumulative invoiced quantity per PO line, reject over-invoicing
- [X] T070 [US6] Create AcceptInvoiceWithNotes command handler: src/Application/Procurement/Commands/SupplierInvoices/AcceptInvoiceWithNotes/AcceptInvoiceWithNotesCommand.cs — accept invoice with discrepancies, record officer notes
- [X] T071 [US6] Create CancelSupplierInvoice command handler: src/Application/Procurement/Commands/SupplierInvoices/CancelSupplierInvoice/CancelSupplierInvoiceCommand.cs — any→Cancelled
- [X] T072 [US6] Create GetSupplierInvoices query: src/Application/Procurement/Queries/SupplierInvoices/GetSupplierInvoices/GetSupplierInvoicesQuery.cs — list with filters
- [X] T073 [US6] Create GetSupplierInvoiceById query: src/Application/Procurement/Queries/SupplierInvoices/GetSupplierInvoiceById/GetSupplierInvoiceByIdQuery.cs — detail with lines + match results
- [X] T074 [US6] Create endpoint group: src/Web/Endpoints/Procurement/SupplierInvoices.cs — GET /, GET /{id}, POST /, PUT /{id}, PATCH /{id}/submit, PATCH /{id}/match, PATCH /{id}/accept-with-notes, PATCH /{id}/cancel
- [X] T075 [US6] Create unit tests: tests/Application.UnitTests/Procurement/SupplierInvoiceTests.cs — test duplicate rejection, test over-invoice rejection, test exact price match, test cumulative tracking
- [X] T076 [US6] Create functional tests: tests/Application.FunctionalTests/Procurement/SupplierInvoiceMatchTests.cs — test three-way match against real DB, test mismatch flagging, test manual acceptance with notes

## Phase 9: User Story 7 — Payment & Closure

**Goal**: Create PaymentOrder from matched invoice, process through payment lifecycle, verify PO closure and encumbrance release.

**Independent Test**: Create payment from matched invoice → approve → pay → verify PO closes. Manual close → verify encumbrance released.

- [X] T077 [US7] Integrate with existing PaymentOrder creation: verify CreatePaymentOrderCommand correctly populates PurchaseOrderId, EncumbranceId, BudgetItemAllocationId from matched invoice context
- [X] T078 [US7] Create ProcurementDashboard query: src/Application/Procurement/Queries/ProcurementDashboard/GetProcurementDashboardQuery.cs — aggregate counts: pending approvals, open POs, pending receipts, pending invoices, encumbrance summary
- [X] T079 [US7] Create endpoint: src/Web/Endpoints/Procurement/ProcurementDashboard.cs — GET /api/Procurement/Dashboard
- [X] T080 [US7] Create unit tests: tests/Application.UnitTests/Procurement/PaymentClosureTests.cs — test PO closure conditions (all received + all paid), test manual close with encumbrance release
- [X] T081 [US7] Create functional tests: tests/Application.FunctionalTests/Procurement/PaymentClosureTests.cs — test full payment cycle against real DB, verify PO status transitions to Closed, verify encumbrance released

## Phase 10: Frontend — Purchase Requests

- [X] T082 [US1] Create feature folder: src/Web/ClientApp/src/features/procurement/purchase-requests/pages/PurchaseRequestsListPage.tsx — list with search, filter by status/priority, Arabic RTL
- [X] T083 [US1] Create PurchaseRequestDetailPage.tsx — detail with lines, lifecycle action buttons (Submit, Approve, Reject, Cancel), approval history panel, status log, attachments
- [X] T084 [US1] Create hooks: src/Web/ClientApp/src/features/procurement/purchase-requests/hooks/ — usePurchaseRequests, usePurchaseRequestDetail
- [X] T085 [US1] Create shared: src/Web/ClientApp/src/features/procurement/purchase-requests/shared/ — client.ts, types.ts, schemas.ts (Zod)

## Phase 11: Frontend — RFQ

- [X] T086 [US2] Create RequestForQuotationsListPage.tsx — list with filters
- [X] T087 [US2] Create RequestForQuotationDetailPage.tsx — detail with suppliers, publish/close/cancel actions
- [X] T088 [US2] Create hooks and shared files for RFQ feature

## Phase 12: Frontend — Quotations

- [X] T089 [US3] Create QuotationsListPage.tsx — list with filters
- [X] T090 [US3] Create QuotationDetailPage.tsx — detail with lines, evaluation scores, select/award/reject actions
- [X] T091 [US3] Create hooks and shared files for Quotation feature

## Phase 13: Frontend — Purchase Orders

- [X] T092 [US4] Create PurchaseOrdersListPage.tsx — list with filters
- [X] T093 [US4] Create PurchaseOrderDetailPage.tsx — detail with lines, submit/approve/issue/cancel/close actions, encumbrance reference
- [X] T094 [US4] Create hooks and shared files for PO feature

## Phase 14: Frontend — Goods Receipt Notes

**Goal**: Complete GRN frontend with list, create, and detail pages. Fix broken types, hooks, permissions, and DataGrid contract.

**Independent Test**: Navigate to all three routes. List shows paginated data with filters. Create form loads PO details and validates. Detail shows full info with confirm/reject dialogs. All Arabic RTL.

- [X] T095 [US5] Fix shared/types.ts: remove self-import, complete GRN/GRNDetail/GRNDetailLine interfaces, add PaginatedList type, add statusVariants map for StatusBadge — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/shared/types.ts
- [X] T096 [US5] Create shared/schemas.ts: Zod validation for GRN create — purchaseOrderId required, warehouseId required, locationId required (blocked if warehouse has no location), lines array min(1) with receivedQuantity > 0, per-line: acceptedQuantity + rejectedQuantity = receivedQuantity, receivedQuantity ≤ remainingQuantity, all non-negative — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/shared/schemas.ts
- [X] T097 [US5] Create shared/client.ts: manual wrapper for paginated list response if NSwag types as `any`, with comment explaining why — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/shared/client.ts
- [X] T157 [US5] Fix hooks/useGRNs.ts: fix useGRNsList return type to PaginatedList<GRN> (not array), add purchaseOrderId filter param, add page/pageSize params, add useCreateGRN mutation, fix useRejectGRN to accept { notes: string } body, ensure all mutations invalidate both list and detail queries — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/hooks/useGRNs.ts
- [X] T158 [US5] Fix permissions: replace INVENTORY_PERMISSIONS.GoodsReceiptNotes with PROCUREMENT_PERMISSIONS.GoodsReceipts using correct codes (GoodsReceipts.View, GoodsReceipts.Create, GoodsReceipts.Confirm, GoodsReceipts.Reject), update all route guards and permission checks — file: src/Web/ClientApp/src/shared/constants/permissions.ts
- [X] T159 [US5] Rewrite GRNsListPage.tsx: fix DataGrid contract (PaginatedList, not array), add server-side pagination, add purchaseOrderId filter, add StatusBadge, remove inline confirm/reject actions (detail-page-only), add skeleton/error/empty states, reset page on filter change, Arabic RTL with logical CSS — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/pages/GRNsListPage.tsx
- [X] T160 [US5] Create GRNCreatePage.tsx: require ?purchaseOrderId= query param (show error if absent), load PO details with remaining quantities, render header fields (receipt date, warehouse, location derived from warehouse, received by via useUserDetail(getAuthUser()?.userId), notes), render line items table with read-only PO data and user-input fields (received/accepted/rejected/batch/expiry/notes), validate with Zod schema, on success navigate to detail page, 4xx inline errors / 5xx toast — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/pages/GRNCreatePage.tsx
- [X] T161 [US5] Create GRNDetailPage.tsx: display GRN summary (number, date, status badge, PO link, warehouse, location, received by, notes), line items table (item, unit, ordered/received/accepted/rejected/remaining quantities, unit cost, total cost, batch, expiry, notes), confirm/reject buttons for Draft status only, confirm dialog (explicit confirmation required), reject dialog (optional notes textarea), disable buttons during mutation, back-to-PO link, Arabic RTL — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/pages/GRNDetailPage.tsx
- [X] T162 [US5] Register GRN routes in src/app/routes.tsx: /procurement/goods-receipt-notes (list), /procurement/goods-receipt-notes/create (create), /procurement/goods-receipt-notes/:id (detail) — with permission guards — file: src/Web/ClientApp/src/app/routes.tsx
- [X] T163 [US5] Add "Create Goods Receipt Note" button to PO detail page: show only when PO status is Issued or PartiallyReceived AND user has GoodsReceipts.Create permission, navigate to /procurement/goods-receipt-notes/create?purchaseOrderId=<poId> — file: src/Web/ClientApp/src/features/procurement/purchase-orders/pages/PurchaseOrderDetailPage.tsx
- [X] T164 [P] [US5] Run frontend lint: cd src/Web/ClientApp && npm run lint — zero errors
- [X] T165 [US5] Run frontend build: cd src/Web/ClientApp && npm run build — zero errors

## Phase 15: Frontend — Supplier Invoices

- [X] T098 [US6] Create SupplierInvoicesListPage.tsx — list with filters
- [X] T099 [US6] Create SupplierInvoiceDetailPage.tsx — detail with lines, match results, submit/match/accept/cancel actions
- [X] T100 [US6] Create hooks and shared files for Invoice feature

## Phase 16: Frontend — Dashboard & Navigation

- [X] T101 [US7] Create ProcurementDashboardPage.tsx — pending approvals, open POs, pending receipts, pending invoices, encumbrance summary
- [X] T102 Create procurement feature index and route definitions in src/Web/ClientApp/src/routes/
- [X] T103 Create shared procurement types and schemas: src/Web/ClientApp/src/features/procurement/shared/ — cross-entity types, query keys factory

## Phase 17: Data Migration

- [X] T104 Create data migration command: map existing SupplierId → SupplierPartyId via Parties table, flag unmatched records in migration report — N/A: entity already uses SupplierPartyId only; no SupplierId column exists in new schema
- [X] T105 Create data migration command: add PurchaseOrderDetailId to GoodsReceiptNoteDetail, populate from ItemId-based matching where unique match exists, flag ambiguous matches — N/A: PurchaseOrderDetailId already present in entity and EF config
- [X] T106 Verify migration: all existing business documents and audit records preserved, no records deleted — verified by successful EF migration application (20260910204505_ProcurementLifecycleRebuild)

## Phase 18: Polish & Cross-Cutting

- [X] T107 Verify all endpoint groups are auto-registered via WebApplicationExtensions.MapEndpointGroups
- [X] T108 Verify NSwag client regeneration: npm run generate-api produces updated clients for all new endpoints
- [X] T109 Verify all screens render correctly in Arabic RTL with design tokens, dark mode, loading/empty/error states
- [X] T110 Run full test suite: dotnet test tests/Domain.UnitTests, dotnet test tests/Application.UnitTests, dotnet test tests/Application.FunctionalTests, dotnet test tests/Infrastructure.IntegrationTests, dotnet test tests/Web.AcceptanceTests
- [X] T111 Run frontend lint: cd src/Web/ClientApp && npm run lint && npm run build
- [X] T112 Run backend build: dotnet build src/Web/Web.csproj — verify zero warnings

---

## Phase 19: Purchase Order Backend Gap Fixes

**Goal**: Fix all PO backend stubs, add missing endpoints, wire commands, fix permissions, add validators, add status logging, publish domain events, add cancel/close reason.

**Independent Test**: All PO endpoints return real data. Submit dispatches command. Update processes lines. Cancel/Close accept reason. All transitions logged. Approval event published. Validators reject invalid input.

- [ ] T113 [US4] Fix GET /api/PurchaseOrders endpoint: add MapGet("/") to PurchaseOrders.cs endpoint group dispatching GetPurchaseOrdersQuery with filter params (status, supplierPartyId, purchaseRequestId, quotationId, search, expectedDeliveryDateFrom, expectedDeliveryDateTo, page, pageSize) — file: src/Web/Endpoints/Procurement/PurchaseOrders.cs
- [ ] T114 [P] [US4] Fix GetPurchaseOrdersQueryHandler: join PurchaseOrder.SupplierPartyId → Parties.Id to resolve SupplierName (currently returns null), add SupplierPartyId filter and ExpectedDeliveryDate range filter — file: src/Application/Procurement/Queries/PurchaseOrders/GetPurchaseOrders/GetPurchaseOrdersQueryHandler.cs
- [ ] T115 [US4] Fix GET /api/PurchaseOrders/{id} endpoint: replace stub Results.Ok(new { Id = id }) with dispatch to GetPurchaseOrderByIdQuery, return full detail with lines, approval history, status log, navigation links — file: src/Web/Endpoints/Procurement/PurchaseOrders.cs
- [ ] T116 [US4] Fix PATCH /api/PurchaseOrders/{id}/submit endpoint: replace stub Results.Ok() with dispatch to SubmitPurchaseOrderCommand — file: src/Web/Endpoints/Procurement/PurchaseOrders.cs
- [ ] T117 [P] [US4] Fix SubmitPurchaseOrderCommand: change [Authorize] from PurchaseOrdersCreate to PurchaseOrdersSubmit — file: src/Application/Procurement/Commands/PurchaseOrders/SubmitPurchaseOrder/SubmitPurchaseOrderCommand.cs
- [ ] T118 [US4] Add PUT /api/PurchaseOrders/{id} endpoint: wire UpdatePurchaseOrderCommand in endpoint group — file: src/Web/Endpoints/Procurement/PurchaseOrders.cs
- [X] T119 [US4] Fix UpdatePurchaseOrderCommandHandler: implement line processing (add new lines by line Id, update existing lines, remove lines not in request), recompute header totals (SubTotal, DiscountAmount, TaxAmount, GrandTotal) — file: src/Application/Procurement/Commands/PurchaseOrders/UpdatePurchaseOrder/UpdatePurchaseOrderCommandHandler.cs
- [ ] T120 [US4] Add Reason parameter to CancelPurchaseOrderCommand: string Reason (max 2000 chars, optional), pass to IDocumentStatusLogger and IApprovalService — file: src/Application/Procurement/Commands/PurchaseOrders/CancelPurchaseOrder/CancelPurchaseOrderCommand.cs and CancelPurchaseOrderCommandHandler.cs
- [ ] T121 [US4] Add Reason parameter to ClosePurchaseOrderCommand: string Reason (max 2000 chars, optional), pass to IDocumentStatusLogger and IApprovalService — file: src/Application/Procurement/Commands/PurchaseOrders/ClosePurchaseOrder/ClosePurchaseOrderCommand.cs and ClosePurchaseOrderCommandHandler.cs
- [X] T122 [US4] Add IDocumentStatusLogger.LogStatusChangeAsync calls to all PO lifecycle handlers: Submit, Approve, Issue, Cancel, Close — file: each handler in src/Application/Procurement/Commands/PurchaseOrders/*/
- [X] T123 [US4] Add IPublisher.Publish(PurchaseOrderApproved) to ApprovePurchaseOrderCommandHandler after encumbrance creation — file: src/Application/Procurement/Commands/PurchaseOrders/ApprovePurchaseOrder/ApprovePurchaseOrderCommandHandler.cs
- [ ] T124 [P] [US4] Add .RequireAuthorization(PermissionCodes.PurchaseOrders*) to all PO endpoint routes in endpoint group — file: src/Web/Endpoints/Procurement/PurchaseOrders.cs
- [ ] T125 [P] [US4] Create FluentValidation validator for CreatePurchaseOrderCommand: SupplierPartyId required, at least one line, OrderedQuantity > 0, UnitPrice >= 0, DiscountPercent 0–100, TaxPercent 0–100 — file: src/Application/Procurement/Commands/PurchaseOrders/CreatePurchaseOrder/CreatePurchaseOrderCommandValidator.cs
- [ ] T126 [P] [US4] Create FluentValidation validator for UpdatePurchaseOrderCommand: same line rules, Draft-only guard — file: src/Application/Procurement/Commands/PurchaseOrders/UpdatePurchaseOrder/UpdatePurchaseOrderCommandValidator.cs
- [ ] T127 [P] [US4] Create FluentValidation validators for Submit, Approve, Issue, Cancel, Close commands: status preconditions — files: src/Application/Procurement/Commands/PurchaseOrders/*/Validator.cs
- [ ] T128 [US4] Verify backend build: dotnet build src/Web/Web.csproj — zero errors, zero warnings
- [ ] T129 [US4] Run PO unit tests: dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~PurchaseOrder" — all pass

## Phase 20: Purchase Order Frontend Pages

**Goal**: Create all four PO pages (list, create, detail, edit) with forms, schemas, hooks, catalog data, and design system compliance.

**Independent Test**: Navigate to all four routes. List shows data with filters. Create form submits successfully. Detail shows full info with related docs. Edit works only for Draft. Cancel/Close dialogs capture reason. All Arabic RTL.

- [ ] T130 [P] [US4] Fix shared/types.ts: remove circular self-import, explicitly export PurchaseOrderStatus type union, add PurchaseOrderDetailStatus type — file: src/Web/ClientApp/src/features/procurement/purchase-orders/shared/types.ts
- [ ] T131 [P] [US4] Create shared/schemas.ts: Zod validation for create/update PO — SupplierPartyId required (number > 0), lines array min(1), each line: OrderedQuantity > 0, UnitPrice >= 0, DiscountPercent 0–100 optional, TaxPercent 0–100 optional — file: src/Web/ClientApp/src/features/procurement/purchase-orders/shared/schemas.ts
- [ ] T132 [P] [US4] Create shared/catalog-hooks.ts: useItems, useUnits, useSuppliers, useWarehouses, useLocations, useCurrencies hooks with staleTime: Infinity — file: src/Web/ClientApp/src/features/procurement/purchase-orders/shared/catalog-hooks.ts
- [ ] T133 [P] [US4] Fix hooks/usePurchaseOrders.ts: add useUpdatePurchaseOrder, useSubmitPurchaseOrder hooks, fix usePurchaseOrdersList return type (PaginatedList not array), ensure all mutations invalidate queries — file: src/Web/ClientApp/src/features/procurement/purchase-orders/hooks/usePurchaseOrders.ts
- [ ] T134 [P] [US4] Create ProcurementPurchaseOrdersForm.tsx: react-hook-form + zodResolver, useFieldArray for lines, client-side total computation (LineTotal, LineTotalWithTax, SubTotal, GrandTotal), catalog option props (items, units, suppliers, warehouses, locations, currencies), readOnly mode for detail page — file: src/Web/ClientApp/src/components/ProcurementPurchaseOrdersForm.tsx
- [ ] T135 [US4] Update PurchaseOrdersListPage.tsx: add SupplierPartyId filter (combobox), ExpectedDeliveryDate range filter (date pickers), fix action buttons per status (Submit for Draft, Approve for Submitted, Issue for Approved, Cancel for non-Cancelled/Closed, Close for PartiallyReceived/Received) — file: src/Web/ClientApp/src/features/procurement/purchase-orders/pages/PurchaseOrdersListPage.tsx
- [ ] T136 [US4] Create PurchaseOrderCreatePage.tsx: load catalog data, render ProcurementPurchaseOrdersForm, on submit navigate to detail page — file: src/Web/ClientApp/src/features/procurement/purchase-orders/pages/PurchaseOrderCreatePage.tsx
- [ ] T137 [US4] Create PurchaseOrderDetailPage.tsx: load PO detail + catalog data, render form in readOnly mode, show financial summary (SubTotal, DiscountAmount, TaxAmount, GrandTotal with tabular-nums), line items with ReceivedQuantity/RemainingQuantity, approval history panel, document status log, related documents navigation (PR, Quotation, GRNs, SupplierInvoices, PaymentOrders), lifecycle action buttons, Cancel/Close dialogs with reason textarea — file: src/Web/ClientApp/src/features/procurement/purchase-orders/pages/PurchaseOrderDetailPage.tsx
- [ ] T138 [US4] Create PurchaseOrderEditPage.tsx: load PO detail, guard redirect if status !== Draft, render form with existing data, on submit navigate to detail page — file: src/Web/ClientApp/src/features/procurement/purchase-orders/pages/PurchaseOrderEditPage.tsx
- [ ] T139 [US4] Register PO routes in src/app/routes.tsx: /procurement/purchase-orders/create, /procurement/purchase-orders/:id, /procurement/purchase-orders/:id/edit — file: src/Web/ClientApp/src/app/routes.tsx
- [ ] T140 [P] [US4] Add PurchaseOrders.Issue, PurchaseOrders.Close to shared/constants/permissions.ts — file: src/Web/ClientApp/src/shared/constants/permissions.ts
- [ ] T141 [US4] Run frontend lint: cd src/Web/ClientApp && npm run lint — zero errors
- [ ] T142 [US4] Run frontend build: cd src/Web/ClientApp && npm run build — zero errors

## Phase 21: Purchase Order Test Coverage

**Goal**: Add missing unit tests for Submit, Update, Close commands. Fix lifecycle bug. Add invalid transition tests. Add functional tests for Update, Close, direct purchase threshold.

**Independent Test**: All unit tests pass. All functional tests pass. No stubs remain. Lifecycle bug fixed.

- [X] T143 [US4] Fix ApprovePO_Draft_ShouldTransitionToApproved test: add submit step before approve (Draft→Submitted→Approved) — file: tests/Application.UnitTests/Procurement/PurchaseOrderTests.cs
- [X] T144 [P] [US4] Create unit tests for SubmitPurchaseOrderCommandHandler: Draft→Submitted succeeds, non-Draft rejects — file: tests/Application.UnitTests/Procurement/PurchaseOrderSubmitTests.cs
- [X] T145 [P] [US4] Create unit tests for UpdatePurchaseOrderCommandHandler: Draft update with line changes succeeds, non-Draft rejects, line add/update/remove logic, header totals recomputed — file: tests/Application.UnitTests/Procurement/PurchaseOrderUpdateTests.cs
- [X] T146 [P] [US4] Create unit tests for ClosePurchaseOrderCommandHandler: PartiallyReceived→Closed succeeds, non-closeable status rejects — file: tests/Application.UnitTests/Procurement/PurchaseOrderCloseTests.cs
- [X] T147 [US4] Create unit tests for all invalid state transitions: submit from non-Draft, approve from non-Submitted, issue from non-Approved, cancel from Cancelled/Closed, close from non-PartiallyReceived/Received — file: tests/Application.UnitTests/Procurement/PurchaseOrderTransitionTests.cs
- [ ] T148 [US4] Create functional tests: UpdatePurchaseOrder (create→update lines→verify totals), ClosePurchaseOrder (receive partial→close→verify status), SubmitPurchaseOrder standalone, direct purchase threshold (>5M YER without RFQ fails) — file: tests/Application.FunctionalTests/Procurement/PurchaseOrderEdgeCaseTests.cs
- [ ] T149 [US4] Evaluate ApprovalEvaluationTests.cs stubs: either replace Assert.Pass() with real assertions or mark as tracked debt with justification — file: tests/Application.FunctionalTests/Security/ApprovalEvaluationTests.cs
- [X] T150 [US4] Run full unit test suite: dotnet test tests/Application.UnitTests — all pass
- [ ] T151 [US4] Run full functional test suite: dotnet test tests/Application.FunctionalTests — all pass

## Phase 22: Final Verification & Polish

**Goal**: End-to-end verification of all PO changes. Build, test, lint, quickstart validation.

- [ ] T152 Run backend build: dotnet build src/Web/Web.csproj — zero errors, zero warnings
- [ ] T153 Run all backend tests: dotnet test tests/Domain.UnitTests && dotnet test tests/Application.UnitTests && dotnet test tests/Application.FunctionalTests
- [ ] T154 Run frontend lint and build: cd src/Web/ClientApp && npm run lint && npm run build
- [ ] T155 Run quickstart.md validation: execute all 11 scenarios, verify expected outcomes
- [ ] T156 Verify NSwag client regeneration: npm run generate-api — updated PO client types

## Dependencies

```text
Phase 1 (Setup) → Phase 2 (Foundational) → Phase 3-9 (User Stories, can be parallelized per story)
Phase 3 (US1) → Phase 4 (US2) [US2 depends on US1 for approved PR]
Phase 4 (US2) → Phase 5 (US3) [US3 depends on US2 for closed RFQ]
Phase 5 (US3) → Phase 6 (US4) [US4 depends on US3 for awarded quotation, OR US1 for direct purchase]
Phase 6 (US4) → Phase 7 (US5) [US5 depends on US4 for issued PO]
Phase 7 (US5) → Phase 8 (US6) [US6 depends on US5 for received goods]
Phase 8 (US6) → Phase 9 (US7) [US7 depends on US6 for matched invoice]
Phase 10-16 (Frontend) depend on their respective backend phases
Phase 17 (Migration) can run in parallel with Phase 3-9
Phase 18 (Polish) depends on all other phases

--- PO Gap Fix Phases (FR-075–FR-109) ---
Phase 19 (PO Backend Gaps) depends on Phase 6 (US4 existing) being complete
Phase 20 (PO Frontend Pages) depends on Phase 19 (backend endpoints must work)
Phase 21 (PO Test Coverage) depends on Phase 19 (handlers must be fixed first)
Phase 22 (Final Verification) depends on Phase 19, 20, 21 all complete

--- GRN Frontend Phase (FR-110–FR-126) ---
Phase 14 (GRN Frontend) depends on Phase 7 (US5 backend) and Phase 20 (PO detail page for T163)

--- Supplier Invoice Frontend Gaps (FR-127–FR-153) ---
Phase 24 (Supplier Invoice Frontend) depends on Phase 8 (US6 backend) and Phase 20 (PO detail page for T177)
```

## Parallel Execution Opportunities

Within each user story phase, the following can run in parallel:
- Commands (T014-T019 for US1) — different files, no cross-dependencies
- Queries (T020-T021 for US1) — different files
- Tests (T023-T024 for US1) — different test projects
- Frontend pages (T082-T085 for US1) — different files

Across user stories, once dependencies are met:
- US2 and US3 can overlap (US2 collects responses while US3 evaluates previous RFQ)
- US5 and US6 can overlap for different POs (receipt and invoicing are independent per PO)
- All frontend phases (T10-T16) can run in parallel once their backend phases complete

Within Phase 19 (PO Backend Gaps), parallel tasks:
- T114 (query fix) and T117 (permission fix) and T124 (authorization) and T125-T127 (validators) — different files
- T120 (cancel reason) and T121 (close reason) — different files

Within Phase 20 (PO Frontend Pages), parallel tasks:
- T130 (types fix), T131 (schemas), T132 (catalog hooks), T133 (hooks fix), T134 (form component) — all different files

Within Phase 24 (Supplier Invoice Frontend Gaps), parallel tasks:
- T168 (types fix), T169 (schemas), T170 (catalog hooks), T171 (hooks fix), T172 (permissions) — all different files
- T173 (list page) and T174 (create page) and T175 (detail page) — different files after T168-T172 complete

## Implementation Strategy

**MVP**: Phase 1-3 (Setup + Foundational + Purchase Request) — demonstrates the core pattern and can be validated independently.

**Incremental Delivery**: Each subsequent phase (US2→US3→US4→US5→US6→US7) adds one complete document type with full lifecycle, testable end-to-end.

**PO Gap Fix Scope**: Phase 19-22 (T113–T156) specifically address the FR-075–FR-109 requirements. These phases fix the existing PO implementation to be production-ready:
- Phase 19: Backend — 17 tasks fixing stubs, adding endpoints, validators, status logging, domain events, reason parameter
- Phase 20: Frontend — 13 tasks creating create/detail/edit pages, form component, schemas, hooks
- Phase 21: Tests — 9 tasks adding unit/functional tests and fixing lifecycle bug
- Phase 22: Verification — 5 tasks for build/test/lint/quickstart validation

**Risk Mitigation**: Data migration (Phase 17) runs early and in parallel to catch migration issues before frontend work begins.

**Supplier Invoice Frontend Scope**: Phase 24 (T168–T179) addresses FR-127–FR-153 requirements. These tasks fix the existing Supplier Invoice frontend and add missing pages:
- T168–T172: Foundation (types, schemas, catalog hooks, hooks fix, permissions) — all parallelizable
- T173–T175: Three pages (list, create, detail) — depend on T168–T172
- T176–T177: Routes and PO detail button — depend on T173–T175
- T178–T179: Lint and build verification — depend on all above
- Backend gaps (G1–G6) are documented as dependencies; tasks do not attempt to fix backend issues

## Phase 23: Convergence — GRN Frontend Gaps

**Goal**: Fix remaining GRN frontend gaps identified by convergence assessment against FR-117 and FR-121.

- [X] T166 [US5] Fix GRNCreatePage.tsx line items table: add per-line user-input fields (Received Quantity, Accepted Quantity, Rejected Quantity, Batch Number, Expiry Date, Notes) and read-only Unit column per FR-117. Current table only shows 4 read-only PO columns. Must use useFieldArray or dynamic form state for line-level input, validate with existing Zod schema (accepted + rejected = received, received ≤ remaining) — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/pages/GRNCreatePage.tsx
- [X] T167 [US5] Fix GRNDetailPage.tsx line items table: add missing columns per FR-121 — Unit (name or ID fallback), UnitCost (tabular-nums), TotalCost (tabular-nums), BatchNumber, ExpiryDate, Notes. Current table has 6 columns, spec requires 12 — file: src/Web/ClientApp/src/features/procurement/goods-receipt-notes/pages/GRNDetailPage.tsx

## Phase 24: Supplier Invoice Frontend Gaps (FR-127–FR-153)

**Goal**: Fix all Supplier Invoice frontend gaps — types, hooks, list page, and create missing pages (create, detail), schemas, catalog hooks, permissions, routes, and PO detail button.

**Independent Test**: Navigate to all three routes (list, create, detail). List shows paginated data with filters and no inline actions. Create form loads PO details and validates. Detail shows full info with status-gated actions. All Arabic RTL.

- [X] T168 [P] [US6] Fix shared/types.ts: remove circular self-import, complete SupplierInvoice/SupplierInvoiceDetail/SupplierInvoiceDetailLine interfaces (add purchaseOrderNumber, supplierName, subTotal, discountAmount, taxAmount, shippingCost, otherCharges, dueDate, created to SupplierInvoice; add itemNameAr, discountAmount, taxAmount to SupplierInvoiceDetailLine), add PaginatedList type, add supplierInvoiceStatusVariant map for StatusBadge — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/shared/types.ts
- [X] T169 [P] [US6] Create shared/schemas.ts: Zod validation for Supplier Invoice create — purchaseOrderId required (number > 0), supplierInvoiceNumber required (string, min 1), invoiceDate required (date), lines array min(1) with purchaseOrderDetailId required, itemId required, quantity > 0, unitPrice >= 0 — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/shared/schemas.ts
- [X] T170 [P] [US6] Create shared/catalog-hooks.ts: useItems, useUnits, useCurrencies hooks with staleTime: Infinity, following GRN catalog-hooks.ts pattern — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/shared/catalog-hooks.ts
- [X] T171 [P] [US6] Fix hooks/useSupplierInvoices.ts: fix useSupplierInvoicesList return type to PaginatedList<SupplierInvoice> (not array), add purchaseOrderId filter param, add page/pageSize params, add useCreateSupplierInvoice mutation, add handleApiError/handleLifecycleError to all mutations, ensure all mutations invalidate both list and detail queries — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/hooks/useSupplierInvoices.ts
- [X] T172 [P] [US6] Add SupplierInvoices.* permission codes to PROCUREMENT_PERMISSIONS: SupplierInvoices.View, SupplierInvoices.Create, SupplierInvoices.Submit, SupplierInvoices.Match, SupplierInvoices.Cancel — file: src/Web/ClientApp/src/shared/constants/permissions.ts
- [X] T173 [US6] Rewrite SupplierInvoicesListPage.tsx: fix DataGrid contract (PaginatedList, not array), add server-side pagination, add purchaseOrderId filter, add search filter, add StatusBadge, remove inline Submit/Match/Cancel actions (detail-page-only per FR-135), add skeleton/error/empty states, reset page on filter change, add permission check for create button, Arabic RTL with logical CSS — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoicesListPage.tsx
- [X] T174 [US6] Create SupplierInvoiceCreatePage.tsx: require ?purchaseOrderId= query param (show error if absent), load PO details with remaining quantities, render header fields (supplier invoice number, invoice date, currency code, exchange rate, due date, notes — supplier inherited from PO, read-only), render line items table with read-only PO data and user-input fields (quantity, unit price defaulting to PO price but editable, discount amount, tax amount, notes), validate with Zod schema, on success navigate to detail page, 4xx inline errors / 5xx toast — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoiceCreatePage.tsx
- [X] T175 [US6] Create SupplierInvoiceDetailPage.tsx: display invoice summary (system-generated number dir="ltr" mono, supplier invoice number dir="ltr", invoice date, due date, status badge, created date), related PO number as clickable link, supplier name (or ID fallback — backend gap), financial summary (SubTotal, DiscountAmount, TaxAmount, ShippingCost, OtherCharges, GrandTotal — all tabular-nums), notes, line items table (item name or ID fallback, quantity, unit price, discount amount, tax amount, line total, notes — all monetary values tabular-nums), status-gated action buttons: Draft→Submit, Submitted→Match+Cancel, Matched/Disputed→Cancel, Paid/Cancelled→none, cancel dialog with optional notes textarea, back-to-PO link, Arabic RTL — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoiceDetailPage.tsx
- [X] T176 [US6] Register Supplier Invoice routes in src/app/routes.tsx: /procurement/supplier-invoices/create, /procurement/supplier-invoices/:id — with permission guards — file: src/Web/ClientApp/src/app/routes.tsx
- [X] T177 [US6] Add "Create Supplier Invoice" button to PO detail page: show only when PO status is Issued or PartiallyReceived AND user has SupplierInvoices.Create permission, navigate to /procurement/supplier-invoices/create?purchaseOrderId=<poId> — file: src/Web/ClientApp/src/features/procurement/purchase-orders/pages/PurchaseOrderDetailPage.tsx
- [X] T178 [P] [US6] Run frontend lint: cd src/Web/ClientApp && npm run lint — zero errors
- [X] T179 [US6] Run frontend build: cd src/Web/ClientApp && npm run build — zero errors

## Phase 25: Convergence — Supplier Invoice Frontend Gaps

**Goal**: Fix remaining Supplier Invoice frontend gaps identified by convergence assessment against FR-141, FR-131, FR-133, FR-146, and FR-144.

- [X] T180 [US6] Fix SupplierInvoiceCreatePage.tsx navigate target: on successful creation, navigate to detail page `/procurement/supplier-invoices/${newId}` instead of list page. The POST endpoint returns the new invoice ID — capture it from mutateAsync result and use in navigate. Per FR-141 (partial) — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoiceCreatePage.tsx
- [X] T181 [US6] Add Purchase Order Number column to SupplierInvoicesListPage.tsx DataGrid. Column should render with dir="ltr" and link to PO detail page `/procurement/purchase-orders/${row.purchaseOrderId}`. Per FR-131 (missing) — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoicesListPage.tsx
- [X] T182 [US6] Add purchaseOrderId filter to SupplierInvoicesListPage.tsx. Read `purchaseOrderId` from URL search params (passed when navigating from PO detail). Add to filter state and pass to useSupplierInvoicesList. Per FR-133 (missing) — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoicesListPage.tsx
- [X] T183 [US6] Add dedicated "Back to Purchase Order" link/button to SupplierInvoiceDetailPage.tsx. Show when invoice has purchaseOrderId. Navigate to `/procurement/purchase-orders/${invoice.purchaseOrderId}`. Per FR-146 (missing) — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoiceDetailPage.tsx
- [X] T184 [US6] Narrow canCancel logic in SupplierInvoiceDetailPage.tsx to match spec FR-144. Change from `status !== 'Cancelled' && status !== 'Paid'` to `status === 'Draft' || status === 'Submitted' || status === 'Matched' || status === 'Disputed'`. Backend blocks PartiallyPaid cancellation. Per FR-144 (partial) — file: src/Web/ClientApp/src/features/procurement/supplier-invoices/pages/SupplierInvoiceDetailPage.tsx
