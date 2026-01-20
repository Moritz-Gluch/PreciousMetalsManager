# Acceptance Criteria: 

This document contains the full set of acceptance criteria for the MVP of the Precious Metals Portfolio Manager.

## STORY-01: CRUD for Precious Metals
- DataGrid displays all holdings in-memory.
- Add/Edit/Delete dialogs work and update the table.
- Invalid input triggers inline errors.
- Errors during in-memory operations display an alert.

## STORY-02: Display Holdings
- Columns can be sorted ascending/descending.
- Filter by Metal Type or Form/Variant works in real time.
- Placeholder message shown when no holdings match filter.
- Optional: alternating row colors or light styling enhancements.

## STORY-03: Calculate Current Value
- Current Value calculated based on Weight, Purity, and Current Price (manual for MVP).
- Total Value = Current Value × Quantity.
- Values update immediately after changes in Quantity, Weight, Purity, or Current Price.
- Calculated values are read-only in the DataGrid.
- Invalid numeric input triggers inline errors.

## STORY-04: Local Storage
- SQLite database initialized on first launch.
- Add/Edit/Delete operations persist to SQLite.
- Data loaded from SQLite on app start into the in-memory list.
- Current Value and Total Value are calculated after loading.
- Database errors are logged and displayed as alerts.