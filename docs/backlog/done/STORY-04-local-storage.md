# STORY-04: Local Storage

## User Story
As a user, I want my precious metal holdings to be saved locally in a database,
so that I can access my portfolio even after closing and reopening the application.

## Acceptance Criteria / UI Flow

### Database Initialization
- On first app launch, the application initializes a local SQLite database if it does not exist.
- Necessary tables for storing metal holdings are created automatically.

### Persisting Data
- Add/Edit/Delete operations from STORY-01 update the SQLite database automatically.
- Changes in the database are reflected in the DataGrid immediately.

### Loading Data
- On application start, all holdings stored in SQLite are loaded into the in-memory list and displayed in the DataGrid.
- Current Value and Total Value are calculated after loading.

### Error Handling
- Database errors (e.g., failed write/read) are logged and displayed as an alert.
- Invalid or corrupted data in the database is handled gracefully.

### Dependencies
- Requires CRUD operations from STORY-01
- Requires Display functionality from STORY-02
- Requires Calculation functionality from STORY-03