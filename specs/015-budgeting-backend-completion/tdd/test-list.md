# TDD Test List — Budgeting Backend Completion

**Feature**: specs/015-budgeting-backend-completion
**Generated from**: tasks.md unchecked test tasks
**Status**: GREEN phase — all tests written and passing

## Execution Rules

1. Each test task must be observed FAILING before its implementation task starts
2. Tests are never weakened, skipped, or deleted to reach green
3. Parallel tasks [P] within a story can run concurrently
4. Mark task `[X]` in tasks.md only after test is green (implementation done)

---

## US1 — Unique Document Numbers

| Task | Type | Description | Status |
|------|------|-------------|--------|
| T015 | Unit | Typed sequence failures — unknown document type and inactive sequence throw typed exceptions | ✓ GREEN |
| T016 | Functional | N parallel appropriation creates yield N distinct APR numbers, zero unique-index collisions | ✓ GREEN |
| T017 | Unit | CreateBudgetCommand no longer exposes BudgetNumber; validator rejects supplied number | ✓ GREEN |
| T018 | Functional | Number immutability — approve/activate/update leaves BudgetNumber and AppropriationNumber unchanged | ✓ GREEN |

---

## US2 — Encumbrance Full Stack

| Task | Type | Description | Status |
|------|------|-------------|--------|
| T023 | Unit | Encumbrance FSM guard matrix (legal/illegal transitions per data-model.md; reverse only from Active\|Suspended; update/delete Draft-only) | ✓ GREEN |
| T024 | Unit | Create gate decisions via EvaluateControlMethod — None/Warning/Blocking + CreateEncumbranceCommandValidator | ✓ GREEN |
| T025 | Functional | Full lifecycle with exactly one ApprovalHistory row per transition ("Encumbrance", "from -> to") | ✓ GREEN |
| T026 | Functional | Reversal — new negative row with ReversalOfId, original → Reversed, isReversed computed true, availability reduced then restored | ✓ GREEN |
| T027 | Functional | Authorization — 401 unauthenticated and 403 without permission for every /api/Encumbrances route | ✓ GREEN |

---

## US3 — Transfer Appropriation Pairs

| Task | Type | Description | Status |
|------|------|-------------|--------|
| T038 | Unit | Transfer validation — same Budget, source ≠ target, source net active appropriation ≥ amount, amount > 0, Transfer stays Draft-only | ✓ GREEN |
| T039 | Functional | Pair atomicity (failure → neither row), joint status transitions, net-zero item availability with active pair | ✓ GREEN |

---

## US4 — Availability Query API

| Task | Type | Description | Status |
|------|------|-------------|--------|
| T044 | Functional | Availability endpoints — exact figures, EffectiveAllowOverrun precedence, 404 unknown ids | ✓ GREEN |

---

## US5 — Budget Head Fields + Monthly Plans

| Task | Type | Description | Status |
|------|------|-------------|--------|
| T049 | Unit | Budget head mapping — create/update accept TotalAmount, AllowOverrun?, EffectiveFrom, EffectiveTo?, Description | ✓ GREEN |
| T050 | Unit | Monthly plan validation — month 1–12, PlannedAmount ≥ 0, no duplicate months, batch ≤ 12 | ✓ GREEN |
| T051 | Functional | PUT monthly plan replaces idempotently; GET returns current plan; UNIQUE enforced | ✓ GREEN |

---

## US6 — Appropriation Edge Cases

| Task | Type | Description | Status |
|------|------|-------------|--------|
| T057 | Functional | Appropriation edge-case gaps — activate gate Blocking/Warning/None, reduction overflowing availability rejected, concurrency conflict surfaced distinctly | ✓ GREEN |

---

## Summary

- **Total test tasks**: 16
- **Completed**: 16
- **Remaining**: 0
- **Status**: ALL GREEN
