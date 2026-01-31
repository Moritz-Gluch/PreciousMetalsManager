
# DEFECT-01: Misplaced 'No holdings match your criteria' message

## User Story
As a user, when I filter or search for holdings and no results are found, I want to see the message 'No holdings match your criteria' centered inside the DataGrid, so that it is clear there are no matching holdings and the message is visually associated with the grid.

## Current Behavior
The message 'No holdings match your criteria' is displayed next to the DataGrid instead of inside it.

## Steps to Reproduce
1. Apply filters so that no holdings are shown in the DataGrid.
2. Observe where the message appears.

## Acceptance Criteria
- The message 'No holdings match your criteria' is only shown when the DataGrid is empty.
- The message is centered and clearly visible inside the DataGrid area.
- The message is not displayed outside or next to the DataGrid.

