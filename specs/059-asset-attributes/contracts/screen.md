# Screen Contracts: Asset Attributes

## Screen 1: Attribute Definitions List

**Route**: `/assets/attributes`
**Page Component**: `AssetAttributesListPage.tsx`
**Data Source**: `AssetAttributesClient.getDefinitions()`

### Layout
- Page title: "تعريفات الخصائص"
- Filter bar: Search (by Code/Name), DataType dropdown, Active status toggle
- Data table with columns:
  - الكود (Code)
  - الاسم (Name)
  - النوع (DataType) — displayed as Badge with color per type
  - الوحدة (Unit)
  - الحالة (Status) — Badge: "نشط" (green) / "معطّل" (gray)
  - إجراءات (Actions) — Edit button, Toggle Active button
- Pagination: 20 items per page
- Add button: "إضافة تعريف صفة"

### Data Type Badges
| DataType | Label | Color |
|----------|-------|-------|
| Text | نصي | blue |
| Integer | صحيح | green |
| Decimal | عشري | purple |
| Date | تاريخ | orange |
| Boolean | منطقي | teal |

---

## Screen 2: Create/Edit Attribute Definition

**Route**: `/assets/attributes/create` or `/assets/attributes/:id/edit`
**Page Components**: `AssetAttributeCreatePage.tsx`, `AssetAttributeEditPage.tsx`
**Form Component**: `AssetAttributeForm.tsx`

### Form Fields
| Field | Type | Required | Validation |
|-------|------|----------|------------|
| Code | Input text | Yes | Unique, max 50 chars |
| Name | Input text | Yes | Max 200 chars |
| Description | Textarea | No | Max 500 chars |
| DataType | Select dropdown | Yes | One of: Text/Integer/Decimal/Date/Boolean |
| Unit | Input text | No | Max 50 chars |
| SortOrder | Input number | No | Integer ≥ 0 |
| IsActive | Switch | Yes | Default: true |

### Validation Rules
- Code: required, unique (server-side), max 50
- Name: required, max 200
- DataType: required, must be valid enum value
- On edit with linked values: DataType change blocked with error message

### Data Attributes for Testing
- `[data-testid='attr-code']`
- `[data-testid='attr-name']`
- `[data-testid='attr-datatype']`
- `[data-testid='attr-unit']`
- `[data-testid='attr-sortorder']`
- `[data-testid='attr-active']`
- `[data-testid='attr-save']`

---

## Screen 3: Group Attributes Management

**Route**: `/assets/groups/:id/attributes`
**Page Component**: `AssetGroupAttributesPage.tsx`
**Data Source**: `AssetGroupsClient.getGroupById()` + `AssetAttributesClient.getDefinitions()`

### Layout
- Page title: "خصائص المجموعة — {groupName}"
- Back link to group detail
- Table of bound attributes:
  - الصفة (Definition Name)
  - النوع (DataType badge)
  - مطلوب (Required toggle)
  - ترتيب (SortOrder input)
  - إجراءات (Remove button)
- Add button: "إضافة صفة" → opens definition selector (dropdown of unbound active definitions)
- Save button: "حفظ الربط"

### Interactions
- Adding a binding: select from dropdown of active definitions not yet bound
- Removing a binding: confirmation dialog ("إزالة الصفة ستحتفظ بالقيم existing على الأصول")
- Saving: atomic save via `PUT /api/AssetGroups/{id}/attributes`

### Data Attributes for Testing
- `[data-testid='group-attr-table']`
- `[data-testid='group-attr-add']`
- `[data-testid='group-attr-save']`
- `[data-testid='group-attr-required-{defId}']`
- `[data-testid='group-attr-sort-{defId}']`
- `[data-testid='group-attr-remove-{defId}']`

---

## Screen 4: Asset Form — Dynamic Attributes Section

**Route**: Part of `/assets/create` and `/assets/:id/edit`
**Component**: `AssetAttributesSection.tsx` (embedded in `AssetForm.tsx`)

### Layout
- Section header: "الخصائص" (shown only when a group is selected)
- For each bound definition:
  - Label: definition Name + "*" if required
  - Control based on DataType:
    - Text → `<Input type="text" data-attribute='{label}' />`
    - Integer → `<Input type="number" step="1" data-attribute='{label}' />`
    - Decimal → `<Input type="number" step="0.01" data-attribute='{label}' />`
    - Date → `<Input type="date" data-attribute='{label}' />`
    - Boolean → `<Switch data-attribute='{label}' />`
  - Error message below field if validation fails

### Interactions
- On group selection change: fetch group's bindings, render dynamic fields
- On group change on existing asset: retain old values, show warning for incompatible
- Validation on submit: required check, type check, uniqueness

### Data Attributes for Testing
- `[data-attribute='{definitionName}']` — matches acceptance test pattern
- `[data-testid='asset-status']` — asset status badge
- `[data-testid='asset-department']` — derived department display

---

## Screen 5: Asset Detail — Attributes Tab

**Route**: Part of `/assets/:id`
**Component**: `AssetAttributesTab.tsx` (tab in `AssetDetailPage.tsx`)

### Layout
- Tab title: "الخصائص"
- Table of attribute values:
  - الصفة (Definition Name)
  - القيمة (Value) — formatted per type
  - النوع (DataType badge)
  - الحالة (Status) — "نشط" or "معطّل" badge if definition is deactivated

### Value Formatting
| DataType | Display Format |
|----------|---------------|
| Text | Raw string |
| Integer | Locale-formatted integer |
| Decimal | Locale-formatted decimal (4 decimal places) |
| Date | Localized date string |
| Boolean | "نعم" / "لا" |
| Empty | "—" (em dash) |

### Data Attributes for Testing
- `[data-testid='attr-tab']`
- `[data-testid='attr-value-{defId}']`
- `[data-testid='attr-status-{defId}']`
