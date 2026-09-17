# Data Model: Asset Attributes (خصائص الأصول)

## Overview

Three tables support the flexible attributes system: definitions (metadata), group bindings (which definitions apply to which groups), and values (actual data per asset). This extends the 058 asset module's 13-table model.

## Tables

### 11. AssetAttributeDefinitions (تعريفات الخصائص)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | No | Auto-increment |
| Code | nvarchar(50) | No | UNIQUE, case-insensitive |
| Name | nvarchar(200) | No | Required |
| Description | nvarchar(500) | Yes | Optional |
| AttributeDataType | int | No | FK to enum: 1=Text, 2=Integer, 3=Decimal, 4=Date, 5=Boolean |
| Unit | nvarchar(50) | Yes | Optional unit of measure |
| IsActive | bit | No | Default: true |
| SortOrder | int | Yes | Default display order; null falls back to definition default |
| RowVersion | rowversion | No | Concurrency token |
| Created | datetime2 | No | Audit |
| CreatedBy | nvarchar(450) | Yes | Audit (nullable for system-generated) |
| LastModified | datetime2 | No | Audit |
| LastModifiedBy | nvarchar(450) | Yes | Audit |

**Constraints**:
- UNIQUE INDEX on Code (case-insensitive via SQL Server default collation)
- CHECK: AttributeDataType BETWEEN 1 AND 5

### 12. AssetGroupAttributes (ربط الخصائص بالمجموعات)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| AssetGroupId | int FK | No | Composite PK part 1 → AssetGroups.Id |
| AssetAttributeDefinitionId | int FK | No | Composite PK part 2 → AssetAttributeDefinitions.Id |
| IsRequired | bit | No | Default: false |
| SortOrder | int | Yes | Group-level sort order; null falls back to definition default |

**Constraints**:
- COMPOSITE PRIMARY KEY (AssetGroupId, AssetAttributeDefinitionId)
- FOREIGN KEY AssetGroupId → AssetGroups.Id (CASCADE on delete)
- FOREIGN KEY AssetAttributeDefinitionId → AssetAttributeDefinitions.Id (RESTRICT on delete — cannot delete a definition that is bound to any group)

### 13. AssetAttributeValues (قيم خصائص الأصول)

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| Id | int PK | No | Auto-increment |
| AssetId | int FK | No | → Assets.Id |
| AssetAttributeDefinitionId | int FK | No | → AssetAttributeDefinitions.Id |
| TextValue | nvarchar(1000) | Yes | For DataType = Text |
| IntegerValue | int | Yes | For DataType = Integer |
| DecimalValue | decimal(18,4) | Yes | For DataType = Decimal |
| DateValue | date | Yes | For DataType = Date |
| BooleanValue | bit | Yes | For DataType = Boolean |
| RowVersion | rowversion | No | Concurrency token |
| Created | datetime2 | No | Audit |
| CreatedBy | nvarchar(450) | Yes | Audit |
| LastModified | datetime2 | No | Audit |
| LastModifiedBy | nvarchar(450) | Yes | Audit |

**Constraints**:
- UNIQUE INDEX on (AssetId, AssetAttributeDefinitionId) — one value row per asset per definition
- FOREIGN KEY AssetId → Assets.Id (CASCADE on delete)
- FOREIGN KEY AssetAttributeDefinitionId → AssetAttributeDefinitions.Id (RESTRICT on delete)
- CHECK: Exactly one of TextValue/IntegerValue/DecimalValue/DateValue/BooleanValue is non-null (enforced at application level, not DB — multiple nulls allowed for optional attributes with no value row)

## Relationships

```
AssetGroups ──(1:N)── AssetGroupAttributes ──(N:1)── AssetAttributeDefinitions
Assets ──(1:N)── AssetAttributeValues ──(N:1)── AssetAttributeDefinitions
```

## Migration Notes

- The current code has `NumericValue` (decimal) instead of `IntegerValue` + `DecimalValue`
- This migration must:
  1. Add `IntegerValue` column (int, nullable)
  2. Rename `NumericValue` to `DecimalValue` (decimal(18,4))
  3. Update the `AssetAttributeDataType` enum to add `Integer = 2`, shift `Decimal = 3`, `Date = 4`, `Boolean = 5`
  4. Update all seed data to use new enum values
  5. Update `AttributeValueValidator` to handle both Integer and Decimal columns
  6. Update EF configurations and generated web-api-client
