# Research: Budget Classifications Tree #2 of 5

**Date**: 2026-09-04

## R1: Tree Rendering Strategy

**Decision**: Recursive component with `useState` for expand/collapse per node.

**Rationale**: The classification tree is fetched as a pre-built tree from the backend (`GET /api/BudgetClassifications/tree` returns `BudgetClassificationTreeDto[]` with nested children). Recursive rendering is the simplest approach for this data shape. No need for virtualization at the expected scale (reference data, typically <500 nodes).

**Alternatives considered**:
- Flat list with depth map: More complex, unnecessary given backend returns tree shape.
- Virtualized tree (react-window): Only needed if >1000 nodes expected; reference data typically stays small.

## R2: Search Implementation

**Decision**: Client-side filter on the full tree, with ancestor auto-expand.

**Rationale**: The tree is fully loaded on page mount. Client-side search is instant (<50ms for 500 nodes). Ancestor auto-expand ensures matches are visible in hierarchical context. Non-matching branches collapse to reduce noise.

**Alternatives considered**:
- Server-side search: Unnecessary for reference data; adds latency and complexity.
- Flat search results: Loses hierarchy context; violates spec clarification.

## R3: Parent Tree-Select for Create/Edit

**Decision**: Use existing `Combobox` component adapted for tree-select mode. Cycle prevention: exclude self + descendants on edit; no exclusion on create.

**Rationale**: The `Combobox` component from the UI kit supports custom rendering. For tree-select, render the tree structure inside the dropdown. Cycle prevention is computed client-side by collecting all descendant IDs before rendering the select.

**Alternatives considered**:
- Native `<select>`: Doesn't support tree structure or custom rendering.
- Third-party tree-select: Adds dependency; existing Combobox is sufficient.

## R4: Keyboard Navigation

**Decision**: Arrow keys for expand/collapse. Left/Right arrows toggle expand state; Up/Down arrows move focus between visible nodes.

**Rationale**: Standard tree keyboard pattern (WAI-ARIA Treeview pattern). Works with existing focus management.

**Alternatives considered**:
- Tab-only navigation: Doesn't support expand/collapse; poor UX for deep trees.

## R5: Malformed Data Handling

**Decision**: Orphaned nodes (parentId references non-existent node) treated as root-level. Warning toast shown. Tree still renders.

**Rationale**: Resilient to backend data issues without blocking the user. Warning toast informs without interrupting workflow.

**Alternatives considered**:
- Refuse to render: Blocks user entirely; too aggressive for reference data.
- Silent exclusion: Hides data issues; user may not notice missing classifications.

## R6: RTL Indentation

**Decision**: Use CSS logical properties (`padding-inline-start`) for indentation. Each level adds consistent indentation (e.g., 1.5rem per level).

**Rationale**: Logical properties automatically handle RTL/LTR. No direction-specific hacks needed. Consistent indentation makes hierarchy visually clear.

**Alternatives considered**:
- Fixed `margin-right`: Breaks in RTL; violates design token principle.
- CSS custom property per level: Over-engineered for simple indentation.

## R7: Combobox Tree-Select Adaptation

**Decision**: Render tree nodes inside Combobox dropdown. Selected node shown as text input. Dropdown shows tree with expand/collapse.

**Rationale**: Combobox already supports custom rendering. Tree structure inside dropdown gives users hierarchy context when selecting a parent.

**Alternatives considered**:
- Separate tree-select component: Adds new component to maintain; Combobox is sufficient.
