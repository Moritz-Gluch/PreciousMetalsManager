# FEATURE-04: Add Multi-Delete

## User Story
As a user, I want to be able to delete multiple holdings at once, so that I can efficiently manage and clean up my portfolio.

## Acceptance Criteria / UI Flow

### Multi-Selection
- The DataGrid allows selecting multiple holdings.
- For deletion, already existing delete button is used.

### Deletion Process
- When the user clicks "Delete", a confirmation dialog appears.
- Upon confirmation, all selected holdings are removed from the list and database.
- The DataGrid updates immediately to reflect the changes.

### Error Handling
- If deletion fails for any holding, an error message is shown to the user.

### Dependencies
- Requires CRUD and display features from previous stories.
