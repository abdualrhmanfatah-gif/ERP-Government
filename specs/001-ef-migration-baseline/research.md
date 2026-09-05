# Research: EF Migration Baseline

**Date**: 2026-09-02
**Feature**: EF Migration Baseline

## Research Tasks

1. **EF Core Baseline Migration Best Practices**
   - How to generate a baseline migration from an existing database using EF Core tools.
   - Command‑line options for `dotnet ef migrations add` with `--model` and `--database` flags.
   - Use of `IDesignTimeDbContextFactory` for design‑time context.

2. **Multi‑Version EF Core Support**
   - Compatibility considerations when targeting EF Core 6.x, 7.x, and 8.x.
   - Whether to use the lowest common denominator APIs or version‑specific adapters.
   - Package versioning strategy for the migration tooling.

3. **Reconciliation Patterns**
   - Algorithms for comparing EF model snapshot with database schema.
   - Handling of unsupported column types, default values, and stored procedures.
   - Decision: apply model changes to database (up/down) vs. reverse‑engineer database to model.

4. **SQL Server Integration with Aspire**
   - Aspire’s role in managing SQL Server connections for design‑time tools.
   - Connection string retrieval from Aspire service discovery.

5. **Testing Strategies**
   - Unit testing migration up/down operations.
   - Integration testing against a real SQL Server instance (using Aspire hosting).
   - Validation of baseline accuracy by comparing model snapshot with database schema.

## Findings

### 1. EF Core Baseline Migration Best Practices

**Decision**: Use `dotnet ef migrations add` with `--context` and `--output-dir` flags, leveraging `IDesignTimeDbContextFactory` for design‑time context resolution.

**Rationale**: The standard EF Core tooling already supports generating migrations from the model. For an existing database, we need to create a baseline that represents the current schema. The recommended approach is to generate a migration from the model and then reconcile it with the database.

**Alternatives considered**:
- Reverse‑engineering the database to generate the model first (database‑first). Rejected because the project already has a model and we want code‑first baseline.
- Manual creation of a migration script. Rejected because error‑prone and not maintainable.

### 2. Multi‑Version EF Core Support

**Decision**: Target EF Core 8.x as the primary version, but ensure compatibility with 6.x and 7.x by using conditional compilation and version‑agnostic APIs.

**Rationale**: EF Core 8 introduces new features, but the core migration APIs are stable across versions. By using `#if` preprocessor directives or runtime version detection, we can support older versions without sacrificing new capabilities.

**Alternatives considered**:
- Create separate migration branches for each version. Rejected because increases maintenance overhead.
- Use only the oldest supported version’s APIs. Rejected because limits access to improvements.

### 3. Reconciliation Patterns

**Decision**: Implement a comparison algorithm that computes the diff between the EF model snapshot and the database schema, then generate a migration that applies the model changes to the database (up/down operations).

**Rationale**: Aligns with the code‑first approach and ensures the database ends up matching the model. The up operations apply the changes; the down operations revert them.

**Alternatives considered**:
- Reverse‑engineer the database to update the model. Rejected because it would discard intentional model changes.
- Interactive prompt to choose which side to align. Rejected because automation is required for CI/CD pipelines.

### 4. SQL Server Integration with Aspire

**Decision**: Retrieve the SQL Server connection string from Aspire’s service discovery using `IConfiguration` or `IConnectionStrings` at design time.

**Rationale**: Aspire manages service connections, and the migration tooling should use the same connection configuration as the runtime.

**Alternatives considered**:
- Hard‑code connection string in `appsettings.json`. Rejected because Aspire provides dynamic service discovery.
- Use environment variables. Rejected because Aspire already handles this.

### 5. Testing Strategies

**Decision**: Combine unit tests for migration operations, integration tests against an Aspire‑hosted SQL Server, and validation tests that compare the model snapshot with the database schema.

**Rationale**: Multi‑layered testing ensures the baseline migration is correct and remains correct across EF Core versions.

**Alternatives considered**:
- Manual verification only. Rejected because unreliable and not repeatable.
- Snapshot testing only. Rejected because doesn’t catch runtime incompatibilities.