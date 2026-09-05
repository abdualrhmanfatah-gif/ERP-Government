# Feature Specification: Budgeting Frontend Foundation #1 of 5

**Feature Branch**: `011-budgeting-frontend-foundation`

**Created**: 2026-09-04

**Status**: Draft

**Input**: User description: "Budgeting frontend foundation #1 of 5 - shared types/client/AvailabilityIndicator + BudgetTypes and Funds pages. Builds shared infrastructure consumed by all 5 frontend specs + first two reference-data features. features/budgeting/ is clean slate. Reuse existing UI kit components."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Shared Budgeting Types and Client Infrastructure (Priority: P1)

A frontend developer working on any of the 5 budgeting frontend specs needs a single source of truth for all budgeting data transfer objects, enums, API client functions, and cache management. This shared layer ensures every budgeting page consumes identical types and communicates with the backend through a consistent, typed client.

**Why this priority**: All 5 frontend specs depend on this shared layer. Without it, each spec would duplicate types, client logic, and query key management. This is the foundation that prevents drift between specs.

**Independent Test**: Can be fully tested by importing every DTO type, enum, and client function from the shared layer and confirming they compile without type errors and that each enum has a corresponding Arabic label entry.

**Acceptance Scenarios**:

1. **Given** a developer imports BudgetTypeDto from the shared types, **When** they inspect the type, **Then** all fields match the backend DTO shape (id, code, name, description, controlMethod, allowOverrun, isActive, rowVersion) with no missing or extra fields.
2. **Given** a developer imports the budgeting query key factory, **When** they call the factory for any entity group, **Then** it returns a stable, deterministic key array suitable for cache invalidation.
3. **Given** a developer calls the list function for BudgetTypes, **When** the backend returns results, **Then** the response is typed as an array of BudgetTypeDto with no `any` usage.
4. **Given** a developer encounters an unknown enum value from the backend, **When** the Arabic label map is consulted, **Then** the raw enum name is displayed as a fallback rather than crashing or showing blank.

---

### User Story 2 - BudgetType Reference Data Management (Priority: P1)

A budget administrator navigates to the Budget Types page to view, create, edit, and toggle the active state of budget types. Each budget type defines the control regime (None, Warning, or Blocking) and overrun policy that governs downstream budgets.

**Why this priority**: BudgetTypes are prerequisite reference data for creating Budgets (spec #2). The control method and AllowOverrun root the inheritance chain that governs all availability checks.

**Independent Test**: Can be fully tested by navigating to /budgeting/budget-types, viewing the list, creating a new budget type, editing it, toggling its active state, and confirming all CRUD operations reflect in the list.

**Acceptance Scenarios**:

1. **Given** a user with BudgetTypes.View permission, **When** they navigate to /budgeting/budget-types, **Then** a data grid displays all budget types with columns for Code, Name, ControlMethod (Arabic label), AllowOverrun (badge), and IsActive (toggle switch).
2. **Given** a user with BudgetTypes.Create permission, **When** they open the create dialog and fill in Code, Name, Description, ControlMethod (select), and AllowOverrun (switch), **Then** the new budget type appears in the list after save.
3. **Given** a user with BudgetTypes.Update permission, **When** they edit an existing budget type's Name or ControlMethod, **Then** the changes are persisted and the row updates in the list.
4. **Given** a user with BudgetTypes.Update permission, **When** they toggle the IsActive switch on a budget type, **Then** a confirmation dialog appears and upon confirmation the active state is toggled.
5. **Given** a user without BudgetTypes.Create permission, **When** they view the Budget Types page, **Then** the create action button is not visible.
6. **Given** the list is filtered by search text, ControlMethod, or IsActive, **When** the user applies filters, **Then** the grid updates to show only matching records.

---

### User Story 3 - Fund Reference Data Management (Priority: P1)

A budget administrator navigates to the Funds page to view, create, edit, and manage funds. Each fund represents a financial fund (General, Special, or Project) with an operating or capital category, optionally scoped to a fiscal year and linked to a default revenue account.

**Why this priority**: Funds are prerequisite reference data for creating Budgets (spec #2). Every budget must reference a fund.

**Independent Test**: Can be fully tested by navigating to /budgeting/funds, viewing the list, creating a new fund, editing it, and confirming all CRUD operations reflect in the list and detail view.

**Acceptance Scenarios**:

1. **Given** a user with Funds.View permission, **When** they navigate to /budgeting/funds, **Then** a data grid displays all funds with columns for FundNumber, FundName, FundType, FundCategory, FiscalYear, DefaultRevenueDebitAccount, and IsActive.
2. **Given** a user with Funds.Create permission, **When** they open the create dialog and fill in FundNumber, FundName, FundType (select), FundCategory (select), FiscalYear (combobox), LegalAuthority, Description, and DefaultRevenueDebitAccount (combobox), **Then** the new fund appears in the list after save.
3. **Given** a user with Funds.Update permission, **When** they edit an existing fund, **Then** the changes are persisted and reflected in both the list and detail view.
4. **Given** the Account Combobox in the fund form, **When** the accounts endpoint returns an empty list, **Then** the combobox is disabled and displays a hint indicating no accounts are available.
5. **Given** a user with Funds.View permission, **When** they click a fund row, **Then** they navigate to /budgeting/funds/:id and see the fund detail view with all fields displayed.
6. **Given** the list is filtered by search text, FundType, FundCategory, or IsActive, **When** the user applies filters, **Then** the grid updates to show only matching records.

---

### User Story 4 - Availability Indicator Component (Priority: P2)

A budget officer or procurement officer viewing an appropriation or encumbrance form sees a real-time availability indicator that shows whether the current amount is within budget (green), exceeds budget but overruns are allowed (amber), or exceeds budget and overruns are blocked (red).

**Why this priority**: The availability indicator is consumed by specs #3 (Appropriations) and #4 (Encumbrances). Building it here as a shared component ensures consistency and avoids duplication.

**Independent Test**: Can be fully tested by rendering the indicator with mock availability data for each state (green, amber, red, zero, loading) and confirming correct color, amount display, and RTL layout.

**Acceptance Scenarios**:

1. **Given** an availability query returns a positive available amount with ControlMethod Blocking, **When** the indicator renders, **Then** it displays green with the available amount shown via the money display component.
2. **Given** an availability query returns a negative available amount with ControlMethod Warning, **When** the indicator renders, **Then** it displays amber with the overrun amount.
3. **Given** an availability query returns a negative available amount with ControlMethod Blocking, **When** the indicator renders, **Then** it displays red with the overrun amount and a blocked indicator.
4. **Given** an availability query returns any available amount with ControlMethod None, **When** the indicator renders, **Then** it displays green with the available amount (no control enforced).
5. **Given** an availability endpoint returns an empty response (no appropriations exist), **When** the indicator renders, **Then** it displays a zero-state message rather than an error.
6. **Given** the availability query is loading, **When** the indicator renders, **Then** it displays a loading skeleton.
7. **Given** the indicator is rendered in dark mode and RTL, **When** the user views it, **Then** colors adapt to dark mode tokens and layout is right-to-left.

---

### User Story 5 - Routing and Sidebar Navigation (Priority: P1)

A user opens the application and sees a sidebar with a "الموازنة" (Budgeting) group containing links to the implemented features (Budget Types and Funds). As specs #2 through #5 land, additional items appear in this group progressively.

**Why this priority**: Without routes and sidebar entries, users cannot reach the budgeting pages. Progressive activation ensures the sidebar only shows features that are actually implemented.

**Independent Test**: Can be fully tested by loading the application, confirming the "الموازنة" sidebar group appears with only Budget Types and Funds links, and that clicking each link navigates to the correct page.

**Acceptance Scenarios**:

1. **Given** the application loads, **When** the sidebar renders, **Then** a group labeled "الموازنة" appears containing links to "أنواع الموازنة" (Budget Types) and "الأموال" (Funds).
2. **Given** a user without BudgetTypes.View permission, **When** the sidebar renders, **Then** the Budget Types link is not visible.
3. **Given** a user without Funds.View permission, **When** the sidebar renders, **Then** the Funds link is not visible.
4. **Given** a user navigates to /budgeting/budget-types, **When** the page loads, **Then** the Budget Types link in the sidebar is highlighted as active.
5. **Given** the interface is set to RTL, **When** the sidebar renders, **Then** it appears on the right side of the screen with correct text alignment.

---

### User Story 6 - Dark Mode and Design Token Compliance (Priority: P2)

A user toggles between light and dark mode and observes that all budgeting pages, dialogs, indicators, and navigation entries render correctly with no hardcoded colors or fonts.

**Why this priority**: Dark mode compliance is a constitutional requirement (Principle X). Design token adherence ensures visual consistency across the entire application.

**Independent Test**: Can be fully tested by toggling dark mode on every budgeting page and confirming all colors, spacing, typography, and status indicators derive from design tokens.

**Acceptance Scenarios**:

1. **Given** the user is in dark mode, **When** they view any budgeting page, **Then** all text, backgrounds, borders, and status colors match the dark mode token set.
2. **Given** the user is in light mode, **When** they view any budgeting page, **Then** all visual values match the light mode token set.
3. **Given** a developer searches the budgeting feature code, **When** they look for hardcoded color values (hex, rgb, hsl) or font-family declarations, **Then** zero matches are found outside of the design token system.

---

### Edge Cases

- What happens when the BudgetType list endpoint returns an empty array? The page shows an EmptyState component, not an error.
- What happens when a create or update mutation receives a RowVersion conflict (409)? The user sees a toast notification and the list is refetched to show current data.
- What happens when a user lacks permission for a specific action (e.g., BudgetTypes.Create)? The corresponding action button is hidden; no error is thrown.
- What happens when an unknown enum value is returned from the backend? The Arabic label map falls back to displaying the raw enum name as a string.
- What happens when the Fund create dialog references a FiscalYear combobox and no fiscal years exist? The combobox is disabled with a hint message.
- What happens when the Fund create dialog references an Account combobox and no accounts exist? The combobox is disabled with a hint message.
- What happens when a user attempts to edit a record that has been modified by another user since it was loaded? The RowVersion conflict triggers a toast and refetch.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001 (Shared Types)**: System MUST provide a shared types module containing typed interfaces for all 8 backend DTOs (BudgetTypeDto, FundDto, BudgetClassificationDto, BudgetDto, BudgetItemDto, AppropriationDto, EncumbranceDto, AvailabilityDto) and all 8 enums (BudgetControlMethod, FundType, FundCategory, BudgetStatus, AppropriationType, AppropriationStatus, EncumbranceType, EncumbranceStatus). Each enum MUST have an Arabic display label map. The types MUST match the backend DTO shapes exactly with no missing or extra fields.

- **FR-002 (Shared Client)**: System MUST provide a typed API client module with functions for every budgeting endpoint group (BudgetTypes, Funds, BudgetClassifications, Budgets, Appropriations, Encumbrances, Availability). Each function MUST return typed responses matching the DTOs. Error responses MUST be surfaced as structured problem-details errors. A cache key factory MUST be provided with a budgeting scope prefix and per-entity key generators for cache invalidation.

- **FR-003 (AvailabilityIndicator)**: System MUST provide an AvailabilityIndicator component that accepts an appropriation ID, fetches the availability endpoint, and renders one of four visual states: green (available amount positive, OR ControlMethod is None — no control enforced), amber (available amount negative, overrun allowed per ControlMethod Warning), red (available amount negative, overrun blocked per ControlMethod Blocking), or zero-state (no appropriations exist, not an error). Amounts MUST be displayed through the shared MoneyDisplay component. A loading skeleton MUST be shown during data fetching. The component MUST render correctly in dark mode and RTL layout.

- **FR-004 (BudgetTypes Page)**: System MUST provide a Budget Types list page at /budgeting/budget-types with: a DataGrid showing Code, Name, ControlMethod (Arabic label), AllowOverrun (badge), and IsActive (toggle switch); a FilterBar with client-side search, ControlMethod filter, and IsActive filter (all applied on the loaded dataset, no additional server requests); a create/edit Dialog (same dialog component, mode toggled) with fields for Code, Name, Description, ControlMethod (select), and AllowOverrun (switch); and a ConfirmDialog for IsActive toggle confirmation. Edit opens as a dialog overlay on the list page, not a separate route. The page MUST be permission-gated: actions visible only when the corresponding BudgetTypes.* permission is granted.

- **FR-005 (Funds Page)**: System MUST provide a Funds list page at /budgeting/funds with: a DataGrid showing FundNumber, FundName, FundType, FundCategory, FiscalYear, DefaultRevenueDebitAccount, and IsActive; a FilterBar with client-side search, FundType filter, FundCategory filter, and IsActive filter (all applied on the loaded dataset, no additional server requests); a create/edit Dialog (same dialog component, mode toggled) with fields for FundNumber, FundName, FundType (select), FundCategory (select), FiscalYear (combobox), LegalAuthority, Description, DefaultRevenueDebitAccount (combobox), and IsActive; and a detail view at /budgeting/funds/:id. Edit opens as a dialog overlay on the list page. Comboboxes for Account and FiscalYear MUST be disabled with a hint when the option list is empty. The page MUST be permission-gated: actions visible only when the corresponding Funds.* permission is granted.

- **FR-006 (Routes and Sidebar)**: System MUST register routes for /budgeting/budget-types (list), /budgeting/funds (list), and /budgeting/funds/:id (detail) with Arabic labels. The sidebar MUST display a group labeled "الموازنة" containing only the currently implemented budgeting features (Budget Types and Funds in this spec; additional items added by specs #2-#5). Each route and sidebar item MUST carry its required permission identifier. The sidebar MUST progressively activate as subsequent specs land.

- **FR-007 (Quality - RTL and Dark Mode)**: All budgeting pages, components, and navigation entries MUST render correctly in RTL (right-to-left) layout using logical positioning (start/end, not physical left/right). All visual values (colors, spacing, typography, radii, status colors) MUST originate from the central design token system. Zero hardcoded color or font values are permitted in budgeting feature code. Dark mode MUST be fully supported.

- **FR-008 (Quality - Keyboard Navigation)**: DataGrid rows MUST be keyboard-navigable. Dialogs MUST implement focus trapping. Tab order MUST follow a logical reading order in RTL layout.

- **FR-009 (Quality - Loading, Empty, Error States)**: Every query-dependent view MUST display a loading state (skeleton) during fetch, an empty state when no data exists, and an error state with retry capability when the request fails. These states MUST use the shared Loading, EmptyState, and ErrorState components.

- **FR-010 (Quality - Mutation Invalidation)**: Every create, update, or toggle mutation MUST invalidate the relevant query cache entries so that list and detail views reflect the change without requiring a manual page refresh.

- **FR-011 (Permission Gating)**: Every budgeting page MUST check permissions before rendering action buttons (create, edit, toggle). Users without the required permission MUST NOT see the corresponding action. The permission check is for user experience only; server-side authorization remains the sole authority per the project constitution.

### Key Entities

- **BudgetType**: Defines the control regime for budgets. Key attributes: Code, Name, Description, ControlMethod (None/Warning/Blocking), AllowOverrun (root of inheritance chain), IsActive. Referenced by Budget.
- **Fund**: A financial fund with type (General/Special/Project) and category (Operating/Capital). Key attributes: FundNumber, FundName, FundType, FundCategory, FiscalYear (optional), LegalAuthority, DefaultRevenueDebitAccount (optional), IsActive. Referenced by Budget.
- **BudgetClassification**: Hierarchical classification code. Key attributes: Code, Name, ParentId (self-reference), Level (computed). Referenced by BudgetItem. (DTO type defined here for shared layer completeness; CRUD page in spec #2.)
- **Budget**: An approved budget for a fiscal year and fund. Key attributes: BudgetNumber, BudgetName, BudgetTypeId, FiscalYearId, FundId, TotalAmount, Status, AllowOverrun (nullable, inherits from BudgetType). (DTO type defined here; full feature in spec #2.)
- **BudgetItem**: A line item within a budget, forming a tree. Key attributes: ItemCode, ItemName, BudgetId, ParentId, AllowOverrun (nullable, inherits from Budget). (DTO type defined here; full feature in spec #2.)
- **Appropriation**: An authorization to use a budget item's budget. Key attributes: AppropriationNumber, BudgetId, BudgetItemId, AppropriationType, Amount, Status. (DTO type defined here; full feature in spec #3.)
- **Encumbrance**: A reservation against an appropriation. Key attributes: EncumbranceNumber, AppropriationId, EncumbranceType, Amount, Status. (DTO type defined here; full feature in spec #4.)
- **AvailabilityDto**: Computed availability data for a budget item or encumbrance context. Key attributes: netAppropriated, totalEncumbered, available, controlMethod, effectiveAllowOverrun, warning.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: The build completes with zero errors and zero type-checking failures.
- **SC-002**: A user can perform full CRUD operations (list, create, edit, toggle active) against both BudgetTypes and Funds through the live backend, with list data reflecting changes immediately after mutation.
- **SC-003**: Every interface in the shared types module corresponds exactly to a backend DTO shape; a field-by-field comparison between the frontend types and the backend contract reveals zero mismatches.
- **SC-004**: The AvailabilityIndicator component renders all four visual states (green for positive or None control, amber for Warning overrun, red for Blocking overrun, zero for no data) and a loading skeleton, each distinguishable by color and content, in both light and dark mode.
- **SC-005**: The sidebar displays the "الموازنة" group with only the implemented features (Budget Types and Funds), each link navigates to the correct route, and the active link is visually highlighted. The sidebar renders correctly in RTL with the group on the right side.
- **SC-006**: An unauthenticated request to any budgeting endpoint receives a 401 response; an authenticated request from a user lacking the required permission receives a 403 response; no budgeting action button is visible to a user without the corresponding permission.
- **SC-007**: All budgeting pages render correctly in both light and dark mode with zero hardcoded color or font values; RTL layout uses logical properties throughout.

## Clarifications

### Session 2026-09-04

- Q: Should edits open in a dialog overlay or a separate edit route? → A: Dialog overlay on the list page (no route change). Keeps the user on the list, faster for reference data, avoids route proliferation.
- Q: How should the AvailabilityIndicator behave when ControlMethod is None? → A: Always green with the available amount. None means no availability check is enforced, so no restriction to signal.
- Q: Should the Funds detail view be a separate route or an expandable row? → A: Separate route at /budgeting/funds/:id. Funds have many fields that don't fit in an expandable row; supports direct linking.
- Q: Should list filtering be client-side or server-side? → A: Client-side filtering on full dataset. Budget types and funds are bounded reference data; instant feedback without backend changes.

## Assumptions

- The backend unified spec (010-rebuild-budgeting-module) has been executed and all 7 budgeting API endpoints are live and returning data per the contracts defined in that spec.
- The existing UI kit components (DataGrid, PageShell, PageHeader, FilterBar, FilterSearch, FilterSelect, StatusBadge, MoneyDisplay, ConfirmDialog, Dialog, Combobox, Switch, Loading, ErrorState, EmptyState, ButtonBar) exist and are functional as described.
- The existing permission model (usePermission hook, BUDGET_PERMISSIONS constants) is available; the current stub implementation (always-grant) is acceptable for this spec and will be replaced by real RBAC wiring in a future initiative.
- The design token system (tokens.ts, tokens.css.scss) is the single source of truth for all visual values; no new tokens are introduced by this feature.
- TanStack Query is the data-fetching layer; the QueryClient is configured with appropriate stale time and retry settings in the application bootstrap.
- The existing route configuration pattern (flat AppRoutes array in app/routes.tsx) and sidebar navigation pattern (moduleGroups array in layouts/navigation.ts) are reused without architectural changes.
- FiscalYear and Account reference data endpoints exist and return data for the Fund create/edit comboboxes; their types are defined externally and imported as needed.
- RowVersion is returned by every mutating endpoint and must be round-tripped for optimistic concurrency; conflict responses (409-style ProblemDetails) trigger a toast notification and list refetch.
- The shared types module is consumed by all 5 budgeting frontend specs; it is designed as the single source of truth and must not be duplicated or diverged from in any downstream spec.
