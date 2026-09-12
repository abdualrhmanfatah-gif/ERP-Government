# Quickstart: ACC-03 — Journals & Templates

**Purpose**: Validate the feature works end-to-end after implementation.

## Prerequisites

- Backend running (`dotnet run --project src/Web`)
- Frontend dev server running (`npm run dev` in `src/Web/ClientApp`)
- User logged in with `Accounting.Journals.*` and `Accounting.Templates.*` permissions
- Browser: Chrome/Edge latest, RTL mode

## Validation Scenarios

### V1: Journal CRUD (P1)

1. Navigate to `/accounting/journals`
2. **Expected**: Empty state "لا توجد دفاتر" or list with columns: code, name, type, sequence, approval, active
3. Click "إنشاء دفتر"
4. Fill: code=`CASH-01`, name=`دفتر النقدية`, type=Cash, requireApprovalBeforePosting=true
5. Submit → redirect to list
6. **Expected**: New journal appears in list
7. Click edit on the new journal
8. Change name → submit
9. **Expected**: Name updated in list
10. Attempt to create another journal with code=`CASH-01`
11. **Expected**: Server error "Journal code already exists."
12. Filter by type=Cash
13. **Expected**: Only Cash journals shown

### V2: Journal Used-Journal Lock

1. Create a journal entry referencing a journal (via spec 023 or API)
2. Navigate to journal edit for that journal
3. Attempt to change code or type
4. **Expected**: Server blocks with reason displayed
5. Toggle isActive
6. **Expected**: Succeeds (isActive remains editable)

### V3: Template CRUD (P2)

1. Navigate to `/accounting/templates`
2. **Expected**: Empty state "لا توجد قوالب" or list with templateName, journalName, templateType, isActive
3. Click "إنشاء قالب"
4. Fill: templateName=`قالب شراء دوري`, journalId=select a journal, templateType=Recurring
5. Submit → redirect to list
6. **Expected**: New template appears with journal name resolved
7. Edit template → change description → submit
8. **Expected**: Updated

### V3b: Template Lines (US4)

1. Navigate to `/accounting/templates/{id}`
2. **Expected**: Header + lines table `# | الحساب | مدين | دائن | الوصف | عملة/سعر | مركز تكلفة` + BalanceIndicator
3. Click `+ إضافة سطر` → pick postable account, debit=1000, save
4. Add second line credit=1000 → **Expected**: balanced, save enabled
5. Try debit+credit together → **Expected**: blocked with message
6. Delete one line → **Expected**: unbalanced warning, save blocked

### V4: System Template Protection

1. Create a template via API with `isSystemTemplate: true`
2. Navigate to templates list
3. **Expected**: Template appears
4. **Expected**: No delete button/action offered for system templates

### V5: Active-Only Pickers (Spec 023 Integration)

1. Disable a journal (edit → toggle isActive=false)
2. Open journal entry creation screen (spec 023)
3. **Expected**: Disabled journal does not appear in journal picker
4. Disable a template
5. **Expected**: Disabled template does not appear in template picker

### V6: Permission Gating

1. Login as user WITHOUT `Accounting.Journals.Read` permission
2. Navigate to `/accounting/journals`
3. **Expected**: Access denied / guard
4. Repeat for templates without `Accounting.Templates.Read`

## Test Commands

```bash
# Frontend unit tests
cd src/Web/ClientApp
npm run test -- --run src/features/accounting/journals/
npm run test -- --run src/features/accounting/templates/

# Full test suite
npm run test -- --run

# Lint
npm run lint

# Build
npm run build
```
