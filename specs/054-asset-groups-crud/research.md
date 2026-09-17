# Research: Asset Groups CRUD

**Feature**: 054-asset-groups-crud
**Date**: 2026-09-14

## R1: Entity Field Mapping (Spec → Existing Entity)

**Decision**: Use the existing `AssetGroup` entity fields as-is. Map spec GL account names to existing FK fields.

| Spec GL Account Name | Existing Entity Field | Notes |
|---------------------|----------------------|-------|
| AssetAccount | AccountAssetId | int? FK to Account |
| AccumulatedDepreciationAccount | AccountAccumulatedDepreciationId | int? FK to Account |
| DepreciationExpenseAccount | AccountExpenseId | int? FK to Account |
| DisposalAccount | AccountDisposalId | int? FK to Account |
| GainOnDisposalAccount | AccountRevaluationId | int? FK to Account |
| LossOnDisposalAccount | AccountImpairmentId | int? FK to Account |
| MaintenanceExpenseAccount | *(not in entity)* | Spec assumes 7 accounts; entity has 6. Decision: use 6 existing fields. |

**Rationale**: The entity was designed before the spec. Adding a 7th account field requires an EF migration and is out of scope for F1. The 6 existing fields cover the core accounting needs. The "MaintenanceExpenseAccount" can be added in a future iteration if needed.

**Alternatives considered**:
- Add 7th field via migration → rejected as scope creep for F1
- Rename existing fields to match spec → rejected as entity already exists with these names

## R2: Depreciation Parameters Mapping

**Decision**: Map spec depreciation fields to existing entity fields.

| Spec Field | Entity Field | Notes |
|-----------|-------------|-------|
| DepreciationMethod | DepreciationMethod | string, required |
| DefaultUsefulLifeYears | DefaultUsefulLifeYears | int?, optional |
| DefaultSalvageValuePercentage | ResidualValuePercentage | decimal(5,2)?, optional |
| *(not in spec)* | DepreciationRate | decimal(18,4)?, optional — read-only in UI |
| *(not in spec)* | IsDepreciable | bool, default true — toggle in UI |
| *(not in spec)* | AssetCategory | string, required — dropdown (Tangible/Intangible) |

**Rationale**: Entity has additional fields not in the spec. These are already configured and seeded. Include them in the UI for completeness.

## R3: Permission Gaps

**Decision**: Add two new permission constants to `PermissionCodes.cs`.

```csharp
public const string AssetGroupsDeactivate = "AssetGroups.Deactivate";
public const string AssetGroupsActivate = "AssetGroups.Activate";
```

**Rationale**: Constitution Principle VII requires every endpoint to declare a required named permission. The existing permissions (View, Create, Update) don't cover toggle-active operations. Following the Account Groups pattern which uses separate toggle permission.

**Alternatives considered**:
- Reuse `AssetGroups.Update` for toggle → rejected; violates SoD principle
- No permission check → rejected; violates Constitution VII

## R4: Parent Selection UX

**Decision**: Dropdown list of all active groups (flat), not a tree picker.

**Rationale**: 
- Account Groups uses the same pattern (flat dropdown)
- Simpler to implement and test
- Tree picker adds complexity without clear value for ≤100 groups
- User can see hierarchy in the list/tree view before selecting

**Alternatives considered**:
- Tree picker component → rejected; complex, no existing component
- Nested dropdown → rejected; UX complexity

## R5: Cycle Prevention Algorithm

**Decision**: Walk up the parent chain from the new parent to root. If the current group's Id is encountered, reject.

**Rationale**: Standard algorithm for cycle prevention in self-referencing hierarchies. O(depth) where depth ≤ 3.

**Implementation**: In `CreateAssetGroupCommandHandler` and `UpdateAssetGroupCommandHandler`, after setting ParentId, walk up:
```
current = parent.ParentAssetGroupId
while current != null:
    if current == entity.Id → reject "لا يمكن أن تكون المجموعة אבא של نفسها"
    current = groups.Where(g => g.Id == current).Select(g => g.ParentAssetGroupId).FirstOrDefault()
```

## R6: Depth Limit Enforcement

**Decision**: Count ancestors from the new parent to root. If count ≥ 3, reject.

**Rationale**: Spec clarified 3-level max. Enforce at handler level.

**Implementation**: In Create/Update handlers:
```
depth = 1
current = parent.ParentAssetGroupId
while current != null:
    depth++
    current = groups.Where(g => g.Id == current).Select(g => g.ParentAssetGroupId).FirstOrDefault()
if depth > 3 → reject "تم تجاوز الحد الأقصى لمستويات التصنيف"
```

## R7: Deactivation Guard

**Decision**: Check `context.Assets.AnyAsync(a => a.AssetGroupId == groupId)` before deactivating.

**Rationale**: Spec FR-006 requires blocking deactivation if assets are linked. The FK is Restrict, so the check is explicit.

**Alternatives considered**:
- Rely on FK Restrict → rejected; gives confusing DB error instead of user-friendly message
- Soft-restrict with warning → rejected; spec says "blocked"

## R8: Reactivation Flow

**Decision**: Dedicated `POST /api/AssetGroups/{id}/activate` endpoint, separate from update.

**Rationale**: Spec clarified dedicated activate button. Follows Account Groups toggle-active pattern.

**Implementation**: `ToggleAssetGroupActiveCommand` handles both activate and deactivate via an `IsActive` parameter.

## R9: Tree View Data Structure

**Decision**: Return flat list from API; build tree in frontend.

**Rationale**: 
- Simpler API (single endpoint, no nested serialization)
- Account Groups uses the same pattern
- Frontend tree component handles hierarchy construction
- Reduces API payload size for large lists

**Alternatives considered**:
- Nested API response → rejected; complex serialization, harder to cache

## R10: Inactive Group Display

**Decision**: Inactive groups appear in both tree and list views with muted styling and badge.

**Rationale**: Spec clarified. Provides hierarchy context and preserves historical visibility.

**Implementation**: Frontend applies CSS class `opacity-50` and renders `<Badge variant="secondary">معطل</Badge>` for inactive groups.
