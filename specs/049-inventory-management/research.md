# Research: Inventory Management

**Date**: 2026-09-11

## R1: Application Layer Pattern

**Decision**: Follow Parties module pattern — Command/Handler/Validator in single file per action, Query/Handler in separate file.

**Rationale**: Established codebase convention. Verified as-built in `src/Application/Parties/Commands/CreateParty/CreatePartyCommand.cs`.

**Alternatives considered**:
- Separate handler files — not used in this codebase

## R2: Web Endpoint Pattern

**Decision**: Follow Parties endpoint pattern — IEndpointGroup with static Map method, request/response records in same file, mapping extensions.

**Rationale**: Established codebase convention. Verified as-built in `src/Web/Endpoints/Parties/Parties.cs`.

**Alternatives considered**:
- Controllers — prohibited by constitution (Minimal APIs only)

## R3: Frontend Feature Structure

**Decision**: Follow procurement feature pattern — entity-scoped subfolders under `features/inventory/`, each with pages/, hooks/, shared/ (types.ts, schemas.ts).

**Rationale**: Established codebase convention per AGENTS.md layer map.

**Alternatives considered**:
- Flat structure — rejected, doesn't match codebase convention

## R4: Code Generation for Items

**Decision**: Use IDocumentSequenceService to generate item codes (prefix "ITEM-{D6}").

**Rationale**: Per AGENTS.md: "Document numbering: IDocumentSequenceService — {PREFIX}-{D6}, allocated in the same transaction, number at Draft creation."

**Alternatives considered**:
- User-entered codes — rejected, sequential numbering ensures uniqueness

## R5: ItemType Enum Storage

**Decision**: Store as string in database (existing schema), use C# enum in Application layer for validation.

**Rationale**: Entity already has `string ItemType`. Changing to enum would require migration. String storage with enum validation is the established pattern (see PartyType).

**Alternatives considered**:
- Add migration to change to int enum — rejected, spec says no schema changes unless necessary
