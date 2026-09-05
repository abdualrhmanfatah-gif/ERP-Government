---
detected_at: headless
ecosystems: [typescript]
default: typescript
stacks:
  typescript:
    cwd: src/Web/ClientApp
    runner: vitest
    single: 'npx vitest run {file} -t "{name}"'
    file: npx vitest run {file}
    suite: npm test
    watch: npm run test:watch
    coverage: null # @vitest/coverage-v8 not installed
    mutation: null # No stryker or mutation tool installed
    acceptance: null # No acceptance runner configured
    property: null # fast-check not installed
    approval: vitest snapshots
    contract: null
    test_glob: "src/**/*.{test,spec}.{ts,tsx}"
    exemplar:
      unit: src/features/budgeting/__tests__/BudgetItemTree.test.tsx
      acceptance: null
    helpers:
      - "@testing-library/jest-dom/vitest"
      - "@testing-library/react"
      - "vitest (globals: true)"
verified: [single, file, suite]
suite_baseline: red
suite_seconds: 18
suite_failures:
  - name: "AvailabilityIndicator > renders a placeholder when availability is unknown"
    reason: "No QueryClient set, use QueryClientProvider to set one"
    file: src/features/budgeting/__tests__/AvailabilityIndicator.test.tsx
    fix: "Wrap test render in QueryClientProvider; or create vitest.setup.ts with a wrapper"
---

# TDD Stack Profile

## Conventions to match

- Test files live in `__tests__/` directories next to the feature they test (e.g., `features/budgeting/__tests__/`).
- Import `@testing-library/jest-dom/vitest` at the top of every test file for DOM matchers.
- Use `vitest` globals (`describe`, `it`, `expect`, `vi`) — no explicit imports needed (but existing tests import them explicitly; follow the file's convention).
- Use `@testing-library/react` for rendering and querying: `render()`, `screen`, `fireEvent`.
- Doubles use `vi.fn()` from vitest.
- Assertions use `expect` from vitest with `@testing-library/jest-dom` matchers.
- Arabic text in assertions matches RTL UI content directly.
- No shared test utilities or factory modules exist yet. Tests construct their own data inline.
- No vitest.setup.ts exists. The `AvailabilityIndicator` test needs a QueryClientProvider wrapper to pass.

## Exemplars

- **Unit test**: `src/features/budgeting/__tests__/BudgetItemTree.test.tsx` — renders component with props, asserts DOM output, tests keyboard interaction with `fireEvent.keyDown`.
- **Utility test**: `src/features/budgeting/__tests__/LifecycleActions.test.tsx` — tests pure function `actionLabel()` and component behavior (permission filtering, disable state).
- **Component test**: `src/features/budgeting/__tests__/ApprovalHistoryPanel.test.tsx` — renders with typed DTO data, asserts empty state and content display.

## Notes and constraints

- Suite takes ~19 seconds. Per-cycle full runs are viable.
- Suite is red: 1 test fails in `AvailabilityIndicator.test.tsx` (missing QueryClientProvider). This is a pre-existing issue from spec #11 when AvailabilityIndicator was changed to fetch-based. The failing test wraps the component without a QueryClient. Fix: add QueryClientProvider wrapper or create a test helper.
- No coverage tool installed (`@vitest/coverage-v8` missing). Coverage is unmeasured.
- No mutation tool installed. Test strength cannot be verified by automation.
- No property-based library installed. Invariants are sampled at boundaries, not proven.
- No acceptance/E2E runner configured. Outer-loop tests run as component tests in vitest/jsdom, not against a real browser or backend.
- No shared test utilities. Each test constructs its own fixtures inline.
- The 3 passing test files have 18 passing tests total.
