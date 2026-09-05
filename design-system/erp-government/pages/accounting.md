# Accounting Module — Page-Specific Design Rules

> **Extends:** MASTER.md
> **Override Level:** Feature-specific

---

## 1. Chart of Accounts (شجرة الحسابات)

### Visual Overrides

| Override | Value | Reason |
|----------|-------|--------|
| Account code font | `font-family-mono` | Technical identifiers must be LTR monospace |
| Account tree indent | `ms-6` per level | Clear hierarchy visualization |
| Balance column | `tabular-nums` | Financial accuracy |

### RTL Rules

- Account codes: `dir="ltr"` always
- Account names: RTL, start-aligned
- Tree structure: parent on right, children on left (RTL)
- Breadcrumb: Accounts > Parent Account > Child Account

### Component Rules

| Component | Rule |
|-----------|------|
| DataGrid | First column (code) is sticky on scroll |
| StatusBadge | Active/Inactive states only |
| Search | Search by code OR name |
| TreeView | Expandable/collapsible nodes |

### Financial UX Rules

- Balance: `tabular-nums`, currency format
- Account types: parent → child hierarchy enforced
- Debit balance: default color
- Credit balance: default color (visual distinction via icon)
- Zero balance: muted color

---

## 2. Journal Entries (قيود اليومية)

### Visual Overrides

| Override | Value | Reason |
|----------|-------|--------|
| Debit column | `text-start` (RTL) | Consistent alignment |
| Credit column | `text-start` (RTL) | Consistent alignment |
| Balance indicator | Green border if balanced, Red if unbalanced | Immediate visual feedback |
| Entry lines | Alternating row color | Readability for many lines |

### RTL Rules

- Reference numbers: `dir="ltr"`
- Dates: Hijri + Gregorian dual display
- Account names: RTL
- Debit/Credit headers: RTL

### Approval Workflow

```text
Draft → Submitted → Approved → Posted
  ↑         ↓           ↓
  └─── Rejected ────────┘
```

**State Actions:**
- Draft: Edit, Delete, Submit
- Submitted: Approve, Reject (requires reason)
- Approved: Post, Return
- Posted: View only (read-only lock)

### Component Rules

| Component | Rule |
|-----------|------|
| LinesTable | Inline editing for draft entries |
| BalanceIndicator | Always visible, top of form |
| AccountSearch | Combobox with code + name search |
| AmountInput | Currency-aware, tabular-nums |

### Financial UX Rules

- **Unbalanced entry:** Red border on total, disable submit
- **Zero amounts:** Display "0.00" not blank
- **Negative values:** Red color + minus sign
- **Totals row:** Bold + border-top
- **Running balance:** Update in real-time during editing
