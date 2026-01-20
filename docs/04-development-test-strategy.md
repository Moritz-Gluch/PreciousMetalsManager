# Development & Test Strategy: 

This document outlines the development and testing approach to ensure reliable implementation of features.


## 1. Development Approach
- **Feature-by-feature**: Implement stories incrementally (STORY-01 → STORY-04)
- **MVVM architecture** ensures separation of concerns
- **Services** encapsulate business logic (Database, Calculation, API, Export)
- **In-memory first**: MVP CRUD and calculations are done without persistence for rapid iteration
- **Git workflow**:
  - Branch per story / feature
  - Meaningful commits
  - Self-review before merging


## 2. Testing Approach

### Unit Tests
- Test critical logic in isolation:
  - CalculationService (Current Value, Total Value)
  - DatabaseService CRUD operations
  - ExportService (CSV/Excel)
- Run tests after each feature implementation
- Coverage targets: key business logic

### Manual Tests
- Verify UI flows after each story:
  - CRUD dialogs work
  - DataGrid displays data correctly
  - Sorting and filtering function properly
  - Placeholder messages and read-only columns as expected

### Integration Tests (Optional / Future)
- Verify that ViewModel correctly interacts with Services
- Check that changes propagate from UI → ViewModel → Service → Database
- Test full workflow from adding a holding to persistence and display


## 3. Continuous Feedback
- Run unit tests frequently during development
- Validate Acceptance Criteria after each story
- Log and fix errors immediately
- Keep DoD and QA documents up-to-date


## 4. Tools & Environment
- Visual Studio / VS Code
- Git & GitHub
- SQLite for local storage
- LiveCharts2 / OxyPlot for chart testing
