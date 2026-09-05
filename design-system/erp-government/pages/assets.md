# Assets Module — Page-Specific Design Rules

> **Extends:** MASTER.md
> **Override Level:** Feature-specific

---

## 1. Asset Register (سجل الأصول)

### Visual Overrides

| Override | Value | Reason |
|----------|-------|--------|
| Asset code | `font-family-mono` + `dir="ltr"` | System identifier |
| Financial columns | `tabular-nums` | Accuracy |
| Depreciation indicator | Progress bar or gauge | Visual depreciation status |
| Status badge | Color-coded (Active/Disposed/Impaired) | Quick status identification |

### RTL Rules

- Asset codes: `dir="ltr"`
- Asset names: RTL
- Location: RTL
- Financial values: `tabular-nums`

### Financial UX Rules

- **Acquisition cost:** Bold, primary color
- **Accumulated depreciation:** Muted color
- **Net book value:** Prominent, highlighted
- **Impairment:** Red indicator, warning badge
- **Disposal:** Grayed out, read-only

### Depreciation Display

```text
Acquisition Cost:     1,000,000.00 SAR
Accumulated Depr.:   -  250,000.00 SAR
─────────────────────────────────────
Net Book Value:         750,000.00 SAR
```

### Component Rules

| Component | Rule |
|-----------|------|
| DepreciationTable | Year-by-year schedule, cumulative column |
| MovementHistory | Timeline with location changes |
| DocumentsTab | Attachments, certificates, photos |
| FinancialSummary | Cost, depreciation, NBV, impairment |
