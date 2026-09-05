# Quickstart: Budget Classifications Tree #2 of 5

**Date**: 2026-09-04

## Prerequisites

- Spec #1 (shared types/client) is implemented and available
- Backend BudgetClassifications endpoints are live
- Node.js 20+ and npm installed

## Validation Scenarios

### V1: Tree View Loads

1. Navigate to `/budgeting/budget-classifications`
2. **Expected**: Tree renders with root-level classifications visible
3. **Expected**: Each node shows Code, Name, and Level badge
4. **Expected**: Nodes with children have expand/collapse toggle

### V2: Expand/Collapse

1. Click expand toggle on a node with children
2. **Expected**: Children appear indented in RTL layout
3. Click collapse toggle
4. **Expected**: All descendants hidden

### V3: Search Auto-Expand

1. Type search text matching a deep node
2. **Expected**: Matching node and all ancestors auto-expand
3. **Expected**: Non-matching branches remain collapsed
4. Clear search text
5. **Expected**: Tree returns to previous expand/collapse state

### V4: Create Classification

1. Click "إضافة تصنيف جديد"
2. Enter Code and Name
3. Optionally select a parent from tree-select
4. Click save
5. **Expected**: Dialog closes, tree refreshes, new node appears

### V5: Edit Classification

1. Click edit on an existing classification
2. **Expected**: Dialog opens pre-filled with current values
3. **Expected**: Parent tree-select excludes self + descendants
4. Modify Name, click save
5. **Expected**: Dialog closes, tree shows updated values

### V6: Cycle Prevention

1. Edit a classification that has children
2. Open parent tree-select
3. **Expected**: Self and all descendants are excluded from options
4. **Expected**: Inactive nodes ARE available as options

### V7: Toggle Active State

1. Click IsActive switch on a classification
2. **Expected**: Confirmation dialog appears
3. Confirm
4. **Expected**: Tree refreshes, success toast shown

### V8: Keyboard Navigation

1. Focus a tree node
2. Press Right arrow
3. **Expected**: Node expands (if has children)
4. Press Left arrow
5. **Expected**: Node collapses
6. Press Down arrow
7. **Expected**: Focus moves to next visible node

### V9: Route and Sidebar

1. Load app
2. **Expected**: Sidebar "الموازنة" group shows "التصنيفات المالية" link
3. Click link
4. **Expected**: Navigates to `/budgeting/budget-classifications`
5. **Expected**: Sidebar link is highlighted as active

### V10: Dark Mode

1. Toggle dark mode
2. **Expected**: All tree elements render correctly
3. **Expected**: Indentation, badges, toggles all visible

### V11: Empty State

1. Start with no classifications (or clear all)
2. **Expected**: Empty state with create action shown

### V12: Deep Hierarchy

1. Create 5+ levels of nested classifications
2. **Expected**: Each level indented correctly in RTL
3. **Expected**: No horizontal overflow

### V13: Malformed Data Handling

1. Backend returns classification with invalid parentId
2. **Expected**: Node treated as root-level
3. **Expected**: Warning toast shown
4. **Expected**: Tree still renders correctly

### V14: RowVersion Conflict

1. Edit a classification
2. Simulate concurrent edit (change rowVersion)
3. Save
4. **Expected**: Error toast shown
5. **Expected**: Tree refetches automatically
