# Implementation Plan: Unified Party + Document Infrastructure

**Branch**: `016-unified-party-document` | **Date**: 2026-09-05 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/016-unified-party-document/spec.md`

## Summary

Unify the Party concept by absorbing the legacy Supplier entity into a new Party entity, extend ApprovalHistory into the sole approval source with an Action enum and step tracking, add append-only DocumentStatusLog for audit trails, formalize attachments with mandatory requirements, extend DocumentSequenceService prefixes, and strip inline approval columns from PaymentOrder (if present).

## Technical Context

**Language/Version**: C# 13 / .NET 9 (SDK 9.0.x)

**Primary Dependencies**: ASP.NET Core Minimal API, Entity Framework Core 9, MediatR (CQRS), .NET Aspire, FluentValidation

**Storage**: SQL Server (EF Core migrations, `ApplicationDbContext`)

**Testing**: xUnit, FluentAssertions, NSubstitute, Testcontainers (Infrastructure.IntegrationTests), ASP.NET WebApplicationFactory (Application.FunctionalTests), Playwright (Web.AcceptanceTests)

**Target Platform**: Linux server (containerized via Aspire), Windows dev

**Project Type**: Web service (Clean Architecture: Domain → Application → Infrastructure → Web)

**Performance Goals**: Party CRUD <500ms for 10k records; attachment gate <100ms; sequence generation atomic under concurrency

**Constraints**: Arabic-first RTL UI; bilingual data fields; all FK referential actions MUST be Restrict; no cascading deletes; optimistic concurrency on all mutable records

**Scale/Scope**: Government ERP — moderate user count (hundreds), high data integrity requirements, financial audit compliance

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. Layered Architecture | ✅ PASS | New entities in Domain, use cases in Application, endpoints in Web |
| II. Bounded Contexts | ✅ PASS | Party in new Parties module; ApprovalHistory/Attachment in Security; DocumentStatusLog in Security |
| III. Server-Side Rules | ✅ PASS | Attachment gate enforced in use cases; backfill logic in migration |
| IV. Financial Integrity | ✅ PASS | No financial posting changes; approval history preserves audit trail |
| V. Budget Control | ✅ PASS | Attachment gate wired into Budget/Appropriation approval; budget check flow unchanged |
| VI. Data Integrity | ⚠️ REQUIRES DECISION RECORD | Deleting Suppliers module requires decision record per module registry constraint |
| VII. Authorization | ✅ PASS | New endpoints declare permissions; approval history records actor |
| VIII. Approval Workflows | ✅ PASS | ApprovalHistory becomes sole source; Action enum + step tracking; Decision string retained as legacy |
| IX. API Contract | ✅ PASS | New endpoints follow IEndpointGroup pattern; OpenAPI auto-generated |
| X. UI Design System | ✅ PASS | No frontend changes in this feature (API-only) |
| XI. Testing | ✅ PASS | TDD required; functional tests for migration, backfill, gate, append-only |
| XII. Controlled Change | ⚠️ REQUIRES DECISION RECORD | Suppliers module removal is a module registry change |

**Gate Result**: CONDITIONAL PASS — Two decision records required before implementation:
1. **DR-001**: Suppliers module removal (Constitution Principle XII, Module Registry constraint)
2. Registered exception #1 (open endpoint authorization policies) applies to new Party endpoints — use-case-layer authorization is effective control until RBAC remediation.

## Project Structure

### Documentation (this feature)

```text
specs/016-unified-party-document/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── parties.md
│   ├── documents.md
│   └── document-attachment-requirements.md
├── checklists/
│   └── requirements.md  # Spec quality checklist
├── spec.md              # Feature specification
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── Domain/
│   ├── Parties/                          # NEW module
│   │   ├── Entities/
│   │   │   └── Party.cs
│   │   └── Enums/
│   │       └── PartyType.cs
│   ├── Security/
│   │   ├── Entities/
│   │   │   ├── ApprovalHistory.cs        # MODIFY: add ApprovalStep, Action
│   │   │   ├── Attachment.cs             # MODIFY: add DocumentType, IsRequired, AttachmentTypeCode
│   │   │   └── DocumentStatusLog.cs     # NEW entity
│   │   └── Enums/
│   │       └── ApprovalAction.cs         # NEW enum
│   ├── Security/
│   │   └── Entities/
│   │       └── DocumentAttachmentRequirement.cs  # NEW entity
│   ├── FinancialSettings/
│   │   └── Services/
│   │       └── DocumentSequenceService.cs  # MODIFY: add prefixes
│   └── Suppliers/                        # DELETE (entire module)
├── Application/
│   ├── Parties/                          # NEW module
│   │   ├── Commands/
│   │   │   ├── CreateParty/
│   │   │   ├── UpdateParty/
│   │   │   └── TogglePartyActive/
│   │   ├── Queries/
│   │   │   ├── GetParties/
│   │   │   └── GetPartyById/
│   │   └── Common/
│   │       └── IDocumentStatusLogger.cs  # NEW interface
│   ├── Security/
│   │   └── Common/
│   │       ├── ApprovalService.cs        # MODIFY: add Action + step + status log
│   │       └── DocumentStatusLogger.cs   # NEW implementation
│   ├── Documents/                        # NEW module
│   │   ├── Queries/
│   │   │   ├── GetDocumentApprovals/
│   │   │   ├── GetDocumentStatusLog/
│   │   │   └── GetDocumentAttachments/
│   │   └── Commands/
│   │       ├── UploadAttachment/
│   │       └── DeleteAttachment/
│   └── DocumentAttachmentRequirements/   # NEW module
│       ├── Commands/
│       └── Queries/
├── Infrastructure/
│   ├── Data/
│   │   ├── Configurations/
│   │   │   ├── PartyConfiguration.cs        # NEW
│   │   │   ├── DocumentStatusLogConfiguration.cs  # NEW
│   │   │   └── DocumentAttachmentRequirementConfiguration.cs  # NEW
│   │   └── Migrations/
│   │       └── AddPartyAndDocumentInfrastructure.cs  # NEW migration
│   └── Services/
│       └── LocalFileStorageService.cs     # NEW (attachment storage)
├── Web/
│   └── Endpoints/
│       ├── Parties/                       # NEW endpoint group
│       │   └── Parties.cs
│       ├── Documents/                     # NEW endpoint group
│       │   └── Documents.cs
│       └── DocumentAttachmentRequirements/  # NEW endpoint group
│           └── DocumentAttachmentRequirements.cs
└── tests/
    ├── Application.FunctionalTests/
    │   ├── Parties/                       # NEW
    │   └── Documents/                     # NEW
    └── Infrastructure.IntegrationTests/
        └── Migrations/                    # NEW: migration tests
```

**Structure Decision**: New `Parties` module follows existing bounded context convention. Security module extended with DocumentStatusLog and DocumentAttachmentRequirement (shared document infrastructure). PaymentOrder approval refactor stays within Payments module.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| Suppliers module deletion (DR-001) | Unified Party absorbs all Supplier functionality; maintaining two parallel entities creates data inconsistency and migration complexity | Merging in-place would require preserving the Supplier schema while adding Party columns, doubling query complexity for no benefit |
| Registered Exception #1 applies to new endpoints | Party and Document endpoints inherit the existing open authorization placeholder; use-case-layer authorization is the effective control | Adding RBAC now would scope-creep beyond the feature boundary; tracked for remediation |
