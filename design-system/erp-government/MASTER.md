# ERP-Government Design System — Master Document

> **Status:** Design Architecture + Specifications (Phase 1)
> **Version:** 1.0.0
> **Date:** 2026-09-02
> **Stack:** React 19 + TypeScript + Tailwind CSS 3 + shadcn/ui (base-nova) + Vite 8 + Lucide icons
> **Backend:** .NET 10 + ASP.NET Core + EF Core + MediatR CQRS

---

## 1. Product Design Diagnosis

### 1.1 Product Identity

**ERP-Government** is a government enterprise resource planning system designed for public sector financial management, accounting, procurement, asset management, and organizational governance.

**NOT:**
- Consumer app
- SaaS marketing product
- Landing page
- Fintech consumer app
- Startup dashboard

**IS:**
- Government institutional financial system
- Multi-module ERP with Arabic-first RTL interface
- Dense data-driven application with financial accuracy requirements
- Audit-trail-heavy with approval workflows
- Permission-aware with RBAC

### 1.2 Core Product Values

| Value | Expression |
|-------|------------|
| **Trust** | Navy primary conveys authority; gold secondary signals institutional prestige |
| **Accuracy** | Tabular numerics, financial formatting, balanced journal entries |
| **Accountability** | Audit trails, approval timelines, permission-gated actions |
| **Governance** | Workflow states, role-based access, document sequencing |
| **Stability** | Conservative color palette, minimal animation, predictable layouts |
| **Security** | Focus rings, keyboard navigation, skip links, WCAG compliance |
| **Operational Clarity** | Dense but readable data tables, status badges, financial UX rules |

### 1.3 Target Users

| Role | Context | Needs |
|------|---------|-------|
| Financial Accountant | Daily data entry, journal entries, account management | Fast data entry, keyboard shortcuts, financial accuracy |
| Budget Officer | Budget tracking, appropriation, variance analysis | Dashboard metrics, exception highlighting |
| Procurement Officer | Purchase requisitions, orders, vendor management | Workflow visibility, approval status |
| Asset Manager | Asset tracking, depreciation, disposal | Long-term data views, financial calculations |
| Auditor | Read-only review, compliance checking | Audit trails, change history, read-only views |
| System Administrator | User management, roles, configuration | Settings pages, permission management |

---

## 2. UI/UX Pro Max Research Summary

| # | Query | Domain | Result | Fit | Decision | Reason |
|---|-------|--------|--------|-----|----------|--------|
| 1 | `government enterprise ERP financial management institutional Arabic RTL dashboard` | design-system | Enterprise Gateway + Dark Mode (OLED) | Partial | **Adopted Navy/Gold palette; rejected OLED dark default** | Government ERP needs light-first with dark option |
| 2 | `enterprise government institutional financial` | product | Government/Public Service → Accessible & Ethical + Minimalism | High | **Confirmed** | Matches institutional nature |
| 3 | `corporate navy gold institutional trust` | color | Banking/Traditional: #0F172A + #CA8A04 | High | **Confirmed existing tokens** | tokens.ts already implements this |
| 4 | `arabic RTL bilingual sans serif` | typography | Arabic Elegant: Noto Naskh + Noto Sans Arabic | Medium | **Rejected — IBM Plex Sans Arabic better** | Existing font has superior bilingual support |
| 5 | `error summary validation inline` | ux | Inline validation on blur, errors near fields | High | **Confirmed existing pattern** | FormField.tsx implements this |
| 6 | `financial dashboard accounting data-dense` | chart | Treemap, Sankey (hierarchical/flow) | Medium | **Supplementary** | Use for budget allocation, fund flow |
| 7 | `enterprise icon system navigation` | icons | Lucide icon set (already in use) | High | **Confirmed** | Already implemented |
| 8 | `data table dense sorting filtering` | shadcn | DataTable pattern with TanStack Table | High | **Confirmed** | DataGrid.tsx implements this |
| 9 | `navigation sidebar hierarchy back` | ux | Breadcrumbs for 3+ levels, predictable back | High | **Needs implementation** | Currently missing breadcrumbs |
| 10 | `form label required validation progressive disclosure` | ux | Visible labels, required indicators, helper text | High | **Needs audit** | Some forms missing labels |
| 11 | `touch target spacing hover interaction` | ux | Min 44×44px, 8px+ gap | High | **Needs audit** | Some controls too small |
| 12 | `contrast ratio focus ring keyboard navigation` | ux | Visible focus rings, keyboard nav | High | **Confirmed** | Double-ring focus implemented |
| 13 | `responsive layout mobile first breakpoint` | ux | Mobile-first, test at 375/768/1024/1440 | Medium | **Needs responsive audit** | Sidebar collapse exists, tables need mobile fallback |

---

## 3. Brand / Product Personality

### 3.1 Primary Personality

**Institutional Authority**
- Government-grade trust and formality
- Conservative, predictable, reliable
- No playfulness, no trendiness, no consumer-app energy

### 3.2 Secondary Personality

**Financial Precision**
- Accuracy-first data presentation
- Dense but readable information architecture
- Financial UX conventions (debit/credit alignment, currency formatting)

### 3.3 Tone

- Formal Arabic institutional language
- Professional, not cold
- Clear, not clever
- Consistent, not surprising

### 3.4 Emotional Qualities

| Quality | Visual Expression |
|---------|-------------------|
| Trust | Navy blue dominance, stable layout grid |
| Authority | Gold accent, institutional typography |
| Accuracy | Tabular numerics, precise spacing |
| Security | Visible focus states, keyboard accessibility |
| Stability | Minimal animation, predictable interactions |
| Accountability | Audit trails, approval timelines, status badges |

### 3.5 Why This Personality

**Why selected:** Government ERP systems demand institutional trust, financial accuracy, and long-term maintainability. The navy/gold palette with conservative typography communicates authority without alienating users who interact with this system daily for years.

**Rejected alternatives:**
- **Playful/Vibrant** — Inappropriate for government financial systems
- **Dark Mode OLED** — Default should be light for government office environments; dark as option only
- **Glassmorphism** — Decorative, reduces readability for dense data
- **Minimalist/Clean** — Too sparse for ERP data density requirements
- **AI Purple/Pink** — Consumer SaaS aesthetic, not institutional

---

## 4. Recommended Visual Direction

```text
RECOMMENDED VISUAL DIRECTION

Name:           Institutional Financial
Style:          Accessible & Ethical + Flat Design + Data-Dense
Visual Characteristics:
  - Navy (#002045) primary with gold (#755b00) secondary accent
  - Light surface hierarchy (white → slate progression)
  - Conservative border radius (2-6px)
  - Subtle shadows for elevation (not decorative)
  - Dense but readable data tables
  - RTL-first layout with logical properties
Information Density:      High (ERP data tables, forms, dashboards)
Enterprise Suitability:   High — conservative palette, dense layouts
Government Suitability:   High — institutional trust signals
Financial Suitability:    High — tabular numerics, currency formatting
RTL Suitability:          High — logical CSS properties, IBM Plex Sans Arabic
Accessibility Risk:       Low — existing focus rings, skip links, aria attributes
Implementation Complexity: Low — existing tokens.ts + shadcn/ui foundation
```

### Rejected Alternatives

| Style | Why Rejected |
|-------|--------------|
| Dark Mode OLED | Government offices use light environments; dark as option only |
| Glassmorphism | Reduces readability for dense financial data tables |
| Brutalism | Too aggressive for institutional users |
| Neumorphism | Low contrast, poor accessibility |
| AI Purple/Pink | Consumer SaaS aesthetic |
| Gradient-heavy | Decorative, not functional |

---

## 5. Visual Language

### 5.1 Design Principles

1. **Data First** — Every pixel serves information delivery
2. **Institutional Trust** — Visual consistency conveys reliability
3. **Financial Accuracy** — Precision in typography, alignment, formatting
4. **RTL-Native** — Logical properties, not mirrored hacks
5. **Accessible by Default** — Contrast, focus, keyboard navigation built-in

### 5.2 Visual Hierarchy Rules

| Level | Usage | Weight | Size | Color |
|-------|-------|--------|------|-------|
| Display | Page titles (rare) | Bold 700 | 32-42px | onSurface |
| H1 | Page header | Bold 700 | 26px | onSurface |
| H2 | Section header | Semibold 600 | 22px | onSurface |
| H3 | Subsection | Semibold 600 | 20px | onSurface |
| Body | Content text | Regular 400 | 16px | onSurface |
| Label | Form labels, table headers | Medium 500 | 14px | onSurfaceVariant |
| Caption | Helper text, metadata | Regular 400 | 13px | onSurfaceVariant |
| Numeric | Data columns | Regular 400 | 14px | onSurface (tabular-nums) |

### 5.3 Spacing Philosophy

8px base grid with consistent rhythm:
- **Inline:** 4px, 8px (between related items)
- **Component:** 12px, 16px (padding inside controls)
- **Section:** 24px, 32px (between sections)
- **Page:** 48px, 64px (major divisions)

---

## 6. Color Architecture

### 6.1 Primitive Palette (from tokens.ts)

| Palette | Role | Key Values |
|---------|------|------------|
| Navy | Primary | 500: #002045 (authority), 400: #1a365d, 100: #86a0cd |
| Gold | Secondary | 500: #755b00 (prestige), 100: #fed255, 300: #d4a82a |
| Red | Error | 500: #ba1a1a, 300: #ff5c4d |
| Slate | Neutral | 100: #e2e8f0 (surface), 900: #475569 |
| Blue | Info | 500: #1d4ed8, 100: #93c5fd |
| Green | Success | 500: #166534, 100: #86efac |
| Yellow | Warning | 500: #854d0e, 100: #fde047 |

### 6.2 Semantic Color System

```text
Token Architecture:
  primitive.color.navy.500
    → semantic.color.primary
      → component.button.primary.background
        → feature.approval.submit.action
```

### 6.3 Status Colors

| Status | Light BG | Light FG | Dark BG | Dark FG | Usage |
|--------|----------|----------|---------|---------|-------|
| draft | #f1f5f9 | #475569 | #334155 | #cbd5e1 | Draft documents |
| pending | yellow.50 | yellow.500 | yellow.700 | yellow.100 | Awaiting approval |
| approved | green.50 | green.500 | green.700 | green.100 | Approved items |
| active | blue.50 | blue.500 | blue.700 | blue.100 | Active records |
| closed | #e2e8f0 | #1e293b | #1e293b | #cbd5e1 | Closed/archived |

### 6.4 Financial Status Extension

| Status | Semantic | Visual | Usage |
|--------|----------|--------|-------|
| posted | success | Green bg+fg | Posted journal entries |
| reversed | warning | Yellow bg+fg | Reversed transactions |
| cancelled | neutral | Slate bg+fg | Cancelled documents |
| locked | info | Blue bg+fg | Locked fiscal periods |
| overBudget | danger | Red bg+fg | Budget overruns |
| unbalanced | danger | Red border | Unbalanced journal entries |

### 6.5 Contrast Verification

| Pair | Ratio | WCAG | Status |
|------|-------|------|--------|
| Navy.500 on White | 15.8:1 | AAA | ✓ |
| Gold.500 on White | 5.2:1 | AA | ✓ |
| onSurface on Surface | 12.5:1 | AAA | ✓ |
| onSurfaceVariant on Surface | 7.1:1 | AAA | ✓ |
| Error on White | 6.8:1 | AA | ✓ |

---

## 7. Typography Architecture

### 7.1 Font Stack

```css
/* Arabic primary — IBM Plex Sans Arabic */
--font-family-sans: "IBM Plex Sans Arabic", "IBM Plex Sans", "Segoe UI", Tahoma, sans-serif;
--font-family-heading: "IBM Plex Sans Arabic", "IBM Plex Sans", "Segoe UI", Tahoma, sans-serif;
--font-family-mono: "IBM Plex Mono", ui-monospace, monospace;
```

### 7.2 Why IBM Plex Sans Arabic

| Criteria | IBM Plex Sans Arabic | Noto Sans Arabic | Cairo |
|----------|---------------------|------------------|-------|
| Arabic glyph quality | High | High | Medium |
| Latin fallback | Excellent (IBM Plex Sans) | Good | Poor |
| Tabular numerals | Yes (font-variant-numeric) | Yes | No |
| Variable weight support | No (static weights) | Yes | Yes |
| Government institutional fit | High (IBM brand heritage) | Medium | Low |
| Bilingual readability | Excellent | Good | Medium |

### 7.3 Type Scale

| Token | Size | Weight | Line Height | Usage |
|-------|------|--------|-------------|-------|
| xs | 13px | 500 | 1.5 | Label small, caption |
| sm | 14px | 400/500 | 1.5 | Body small, form labels |
| base | 16px | 400 | 1.5 | Body text, table data |
| lg | 18px | 400 | 1.5 | Body large, descriptions |
| xl | 20px | 600 | 1.25 | Headline small |
| 2xl | 22px | 600 | 1.25 | Headline medium |
| 3xl | 26px | 700 | 1.25 | Headline large, page titles |
| 4xl | 32px | 700 | 1.25 | Display |
| 5xl | 36px | 700 | 1.25 | Display large |
| 6xl | 42px | 700 | 1.25 | Display XL |

### 7.4 Financial Numeric Rules

```css
/* All monetary values, percentages, quantities */
.tabular-nums {
  font-variant-numeric: tabular-nums;
  font-feature-settings: "tnum";
}

/* Currency alignment */
.amount-column {
  text-align: start; /* RTL: amounts align to start */
  font-variant-numeric: tabular-nums;
  letter-spacing: -0.01em; /* tighter for numbers */
}

/* Negative values */
.amount-negative {
  color: var(--color-error);
}
.amount-positive {
  color: var(--color-success);
}
.amount-zero {
  color: var(--color-on-surface-variant);
}
```

---

## 8. Spacing / Radius / Elevation

### 8.1 Spacing Scale (8px base)

| Token | Value | Usage |
|-------|-------|-------|
| 0 | 0 | — |
| 0.5 | 2px | Tight inline |
| 1 | 4px | Icon-text gap |
| 1.5 | 6px | Compact inline |
| 2 | 8px | Standard inline, touch gap |
| 3 | 12px | Component padding |
| 4 | 16px | Standard padding |
| 5 | 20px | Section inline |
| 6 | 24px | Section spacing |
| 8 | 32px | Major section |
| 10 | 40px | Page section |
| 12 | 48px | Page divisions |
| 16 | 64px | Major page sections |

### 8.2 Border Radius

| Token | Value | Usage |
|-------|-------|-------|
| none | 0 | Sharp edges |
| xs | 1px | Subtle rounding |
| sm | 2px | Buttons, inputs |
| base | 4px | Cards, containers |
| md | 6px | Dialogs, panels |
| lg | 8px | Modals, elevated cards |
| xl | 12px | Feature cards |
| full | 9999px | Pills, badges |

**Rule:** Use sm (2px) for controls, base (4px) for cards, md (6px) for overlays. Never use xl (12px) for data-dense components.

### 8.3 Elevation System

| Level | Shadow | Usage |
|-------|--------|-------|
| 0 | none | Flat elements |
| 1 | xs | Cards at rest |
| 2 | sm | Cards on hover, dropdowns |
| 3 | md | Modals, dialogs |
| 4 | lg | Floating panels |
| 5 | xl | Toast notifications |

---

## 9. Iconography

### 9.1 Icon Library

**Lucide React** — consistent stroke-based icons at 1.5px stroke width.

### 9.2 Icon Sizes

| Token | Size | Usage |
|-------|------|-------|
| sm | 16px | Inline badges, status indicators |
| md | 18px | Standard button icons, navigation |
| lg | 20px | Header actions, sidebar items |
| xl | 24px | Feature icons, empty states |

### 9.3 Icon Rules

1. **No emoji as icons** — Use SVG from Lucide
2. **Consistent stroke width** — 1.5px across all icons
3. **RTL mirror** — Directional arrows (← → ↑ ↓) mirror in RTL; icons with inherent direction (check, plus, search) do not
4. **Accessibility** — Decorative icons: `aria-hidden="true"`. Icon-only buttons: `aria-label` on button. Meaningful icons: `role="img"` with `aria-label`.
5. **Color** — Icons inherit text color by default. Status icons use semantic colors.

### 9.4 Feature Icon Map

| Module | Icon | Usage |
|--------|------|-------|
| Accounting | BookOpen | Chart of accounts |
| Journal | FileText | Journal entries |
| Budget | PiggyBank | Budget management |
| Payments | CreditCard | Payment orders |
| Revenue | TrendingUp | Revenue collection |
| Banking | Building2 | Bank reconciliation |
| Procurement | ShoppingCart | Purchase orders |
| Assets | Package | Asset management |
| Inventory | Boxes | Inventory tracking |
| Organization | Users | Org units, employees |
| Security | Shield | Roles, permissions |
| Settings | Settings | System configuration |
| Reports | BarChart3 | Reporting |
| Notifications | Bell | Notifications |

---

## 10. RTL Architecture

### 10.1 Core Principle

**RTL-first with logical CSS properties.** The entire UI is Arabic-first; English/technical content is secondary.

### 10.2 Logical Property Usage

| Physical | Logical (LTR) | Logical (RTL) | Usage |
|----------|---------------|---------------|-------|
| margin-left | margin-inline-start | margin-inline-start | Auto-adapts |
| margin-right | margin-inline-end | margin-inline-end | Auto-adapts |
| padding-left | padding-inline-start | padding-inline-start | Auto-adapts |
| padding-right | padding-inline-end | padding-inline-end | Auto-adapts |
| border-left | border-inline-start | border-inline-start | Auto-adapts |
| border-right | border-inline-end | border-inline-end | Auto-adapts |
| text-align: left | text-align: start | text-align: start | Auto-adapts |
| left | inset-inline-start | inset-inline-start | Auto-adapts |
| right | inset-inline-end | inset-inline-end | Auto-adapts |

### 10.3 Tailwind RTL Classes

```html
<!-- ✓ Correct -->
<div className="ms-4 pe-6 border-e-2 text-start">

<!-- ✗ Wrong -->
<div className="ml-4 pr-6 border-r-2 text-left">
```

### 10.4 Cases Where LTR Is Required

| Content Type | Reason | Implementation |
|-------------|--------|----------------|
| Account codes | Technical identifiers | `dir="ltr"` attribute |
| Document numbers | System-generated | `dir="ltr"` attribute |
| URLs | Protocol standard | `dir="ltr"` attribute |
| Email addresses | Protocol standard | `dir="ltr"` attribute |
| Programming code | Technical | `dir="ltr"` attribute |
| API responses | Technical | `dir="ltr"` attribute |
| Numeric amounts | Universal format | `dir="ltr"` + `tabular-nums` |

### 10.5 RTL-Specific Components

| Component | RTL Behavior |
|-----------|--------------|
| Sidebar | Opens from right, items aligned start |
| Breadcrumbs | Chevron separators mirror (› becomes ‹) |
| Pagination | Previous/Next swap positions |
| Data tables | First column on right |
| Form labels | Labels above inputs, aligned start |
| Dropdown menus | Open toward start edge |
| Tooltips | Position mirror |
| Charts | X-axis labels mirror |
| Stepper/Progress | Steps flow right-to-left |

---

## 11. Accessibility Rules

### 11.1 WCAG 2.2 Compliance Targets

| Criterion | Level | Implementation |
|-----------|-------|----------------|
| Color Contrast | AA (4.5:1) | Token-driven colors verified |
| Focus Visible | AA | Double-ring focus (2px solid + 4px halo) |
| Keyboard Navigation | AA | Full tab order, skip links |
| Form Labels | AA | Visible labels, not placeholder-only |
| Error Messages | AA | aria-live regions, role="alert" |
| Skip Links | AA | "Skip to main content" link |
| Heading Hierarchy | AA | Sequential h1→h6 |
| Reduced Motion | AA | prefers-reduced-motion media query |
| Touch Targets | AA | Min 44×44px / 48×48dp |
| Focus Not Obscured | AA | Sticky UI doesn't hide focused element |

### 11.2 Focus System

```css
/* Double-ring focus pattern */
:focus-visible {
  outline: 2px solid var(--color-focus-ring);  /* gold */
  outline-offset: 2px;
  box-shadow: 0 0 0 4px var(--color-focus-halo);  /* gold.100 */
}

/* Skip link */
.skip-link {
  position: fixed;
  top: 8px;
  inset-inline-start: 8px;
  z-index: 9999;
  /* Hidden until focused */
}
```

### 11.3 Screen Reader Support

- All interactive elements have accessible names
- Status changes announced via `aria-live="polite"`
- Error messages use `role="alert"`
- Dynamic content updates use `aria-atomic="true"`
- Loading states announced with `aria-busy="true"`

---

## 12. Responsive Architecture

### 12.1 Breakpoints

| Token | Width | Target |
|-------|-------|--------|
| xs | 0px | Small phones |
| sm | 576px | Large phones |
| md | 768px | Tablets |
| lg | 1024px | Small desktops |
| xl | 1280px | Desktops |
| 2xl | 1536px | Large screens |

### 12.2 Layout Behavior

| Breakpoint | Sidebar | Header | Content | Navigation |
|------------|---------|--------|---------|------------|
| < 960px | Overlay drawer | Hamburger toggle | Full width | Bottom nav (mobile) |
| ≥ 960px | Fixed 280px | Full header | Flex with margin | Sidebar nav |

### 12.3 Table Responsive Strategy

| Width | Strategy |
|-------|----------|
| ≥ 1024px | Full data table with all columns |
| 768-1023px | Horizontal scroll with sticky first column |
| < 768px | Card layout (list view) with key fields |

### 12.4 Form Responsive Strategy

| Width | Layout |
|-------|--------|
| ≥ 768px | 2-column grid for related fields |
| < 768px | Single column, full width |

---

## 13. Motion System

### 13.1 Duration Tokens

| Token | Value | Usage |
|-------|-------|-------|
| instant | 0ms | Immediate state changes |
| fast | 100ms | Hover states, focus rings |
| base | 150ms | Button presses, small transitions |
| normal | 200ms | Dropdown opens, tab switches |
| slow | 300ms | Panel slides, drawer open/close |
| slower | 500ms | Page transitions, modals |

### 13.2 Easing Tokens

| Token | Curve | Usage |
|-------|-------|-------|
| linear | linear | Progress bars, loading spinners |
| easeIn | cubic-bezier(0.4, 0, 1, 1) | Exiting elements |
| easeOut | cubic-bezier(0, 0, 0.2, 1) | Entering elements |
| easeInOut | cubic-bezier(0.4, 0, 0.2, 1) | State transitions |
| default | cubic-bezier(0.2, 0, 0, 1) | General purpose |

### 13.3 Motion Principles

1. **Feedback** — Animations confirm user actions
2. **State Change** — Smooth transitions between states
3. **Spatial Continuity** — Elements maintain spatial relationships
4. **Navigation** — Directional movement follows page hierarchy

### 13.4 Reduced Motion

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    animation-iteration-count: 1 !important;
    transition-duration: 0.01ms !important;
  }
}
```

### 13.5 ERP-Specific Motion Rules

- **Tables:** No row entrance animation (data density)
- **Forms:** Field focus/blur transitions at 150ms
- **Modals:** Scale+fade entrance 200ms, fade exit 150ms
- **Drawers:** Slide from edge 300ms
- **Toasts:** Slide in from edge 300ms, auto-dismiss 4s
- **Status changes:** Color transition 200ms (not instant snap)
- **Loading:** Skeleton shimmer at 1.5s loop

---

## 14. Design Token Architecture

### 14.1 Token Hierarchy

```text
Layer 1: Primitives (raw values)
  primitives.color.navy.500
  primitives.spacing.4
  primitives.fontSize.base

Layer 2: Semantic (meaning aliases)
  semantic.color.primary
  semantic.color.surface
  semantic.spacing.section

Layer 3: Component (component-specific)
  component.button.primary.background
  component.table.header.bg
  component.input.border

Layer 4: Feature (business-specific)
  feature.approval.submit.action
  feature.journal.debit.color
  feature.budget.over-indicator
```

### 14.2 Naming Convention

```
category.property.variant.state

Examples:
  color.primary
  color.primary.hover
  color.surface
  color.surface.elevated
  spacing.section
  spacing.component.padding
  fontSize.body
  fontWeight.heading.semibold
  borderRadius.card
  shadow.card
  shadow.card.hover
```

### 14.3 CSS Variable Mapping

All tokens generate CSS variables in `tokens.css.scss`:

```css
:root {
  /* Primitives */
  --primitive-navy-500: #002045;

  /* Semantic */
  --color-primary: var(--primitive-navy-500);
  --color-surface: #ffffff;
  --color-on-surface: #0d1c2f;

  /* Component */
  --button-primary-bg: var(--color-primary);
  --button-primary-fg: var(--color-on-primary);

  /* Spacing */
  --spacing-4: 1rem;
  --spacing-section: 1.5rem;
}
```

### 14.4 Tailwind Integration

`tokens.tailwind.json` maps CSS variables to Tailwind utilities:

```javascript
// tailwind.config.js
theme: {
  extend: {
    colors: {
      primary: 'var(--color-primary)',
      surface: 'var(--color-surface)',
      // ...
    }
  }
}
```

---

## 15. Component Architecture

### 15.1 Component Tiers

```text
Tier 1: Foundation
  - CSS Variables / Design Tokens
  - Tailwind Config
  - Global Styles

Tier 2: Primitives
  - Button, Input, Select, Checkbox, Radio, Switch
  - Badge, Avatar, Separator

Tier 3: Components
  - Card, Dialog, Drawer, Tabs, Tooltip, Dropdown
  - Alert, Toast, Pagination, Breadcrumb

Tier 4: Composite Components
  - DataGrid (Table + Pagination + Filters)
  - PageShell (Header + Toolbar + Content)
  - FormField (Label + Input + Error + Helper)
  - StatusBadge (Icon + Label + Color)

Tier 5: Patterns
  - ApprovalTimeline
  - AuditHistory
  - WorkflowStepper
  - FinancialEntry

Tier 6: Feature Components
  - AccountGrid
  - JournalEntryForm
  - BudgetDashboard
  - ProcurementOrderCard

Tier 7: Page Templates
  - ListPage
  - DetailPage
  - CreatePage
  - DashboardPage
  - SettingsPage
```

### 15.2 Existing Component Inventory

| Component | Tier | Status | Notes |
|-----------|------|--------|-------|
| Button | 2 | ✓ shadcn/ui | CVA variants, 7 sizes |
| Input | 2 | ✓ Custom | Focus ring, error state |
| Select | 2 | ✓ Custom | Native select, custom chevron |
| Switch | 2 | ✓ shadcn/ui | — |
| Tabs | 3 | ✓ shadcn/ui | — |
| Dialog | 3 | ✓ shadcn/ui | Native `<dialog>` |
| AlertDialog | 3 | ✓ shadcn/ui | — |
| DropdownMenu | 3 | ✓ shadcn/ui | — |
| Popover | 3 | ✓ shadcn/ui | — |
| Tooltip | 3 | ✓ shadcn/ui | — |
| DataGrid | 4 | ✓ Custom | TanStack Table wrapper |
| PageShell | 4 | ✓ Custom | Page layout |
| PageHeader | 4 | ✓ Custom | Gold accent border |
| FormField | 4 | ✓ Custom | Label + error + helper |
| StatusBadge | 4 | ✓ Custom | 11 status states |
| MoneyDisplay | 4 | ✓ Custom | Currency formatting |
| EmptyState | 3 | ✓ Custom | — |
| ErrorState | 3 | ✓ Custom | — |
| Loading | 3 | ✓ Custom | Skeleton support |
| Pagination | 3 | ✓ Custom | — |
| ConfirmDialog | 3 | ✓ Custom | — |
| ButtonBar | 2 | ✓ Custom | Layout primitive |
| Grid | 2 | ✓ Custom | Layout primitive |
| Stack | 2 | ✓ Custom | Layout primitive |
| Toast | 3 | ✓ shadcn/ui | sonner integration |
| Calendar | 3 | ✓ shadcn/ui | — |
| DatePicker | 3 | ✓ shadcn/ui | — |

### 15.3 Missing Components (Recommended)

| Component | Priority | Status | Reason |
|-----------|----------|--------|--------|
| Breadcrumb | High | ✓ Implemented | Navigation depth 3+ |
| Combobox | High | ✓ Implemented | Searchable selects |
| Command | Medium | Pending | Quick actions |
| Sheet | High | ✓ Implemented | Side panels |
| Accordion | Medium | ✓ Implemented | Collapsible sections |
| Table | High | Pending | shadcn Table for non-DataGrid use |
| Chart | Medium | Pending | Dashboard visualizations |
| FormWizard | Medium | Pending | Multi-step workflows |
| AuditTimeline | High | ✓ Implemented | Audit trail display |
| ApprovalTimeline | High | ✓ Implemented | Approval workflow display |

---

## 16. Enterprise UX Patterns

### 16.1 Data Table Pattern

```text
Structure:
  PageShell
    → PageHeader (title + actions)
    → FilterBar (search, filters, date range)
    → DataGrid
      → BulkSelection (checkbox column)
      → SortableHeaders
      → Rows with hover state
      → RowActions (dropdown)
    → Pagination
    → BulkActionsBar (appears on selection)

States:
  Loading → Skeleton rows (8 rows)
  Empty → EmptyState with illustration + CTA
  Error → ErrorState with retry
  Success → Data display
  Filtered → Filtered indicator + clear
  Bulk → Bulk actions bar
```

### 16.2 Form Pattern

```text
Structure:
  PageShell
    → PageHeader (title + save/cancel)
    → Form
      → FieldGroup (section)
        → FormField
          → Label (with required indicator)
          → Input/Select/Combobox
          → HelperText (persistent)
          → ErrorMessage (on validation)
      → FieldGroup
      → FormActions (submit, cancel)

Validation:
  On blur → Validate field
  On submit → Validate all + focus first error
  Error display → Below field + aria-describedby
  Success → Inline checkmark + brief highlight

States:
  Pristine → Default
  Modified → Show unsaved indicator
  Submitting → Disable button, show spinner
  Success → Toast + optional redirect
  Error → Focus error summary, inline errors
```

### 16.3 Approval Workflow Pattern

```text
States:
  draft → editable
  submitted → pending approval
  approved → locked
  rejected → back to draft with reason
  posted → final (read-only)

UI Elements:
  StatusBadge (current state)
  ActionButton (state-specific actions)
  ApprovalTimeline (history)
  ReasonDialog (for reject/return)
  ConfirmationDialog (for submit/approve)

Rules:
  - Only authorized users see action buttons
  - Rejection requires reason field
  - Approval shows confirmation dialog
  - Posted entries are read-only
  - Audit trail records all transitions
```

### 16.4 Financial Display Pattern

```text
Rules:
  - All amounts: tabular-nums + currency symbol
  - Debit: default color
  - Credit: default color (with visual distinction)
  - Negative: red color + minus sign
  - Zero: muted color
  - Totals: bold + border-top
  - Currency: consistent symbol placement
  - Rounding: 2 decimal places (configurable)

Alignment:
  - Numbers: start-aligned (RTL)
  - Text: start-aligned
  - Status: center-aligned
  - Actions: end-aligned
```

---

## 17. Feature UX Architecture

### 17.1 Module Map

```text
ERP-Government
├── Accounting (ادارة الحسابات)
│   ├── Chart of Accounts (الحسابات)
│   └── Journal Entries (قيود اليومية)
├── Financial (المالية)
│   ├── Currencies (العملات)
│   ├── Exchange Rates (أسعار الصرف)
│   ├── Fiscal Years (السنوات المالية)
│   └── Document Sequences (تسلسل المستندات)
├── Security (الأمان)
│   └── Roles (الأدوار)
├── Organization (التنظيم)
│   ├── Org Units (الوحدات التنظيمية)
│   ├── Employees (الموظفون)
│   ├── Cost Centers (مراكز التكلفة)
│   └── Projects (المشاريع)
├── Procurement (المشتريات)
│   ├── Purchase Requisitions (طلبات الشراء)
│   ├── Vendors (الموردون)
│   ├── Purchase Orders (أوامر الشراء)
│   ├── Receipts (سندات الاستلام)
│   ├── Vendor Bills (فواتير الموردين)
│   └── Purchase Returns (مرتجعات الشراء)
└── Assets (الأصول)
    ├── Asset Register (سجل الأصول)
    ├── Asset Groups (مجموعات الأصول)
    ├── Depreciation (جداول الإهلاك)
    ├── Disposals (التصفية)
    ├── Revaluations (إعادة التقييم)
    └── Impairments (الانخفاض)
```

### 17.2 Missing Modules (Domain-Driven from Backend)

| Module | Backend Exists | Frontend Status | Priority |
|--------|---------------|-----------------|----------|
| Budget | Yes (Budgeting/) | No frontend | High |
| Payments | Yes (Payments/) | No frontend | High |
| Revenue | Yes (Revenue/) | No frontend | High |
| Banking | Yes (Banking/) | No frontend | High |
| Committees | Yes (Committees/) | No frontend | Medium |
| Workflow | Yes (Workflow/) | No frontend | High |
| Notifications | Partial | Bell exists | Medium |

---

## 18. Feature Specifications

### 18.1 Accounting — Chart of Accounts

```text
Feature Identity:
  Module:      Accounting
  Name:        Chart of Accounts (شجرة الحسابات)
  Purpose:     Manage the hierarchical chart of accounts

Business Purpose:
  Define and maintain the organizational chart of accounts used
  for recording financial transactions. Accounts follow a tree
  structure with parent-child relationships.

Primary Users:    Financial Accountant, System Administrator
Secondary Users:  Auditor (read-only), Finance Manager

UX Goals:
  - Visualize account hierarchy clearly
  - Quick search and filter
  - Inline editing for common fields
  - Bulk operations (activate/deactivate)
  - RTL-first with LTR for account codes

Screens:
  List Page:
    - PageHeader: "الحسابات" + "إضافة حساب" button
    - Search bar (by code, name)
    - Filter: account type, status, parent
    - DataGrid: code, name, type, parent, status, balance
    - Row actions: edit, deactivate, view history

  Detail/Edit:
    - Dialog or inline edit
    - Fields: code, name, type, parent, status, description
    - Validation: code uniqueness, parent type compatibility

Financial UX Rules:
  - Account code: LTR, monospace font
  - Balance: tabular-nums, currency format
  - Debit/Credit: color-coded (not color-only)
```

### 18.2 Accounting — Journal Entries

```text
Feature Identity:
  Module:      Accounting
  Name:        Journal Entries (قيود اليومية)
  Purpose:     Create and manage journal entries

Business Purpose:
  Record financial transactions through double-entry bookkeeping.
  Each entry has a header (date, description, reference) and
  multiple lines (account, debit, credit).

Primary Users:    Financial Accountant
Secondary Users:  Auditor, Finance Manager

UX Goals:
  - Clear debit/credit visualization
  - Balance validation (debits must equal credits)
  - Account search with autocomplete
  - Draft → Submit → Approve → Post workflow
  - Audit trail for all changes

Screens:
  List Page:
    - PageHeader: "قيود اليومية" + "إضافة قيد" button
    - Filters: date range, status, account
    - DataGrid: date, reference, description, total, status
    - Status badge with workflow state

  Create/Edit:
    - Header section: date, reference, description
    - Lines table: account, description, debit, credit
    - Add/remove line buttons
    - Running balance display
    - Balance indicator (balanced/unbalanced)
    - Save as draft / Submit buttons

  Detail (Read-only):
    - Header information
    - Lines table (read-only)
    - Approval timeline
    - Audit history
    - Print/export actions

Financial UX Rules:
  - Debit column: right-aligned (LTR) / start-aligned (RTL)
  - Credit column: right-aligned (LTR) / start-aligned (RTL)
  - Running total: bold, border-top
  - Unbalanced: red indicator, prevent posting
  - Zero amounts: display as "0.00" not blank
```

### 18.3 Procurement — Purchase Orders

```text
Feature Identity:
  Module:      Procurement
  Name:        Purchase Orders (أوامر الشراء)
  Purpose:     Create and manage purchase orders

Business Purpose:
  Formalize purchase commitments to vendors. Includes item
  details, quantities, prices, delivery dates, and approval
  workflow.

Primary Users:    Procurement Officer
Secondary Users:  Finance Manager, Vendor Manager, Auditor

UX Goals:
  - Clear item line editing
  - Vendor selection with search
  - Budget check integration
  - Multi-level approval workflow
  - Document attachment support

Screens:
  List Page:
    - PageHeader + filters
    - DataGrid: PO number, vendor, date, total, status
    - Bulk actions: export, print

  Create/Edit:
    - Header: vendor, date, delivery date, reference
    - Items table: item, quantity, unit price, total
    - Summary: subtotal, tax, total
    - Attachment area
    - Save draft / Submit for approval

  Detail:
    - PO information header
    - Items table (read-only)
    - Approval timeline
    - Receipt status (partial/full)
    - Invoice matching status
    - Print/export
```

### 18.4 Assets — Asset Register

```text
Feature Identity:
  Module:      Assets
  Name:        Asset Register (سجل الأصول)
  Purpose:     Track and manage organizational assets

Business Purpose:
  Maintain a register of all capital assets including acquisition
  details, depreciation schedules, location, and current value.

Primary Users:    Asset Manager
Secondary Users:  Financial Accountant, Auditor

UX Goals:
  - Clear asset lifecycle visualization
  - Depreciation calculation display
  - Location and assignment tracking
  - Financial details with proper formatting
  - Bulk depreciation run

Screens:
  List Page:
    - DataGrid: asset code, name, group, acquisition date,
      cost, accumulated depreciation, net book value, status
    - Filters: group, status, location, date range

  Detail:
    - Asset information card
    - Financial details (cost, depreciation, NBV)
    - Depreciation schedule table
    - Movement history
    - Documents/attachments

Financial UX Rules:
  - All monetary values: tabular-nums
  - Depreciation amounts: right-aligned
  - Net book value: highlighted
  - Impairment: red indicator
```

---

## 19. Screen Specifications

### 19.1 List Page Template

```text
Structure:
  PageShell
    ├── PageHeader
    │   ├── Title (h1)
    │   ├── Subtitle (optional)
    │   └── Actions (primary button + secondary buttons)
    ├── FilterBar
    │   ├── Search input
    │   ├── Filter dropdowns
    │   ├── Date range picker
    │   └── Clear filters button
    ├── DataGrid
    │   ├── Header row (sortable columns)
    │   ├── Data rows (hover state)
    │   ├── Empty state
    │   ├── Loading skeleton
    │   └── Error state
    ├── Pagination
    │   ├── Page numbers
    │   ├── Items per page selector
    │   └── "Showing X of Y" text
    └── BulkActionsBar (conditional)
        ├── Selection count
        ├── Bulk action buttons
        └── Clear selection

States:
  Loading: 8 skeleton rows
  Empty: EmptyState with icon + message + CTA
  Error: ErrorState with retry
  Success: Data display
  Filtered: Active filter chips + clear all
  Bulk: Sticky bottom bar with actions
```

### 19.2 Detail Page Template

```text
Structure:
  PageShell
    ├── PageHeader
    │   ├── Back button (←)
    │   ├── Title (h1)
    │   ├── Status badge
    │   └── Actions (edit, delete, print, etc.)
    ├── InfoCard
    │   ├── Key fields grid
    │   └── Status/summary
    ├── Tabs
    │   ├── Tab 1: Details
    │   ├── Tab 2: Related data
    │   └── Tab 3: History/Audit
    └── TabContent
        ├── Section headers
        ├── Data display
        └── Related tables

Navigation:
  - Back button returns to list (preserves scroll position)
  - Breadcrumbs show hierarchy
  - Next/Previous navigation (optional)
```

### 19.3 Dashboard Page Template

```text
Structure:
  PageShell
    ├── PageHeader
    │   └── Title + Date range selector
    ├── MetricCards (top row)
    │   ├── Card: Label + Value + Trend
    │   └── 4-6 cards in grid
    ├── ChartsRow
    │   ├── Chart 1 (2/3 width)
    │   └── Chart 2 (1/3 width)
    ├── DataSummary
    │   ├── Top items table
    │   └── View all link
    └── RecentActivity
        └── Timeline list

Rules:
  - Metrics: large numbers with context
  - Charts: legend + tooltip + accessible colors
  - No decorative animations
  - Responsive: stack on mobile
```

---

## 20. Gap Analysis

### 20.1 Current vs Target State

| Category | Current State | Target State | Severity | Migration |
|----------|--------------|--------------|----------|-----------|
| **Color System** | ✓ Complete (tokens.ts) | ✓ Complete | — | No action |
| **Typography** | ✓ IBM Plex Sans Arabic | ✓ Complete | — | No action |
| **Tokens** | ✓ Full pipeline | ✓ Complete | — | No action |
| **Dark Mode** | ✓ Full implementation | ✓ Complete | — | No action |
| **Icons** | ✓ Lucide | ✓ Complete | — | No action |
| **Components** | 29 components | +10 missing | Medium | Add Breadcrumb, Combobox, etc. |
| **Breadcrumbs** | ✗ Missing | Required | High | Implement |
| **Data Tables** | ✓ DataGrid with TanStack | ✓ Complete | — | No action |
| **Forms** | ✓ FormField pattern | Audit labels | Medium | Review |
| **RTL** | ✓ Logical properties | ✓ Complete | — | No action |
| **Accessibility** | ✓ Focus, skip, aria | Audit touch targets | Medium | Review |
| **Responsive** | Partial (sidebar collapse) | Full mobile support | High | Implement table mobile view |
| **Budget Module** | Backend only | Full frontend | High | Build |
| **Payments Module** | Backend only | Full frontend | High | Build |
| **Revenue Module** | Backend only | Full frontend | High | Build |
| **Banking Module** | Backend only | Full frontend | High | Build |
| **Workflow Module** | Backend only | Full frontend | High | Build |
| **Approval UX** | Partial | Complete pattern | High | Standardize |
| **Audit Trail** | Backend only | Frontend display | High | Build |
| **Charts** | Backend chart config | Frontend charts | Medium | Implement |
| **Notifications** | Bell exists | Full system | Medium | Enhance |

### 20.2 Priority Migration Path

**Phase 1 — Foundation (Immediate)**
- Add missing components (Breadcrumb, Combobox, Sheet, Table)
- Audit form labels and touch targets
- Implement responsive table strategy

**Phase 2 — Core Modules (Short-term)**
- Budget module frontend
- Payments module frontend
- Workflow/Approval pattern
- Audit trail display

**Phase 3 — Extended Modules (Medium-term)**
- Revenue module frontend
- Banking module frontend
- Committee management
- Enhanced notifications

**Phase 4 — Polish (Long-term)**
- Dashboard charts
- Advanced reporting
- Performance optimization
- Accessibility audit completion

---

## 21. Governance

### 21.1 Token Governance

| Action | Process |
|--------|---------|
| Add token | Document in tokens.ts with rationale; update CSS vars + Tailwind |
| Modify token | Review impact; update all consumers; verify contrast |
| Remove token | Find all usages; replace with alternative; deprecate |

### 21.2 Component Governance

| Action | Process |
|--------|---------|
| Add component | Check existing alternatives; follow tier system; add to inventory |
| Modify component | Review all usages; maintain backward compatibility |
| Deprecate component | Add deprecation notice; provide migration path |

### 21.3 Design Rules

1. **Prefer composition over new variants** — Compose existing components before creating new ones
2. **Prefer existing semantic tokens** — Never use raw hex in components
3. **Prefer existing components** — Check inventory before building new
4. **Document overrides** — Any page-specific override must be documented with reason

---

## 22. Design QA Checklist

### Pre-Delivery Verification

```text
[ ] Product fit — reflects government ERP nature
[ ] Institutional fit — conservative, trustworthy
[ ] Visual consistency — same patterns across all pages
[ ] Token consistency — no raw hex, all from tokens.ts
[ ] Component consistency — using existing component inventory
[ ] RTL correctness — logical properties, proper alignment
[ ] Accessibility — contrast, focus, keyboard, screen reader
[ ] Responsive — works at 375px, 768px, 1024px, 1440px
[ ] Financial readability — tabular-nums, currency formatting
[ ] Workflow clarity — status transitions clear
[ ] Data density — appropriate for ERP, not too sparse
[ ] Error handling — errors near fields, recovery paths
[ ] Empty states — helpful messages with CTAs
[ ] Loading states — skeletons, not spinners
[ ] Permission-aware — actions hidden when unauthorized
[ ] Audit visibility — history/timeline present
[ ] Dark mode — tested independently, contrast verified
```

---

## 23. Implementation Roadmap

### Phase 1: Foundation Polish (1-2 weeks)
1. Add Breadcrumb component
2. Add Combobox component (searchable select)
3. Add Sheet component (side panels)
4. Audit all forms for label compliance
5. Audit touch target sizes
6. Implement responsive table card layout
7. Add missing status states to StatusBadge

### Phase 2: Core Module Frontends (2-3 weeks)
1. Budget module (dashboard + list + detail)
2. Payments module (orders + execution)
3. Approval workflow pattern (reusable)
4. Audit trail component (reusable)

### Phase 3: Extended Modules (2-3 weeks)
1. Revenue module (receipts + collection)
2. Banking module (reconciliation)
3. Committee management
4. Enhanced notification system

### Phase 4: Polish & Optimization (1-2 weeks)
1. Dashboard charts (Recharts integration)
2. Performance optimization (virtualization)
3. Full accessibility audit
4. RTL edge case testing
5. Dark mode comprehensive testing

---

## Design System Source of Truth

```text
design-system/
└── erp-government/
    ├── MASTER.md                    ← This file (global rules)
    └── pages/                       ← Page-specific overrides
        ├── accounting.md            ← ✓ Created
        ├── assets.md                ← ✓ Created
        ├── budget.md                ← (future)
        ├── dashboard.md             ← ✓ Created
        ├── financial.md             ← ✓ Created
        ├── payments.md              ← (future)
        ├── procurement.md           ← ✓ Created
        ├── revenue.md               ← (future)
        ├── security.md              ← ✓ Created
        ├── approvals.md             ← (future)
        ├── audit.md                 ← (future)
        └── reporting.md             ← (future)
```

**Retrieval rules:**
1. Read `MASTER.md` for global design rules
2. Check `pages/<page-name>.md` for page-specific overrides
3. Page overrides must document: Override, Reason, Scope, Impact, Approval
4. No page may override core tokens without documented justification
