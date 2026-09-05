# Dashboard Design Rules

## Override Authority

This file contains page-specific overrides for the Dashboard module.
Base rules: `design-system/erp-government/MASTER.md`

## Dashboard Layout

### KPI Cards Grid

```text
Structure:
  PageShell
    → PageHeader (title + quick action button)
    → KPI Grid (3 columns on desktop, 1 on mobile)
      → KpiCard (budget utilization)
      → KpiCard (pending approvals)
      → KpiCard (cash position)

Grid Rules:
  - Desktop: grid-cols-1 lg:grid-cols-3
  - Gap: gap-6 (24px)
  - Mobile: single column, full width
  - Cards: min-height 120px
  - Click targets: entire card clickable
  - Focus visible: outline-secondary with offset
```

### KPI Card States

| State | Visual | Behavior |
|-------|--------|----------|
| Loading | Skeleton with spinner | `aria-busy="true"` |
| Error | ErrorState with retry | `onRetry` callback |
| Empty | EmptyState message | Non-interactive |
| Data | Value + subtitle | Clickable if `onClick` provided |

### KPI Card Variants

| Variant | Background | Text | Icon Color | Usage |
|---------|------------|------|------------|-------|
| default | surface-container-low | on-surface | primary | Standard metrics |
| primary | primary (navy) | white | secondary (gold) | Hero/highlight metric |

### Quick Action Button

```text
Position: Top-right of page header
Variant: primary
Icon: Plus (16px)
Label: "إجراء سريع"
Action: Navigate to journal entry create
Touch target: h-11 (44px)
```

## Color Usage

### Dashboard-Specific Colors

| Element | Light Mode | Dark Mode |
|---------|------------|-----------|
| KPI card bg | surface-container-low | surface-container |
| KPI card border | border-container | border-container |
| KPI value | on-surface | on-surface |
| KPI subtitle | on-surface-variant | on-surface-variant |
| Primary variant bg | primary (#002045) | primary-dark |
| Primary variant value | white | white |
| Primary variant icon | secondary (#755b00) | secondary-light |

### Status Colors for KPIs

| KPI | Success | Warning | Danger |
|-----|---------|---------|--------|
| Budget utilization | < 80% green | 80-95% yellow | > 95% red |
| Pending approvals | 0 green | 1-5 yellow | > 5 red |
| Cash position | Positive green | Zero yellow | Negative red |

## Typography

### Page Title

```text
Class: text-headline-sm font-bold text-on-surface
Size: 20px (1.25rem)
Weight: 700
Line height: 1.875rem
```

### KPI Title

```text
Class: text-sm font-semibold
Size: 14px (0.875rem)
Weight: 600
Color: text-on-surface-variant (default) or text-on-primary-container (primary)
```

### KPI Value

```text
Class: text-headline-lg font-bold tabular-nums
Size: 32px (2rem)
Weight: 700
Line height: 2.5rem
Font feature: tabular-nums for aligned numbers
```

### KPI Subtitle

```text
Class: text-xs
Size: 13px (0.8125rem)
Color: text-on-surface-variant
```

## Accessibility

### Keyboard Navigation

- Tab order: Page title → Quick action button → KPI cards (left to right, top to bottom)
- Enter/Space on KPI card: triggers `onClick` action
- Focus visible: 2px outline with secondary color, 2px offset

### ARIA Attributes

```tsx
<div role="region" aria-label={title} ...>
  {/* Loading state */}
  <div aria-busy="true">...</div>
  
  {/* Interactive card */}
  <div 
    tabIndex={0}
    onKeyDown={(e) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        onClick();
      }
    }}
  >
    ...
  </div>
</div>
```

### Screen Reader Announcements

- KPI values announced with context: "نسبة تنفيذ الميزانية: 75.5%"
- Loading state: `aria-busy="true"` with "جاري التحميل" text
- Error state: `role="alert"` with error message
- Empty state: Descriptive text "لا توجد ميزانيات"
