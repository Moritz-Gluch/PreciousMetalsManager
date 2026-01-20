# STORY-03: Calculate Current Value

## User Story
As a user, I want the application to automatically calculate the current value and total value of my precious metal holdings,
so that I can quickly see the financial worth of my portfolio.

## Acceptance Criteria / UI Flow

### Current Value Calculation
- The application calculates the **Current Value per unit** based on:
  - Weight
  - Purity
  - Current market price per metal (for MVP, will be a manually entered value initially)
- Display Current Value in the DataGrid next to each holding.

### Total Value Calculation
- The application calculates **Total Value** as: Total Value = Current Value × Quantity
- Total Value updates automatically when:
  - A holding is added, edited, or deleted (in-memory)
  - Current Value changes (if using market price updates)

### UI Feedback
- Calculated values are **read-only** in the DataGrid.
- Values update immediately after any change to Quantity, Weight, Purity, or Current Price.
- Invalid numeric input triggers an inline error message.

### Dependencies
- Requires CRUD operations from STORY-01 (in-memory list)
- Requires Display functionality from STORY-02 (DataGrid)