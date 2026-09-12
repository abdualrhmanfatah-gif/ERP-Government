# Specification Quality Checklist: Payments Group (PAY-01..04)

**Purpose**: Validate specification completeness and quality before proceeding to planning
**Created**: 2026-09-08 (amended 2026-09-09)
**Feature**: [spec.md](../spec.md)

## Content Quality

- [x] No implementation details (languages, frameworks, APIs) — *exception: binding data contract per the CONTRACT NOTE, mirroring the backend OpenAPI document (Constitution IX); same accepted pattern as specs/044-reports-group*
- [x] Focused on user value and business needs
- [x] Written for non-technical stakeholders
- [x] All mandatory sections completed

## Requirement Completeness

- [x] No [NEEDS CLARIFICATION] markers remain — *all open items are tracked as OQ-N1..N4 engineering entries per sub-module; stakeholder Q1–Q3 resolved and adopted into canonical spec*
- [x] Requirements are testable and unambiguous
- [x] Success criteria are measurable
- [x] Success criteria are technology-agnostic (no implementation details)
- [x] All acceptance scenarios are defined
- [x] Edge cases are identified
- [x] Scope is clearly bounded (request-first and order-first paths documented; group boundaries in Shared Group Context)
- [x] Dependencies and assumptions identified

## Feature Readiness

- [x] All functional requirements have clear acceptance criteria
- [x] User scenarios cover primary flows
- [x] Feature meets measurable outcomes defined in Success Criteria
- [x] No implementation details leak into specification — *see Content Quality exception above; contract fields/routes/enums are binding requirements per the CONTRACT NOTE*

## Notes

- Validation iteration 1 (amendment): all items pass; spec updated with request-first model.
- Stakeholder clarifications Q1–Q3 from `feature-context-request-first-disbursement.md` adopted into Clarifications > Session 2026-09-09.
- Clarification session 2026-09-09: Q4 (request-first order follows full lifecycle with budget check), Q5 (PaymentOrder beneficiary model), Q6 (fund/appropriation deferred to order preparation).
- **ADR-001 schema simplification** (2026-09-09): six structural conflicts resolved:
  - D-1: BeneficiaryName only (no Party ID) — BeneficiaryId removed from DisbursementRequest, BeneficiaryPartyId removed from PaymentOrder.
  - D-2: DisbursementRequestId UNIQUE on PaymentOrder — PaymentOrderId removed from DisbursementRequest.
  - D-3: Budget check and HasWarning removed from request — check runs only at order submit.
  - D-4: PaymentOrderLine removed — AccountId added (optional in Draft, mandatory at Submit).
  - D-5: Approval data (ApprovedAmount, IssuingAuthority, ApprovalDate) removed from DisbursementRequest — stored in ApprovalHistory only.
  - D-6: Payment links via PaymentOrderId UNIQUE — request link derived from order.
- PAY-02 rewritten: independent request creation, amount approval, issuing authority, atomic order generation.
- PAY-01 updated: two creation paths, 8 states (PartiallyPaid removed), vendorId removed, AccountId added.
- PAY-03 updated: single payment per order (PaymentOrderId UNIQUE).
- data-model.md updated per ADR-001: all schema changes documented.
- GAP-ADD permission codes (PaymentOrders.*, DisbursementRequests.*) must be added in the same feature (FR-G02).
- Open questions (OQ-N3, OQ-N4 across sub-modules) are engineering/planning items — non-blocking; resolved during plan/tasks.
