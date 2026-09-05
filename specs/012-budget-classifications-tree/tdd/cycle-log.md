# Cycle Log: Budget Classifications Tree

## Baseline

- suite: `npm test` -> 19 passed, 0 failed (after fixing pre-existing AvailabilityIndicator QueryClientProvider issue)
- commit: headless
- recorded: cycle 0, before any change

### Pre-existing fix

- `AvailabilityIndicator.test.tsx`: wrapped render calls in `TestWrapper` (QueryClientProvider). Created `src/test-utils.tsx` with shared `TestWrapper`. Suite went from 18 pass / 1 fail to 19 pass / 0 fail.

## Cycle 1: U1 — orphaned parentId moves node to root level

- test: `src/features/budgeting/__tests__/normalizeTree.test.ts::moves a node with orphaned parentId to root level` (new)
- red: `npx vitest run src/features/budgeting/__tests__/normalizeTree.test.ts -t "moves a node with orphaned parentId to root level"` -> `AssertionError: expected 999 to be undefined` (1 failed)
- green: `src/features/budgeting/classifications/pages/ClassificationsListPage.ts` implemented normalizeTree with id lookup + orphan detection. Suite `npm test` -> 20 passed, 0 failed
- refactor: none needed, pure function under 20 lines
- commit: headless (uncommitted)

## Cycle 2: U2 — cycle (parentId is descendant) moves node to root

- test: `src/features/budgeting/__tests__/normalizeTree.test.ts::moves a node forming a cycle (parentId is descendant) to root level` (new)
- red: `npx vitest run src/features/budgeting/__tests__/normalizeTree.test.ts -t "moves a node forming a cycle"` -> `AssertionError: expected [ { id: 1, ... } ] to have length 2 but got 1` (1 failed)
- green: `ClassificationsListPage.ts` added cycle detection via descendantMap + collectAllDescendants flattens orphan subtrees. Also fixed: orphans not included in return, hasInvalidParent condition (must be defined to be invalid). Suite `npm test` -> 21 passed, 0 failed
- refactor: none needed
- commit: headless (uncommitted)

## Cycle 3: U4 — valid tree returned unchanged

- test: `src/features/budgeting/__tests__/normalizeTree.test.ts::returns a valid tree unchanged` (new)
- red: N/A — test passed on first run (existing implementation already handled this case)
- deliberate mutant: changed `return [...normalize(nodes), ...orphans]` to `return []`. Test failed: `expected [] to have length 1 but got 0`. Mutant caught, test is valid.
- green: restored original return. Suite `npm test` -> 22 passed, 0 failed
- refactor: none needed
- commit: headless (uncommitted)

## Cycle 4-7: U5, U6, U9 — expand/collapse state + empty state

- test: `ClassificationsListPage.test.tsx` — 4 tests (empty state, render nodes, root expanded, deeper collapsed)
- red: Component didn't exist as default export. Created hooks file + full component with TreeItem recursive renderer, expand/collapse state, getInitialExpanded.
- green: Suite `npm test` -> 26 passed, 0 failed
- refactor: extracted getInitialExpanded helper, used useMemo for tree normalization
- commit: headless (uncommitted)
