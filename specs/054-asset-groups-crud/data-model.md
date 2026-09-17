# Data Model: Asset Groups CRUD

**Feature**: 054-asset-groups-crud
**Date**: 2026-09-14

## Entity: AssetGroup (existing — no schema changes)

**Table**: `AssetGroups`

| Column | Type | Nullable | Constraints | Notes |
|--------|------|----------|-------------|-------|
| `Id` | int | NO | PK, IDENTITY(1,1) | BaseEntity |
| `Code` | nvarchar(50) | NO | UNIQUE INDEX | Natural key, user-entered |
| `Name` | nvarchar(200) | NO | | Arabic display name |
| `Description` | nvarchar(500) | YES | | Optional description |
| `ParentAssetGroupId` | int | YES | FK → AssetGroups.Id, ON DELETE RESTRICT | Self-referencing parent |
| `AccountAssetId` | int | YES | FK → Accounts.Id | GL: Asset account |
| `AccountAccumulatedDepreciationId` | int | YES | FK → Accounts.Id | GL: Accumulated depreciation |
| `AccountExpenseId` | int | YES | FK → Accounts.Id | GL: Depreciation expense |
| `AccountDisposalId` | int | YES | FK → Accounts.Id | GL: Disposal account |
| `AccountRevaluationId` | int | YES | FK → Accounts.Id | GL: Revaluation/gain on disposal |
| `AccountImpairmentId` | int | YES | FK → Accounts.Id | GL: Impairment/loss on disposal |
| `DepreciationMethod` | nvarchar(50) | NO | | e.g., "StraightLine", "DecliningBalance" |
| `DepreciationRate` | decimal(18,4) | YES | | Optional depreciation rate |
| `DefaultUsefulLifeYears` | int | YES | | Default useful life for new assets |
| `ResidualValuePercentage` | decimal(5,2) | YES | | Salvage value percentage |
| `IsDepreciable` | bit | NO | DEFAULT 1 | Whether assets in this group are depreciable |
| `AssetCategory` | nvarchar(50) | NO | | "Tangible" or "Intangible" |
| `IsActive` | bit | NO | DEFAULT 1 | Soft-delete flag |
| `RowVersion` | rowversion | NO | OPTIMISTIC CONCURRENCY | Auto-generated |
| `Created` | datetimeoffset | NO | | Audit — creation timestamp |
| `CreatedBy` | nvarchar(256) | YES | | Audit — creator user ID |
| `LastModified` | datetimeoffset | NO | | Audit — last modification timestamp |
| `LastModifiedBy` | nvarchar(256) | YES | | Audit — last modifier user ID |

## Relationships

```
AssetGroup ──┬── ParentAssetGroup (self-ref, 0..1 → 0..*)
             └── Assets (1 → 0..*)
```

- `AssetGroup.ParentAssetGroupId` → `AssetGroup.Id` (Restrict)
- `Asset.AssetGroupId` → `AssetGroup.Id` (Restrict)

## Validation Rules (from spec + entity)

| Rule | Where Enforced | Error Message |
|------|---------------|---------------|
| Code required, max 50 chars | Create/Update handler | "كود المجموعة مطلوب" |
| Code unique | Create handler + DB index | "كود المجموعة مستخدم بالفعل" |
| Name required, max 200 chars | Create/Update handler | "اسم المجموعة مطلوب" |
| ParentAssetGroupId ≠ self | Create/Update handler | "لا يمكن أن تكون المجموعة والدتها" |
| No ancestor cycle | Create/Update handler | "لا يمكن إنشاء حلقة في التسلسل الهرمي" |
| Max depth 3 levels | Create/Update handler | "تم تجاوز الحد الأقصى لمستويات التصنيف" |
| Cannot deactivate if assets linked | ToggleActive handler | "لا يمكن تعطيل مجموعة مرتبطة بأصول" |
| Cannot activate already active | ToggleActive handler | "المجموعة مفعلة بالفعل" |
| Cannot deactivate already inactive | ToggleActive handler | "المجموعة معطلة بالفعل" |
| Cannot edit inactive group | Update handler | "يجب تفعيل المجموعة قبل التعديل" |
| DepreciationMethod required | Create/Update handler | "طريقة الإهلاك مطلوبة" |
| AssetCategory required | Create/Update handler | "فئة الأصول مطلوبة" |
| RowVersion must match | Update/ToggleActive handler | "تم تعديل المجموعة من مستخدم آخر" |

## State Transitions

```
[Created] → IsActive=true ──→ [Active]
                 │
                 ↓ (deactivate)
           IsActive=false ──→ [Inactive] ──→ (activate) ──→ [Active]
```

- Active ↔ Inactive (toggle)
- No hard delete
- Inactive groups cannot be edited (must activate first)

## Seed Data (existing)

| Code | Name | DepreciationMethod | DefaultUsefulLifeYears | AssetCategory |
|------|------|-------------------|----------------------|---------------|
| AG-001 | مباني وإنشاءات | StraightLine | 50 | Tangible |
| AG-002 | أثاث ومعدات | StraightLine | 10 | Tangible |
| AG-003 | مركبات | StraightLine | 5 | Tangible |

## Migration

No migration needed — entity and table already exist. The only schema-adjacent change is adding two permission constants to `PermissionCodes.cs` (code-only, not database).
