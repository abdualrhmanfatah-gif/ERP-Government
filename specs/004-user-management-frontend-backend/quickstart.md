# Quickstart Validation Guide: User Management

**Date**: 2026-09-02
**Feature**: 004-user-management-frontend-backend

## Prerequisites

- .NET 10.0 SDK installed
- SQL Server running (local or container via Aspire)
- Node.js 20+ installed
- Backend and frontend build successfully

## Validation Scenarios

### V1: User List Page Loads and Displays Users

**Steps**:
1. Start the application: `dotnet run --project src/AppHost`
2. Navigate to the Security > Users page
3. Verify the user list displays with columns: Login, Name, Status, Department, Roles, Last Login

**Expected**: Table renders with existing user data; pagination controls visible if > 20 users.

### V2: Search and Filter Users

**Steps**:
1. On the user list page, type a search term in the search box
2. Verify results filter in real-time
3. Select "Inactive" from the status filter dropdown
4. Verify only inactive users appear
5. Select a department from the department filter
6. Verify only users in that department appear

**Expected**: All three filters work independently and can be combined.

### V3: Create New User

**Steps**:
1. Click "Create User" button
2. Fill in: Login, Name, Department, Account Type, Initial Password
3. Submit the form
4. Verify redirect to the new user's detail page
5. Navigate back to the user list
6. Verify the new user appears in the list

**Expected**: User created successfully; login uniqueness validated; password stored securely (not visible in list).

### V4: Edit User Details

**Steps**:
1. Click on a user in the list to open detail page
2. Click "Edit" or modify a field inline
3. Change the user's name
4. Save changes
5. Verify success confirmation
6. Refresh the page
7. Verify the change persisted

**Expected**: Changes saved; optimistic concurrency token round-tripped; audit entry created.

### V5: Deactivate and Reactivate User

**Steps**:
1. Open an active user's detail page
2. Click "Deactivate" and confirm
3. Verify user status changes to Inactive
4. Attempt to log in as that user (should fail)
5. Click "Reactivate"
6. Verify user status changes to Active
7. Verify the user can log in again

**Expected**: Deactivation revokes all sessions; reactivation restores access.

### V6: Assign and Remove Roles

**Steps**:
1. Open a user's detail page > Roles tab
2. Select a role from the dropdown
3. Click "Assign"
4. Verify the role appears in the assigned roles list
5. Click "Remove" on that role
6. Verify the role is removed

**Expected**: Duplicate assignment prevented; SoD conflicts blocked with error message.

### V7: Manage Sessions

**Steps**:
1. Open a user's detail page > Sessions tab
2. Verify active sessions are listed with device, IP, login time
3. Click "Revoke" on a specific session
4. Verify that session is removed from the list
5. Click "Revoke All"
6. Verify all sessions are removed

**Expected**: Session list accurately reflects active sessions; revocation is immediate.

### V8: View Audit Trail

**Steps**:
1. Make several changes to a user (edit name, assign role, deactivate)
2. Open the user's detail page > Audit tab
3. Verify all changes appear in chronological order
4. Verify each entry shows: action, actor, timestamp, field changes

**Expected**: Complete audit trail with per-field before/after values.

### V9: RTL and Dark Mode Rendering

**Steps**:
1. Switch the UI language to Arabic
2. Navigate through all user management screens
3. Verify RTL layout renders correctly (text alignment, icon placement, navigation)
4. Toggle dark mode
5. Verify all screens render correctly in dark mode

**Expected**: No layout breaking; all design tokens applied correctly.

### V10: Concurrency Conflict Handling

**Steps**:
1. Open the same user in two browser tabs
2. Edit the user in tab 1 and save
3. Edit the user in tab 2 and save
4. Verify a concurrency conflict message appears in tab 2
5. Refresh tab 2 and verify the latest data is shown

**Expected**: Conflict detected and surfaced to user; no data loss.

## Backend Unit Test Validation

Run the following to verify use case logic:

```bash
dotnet test tests/Application.UnitTests --filter "FullyQualifiedName~Security"
```

## Frontend Test Validation

```bash
cd src/Web/ClientApp
npm run test -- --run src/features/security/users
```

## Browser Acceptance Test Validation

```bash
dotnet test tests/AcceptanceTests --filter "Category=UserManagement"
```
