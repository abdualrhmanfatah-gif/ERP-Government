# API Contracts: Asset Management Module

**Feature**: 058-asset-module-spec | **Date**: 2026-09-15

OpenAPI (nswag-generated) is the contract source; frontend clients are regenerated from it (Principle IX). All failures use the unified problem-details contract (Principle XIII, `docs/error-handling.md`) with stable error codes and trace identifiers. Every mutating request round-trips `rowVersion` (Principle IX). List endpoints use the standard pagination envelope (`items`, `totalCount`, `page`, `pageSize`).

Permissions follow the `PermissionCodes` convention resolved in R6.

## Endpoint Group: `/api/AssetGroups` (re-pointed, 054)

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/api/AssetGroups` | AssetGroups.View | PaginatedList<AssetGroupResponse> |
| GET | `/api/AssetGroups/{id}` | AssetGroups.View | AssetGroupDetailResponse (incl. attribute bindings) |
| POST | `/api/AssetGroups` | AssetGroups.Create | int |
| PUT | `/api/AssetGroups/{id}` | AssetGroups.Update | Result |
| POST | `/api/AssetGroups/{id}/deactivate` | AssetGroups.Deactivate | Result |
| POST | `/api/AssetGroups/{id}/activate` | AssetGroups.Activate | Result |

Account fields in payloads: `assetAccountId?`, `depreciationAccountId?`, `disposalAccountId?`, `revaluationAccountId?`, `impairmentLossAccountId?` — the three removed fields (C5) do NOT appear.

## Endpoint Group: `/api/AssetAttributes`

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/api/AssetAttributes/definitions` | AssetGroups.View | List<AttributeDefinitionResponse> |
| POST | `/api/AssetAttributes/definitions` | AssetGroups.Create | int |
| PUT | `/api/AssetAttributes/definitions/{id}` | AssetGroups.Update | Result |
| PUT | `/api/AssetGroups/{id}/attributes` | AssetGroups.Update | Result (set bindings: definitionId, isRequired, sortOrder?) |

## Endpoint Group: `/api/Assets` (re-pointed, 055)

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/api/Assets` | Assets.View | PaginatedList<AssetResponse> (filters: search, status, groupId, locationId, employeeId, costCenterId, fundId) |
| GET | `/api/Assets/{id}` | Assets.View | AssetDetailResponse (incl. attribute values, derived currentDepartment) |
| POST | `/api/Assets` | Assets.Create | int |
| PUT | `/api/Assets/{id}` | Assets.Update | Result |
| POST | `/api/Assets/{id}/deactivate` | Assets.Update | Result |
| GET | `/api/Assets/{id}/transactions` | Assets.View | List<TransactionSummaryResponse> (audit trail view, US6) |

Custodian payload field: `employeeId?` (Q1). `currentDepartment` is read-only and derived.

## Endpoint Group: `/api/AssetTransfers`

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/api/AssetTransfers` | AssetTransfers.View | PaginatedList<TransferResponse> |
| GET | `/api/AssetTransfers/{id}` | AssetTransfers.View | TransferDetailResponse |
| POST | `/api/AssetTransfers` | AssetTransfers.Create | int (draft) |
| PUT | `/api/AssetTransfers/{id}` | AssetTransfers.Create | Result (edit draft; rowVersion) |
| POST | `/api/AssetTransfers/{id}/execute` | AssetTransfers.Execute | Result (validates staleness FR-052; executes transfer + card update atomically) |

No posting step — transfers carry no GL effect.

## Endpoint Groups: `/api/AssetDisposals`, `/api/AssetRevaluations`, `/api/AssetImpairments`

Identical shape per group (existing permission groups):

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/{group}` | {Group}.View | PaginatedList<{Group}Response> |
| GET | `/{group}/{id}` | {Group}.View | {Group}DetailResponse |
| POST | `/{group}` | {Group}.Create | int (draft; validation per FR-060/070/080) |
| PUT | `/{group}/{id}` | {Group}.Create | Result (edit draft) |
| POST | `/{group}/{id}/approve` | {Group}.Approve | Result |
| POST | `/{group}/{id}/post` | {Group}.Post | Result — synchronous gates (FR-094a/IV); on success the document enters `Posting` and the entry link appears when the consumer completes |
| POST | `/api/AssetImpairments/{id}/reverse` | AssetImpairments.Post | Result (creates linked reversal transaction, FR-080/100) |

## Endpoint Group: `/api/AssetDepreciation`

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/api/AssetDepreciation/schedules` | AssetDepreciation.View | PaginatedList<ScheduleResponse> (filters: assetId, fiscalYearId, status) |
| POST | `/api/AssetDepreciation/run` | AssetDepreciation.Run | RunResult (records created with snapshots; duplicate normal-run rejected FR-094) |
| POST | `/api/AssetDepreciation/schedules/{id}/post` | AssetDepreciation.Post | Result (gates FR-094a; aggregated entry per Q3: B) |
| POST | `/api/AssetDepreciation/schedules/{id}/reverse` | AssetDepreciation.Reverse | Result (new linked record from ORIGINAL entry's accounts, FR-100) |
| GET | `/api/AssetDepreciation/preview` | AssetDepreciation.View | List<SchedulePreview> (no persistence) |

## Endpoint Group: `/api/AssetCounts`

| Verb | Route | Permission | Produces |
|------|-------|------------|----------|
| GET | `/api/AssetCounts` | AssetCounts.View | PaginatedList<CountResponse> (scope label shown) |
| GET | `/api/AssetCounts/{id}` | AssetCounts.View | CountDetailResponse (lines + discrepancy summary) |
| POST | `/api/AssetCounts` | AssetCounts.Create | int — scope validated per FR-111/111a; blocked if permissions cannot cover the declared scope |
| POST | `/api/AssetCounts/{id}/start` | AssetCounts.Execute | Result — freezes scope + generates lines (set-based, R11) |
| PUT | `/api/AssetCounts/{id}/lines/{lineId}` | AssetCounts.Execute | Result — records observation; updates the single line (UNIQUE count+asset); audit-trailed (FR-116); rowVersion on line AND header |
| POST | `/api/AssetCounts/{id}/complete` | AssetCounts.Execute | Result — blocked while any line NotExamined (FR-114) |
| POST | `/api/AssetCounts/{id}/review` | AssetCounts.Review | Result (reviewer sign-off per current procedure) |

## Representative payloads

**POST /api/AssetCounts** (scope semantics, FR-111):

```json
{ "countDate": "2026-09-15", "countType": "سنوي", "locationId": null, "departmentId": null, "notes": null }
```

`null` + `null` = entity-wide «جميع المواقع — جميع الإدارات», bounded by the caller's permissions; a partial-coverage attempt returns `409` with code `ASSET-COUNT-SCOPE-COVERAGE`.

**PUT /api/AssetCounts/{id}/lines/{lineId}** (re-examination, FR-116):

```json
{ "isFound": 2, "physicalLocationId": 3, "physicalEmployeeId": 7, "physicalStatus": "تالف", "discrepancyNotes": "غير موجود في الموقع النظامي", "rowVersion": "AAAAAA==" }
```

`isFound`: 0 not examined, 1 found, 2 not found. Duplicate line insertion is impossible (UNIQUE constraint); concurrent overwrites conflict via `rowVersion` (409, `CONCURRENCY-CONFLICT`).

## Error contract

All expected rejections return problem-details with stable codes, e.g. `ASSET-POST-GATE-FAILED` (missing group account / incomplete template / undefined substitution role / unbalanced entry — FR-094a), `ASSET-DUPLICATE-RUN`, `ASSET-COUNT-SCOPE-COVERAGE`, `ASSET-COUNT-LINES-PENDING`, `ASSET-DISPOSAL-DUPLICATE`, `CONCURRENCY-CONFLICT`. Unexpected faults return 500 with a trace identifier and no internal details (XIII).
