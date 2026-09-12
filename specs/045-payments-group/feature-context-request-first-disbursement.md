# Spec Kit amendment input: Request-first disbursement

## Document metadata

- Generated: 2026-09-09
- Input type: simple_description, with user-provided proposed schema
- Status: DISCOVERY_COMPLETE; amendment input, not an approved implementation specification
- Target: existing `specs/045-payments-group/spec.md`
- Do not create another numbered feature or replace unrelated PAY-04 requirements.
- Confirmed requirements below come from the user. Proposals and unresolved questions are explicitly separated.
- Stakeholder follow-up: Q1–Q3 answered; the affirmative answer to Q2 is interpreted as retaining two signatures.

## Original request

> يتم تقديم طلبات ويتم مراجعتها ممن يملك الصلاحيه ويتم الموافقه مع تحديد المبلغ ويتحول الى امر صرف مع تعبئه جهه الامر التي وافقت او الرفض مع ذكر السبب

The user also proposed independent requests, general-purpose payment orders, deduction details, no order lines, and one payment per order. The subsequent discussion proposed database changes; assistant suggestions are not stakeholder-approved decisions.

## Problem statement

The current creation handler requires an already-approved payment order before a request can exist. The user needs the request to initiate review, with an approved amount and issuing authority producing the order afterward. The existing workflow therefore cannot represent the requested business sequence.

- Evidence: the user request above and `CreateDisbursementRequestCommand.cs:38`.
- Frequency: every request-led disbursement; actual transaction volume is unknown.
- Impact: request review and amount authorization cannot be captured before order creation.

## Core intent

- WHO: request submitter, authorized reviewer, finance preparer, treasury payment recorder.
- WHAT: independent request, reasoned approval or rejection, approved amount, issuing authority, linked order, single completed payment.
- WHEN: a beneficiary needs an expense, assistance, or other authorized disbursement.
- WHY: preserve the original request and make the expenditure traceable to the actual authorization.

## Confirmed scope

1. Requests precede orders in the request-led workflow.
2. A request records requester, beneficiary, requested amount, currency, purpose, date, financial year and relevant optional details.
3. A permitted reviewer approves while specifying an amount, or rejects with a reason.
4. Completed approval produces a linked order carrying the approved amount and issuing authority.
5. Orders are general-purpose; a purchase-order reference is optional outside procurement.
6. The proposed target has deduction details and no payment-order lines.
7. Each order is paid once; partial payment is not a target capability.
8. Issuing authority is an authorized officeholder acting as General Manager (المدير العام) or Finance Director (المدير المالي), not an internal department or external organization. Preserve both the actual actor and the capacity in which the authorization was issued.
9. Retain the two-signature approval requirement with distinct qualified users. The user's affirmative answer is interpreted as retaining this existing requirement; it does not establish one mandatory signature from each of the two named positions.
10. The approved amount may equal or be lower than the requested amount; exceeding it is prohibited. A positive approved amount is the proposed validation default. A mandatory reduction reason remains a proposal, unlike the explicitly required rejection reason.
11. `BeneficiaryName` is required for every beneficiary. `BeneficiaryPartyId` is optional and links to `Parties` only when the beneficiary already exists there. A generated order copies both values from its source document.

## Non-goals

| Non-goal | Reason |
|---|---|
| Implementing payroll or new procurement workflows | Order categories do not authorize additional modules. Preserve existing integrations and assess impact. |
| Redesigning bank-account management | PAY-04 is already covered by the existing feature. |
| Connecting to banks or building a payment retry/reversal subsystem | The request concerns document flow and recording payment, not external execution infrastructure. Preserve existing financial correction rules. |
| Replacing global audit types, monetary precision, or RBAC architecture | These are repository-wide conventions, not required by the requested outcome. |

## Proposed acceptance requirements for the amended spec

Use the unique `RFD-` prefix to avoid collisions with existing per-module FR identifiers. These requirements form a draft; rules explicitly marked as proposals require validation during clarify/plan.

- **RFD-001**: The system MUST allow creation of a disbursement request without an existing payment order and allocate its unique request number server-side.
- **RFD-002**: The system MUST preserve the originally requested amount and currency separately from the amount approved by the reviewer. Requester identity MUST remain separate from beneficiary information.
- **RFD-003**: The system MUST restrict review decisions to authorized actors and record the actor, time, decision, reason where applicable, issuing authority and approved amount for approval decisions. Issuing capacity MUST identify General Manager or Finance Director and be traceable to the actual authorized officeholder. Identity and authority MUST be resolved or validated server-side; merely selecting a title MUST NOT grant authority.
- **RFD-004**: Rejection MUST require a nonblank reason, leave the request rejected, expose the reason to the submitter, and create no order.
- **RFD-005**: Completion of two valid signatures by distinct qualified users MUST generate the linked order. The first signature MUST NOT generate an order. Both signatures MUST authorize the same request revision and amount; signatures on different amounts MUST NOT be combined as final authorization. Existing signer qualification rules remain applicable until the role-to-position mapping is specified; the two named issuing capacities do not imply that both positions must sign every request.
- **RFD-006**: Final approval, numbering, order creation and decision/status history MUST succeed atomically. Failure MUST NOT leave a newly approved request without its required order.
- **RFD-007**: Repeated submission of the same final decision and concurrent approval attempts MUST NOT create duplicate orders or final approval records. Stale decisions MUST produce an explicit conflict or a documented idempotent result.
- **RFD-008**: A request MUST have at most one resulting order. An order MUST retain traceability to its source request and final authorization when request-led; other existing order sources MUST remain representable.
- **RFD-009**: The generated order MUST carry the approved gross amount, beneficiary, currency and issuing authority. It MUST NOT be eligible for payment before financial preparation, budget controls, required order approvals and treasury gates are satisfied.
- **RFD-010**: Finance MUST NOT silently replace the approved amount, currency or beneficiary. Changes affecting the authorization MUST follow a defined reauthorization process. The process must be specified before enabling such edits.
- **RFD-011**: The target order MUST use the user-requested single-account/header model without order lines. Existing orders spanning multiple accounts or financial dimensions MUST NOT be silently collapsed during migration. The migration plan MUST identify and resolve incompatible records before destructive schema changes.
- **RFD-012**: Deductions MUST remain traceable individual records. Net amount MUST be computed server-side as gross amount minus deduction amounts; deduction total MUST NOT exceed gross. Derived totals MUST NOT become stored aggregate columns.
- **RFD-013**: A completed payment MUST reference its order and snapshot the server-computed payable amount, payment method, execution actor, execution time and applicable reference. There MUST be at most one completed payment per order, enforced against concurrent attempts.
- **RFD-014**: Successful payment MUST update the order's execution state and preserve the established event-driven posting integration without duplicate financial effects. Failure MUST NOT produce a completed payment or a paid order.
- **RFD-015**: Users MUST be able to trace request, requested amount, final approved amount, issuing authority, order, deductions, net amount and payment from detail views. Approval state and payment progress MUST be distinguishable.
- **RFD-016**: Migration MUST preserve existing financial records, links and audit history. It MUST NOT invent original requested amounts, issuing authorities or approval decisions absent from legacy evidence.
- **RFD-017**: APIs, generated clients, request/order/payment pages, approval panels, pending-approval queries, reports and posting consumers MUST be updated consistently with the changed relationships.
- **RFD-018**: The system MUST reject an approved amount greater than the requested amount. Both the original requested amount and the final authorized amount MUST remain visible. Positive approved amounts are the proposed default; zero/negative values MUST NOT be treated as payment authorization without an explicitly defined alternative business outcome.
- **RFD-019**: Disbursement requests and payment orders MUST carry required `BeneficiaryName` and optional `BeneficiaryPartyId`. When a user selects an existing active Party, the server MUST validate it, store its ID and snapshot its current name into `BeneficiaryName`. When the beneficiary is not registered, the ID MUST remain null and a manually entered name MUST be accepted. Generated orders MUST copy both the optional link and the immutable authorization-time name snapshot from their source document. All affected persistence, API, generated-client, UI, filter and reporting contracts MUST use beneficiary terminology.

## User scenarios and acceptance examples

### US-RFD-1: Submit an independent request (P1)

- Given no payment order exists, when a submitter creates a request with beneficiary, positive requested amount, currency and purpose, then a numbered draft request is saved without requiring an order identifier.
- Given a submitted request, when its submitter tries to change the reviewed amount or beneficiary, then the server follows the declared edit/resubmission rules rather than silently changing the decision basis.

### US-RFD-2: Approve an amount and generate an order (P1)

- Given a submitted request for 100,000, when the applicable final approval authorizes 80,000 and identifies the issuing authority, then the original 100,000 remains visible and exactly one linked order is generated for gross 80,000.
- Given an intermediate signature in a multi-step approval, when it is recorded, then no order exists yet.
- Given a request for 100,000, when 100,000 is approved through both valid signatures, then the amount-limit check passes and the order gross amount is 100,000.
- Given a request for 100,000, when 100,001 is proposed for approval, then the server refuses it without recording that approval or generating an order.
- Given one user's first signature, when that same user attempts the second signature, then the server refuses it.
- Given the first signature authorizes 80,000, when a second signature proposes 75,000, then the signatures cannot finalize an order together; the specified reauthorization procedure is required.
- Given a final authorization issued by a permitted General Manager or Finance Director, when the order is generated, then the issuing capacity and responsible person remain traceable even after the position changes hands.
- Given two concurrent final-approval attempts, when processed, then at most one resulting order exists and history contains no duplicate final decision.
- Given order creation fails, when approval returns, then neither the new final approval nor a newly approved request is committed.

### US-RFD-3: Reject with a reason (P1)

- Given a request awaiting review, when rejected with a reason, then the request shows that reason and no order is generated.
- Given a blank or whitespace-only rejection reason, when rejection is attempted, then it is refused without changing state.
- Given an unauthorized actor, when approval or rejection is attempted, then the decision is refused server-side.

### US-RFD-4: Prepare and pay the order (P1)

- Given an order for 80,000 with deductions of 5,000, when its valid payment is recorded after all applicable gates, then one payment for 75,000 exists and the order is paid.
- Given that payment, when another payment is attempted for the same order, then it is refused, including concurrent attempts.
- Given missing budget information or a blocking failed budget check, when financial approval/payment is attempted, then the configured control and explicit override rules apply; request approval cannot bypass them.
- Given a request-originated order, when it is generated, then its beneficiary name exactly matches the approved request.
- Given a procurement-originated order, when it is generated, then its beneficiary name is copied from the purchase order's supplier name snapshot and every downstream screen uses beneficiary terminology.
- Given an active Party is selected as beneficiary, when the request is saved, then its ID is stored and its current name is copied into `BeneficiaryName`.
- Given a beneficiary does not exist in Parties, when a valid name is entered without an ID, then the request is accepted.
- Given a supplied Party ID does not exist or is inactive, when the request is saved, then the server refuses it instead of silently retaining an invalid link.
- Given a linked Party is renamed after approval, when the historical request or order is displayed, then the saved beneficiary-name snapshot remains unchanged.

### US-RFD-5: Preserve historical records (P1)

- Given legacy orders, requests and payments, when migration is validated, then counts, monetary values, audit records and source links reconcile against the pre-migration inventory.
- Given an old multi-line order with different accounts or allocations, when assessed for migration, then it is identified explicitly and is not flattened into an arbitrary account.

## Success criteria

- **SC-RFD-001**: Every newly finalized approved request has exactly one linked order with matching approved gross amount, currency, beneficiary and issuing authority.
- **SC-RFD-002**: Every rejected request has a visible nonblank rejection reason and no generated order.
- **SC-RFD-003**: Duplicate/concurrent approval and payment verification produces zero duplicate orders or completed payments.
- **SC-RFD-004**: Every completed payment matches its order's server-calculated net amount and is traceable to the authorization.
- **SC-RFD-005**: Migration reconciliation accounts for all legacy financial/audit records; missing historical facts are explicitly identified, never fabricated.

## Codebase research and contradictions

| Reference | Evidence / relevance |
|---|---|
| `src/Application/Payments/Commands/DisbursementRequests/CreateDisbursementRequest/CreateDisbursementRequestCommand.cs:38` | Creation currently requires an Approved order; contradicts request-first target. |
| `src/Application/Payments/Commands/DisbursementRequests/ApproveDisbursementRequest/ApproveDisbursementRequestCommand.cs:50` | Existing qualified dual-signature process; approval completion must be reconciled with the new trigger. |
| `src/Application/Payments/Commands/Payments/RecordPayment/RecordPaymentCommand.cs:30` | Payment currently starts from request ID and derives net amount from its order. |
| `src/Domain/Security/Entities/ApprovalHistory.cs` | Existing decision actor/time/reason/evaluation history; reuse instead of inline approval fields. |
| `src/Domain/Organization/Entities/OrganizationalUnit.cs` | Not sufficient as issuing authority: the stakeholder identified an officeholder/position, not a department. |
| `src/Domain/Payments/Entities/PaymentOrderLine.cs` | Existing account and financial dimensions live on lines; removing them needs an explicit mapping. |
| `src/Infrastructure/Data/Configurations/Payments/DisbursementRequestConfiguration.cs` | Unique order reference currently resides on the request. |
| `src/Infrastructure/Data/Configurations/Payments/PaymentConfiguration.cs` | Payment uniqueness currently resides on request ID, not order ID. |

Existing `spec.md`, `plan.md`, `data-model.md`, API contracts and quickstart describe order-first behavior. Some spec scenarios describe partial payments even though the plan defers them and the current record-payment handler is single-payment. Rewrite conflicting normative sections rather than leaving contradictory requirements beside an appendix.

The target changes persistence and public contracts. DEP-027 exempts Spec 045 from TDD and new automated tests while retaining existing backend regression suites, scoped builds, frontend lint/build and manual quickstart evidence.

## Stakeholder clarifications

### Q1: Meaning of issuing authority

- Category: User / Integration
- Owner: User/Stakeholder
- Blocking: Resolved at the business-meaning level; persistence mapping belongs in the plan.
- Original gap: The authority was not defined as an internal unit, an official person/position, or an external issuing body.
- Question: What does جهة الأمر represent in actual documents: an internal department, a named official/position, or an external body?
- Why it matters: Determines the reference data and how a reviewer is allowed to approve on its behalf.
- Resolution: User answered "الندير العام او المدير المالي", understood as "المدير العام أو المدير المالي". Represent the authorized officeholder and issuing capacity. Do not equate the authority with a department. Choose the identity/position persistence model in the plan after inspecting existing personnel and security references.

### Q2: Required approvals and amount ownership

- Category: Behavior
- Owner: User/Stakeholder
- Blocking: Resolved for signature count; signer responsibilities and position mapping must be specified during planning/clarification.
- Gap: The existing spec explicitly requires two qualified distinct signers; the new description says review by someone with permission but does not explicitly repeal the two-signature decision.
- Question: Should the existing two-signature process remain, with both signing the same proposed amount, or should a different approval process replace it?
- Why it matters: Defines the final approval event that creates the order and what happens when the amount changes between signatures.
- Resolution: User answered "نعم" to whether approval remains with two signatures or changes. Interpret as retaining two distinct qualified signatures, as stated in the response to the user. Do not infer that the General Manager and Finance Director must each sign, or that the second signer automatically becomes the issuing authority. Preserve the existing qualifications until mapping is explicitly defined. Binding both signatures to the same amount/revision is a financial-integrity requirement, not a separately asserted stakeholder answer.

### Q3: Approved amount limit

- Category: Behavior
- Owner: User/Stakeholder
- Blocking: Resolved for the upper limit.
- Gap: The user requires selecting an approved amount but did not specify whether increases beyond the requested amount are allowed.
- Question: May the reviewer approve more than the requested amount, or only an equal/lower positive amount?
- Why it matters: Determines validation and whether a larger amount requires a corrected request.
- Resolution: User explicitly selected "يساويه أو يقل عنه". Reject approval above the requested amount; allow equality and reductions subject to other validations. Positive amount remains the proposed default; requiring a reason for reduction was not explicitly confirmed.

## Additional planning issues

- Define which signature supplies the order's issuing authority when the signers act in different capacities. Do not silently assume it is the final signer. Map the existing AccountsManager/AuthorizingOfficer qualification rules to the named offices without treating their labels as automatically equivalent.
- Generated-order `Draft` status and deriving request execution progress from the order are proposed defaults, not previously confirmed user decisions.
- Inventory actual database usage and legacy records before choosing a migration strategy. No database reset or fabricated backfill is authorized.
- Define zero-net treatment, post-rejection resubmission, cancellation after order generation and reauthorization as explicit lifecycle rules; preserve existing restrictions until superseded.
- Identify the actual cash source model. Existing `Budgeting.Fund` is a budget fund and must not automatically be treated as a cashbox.
- Define beneficiary-name normalization, maximum length and required validation. The optional Party link supplements the name snapshot and never replaces it.
- Inventory and migrate every legacy supplier-specific payee reference in payment-order data, contracts, filters and reports into the unified beneficiary fields before removing the old contract surface.
- Optional procurement source does not imply building payroll links or new direct-order sources.

## Engineering constraints for plan/tasks

- Keep `decimal(23,2)` for monetary amounts, existing shared audit types, BaseAuditableEntity inheritance and RowVersion.
- Record approvals via ApprovalHistory and transitions via DocumentStatusLog. Choose any approval detail storage changes in data-model/plan; do not duplicate inline approval columns.
- Preserve budget/appropriation/encumbrance controls, financial-year gates and existing event-driven ledger posting.
- Use new migrations and Restrict FKs; do not edit applied migrations or delete financial/audit records.
- Add a decision record for breaking contract/schema design before implementation, per constitution IX/XII.
- Regenerate NSwag clients after endpoint changes; synchronize contracts, docs/database-schema.md and affected repository conventions.
- DEP-027 verification: no TDD or new automated tests for Spec 045; use scoped builds, frontend lint/build, manual quickstart evidence, and the existing five-project backend suite at convergence/pre-merge gates. Existing assertions remain intact.
- Preserve current unrelated worktree changes, particularly existing edits to `tasks.md` and Payments UI files. Do not mark old completed tasks incomplete or replace their history to disguise the new work; add traceable amendment tasks after replanning.

## Parking lot

| Idea | Source | Reason deferred |
|---|---|---|
| PaymentAttempt / new reversal subsystem | Earlier assistant discussion | Not requested as part of this workflow redesign. |
| PayrollRunId and payroll generation | Earlier assistant discussion | Requires a separate payroll scope. |
| Global audit user FK conversion and four-decimal money | Original proposed schema | Conflicts with existing conventions and is unnecessary for the flow. |

## Goals after stakeholder clarification

1. Amend PAY-01..03 in the existing feature to express request-first authorization and single-payment execution.
2. Carry forward the clarified officeholder meaning, retained two-signature requirement and equal-or-lower amount limit; distinguish remaining workflow details from confirmed stakeholder decisions.
3. Produce synchronized spec, plan, data model, contracts, migration strategy and additive implementation tasks.

## Spec Kit handoff

Pass this file as input to `/speckit.specify`, explicitly requesting an amendment of the existing feature rather than a new feature/branch. During specification, replace stale order-first scenarios and preserve unaffected PAY-04 behavior. Carry Q1–Q3 answers into the canonical spec without re-asking them. Use `/speckit.clarify` only for remaining material workflow questions, then `/speckit.plan`, `/speckit.tasks`, `/speckit.implement`, and `/speckit.converge`.

Do not implement from this discovery file alone; the amended canonical documents must agree and required business decisions must be resolved first.
