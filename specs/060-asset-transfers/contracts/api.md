# API Contract: Asset Transfers

Base route: `/api/AssetTransfers` (minimal API group, `AssetTransfers` endpoint class).

All failures follow the unified ProblemDetails contract (`code`, Arabic `detail`, 400/404/409/500).

## GET /api/AssetTransfers

Permission: `AssetTransfers.View`

Query: `search?`, `status?` (`Draft|Executed|Cancelled|All`), `page=1`, `pageSize=20`

Response `200`:
```json
{
  "items": [
    {
      "id": 12,
      "documentNumber": "TRF-000051",
      "assetId": 3,
      "assetCode": "AST-000123",
      "assetName": "حاسوب محمول",
      "transactionDate": "2026-09-16",
      "status": "Executed",
      "fromLocationName": "المبنى أ",
      "toLocationName": "المبنى ب",
      "fromEmployeeName": "أحمد",
      "toEmployeeName": "سارة"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```
Search matches document number, asset code, asset name. Order: `TransactionDate desc, Id desc`.

## GET /api/AssetTransfers/{id}

Permission: `AssetTransfers.View`

Response `200`:
```json
{
  "id": 12,
  "documentNumber": "TRF-000051",
  "assetId": 3,
  "assetCode": "AST-000123",
  "assetName": "حاسوب محمول",
  "transactionDate": "2026-09-16",
  "status": "Draft",
  "currencyId": 1,
  "notes": null,
  "occurredAt": "2026-09-16T10:15:00+00:00",
  "fromLocationId": 2, "fromLocationName": "المبنى أ",
  "toLocationId": 5, "toLocationName": "المبنى ب",
  "fromEmployeeId": 7, "fromEmployeeName": "أحمد",
  "toEmployeeId": 9, "toEmployeeName": "سارة",
  "fromDepartmentId": 4, "fromDepartmentName": "إدارة تقنية المعلومات",
  "toDepartmentId": 6, "toDepartmentName": "إدارة المشتريات",
  "rowVersion": "AAAAAAAAB9E=",
  "assetRowVersion": "AAAAAAAAB9I="
}
```
`404` `Assets.TransferNotFound` when missing or when the transaction is not a Transfer.

## POST /api/AssetTransfers

Permission: `AssetTransfers.Create`

```json
{ "assetId": 3, "transactionDate": "2026-09-16", "toLocationId": 5, "toEmployeeId": null, "notes": "انتقال مكتب" }
```

- `201` with body `12` (created id); `Location: /api/AssetTransfers/12`
- `400` validation (`Assets.InvalidTransferDestination`, FluentValidation field errors)
- `404` `Assets.AssetNotFound` / `Inventory.LocationNotFound` / `Organization.EmployeeNotFound`
- `409` `Request.ConcurrencyConflict` (card changed while saving the draft)
- No card or ledger effect.

## PUT /api/AssetTransfers/{id}

Permission: `AssetTransfers.Create`

```json
{ "transactionDate": "2026-09-17", "toLocationId": 6, "toEmployeeId": 9, "notes": null, "rowVersion": "AAAAAAAAB9E=" }
```

- `200` with body `12`
- `400` validation; `404` not found; `409` concurrency / not a draft (`Assets.InvalidStatusTransition` when status ≠ Draft)
- Re-snapshots from-values from the card.

## POST /api/AssetTransfers/{id}/execute

Permission: `AssetTransfers.Execute`

```json
{ "rowVersion": "AAAAAAAAB9E=", "assetRowVersion": "AAAAAAAAB9I=" }
```

- `200` with body `12`
- Idempotent: if already `Executed`, returns `200` with the same id and no new effect
- `400` `Assets.InvalidStatusTransition` when `Cancelled`
- `404` `Assets.TransferNotFound`
- `409` `Request.ConcurrencyConflict` on token mismatch; `Assets.TransferSourceChanged` when the card no longer matches from-values
- Effects (one operation): card location/custodian updated, detail `OccurredAt` + from/to department snapshots finalized, status `Executed`

## POST /api/AssetTransfers/{id}/cancel

Permission: `AssetTransfers.Create`

Empty body.

- `200` with body `12`; status → `Cancelled`
- `400` `Assets.InvalidStatusTransition` when not `Draft`; `404` not found
- No card effect.
