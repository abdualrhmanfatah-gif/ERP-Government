# DEP-029: Uniform Error Handling and Recovery

**Date**: 2026-09-14
**Status**: Accepted governance standard; runtime implementation pending
**Implements**: Constitution XII; amendment 1.3.1 -> 1.4.0 (III, IX, new XIII)
**Remediation owner**: Maintainer implementing each affected use case and shared infrastructure path

## Context and decision

The [2026-09-14 audit](../error-handling-review-2026-09-14.md) found incompatible error payloads,
lost transport metadata, invisible form failures, false failures after successful writes, exposed
internal error messages, and diagnostic/recovery gaps. A single exception handler cannot fix
these paths because application rejection, HTTP delivery, UI presentation, and background recovery
have different owners.

Adopt structured application failures for expected rejection, recognized central validation/auth
exceptions, shared ProblemDetails conversion in Web, and one normalized frontend error contract.
The Constitution owns durable guarantees; [the guide](../error-handling.md) owns implementation
details; AGENTS.md provides short mandatory reading triggers. This keeps HTTP out of application
logic and prevents independently maintained copies of the same policy.

## Scope and migration

This change documents the standard and planning gate only. It does not change runtime code,
public payloads, schema, financial rules, or authorization enforcement. The guide's migration
sequence governs later implementation: inventory current callers, normalize legacy clients first,
introduce shared conversion, migrate endpoints with regenerated OpenAPI clients, then prove
presentation and durable recovery. Map work to existing specs rather than creating a duplicate.

Changing public error payloads is a contract migration under Principle IX. This decision records
the target shape and reason; each implementation plan must enumerate affected consumers and its
compatibility/remediation strategy before changing the wire format. Legacy adapters may remain
only as explicitly tracked migration work, not as a second target contract.

## Consequences and boundaries

- Existing gaps remain open defects; publication of this standard does not certify compliance or
  register a waiver. Closure requires path-specific evidence in the implementing spec/plan.
- Registered exceptions #1-#6 remain unchanged; DEP-027 still applies only to Spec 045. The
  frontend-testing conflict between Constitution XI and the explicit AGENTS.md override predates
  this decision; this amendment does not alter either. Follow the user-supplied frontend override
  for this work and record the governance discrepancy rather than adding frontend tests.
- DEP-028 continues to govern posting architecture. Recovery work must preserve its financial
  effects and idempotency; this decision does not restore retired posting machinery.
- Roll out compatible consumers before changing producers. If a producer rollout is reverted,
  retain the compatible normalizer so it can consume the previous payload. Never roll back by
  replaying financial work or rewriting financial/audit history.
- No new packages or schema changes are required by this documentation amendment. Any later
  persistence changes need a reviewed migration and database-schema documentation.
