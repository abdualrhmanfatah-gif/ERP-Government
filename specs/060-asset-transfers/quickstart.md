# Quickstart: Asset Transfers

## Backend verification

```bash
dotnet build src/Web/Web.csproj
dotnet test tests/Application.UnitTests --filter AssetTransfers
```

Expected: build clean; all transfer tests green.

## Manual UI flow

1. Run the app: `dotnet run --project src/AppHost`
2. Open `الأصول ← نقل الأصول` (`/assets/transfers`) — list loads (empty or seeded).
3. Click «نقل جديد»:
   - pick an active asset (search) → from-values shown from the card
   - pick a destination location and/or employee (at least one different)
   - save → lands on the detail page with status «مسودة» and a `TRF-` number
4. On the detail page:
   - «تعديل» → change date/destination → save
   - «تنفيذ» → confirm → status «منفذة»; open the asset card and verify location/custodian changed and the transfer appears in its history with no journal link
   - repeated confirm on an executed transfer → same result, no second effect
5. Create a second draft, then «إلغاء» → status «ملغاة», card untouched.
6. Cancel/execute on the wrong status → rejected with an Arabic message.

## Conflict checks

1. Create a draft for an asset.
2. From another tab/session, edit the asset's location or custodian.
3. Execute the draft → 409 «تغيّرت بيانات الأصل…» and the card is NOT overwritten; open the draft in edit mode (re-snapshots from-values), save, execute → succeeds.
