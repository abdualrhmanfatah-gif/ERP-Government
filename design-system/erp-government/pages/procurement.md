# Procurement Module — Page-Specific Design Rules

> **Extends:** MASTER.md
> **Override Level:** Feature-specific

---

## 1. Purchase Requisitions (طلبات الشراء)

### Visual Overrides

| Override | Value | Reason |
|----------|-------|--------|
| Priority indicator | Color-coded badge (High/Medium/Low) | Quick prioritization |
| Budget check | Inline green/red indicator | Immediate budget visibility |
| Items table | Compact density | Many items per requisition |

### RTL Rules

- Requisition number: `dir="ltr"`
- Item descriptions: RTL
- Vendor names: RTL
- amounts: `tabular-nums`

### Workflow

```text
Draft → Submitted → Approved → Ordered → Received
  ↑         ↓
  └─── Rejected
```

---

## 2. Purchase Orders (أوامر الشراء)

### Visual Overrides

| Override | Value | Reason |
|----------|-------|--------|
| PO number | `font-family-mono` + `dir="ltr"` | System identifier |
| Total amount | Large, bold, highlighted | Key financial figure |
| Delivery status | Progress bar | Visual completion tracking |

### Component Rules

| Component | Rule |
|-----------|------|
| VendorSelector | Combobox with search + recent vendors |
| ItemsEditor | Inline editing, add/remove rows |
| TaxCalculation | Auto-calculate, show breakdown |
| AttachmentArea | Drag-drop + file list |
| ApprovalTimeline | Show full approval chain |

### Financial UX Rules

- Subtotal, Tax, Total: clear visual hierarchy
- Line total: auto-calculated (read-only)
- Currency: consistent throughout (single currency per PO)
- Rounding: 2 decimal places
