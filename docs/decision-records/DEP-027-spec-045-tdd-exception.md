# DEP-027: Spec 045 TDD Exception

**Date**: 2026-09-09
**Status**: Accepted
**Decider**: Product stakeholder
**Applies To**: `specs/045-payments-group/` only
**Implements**: Constitution Principle XII exception process

## Context

Spec 045 restructures the existing Payments group while much of its backend and frontend surface already exists. The stakeholder explicitly chose to exempt this specification from test-first development and from adding new automated tests. Applying the repository-wide TDD rule to this feature would expand its scope beyond the requested implementation workflow.

## Decision

Spec 045 does not require red-green-refactor cycles or new automated test cases. Implementation uses scoped builds, frontend lint/build, manual quickstart scenarios, and the existing five backend test projects as regression gates. Existing tests remain intact: assertions are not weakened, skipped, or deleted to obtain a passing result. Test and fixture maintenance required to keep existing behavior valid remains in scope.

## Scope

- **IN**: implementation tasks belonging to `specs/045-payments-group/`.
- **OUT**: every other specification, defect fix, and feature; normal backend TDD remains mandatory there.

## Consequences and Remediation

Spec 045 gains no new automated coverage for its new business behavior. Manual quickstart evidence must cover request creation, dual approval, atomic order creation, budget blocking, single payment, audit history, concurrency refusal, and migration reconciliation before convergence. Existing backend suites still gate convergence and merge. A later testing-hardening specification may add automated coverage without reopening this decision.
