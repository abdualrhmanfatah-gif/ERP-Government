# Security Module Design Rules

## Override Authority

This file contains page-specific overrides for the Security module.
Base rules: `design-system/erp-government/MASTER.md`

## Role-Based Access Control (RBAC)

### Roles List Page

```text
Structure:
  PageShell
    → PageHeader (title + create button)
    → DataGrid
      → Columns: name, code, description, roleLevel, userCount
      → Actions: view, edit, permissions
      → Row click: navigate to detail
```

### Role Detail Page

```text
Structure:
  PageShell
    → PageHeader (title + actions)
    → InfoCard (role details)
    → RolePermissionsTable (embedded)
    → AssignedUsersList (embedded)

Sections:
  - Role info: name, code, description, level
  - Permissions: table with assign/remove actions
  - Users: list with assign/unassign actions
```

### Role Form

```text
Fields:
  - code (text, required, LTR, disabled on edit)
  - name (text, required)
  - description (textarea, optional)
  - roleLevel (select, optional)
  - maxSessionDuration (number, optional)
  - requiresMfa (checkbox, optional)

Validation:
  - code: required, max 50 chars
  - name: required, max 200 chars
  - description: max 500 chars
```

### Role Permissions Table

```text
Structure:
  → Search input (filter permissions)
  → Available permissions list
  → Assigned permissions table
  → Assign/Remove buttons

Layout:
  - Two-column: available | assigned
  - Search: sticky at top
  - Permissions: grouped by module
  - Actions: icon buttons with labels
```

## User Roles Management

### User Roles Page

```text
Structure:
  PageShell
    → PageHeader (title + back button)
    → UserInfo card (user details)
    → AssignedRoles table
    → RoleAssignmentDialog (modal)

Sections:
  - User info: name, email, status
  - Roles: table with remove action
  - Assign: dialog with role selection
```

### Role Assignment Dialog

```text
Structure:
  Dialog
    → DialogHeader (title)
    → DialogContent
      → Role selection (combobox)
      → Expiry date (date picker)
    → DialogFooter (cancel, assign)

Rules:
  - Already assigned roles: disabled in selection
  - Expiry date: optional, defaults to no expiry
  - Confirmation: required before assignment
```

## Color Usage

### Role Level Colors

| Level | Color | Usage |
|-------|-------|-------|
| Admin | destructive | System administrators |
| Manager | primary | Department managers |
| User | secondary | Standard users |
| Viewer | muted | Read-only access |

### Permission Colors

| State | Color | Usage |
|-------|-------|-------|
| Assigned | success-bg | Granted permissions |
| Available | surface | Unassigned permissions |
| Denied | error-bg | Explicitly denied |
| Inherited | info-bg | Role inheritance |

### Status Colors

| Status | Color | Usage |
|--------|-------|-------|
| Active | status-active-bg | Active roles/users |
| Inactive | status-draft-bg | Disabled roles/users |
| Locked | status-closed-bg | Locked accounts |

## Accessibility

### Keyboard Navigation

- Tab order: Page header → Info cards → Tables → Actions
- Enter/Space: Activate buttons, open dialogs
- Arrow keys: Navigate within tables
- Escape: Close dialogs, cancel operations

### Screen Reader

- Role names: Announce role name and level
- Permissions: "صلاحية {permission name}: {assigned/available}"
- Actions: "إضافة صلاحية {permission name}"
- Status: Announce role/user status

### Focus Management

- Dialogs: Auto-focus first interactive element
- Search: Auto-focus on open
- Tables: Focus row on selection
- Actions: Visible focus ring on all buttons

### ARIA Patterns

```tsx
// Role table
<table aria-label="الأدوار">
  <thead>...</thead>
  <tbody>
    <tr aria-selected={isSelected}>
      <td>{role.name}</td>
      <td>
        <Button aria-label="تعديل {role.name}">...</Button>
      </td>
    </tr>
  </tbody>
</table>

// Permission assignment
<div role="group" aria-label="الصلاحيات المحددة">
  {permissions.map(p => (
    <div role="checkbox" aria-checked={p.assigned}>
      {p.name}
    </div>
  ))}
</div>
```

## Security Considerations

### Visual Security Indicators

```text
- MFA required: Shield icon + "مصادقة ثنائية" badge
- Session timeout: Clock icon + duration text
- Last login: Timestamp display
- Failed attempts: Warning badge with count
```

### Sensitive Data Display

```text
- Passwords: Never displayed, only strength indicator
- MFA codes: Masked after display
- Session tokens: Not displayed
- Audit logs: Read-only, no modification
```

### Permission Boundary Indicators

```text
- Own role: "دورك الحالي" badge
- Protected role: Lock icon
- System role: Star icon
- Custom role: Pencil icon
```
