# STORY-01: CRUD for Precious Metals


## User Story
As a user, I want to add, edit, and delete my precious metal holdings,
so that I can manage my portfolio accurately.


## Acceptance Criteria / UI Flow

### View holdings
- A DataGrid displays all metals in the portfolio. (in-memory for this story)
- Columns:
  - Metal Type
  - Form / Variant 
  - Purity
  - Weight
  - Quantity
  - Purchase Price
  - Purchase Date
  - Current Value
  - Total Value
- The grid updates automatically after any CRUD operation.

### Add a holding
- User clicks an “Add” button above the table.
- A dialog opens with input fields: 
  - Metal Type      (dropdown with Elements [Gold, Silver, etc.])
  - Form / Variant  (string)
  - Purity          (number)
  - Weight          (number)
  - Quantity        (number)
  - Purchase Price  (number)
  - Purchase Date   (date picker)
- User clicks Save → data validated and added to the in-memory list, table refreshes automatically.

### Edit a holding
- User selects a row → Edit button → dialog opens prefilled
- User edits values → Save → updates in-memory list, table updates.

### Delete a holding
- User selects a row → Delete → confirmation → remove from in-memory list, table updates.

### Error handling
- Invalid input triggers inline error message
- Errors during in-memory operations are displayed as alert

