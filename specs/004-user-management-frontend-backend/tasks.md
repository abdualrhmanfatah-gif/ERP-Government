# Tasks: User Management Frontend + Backend

**Input**: Design documents from `/specs/004-user-management-frontend-backend/`

**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in spec. Test tasks omitted per template rules.

**Organization**: Tasks grouped by user story. Backend API is complete — work is primarily frontend.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Generate API client, create feature folder, define shared types

- [x] T001 Regenerate TypeScript API client via NSwag (`npm run generate-api` in `src/Web/ClientApp`) to pick up any missing user-management endpoints
- [x] T002 Create feature folder structure `src/Web/ClientApp/src/features/security/users/` with subdirectories: `pages/`, `components/`, `hooks/`
- [x] T003 [P] Define TypeScript types for User management in `src/Web/ClientApp/src/features/security/users/types.ts` (User, UserRole, UserPermission, UserSession, AuditEntry matching OpenAPI DTOs)
- [x] T004 [P] Create API client wrapper in `src/Web/ClientApp/src/features/security/users/client.ts` wrapping generated client methods for user CRUD, roles, permissions, sessions

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Shared hooks and components that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [x] T005 Create `useUsers` query hook in `src/Web/ClientApp/src/features/security/users/hooks/useUsers.ts` (list with search, filters, pagination using TanStack React Query)
- [x] T006 Create `useUserDetail` query hook in `src/Web/ClientApp/src/features/security/users/hooks/useUserDetail.ts` (single user with roles, permissions, sessions)
- [x] T007 [P] Create `useUserMutations` hook in `src/Web/ClientApp/src/features/security/users/hooks/useUserMutations.ts` (create, update, deactivate, reactivate mutations with optimistic concurrency handling)
- [x] T008 [P] Create `useUserRoles` hook in `src/Web/ClientApp/src/features/security/users/hooks/useUserRoles.ts` (assign/remove role mutations)
- [x] T009 [P] Create `useUserPermissions` hook in `src/Web/ClientApp/src/features/security/users/hooks/useUserPermissions.ts` (assign/remove permission mutations)
- [x] T010 [P] Create `useUserSessions` hook in `src/Web/ClientApp/src/features/security/users/hooks/useUserSessions.ts` (list/revoke sessions)
- [x] T011 Register user management navigation entry with permission identifier `UsersView` in `src/Web/ClientApp/src/app/` (navigation config matching backend permission naming per Principle X)
- [x] T012 Wire up routing for user management pages in `src/Web/ClientApp/src/AppRoutes.jsx` (routes: `/security/users`, `/security/users/create`, `/security/users/:id`)

**Checkpoint**: Foundation ready — user story implementation can now begin

---

## Phase 3: User Story 1 — View and Search Users (Priority: P1) 🎯 MVP

**Goal**: Administrators can view a paginated, searchable, filterable list of all users

**Independent Test**: Navigate to user list page; verify table renders with columns; apply search, status filter, department filter; verify results update

### Implementation for User Story 1

- [x] T013 [P] [US1] Create `UserListPage` in `src/Web/ClientApp/src/features/security/users/pages/UserListPage.tsx` — page shell with heading, search input, filter dropdowns, data table
- [x] T014 [P] [US1] Create `UserTable` component in `src/Web/ClientApp/src/features/security/users/components/UserTable.tsx` — TanStack React Table with columns: Login, Name, Status (badge), Department, Roles (tag list), Last Login, Actions (view link)
- [x] T015 [P] [US1] Create `UserFilters` component in `src/Web/ClientApp/src/features/security/users/components/UserFilters.tsx` — search input (debounced), status dropdown (Active/Inactive/All), department dropdown (fetched from Organization module)
- [x] T016 [US1] Integrate `useUsers` hook with `UserListPage`, wire search/filter state to query params, handle loading/empty/error states
- [x] T017 [US1] Add pagination controls to `UserTable` (page size selector, next/prev, page indicator)

**Checkpoint**: User list page fully functional — administrators can browse, search, and filter users

---

## Phase 4: User Story 2 — Create New User (Priority: P1)

**Goal**: Administrators can create a new user account with all required fields including initial password

**Independent Test**: Click "Create User"; fill form; submit; verify redirect to detail page; verify user appears in list

### Implementation for User Story 2

- [x] T018 [P] [US2] Create `CreateUserPage` in `src/Web/ClientApp/src/features/security/users/pages/CreateUserPage.tsx` — page shell with create-user form
- [x] T019 [P] [US2] Create `UserForm` component in `src/Web/ClientApp/src/features/security/users/components/UserForm.tsx` — react-hook-form + Zod validation for fields: login, name, department (dropdown), account type (dropdown), initial password, confirm password
- [x] T020 [US2] Wire `UserForm` to `useUserMutations.createUser`, handle success (redirect to detail page) and error (display validation errors, login uniqueness conflict)
- [x] T021 [US2] Add password strength indicator and minimum complexity validation to `UserForm` (Zod schema)
- [x] T022 [US2] Add "Create User" navigation entry with permission `UsersCreate` in navigation config

**Checkpoint**: Administrator can create new user accounts end-to-end

---

## Phase 5: User Story 3 — Edit User Details (Priority: P2)

**Goal**: Administrators can edit user profile information with optimistic concurrency protection

**Independent Test**: Open user detail; edit name; save; verify change persists; open same user in two tabs; edit in both; verify concurrency conflict surfaced

### Implementation for User Story 3

- [x] T023 [P] [US3] Create `UserDetailPage` in `src/Web/ClientApp/src/features/security/users/pages/UserDetailPage.tsx` — page shell with user header (name, login, status badge, MFA indicator), tab navigation, content area
- [x] T024 [US3] Add "Profile" tab content to `UserDetailPage` — displays and allows editing of: name, department, account type; includes save button with optimistic concurrency token round-trip
- [x] T025 [US3] Handle 409 Conflict response in `UserDetailPage` — display concurrency conflict message, prompt user to refresh, reload fresh data
- [x] T026 [US3] Add success/error toast notifications for profile update operations

**Checkpoint**: User profile editing works with concurrency protection

---

## Phase 6: User Story 4 — Deactivate and Reactivate Users (Priority: P2)

**Goal**: Administrators can deactivate and reactivate user accounts with confirmation dialogs

**Independent Test**: Deactivate an active user; verify status changes to Inactive; reactivate; verify status returns to Active; verify self-deactivation blocked

### Implementation for User Story 4

- [x] T027 [P] [US4] Create `DeactivateUserDialog` component in `src/Web/ClientApp/src/features/security/users/components/DeactivateUserDialog.tsx` — confirmation dialog with warning text, confirm/cancel buttons
- [x] T028 [P] [US4] Create `ReactivateUserDialog` component in `src/Web/ClientApp/src/features/security/users/components/ReactivateUserDialog.tsx` — confirmation dialog
- [x] T029 [US4] Add deactivate/reactivate action buttons to `UserDetailPage` header — show deactivate for active users, reactivate for inactive users; wire to `useUserMutations.deactivate`/`reactivate`
- [x] T030 [US4] Handle server-side self-deactivation block (FR-017) — display error message when server returns 400 for self-deactivation attempt
- [x] T031 [US4] After successful deactivation, auto-revoke displayed sessions from the sessions list in `UserDetailPage`

**Checkpoint**: Deactivate/reactivate lifecycle fully functional

---

## Phase 7: User Story 5 — Assign and Remove User Roles (Priority: P2)

**Goal**: Administrators can assign and remove roles on the user detail page with SoD conflict blocking

**Independent Test**: Open user detail > Roles tab; assign a role; verify it appears; remove it; verify SoD conflict error displayed when applicable

### Implementation for User Story 5

- [x] T032 [P] [US5] Create `RolesTab` component in `src/Web/ClientApp/src/features/security/users/components/RolesTab.tsx` — displays assigned roles list with remove buttons, role assignment dropdown with assign button
- [x] T033 [US5] Wire `RolesTab` to `useUserRoles` hook — list assigned roles, filter available roles (exclude already assigned), assign/remove mutations
- [x] T034 [US5] Handle SoD conflict error (FR-013/FR-021) — display error message when server returns conflict for role assignment
- [x] T035 [US5] Handle duplicate assignment prevention (FR-011) — show "already assigned" message when attempting duplicate

**Checkpoint**: Role management fully functional with SoD enforcement

---

## Phase 8: User Story 6 — Manage User Sessions (Priority: P3)

**Goal**: Administrators can view and revoke active user sessions

**Independent Test**: Open user detail > Sessions tab; verify sessions listed; revoke one; revoke all; verify list updates

### Implementation for User Story 6

- [x] T036 [P] [US6] Create `SessionsTab` component in `src/Web/ClientApp/src/features/security/users/components/SessionsTab.tsx` — displays active sessions table with columns: Device, IP, Login Time, Actions (revoke button); includes "Revoke All" button
- [x] T037 [US6] Wire `SessionsTab` to `useUserSessions` hook — list sessions, revoke one, revoke all mutations with confirmation dialogs
- [x] T038 [US6] Add session count badge to `UserDetailPage` header showing active session count from `UserDetailDto.activeSessionCount`

**Checkpoint**: Session management fully functional

---

## Phase 9: User Story 7 — View User Audit Trail (Priority: P3)

**Goal**: Administrators can view a chronological history of changes to a user account

**Independent Test**: Open user detail > Audit tab; verify entries listed with action, actor, timestamp, field diffs

### Implementation for User Story 7

- [x] T039 [P] [US7] Create `AuditTab` component in `src/Web/ClientApp/src/features/security/users/components/AuditTab.tsx` — chronological list of audit entries with expandable detail showing before/after field values
- [x] T040 [US7] Fetch audit data from backend entity-change history endpoint; map to `AuditEntry` type; handle empty state

**Checkpoint**: Audit trail visible for all user changes

---

## Phase 10: Polish & Cross-Cutting Concerns

**Purpose**: RTL, dark mode, permission gating, error handling, final validation

- [x] T041 Verify all user management screens render correctly in RTL layout (Arabic) — check logical properties, text alignment, icon placement, navigation direction
- [x] T042 [P] Verify all screens render correctly in dark mode — check contrast, design token usage, badge visibility
- [x] T043 Add permission gating to all pages/routes — redirect or show "access denied" when user lacks required permission (UsersView for list, UsersCreate for create, UsersUpdate for edit, etc.)
- [x] T044 [P] Add error boundary and fallback UI for user management feature area
- [ ] T045 Run quickstart.md validation scenarios V1 through V10 end-to-end
- [ ] T046 Run frontend tests: `cd src/Web/ClientApp && npm run test -- --run src/features/security/users`

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **US1 (Phase 3)**: Depends on Foundational — No dependencies on other stories
- **US2 (Phase 4)**: Depends on Foundational — No dependencies on other stories (shares UserForm with US3)
- **US3 (Phase 5)**: Depends on Foundational — Creates UserDetailPage (US4-US7 add tabs to it)
- **US4 (Phase 6)**: Depends on US3 (needs UserDetailPage to exist)
- **US5 (Phase 7)**: Depends on US3 (needs UserDetailPage tabs)
- **US6 (Phase 8)**: Depends on US3 (needs UserDetailPage tabs)
- **US7 (Phase 9)**: Depends on US3 (needs UserDetailPage tabs)
- **Polish (Phase 10)**: Depends on all desired user stories being complete

### User Story Dependencies

- **US1 (View/Search)**: Independent after Foundational
- **US2 (Create)**: Independent after Foundational; shares UserForm with US3
- **US3 (Edit)**: Creates UserDetailPage — US4/US5/US6/US7 all add tabs to it
- **US4 (Deactivate/Reactivate)**: Depends on US3 (UserDetailPage)
- **US5 (Roles)**: Depends on US3 (UserDetailPage)
- **US6 (Sessions)**: Depends on US3 (UserDetailPage)
- **US7 (Audit)**: Depends on US3 (UserDetailPage)

### Parallel Opportunities

- T003, T004 (types + client) can run in parallel
- T007, T008, T009, T010 (mutation hooks) can run in parallel
- T013, T014, T015 (US1 components) can run in parallel
- T018, T019 (US2 page + form) can run in parallel
- T023 (US3 detail page) must complete before T027-T040 (tab components)
- T027, T028 (deactivate/reactivate dialogs) can run in parallel
- T032, T036, T039 (Roles/Sessions/Audit tabs) can run in parallel after US3

---

## Implementation Strategy

### MVP First (US1 + US2)

1. Complete Phase 1: Setup (API client, types)
2. Complete Phase 2: Foundational (hooks, routing)
3. Complete Phase 3: US1 (user list page) — **STOP AND VALIDATE**
4. Complete Phase 4: US2 (create user form) — **STOP AND VALIDATE**
5. **MVP ready**: administrators can list, search, filter, and create users

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. US1 (list/search) → Test → Deploy (MVP)
3. US2 (create) → Test → Deploy
4. US3 (edit/detail page) → Test → Deploy
5. US4 + US5 + US6 + US7 (tabs on detail page) → Test → Deploy
6. Polish (RTL, dark mode, permission gating) → Final validation

### Parallel Team Strategy

With multiple developers:
1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 (list page)
   - Developer B: US2 (create form) + UserForm component
   - Developer C: US3 (detail page)
3. After US3 completes:
   - Developer A: US4 (deactivate/reactivate)
   - Developer B: US5 (roles tab)
   - Developer C: US6 + US7 (sessions + audit tabs)

---

## Notes

- Backend API is fully implemented — no backend tasks needed
- All business rules (self-deactivation, SoD, login uniqueness) enforced server-side
- Optimistic concurrency round-trip required on all user updates (FR-019/FR-020)
- Arabic-first RTL using logical CSS properties (start/end, not left/right)
- Design tokens from central source — no hard-coded values
- Frontend permission checks are UX-only; server is sole authority (Principle VII)
