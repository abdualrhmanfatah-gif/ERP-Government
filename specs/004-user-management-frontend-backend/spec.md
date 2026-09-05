# Feature Specification: User Management Frontend + Backend

**Feature Branch**: `004-user-management-frontend-backend`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "User Management Frontend + Backend"

## Clarifications

### Session 2026-09-02

- Q: When an administrator creates a new user, how is the initial password delivered to the new user? → A: Administrator sets initial password directly in the create form.
- Q: When an administrator assigns a role that would create a separation-of-duties conflict, what should happen? → A: Block the assignment entirely with an error message.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View and Search Users (Priority: P1)

As a security administrator, I want to view a list of all users with search and filter capabilities, so that I can quickly locate accounts and assess the current user base.

**Why this priority**: Visibility into the user population is the foundation for all other management actions. Without a user list, administrators cannot perform any management task.

**Independent Test**: Can be fully tested by navigating to the user list page, applying filters (status, department, role), and verifying that results update correctly. Delivers immediate value by giving administrators visibility.

**Acceptance Scenarios**:

1. **Given** an authenticated administrator with user-view permission, **When** they navigate to the user management page, **Then** a list of all users is displayed showing name, login, status, department, roles, and last login date.
2. **Given** the user list is displayed, **When** the administrator enters a search term, **Then** the list filters to show only users matching the term across login, name, or department fields.
3. **Given** the user list is displayed, **When** the administrator selects a status filter (active/inactive/all), **Then** the list updates to show only users matching that status.
4. **Given** the user list is displayed, **When** the administrator selects a department filter, **Then** the list updates to show only users in that department.

---

### User Story 2 - Create New User (Priority: P1)

As a security administrator, I want to create a new user account with a login, initial password, department assignment, and account type, so that new employees can be onboarded into the system.

**Why this priority**: Account creation is a core administrative function. Without it, new users cannot access the system.

**Independent Test**: Can be tested by filling out the create-user form, submitting it, and verifying the new user appears in the list with correct attributes. Delivers value by enabling user onboarding.

**Acceptance Scenarios**:

1. **Given** an administrator with user-create permission, **When** they open the create-user form and fill in all required fields (login, name, department, account type, initial password), **Then** a new user account is created and the administrator is redirected to the user detail page.
2. **Given** a create-user form with a login that already exists, **When** the administrator submits the form, **Then** a validation error is displayed indicating the login is taken.
3. **Given** a create-user form with missing required fields, **When** the administrator submits the form, **Then** inline validation errors appear on the empty required fields.

---

### User Story 3 - Edit User Details (Priority: P2)

As a security administrator, I want to edit an existing user's profile information (name, department, account type, active status), so that user records stay accurate as organizational changes occur.

**Why this priority**: Keeping user data current is important for accurate access control and reporting, but slightly less critical than creating users.

**Independent Test**: Can be tested by navigating to a user's detail page, modifying fields, saving, and verifying the changes persist. Delivers value by maintaining data accuracy.

**Acceptance Scenarios**:

1. **Given** an administrator viewing a user's detail page, **When** they modify editable fields and click save, **Then** the changes are persisted and a success confirmation is displayed.
2. **Given** an administrator attempting to edit a user while another administrator has the record open, **When** the second administrator saves, **Then** a concurrency conflict is surfaced and the administrator is prompted to refresh.

---

### User Story 4 - Deactivate and Reactivate Users (Priority: P2)

As a security administrator, I want to deactivate a user account (preventing login) and reactivate it later, so that departing employees lose access immediately and returning employees can be restored without data loss.

**Why this priority**: Immediate access revocation is a security requirement. Reactivation avoids recreating accounts.

**Independent Test**: Can be tested by deactivating a user, verifying they cannot log in, then reactivating and verifying login works again. Delivers value by enforcing security policy.

**Acceptance Scenarios**:

1. **Given** an administrator viewing an active user, **When** they click deactivate and confirm, **Then** the user's status changes to inactive and the user can no longer authenticate.
2. **Given** an administrator viewing an inactive user, **When** they click reactivate, **Then** the user's status changes to active and the user can authenticate again.
3. **Given** an administrator deactivates a user who has active sessions, **When** the deactivation completes, **Then** all active sessions for that user are revoked.

---

### User Story 5 - Assign and Remove User Roles (Priority: P2)

As a security administrator, I want to assign roles to a user and remove roles from a user, so that the user's permissions reflect their current job responsibilities.

**Why this priority**: Role assignment is the primary mechanism for granting permissions. Essential for access control but builds on the user detail page.

**Independent Test**: Can be tested by assigning a role to a user, verifying the user has the role's permissions, then removing the role and verifying permissions are revoked. Delivers value by enabling RBAC.

**Acceptance Scenarios**:

1. **Given** an administrator viewing a user's roles tab, **When** they select a role from the available roles dropdown and click assign, **Then** the role appears in the user's assigned roles list.
2. **Given** a user with an assigned role, **When** the administrator clicks remove on that role, **Then** the role is removed from the user's assigned roles list.
3. **Given** an administrator attempts to assign a role the user already has, **When** they click assign, **Then** a message indicates the role is already assigned.

---

### User Story 6 - Manage User Sessions (Priority: P3)

As a security administrator, I want to view active sessions for a user and revoke individual or all sessions, so that I can respond to security incidents by terminating compromised sessions.

**Why this priority**: Session management is a security measure for incident response. Important but used less frequently than core CRUD operations.

**Independent Test**: Can be tested by viewing a user's sessions, revoking a single session, then revoking all sessions. Delivers value by enabling security incident response.

**Acceptance Scenarios**:

1. **Given** an administrator viewing a user's sessions tab, **When** the page loads, **Then** all active sessions for that user are displayed with device, IP, and login time.
2. **Given** an administrator viewing a user's sessions, **When** they click revoke on a specific session, **Then** that session is terminated and removed from the list.
3. **Given** an administrator viewing a user's sessions, **When** they click revoke all sessions, **Then** all sessions for that user are terminated and the list becomes empty.

---

### User Story 7 - View User Audit Trail (Priority: P3)

As a security administrator, I want to view a history of changes made to a user's account (created, updated, deactivated, roles changed), so that I can investigate security concerns and verify compliance.

**Why this priority**: Audit visibility supports compliance and incident investigation. Valuable but not required for basic user management operations.

**Independent Test**: Can be tested by making several changes to a user and verifying all changes appear in the audit trail with timestamps and actor information. Delivers value by providing accountability.

**Acceptance Scenarios**:

1. **Given** an administrator viewing a user's detail page, **When** they navigate to the audit tab, **Then** a chronological list of all changes to that user is displayed.
2. **Given** the audit trail is displayed, **When** the administrator views a change entry, **Then** the entry shows the action taken, the actor who performed it, the timestamp, and the before/after values for changed fields.

---

### Edge Cases

- What happens when an administrator tries to deactivate their own account?
- What happens when an administrator tries to remove their own last role that grants administrative access?
- What happens when a user is deactivated while they are in the middle of creating a document?
- What happens when concurrent administrators edit the same user simultaneously?
- What happens when the login field is changed to a value that conflicts with a case variation of an existing login?
- What happens when an administrator tries to assign a role that would create a separation-of-duties conflict? → System MUST block the assignment with an error message (see FR-021).
- What happens when a user with active pending approvals is deactivated?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST display a paginated list of all users with sortable columns for login, name, status, department, and last login.
- **FR-002**: System MUST support searching users by login, name, or department with real-time filtering.
- **FR-003**: System MUST support filtering the user list by account status (active, inactive, all) and by department.
- **FR-004**: System MUST allow administrators to create new user accounts with login, name, department, account type, and an initial password set by the administrator.
- **FR-005**: System MUST validate that the login is unique (case-insensitive) before creating a user.
- **FR-006**: System MUST allow administrators to edit user profile fields including name, department, and account type.
- **FR-007**: System MUST allow administrators to deactivate user accounts, which immediately prevents authentication.
- **FR-008**: System MUST revoke all active sessions when a user is deactivated.
- **FR-009**: System MUST allow administrators to reactivate previously deactivated user accounts.
- **FR-010**: System MUST allow administrators to view, assign, and remove roles on a per-user basis.
- **FR-011**: System MUST prevent duplicate role assignments for the same user.
- **FR-012**: System MUST allow administrators to view, assign, and remove direct permissions on a per-user basis.
- **FR-013**: System MUST block role assignments that would create a separation-of-duties conflict, displaying an error message and preventing the assignment from being saved.
- **FR-014**: System MUST allow administrators to view active sessions for a user, including device info, IP address, and login time.
- **FR-015**: System MUST allow administrators to revoke individual sessions or all sessions for a user.
- **FR-016**: System MUST display an audit trail of changes to each user account, including actor, timestamp, action, and field-level diffs.
- **FR-017**: System MUST prevent an administrator from deactivating their own account.
- **FR-018**: System MUST prevent an administrator from removing their own last role that grants user-management permissions.
- **FR-019**: System MUST display the current user's effective permissions inherited from roles and direct assignments in a consolidated view.
- **FR-020**: System MUST use optimistic concurrency control on all user updates and surface conflicts to the administrator.
- **FR-021**: System MUST support Arabic-first RTL layout on all user management screens.

### Key Entities

- **User**: Represents a system account. Key attributes: login (unique identifier), name, active status, department assignment, account type, password state, MFA state, last login timestamp, failed login attempts, lock state.
- **UserRole**: Assignment of a role to a user. Links a User to a Role with an assignment timestamp.
- **UserPermission**: Direct permission assignment to a user, separate from role-based permissions. Links a User to a specific permission.
- **UserSession**: An active authenticated session. Tracks device, IP address, login time, and revocation state.
- **AuditEntry**: Immutable record of a change to a user account. Captures actor, timestamp, action type, and per-field before/after values.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Administrators can create a new user account in under 2 minutes from opening the form.
- **SC-002**: The user list loads and displays results within 2 seconds for up to 10,000 users.
- **SC-003**: Administrators can locate a specific user via search within 30 seconds.
- **SC-004**: Deactivating a user prevents authentication within 1 second across all active sessions.
- **SC-005**: Role assignment changes take effect on the user's next request (no stale permissions).
- **SC-006**: 95% of user management operations (create, edit, deactivate) complete without errors on first attempt.
- **SC-007**: All user management screens render correctly in both RTL (Arabic) and LTR layouts, and in both light and dark modes.

## Assumptions

- The backend API for user CRUD, role assignment, permission assignment, session management, and audit logging already exists and is functional.
- The frontend uses a React-based application with an existing design system and component library.
- User authentication and authorization are handled by the existing Security module; this feature focuses on administrative management of user accounts.
- Department data is available from the Organization module and can be referenced for user assignment.
- The existing permission codes (UsersView, UsersCreate, UsersUpdate, UsersDeactivate, UsersManageRoles, UsersManageSessions, UsersResetLogin) define the authorization boundaries.
- Audit trail data is already captured by the backend for user entity changes; this feature surfaces it in the frontend.
- Optimistic concurrency tokens are already implemented on the User entity.
