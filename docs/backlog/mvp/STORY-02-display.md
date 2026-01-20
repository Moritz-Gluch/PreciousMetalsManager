# STORY-02: Display Holdings

## User Story
As a user, I want to sort and filter my precious metal holdings in the table,
so that I can quickly find and analyze specific items in my portfolio.

## Acceptance Criteria / UI Flow

### Sorting
- User can click on any column header to sort by that column (ascending/descending).
- Sorting works for all columns with numeric, date, or string values.

### Filtering
- User can filter holdings by:
  - Metal Type (dropdown)
  - Form / Variant (dropdown or search box)
- Filtered results update the DataGrid in real time.

### UI Enhancements
- If no holdings match the filter, display a placeholder message: "No holdings match your criteria."
- Alternating row colors or light styling improvements

### Dependencies
- Requires CRUD operations from STORY-01 (in-memory list)
