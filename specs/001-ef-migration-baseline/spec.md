# Feature Specification: EF Migration Baseline

**Feature Branch**: `001-ef-migration-baseline`

**Created**: 2026-09-02

**Status**: Draft

**Input**: User description: "AI-01: EF Migration Baseline 🔴 BLOCKING F:\Projects\ERP-Government\plan\feature-discovery.md"

## Clarifications

### Session 2026-09-02

- Q: How should database connection credentials be managed during baseline generation? → A: Use existing project configuration (e.g., appsettings.json).
- Q: Which database providers must be supported for the migration baseline? → A: SQL Server only.
- Q: Should the baseline migration be generated from the actual database schema or from the EF model definition? → A: From the EF model definition (code-first) with mandatory reconciliation.
- Q: When the EF model and existing database schema differ, how should the baseline migration handle these differences? → A: Generate migration to apply model changes to database (up/down).
- Q: Which EF Core version(s) must the baseline migration feature support? → A: All supported EF Core versions (6.x, 7.x, 8.x).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Establish Baseline Migration (Priority: P1)

As a developer, I want to create an initial baseline migration from the EF model definition, reconciled with the existing database schema, so that the migration system can track future changes and generate correct migrations.

**Why this priority**: This is foundational for database schema management. Without a baseline, migrations cannot function properly, blocking all database-related development.

**Independent Test**: Can be fully tested by running migration commands to generate a baseline migration and verifying that the migration contains the current schema.

**Acceptance Scenarios**:

1. **Given** a project with an existing database schema and migration system configured, **When** the developer runs the command to generate a baseline migration, **Then** a migration file is created that represents the current schema.
2. **Given** the baseline migration exists, **When** the developer runs commands to add a new migration, **Then** the system correctly compares the new schema against the baseline and generates only the changes.

---

### User Story 2 - Validate Baseline Against Existing Database (Priority: P2)

As a developer, I want to validate that the baseline migration accurately reflects the current database schema, so that I can trust the migration history.

**Why this priority**: Ensures the baseline is correct, preventing future migration errors.

**Independent Test**: Can be tested by applying the baseline migration to a test database and comparing it with the original schema.

**Acceptance Scenarios**:

1. **Given** a baseline migration exists, **When** the developer runs a validation command, **Then** the system confirms that the migration matches the database schema.
2. **Given** a baseline migration that does not match the database schema, **When** validation is run, **Then** the system reports discrepancies.

---

### User Story 3 - Document Baseline Process (Priority: P3)

As a developer, I want clear documentation on how to create and manage the migration baseline, so that other team members can follow the process.

**Why this priority**: Facilitates team adoption and reduces onboarding time.

**Independent Test**: Can be tested by having a new developer follow the documentation to set up a baseline.

**Acceptance Scenarios**:

1. **Given** the documentation is available, **When** a developer follows the steps, **Then** they can successfully create a baseline migration.
2. **Given** the documentation, **When** a developer encounters an issue, **Then** troubleshooting steps are provided.

---

### Edge Cases

- What happens when the database schema is empty? The baseline migration should be empty or represent no changes.
- How does system handle when the migration system is not properly configured? Provide clear error messages.
- What if the database server is unreachable during baseline generation? The process should fail gracefully with connection error details.
- What happens when the EF model and database schema differ? The baseline migration should include up/down operations to reconcile differences.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST allow developers to generate an initial baseline migration from the EF model definition, reconciled with the existing database schema.
- **FR-002**: System MUST validate that the baseline migration accurately reflects the database schema.
- **FR-003**: System MUST provide commands to apply the baseline migration to a database.
- **FR-004**: System MUST support standard migration operations (add, remove, generate script).
- **FR-005**: System MUST handle cases where no database exists or schema is empty.
- **FR-006**: System MUST log migration operations and errors appropriately.
- **FR-007**: System MUST integrate with existing project structure and development workflow.
- **FR-008**: System MUST perform mandatory reconciliation between the EF model and existing database schema when generating the baseline migration.
- **FR-009**: When the EF model and database schema differ, the baseline migration MUST include up operations to apply model changes to the database and down operations to revert to the original schema.
- **FR-010**: System MUST support all currently supported EF Core versions (6.x, 7.x, 8.x) and detect the project's version to ensure compatibility.

### Key Entities

- **Migration**: Represents a versioned change to the database schema, containing up and down operations.
- **Database Schema**: The current structure of tables, columns, relationships, etc.
- **Migration Context**: The configuration that defines the model and mapping.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Developers can create a baseline migration in under 5 minutes.
- **SC-002**: The baseline migration accurately represents 100% of the database schema.
- **SC-003**: All subsequent migrations generated by the system are correct relative to the baseline.
- **SC-004**: Team members can follow documentation to set up a baseline without assistance.

## Assumptions

- The project already has an existing database schema that needs to be baselined.
- The migration system is configured in the project with a valid context.
- Developers have access to the database server for validation.
- The baseline is a one-time operation; future schema changes will be handled by standard migrations.
- This feature does not include data migration, only schema structure.
- Database connection credentials are stored in existing project configuration (e.g., appsettings.json) and will be used by the migration system.
- The migration baseline feature will support only SQL Server as the database provider.
- The feature will support all currently supported EF Core versions (6.x, 7.x, 8.x) and detect the project's version.