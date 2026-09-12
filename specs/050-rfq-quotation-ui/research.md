# Research: RFQ & Quotation Management

**Feature**: 050-rfq-quotation-ui
**Date**: 2026-09-11

## R1: Existing Command/Query Inventory

**Decision**: Reuse all existing commands and queries. Add only UpdateQuotation and UpdateRFQ.

**Rationale**: Exploration confirmed all 5 RFQ commands and 7 Quotation commands already exist with working handlers. GetQuotationsQuery and GetQuotationByIdQuery both exist. Only endpoint wiring and 2 new commands are needed.

**Alternatives considered**:
- Rewrite all commands from scratch → Rejected: wasteful, existing handlers are tested and functional
- Extend Create commands for updates → Rejected: cleaner separation with dedicated Update commands (per clarification Q2)

## R2: Endpoint Wiring Gaps

**Decision**: Fix 6 endpoint gaps in Quotations.cs and 2 in RequestForQuotations.cs.

**Rationale**: The exploration found:
- `GET /api/Quotations` — not wired (GetQuotationsQuery exists but no route)
- `GET /api/Quotations/{id}` — stub returns `new { Id = id }` instead of dispatching query
- `PATCH /api/Quotations/{id}/submit` — command exists, no route
- `PATCH /api/Quotations/{id}/reject` — command exists, no route
- `PUT /api/Quotations/{id}` — new UpdateQuotation command needed
- `PATCH /api/RequestForQuotations/suppliers/{id}/response` — RecordRFQResponse exists, no route
- `PUT /api/RequestForQuotations/{id}` — new UpdateRFQ command needed

**Alternatives considered**:
- Create entirely new endpoint files → Rejected: existing files are partially wired, extending is idiomatic

## R3: FluentValidation Validators

**Decision**: Create CreateRFQCommandValidator and CreateQuotationCommandValidator (and UpdateQuotationCommandValidator, UpdateRFQCommandValidator).

**Rationale**: Constitution Principle III requires server-side business rule enforcement. Only CreatePurchaseRequestCommandValidator exists. RFQ and Quotation commands lack validators.

**Alternatives considered**:
- Inline validation in handlers → Rejected: violates existing pattern, FluentValidation is the project standard
- Skip validators (handlers already check) → Rejected: constitution requires explicit validation layer

## R4: Frontend Component Architecture

**Decision**: Feature-scoped pages in `features/procurement/`, shared components in `src/components/` with domain prefix.

**Rationale**: AGENTS.md convention: "NO components inside features; any feature-specific component goes in src/components/ with ProcurementRFQ* or ProcurementQuotations* prefix."

**Alternatives considered**:
- Components inside feature folders → Rejected: violates AGENTS.md architectural rule
- All components in shared/components → Rejected: domain-prefixed components belong in src/components/

## R5: UpdateQuotation Command Design

**Decision**: New UpdateQuotationCommand with same field shape as CreateQuotationCommand plus Id, reusing QuotationLineDto for lines.

**Rationale**: Follows existing pattern (CreatePurchaseRequestCommand vs UpdatePurchaseRequestCommand). Handler validates status == Draft before allowing update. Details are replaced atomically (delete existing, insert new).

**Alternatives considered**:
- Partial update (only changed fields) → Rejected: adds complexity, full replace is simpler and matches create pattern
- Merge logic in Create handler → Rejected: violates single-responsibility, cleaner as separate command

## R6: UpdateRFQ Command Design

**Decision**: New UpdateRFQCommand allowing modification of DeadlineDate, CurrencyCode, TermsAndConditions, Notes, and supplier list while in Draft status.

**Rationale**: RFQ edit page needs a backend endpoint. Handler validates status == Draft. Supplier list is replaced atomically (existing suppliers removed, new list inserted).

**Alternatives considered**:
- Separate AddSupplier/RemoveSupplier commands → Rejected: over-engineered for draft editing; full replace is sufficient

## R7: Frontend Data Fetching Pattern

**Decision**: Use TanStack Query with NSwag-generated clients. Manual wrapper in `shared/client.ts` only for custom retry or request composition.

**Rationale**: Follows AGENTS.md API Strategy. NSwag generates typed clients from OpenAPI. TanStack Query handles caching, loading states, error states.

**Alternatives considered**:
- Manual fetch with custom hooks → Rejected: loses caching and automatic refetch benefits
- MSW/mock servers → Rejected: explicitly prohibited by AGENTS.md

## R8: Zod Schema Design

**Decision**: Create `features/procurement/request-for-quotations/shared/schemas.ts` and `features/procurement/quotations/shared/schemas.ts` with Zod schemas for create/edit forms.

**Rationale**: AGENTS.md convention: "every form has a Zod schema in features/<domain>/<entity>/shared/schemas.ts; same schema reused for defaultValues + validation."

**Alternatives considered**:
- Co-locate schemas with form components → Rejected: violates established pattern
- Shared schemas across features → Rejected: features must not import from other features
