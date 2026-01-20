# Quality Assurance: Precious Metals Portfolio Manager

This document outlines the testing and QA measures to ensure the quality of the application.

## 1. Unit Testing
- **CalculationService**
  - Test Current Value calculation for multiple metals and edge cases
  - Test Total Value calculation
- **DatabaseService**
  - Test CRUD operations in an in-memory SQLite database
- **ExportService**
  - Test CSV/Excel export functionality
- All tests pass successfully before a story is marked as done

## 2. Integration Testing
- Verify interaction between ViewModel and Services:
  - CRUD operations correctly update DataGrid
  - Calculations reflect changes immediately
  - Data persists correctly when saving/loading from SQLite

## 3. UI Testing (Manual)
- Check that Add/Edit/Delete dialogs function as expected
- Verify DataGrid sorting and filtering (STORY-02)
- Confirm placeholder messages appear when no data matches filter
- Verify read-only columns (Current Value / Total Value) cannot be edited

## 4. Error Handling / Robustness
- Invalid inputs trigger inline errors
- Database/API errors are logged and alert displayed
- Application does not crash under normal or edge-case scenarios

## 5. Review & Approval
- Code reviewed by self 
- All acceptance criteria for story met
- Definition of Done fully satisfied

## 6. Continuous Quality Measures
- Use of Git for version control
- Meaningful commit messages
- Regular commits per story
- Optional: Static code analysis tools or linters (if applied)