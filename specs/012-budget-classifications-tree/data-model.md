# Data Model: Budget Classifications Tree #2 of 5

**Date**: 2026-09-04

## Frontend Types (from spec #1 shared/types.ts)

### BudgetClassificationDto

```typescript
interface BudgetClassificationDto {
  id: number;
  code: string;
  name: string;
  parentId?: number;
  level: number;        // Computed server-side
  isActive: boolean;
  rowVersion: string;
}
```

### BudgetClassificationTreeDto

```typescript
interface BudgetClassificationTreeDto extends BudgetClassificationDto {
  children?: BudgetClassificationTreeDto[];
}
```

## Local State Models

### Tree Expand State

```typescript
// Set of expanded node IDs
type ExpandedNodes = Set<number>;

// Initial state: root nodes with children expanded
function getInitialExpandState(nodes: BudgetClassificationTreeDto[]): ExpandedNodes
```

### Search/Filter State

```typescript
interface ClassificationFilters {
  search: string;           // Text filter on code/name
  isActive?: boolean;       // Active state filter
}
```

### Dialog State

```typescript
interface ClassificationDialogState {
  open: boolean;
  mode: 'create' | 'edit';
  editItem: BudgetClassificationDto | null;
}
```

## Tree Validation Rules

| Rule | Description | Enforcement |
|------|-------------|-------------|
| Cycle prevention | On edit, exclude self + descendants from parent select | Client-side |
| Required fields | Code and Name must be non-empty | Client-side validation |
| Malformed data | Orphaned parentId → treat as root-level | Client-side normalization |
| Backend cycles | Backend returns 400 for cycle violations | Server-side |

## Tree Normalization

When backend returns tree data, normalize to handle malformed nodes:

```typescript
function normalizeTree(nodes: BudgetClassificationTreeDto[]): BudgetClassificationTreeDto[]
```

Logic:
1. Build a `Map<id, node>` lookup
2. For each node, verify `parentId` references a valid node in the tree
3. If `parentId` is invalid or creates a cycle, move node to root level
4. Show warning toast if any nodes were normalized
