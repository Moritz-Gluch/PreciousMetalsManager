# TASK-07: Optimize Data Refresh After User Interaction

## User Story
As a user, I want the application to update the displayed data efficiently after add, edit, or delete operations, so that I do not have to wait for the entire dataset to reload every time, but still always see the most up-to-date and correct information.

## Acceptance Criteria

- After the initial load, only the affected data (added, edited, or deleted holding) is updated in the UI and in memory, not the entire dataset.
- The application ensures that the displayed data is always correct and reflects the latest state from the database.
- No unnecessary full reloads from the database occur after each operation.
- The solution is robust even with large datasets.
- Edge cases (concurrent changes, failed updates) are handled gracefully and do not lead to inconsistent UI states.

## Dependencies
- Existing CRUD and data loading logic.
