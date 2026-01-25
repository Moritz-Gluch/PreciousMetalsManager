# TASK-04: Adjust Available Filter Values for Metal Type

## User Story
As a user, I want the metal type filter in the DataGrid to show only those types that actually exist in my holdings, so that the filter options are always relevant and easy to use.

## Acceptance Criteria

- The metal type filter displays only metal types that are present in the current dataset.
- The filter includes an 'All' option as the default case.
- Hardcoded filter values are removed; options are generated dynamically based on the data.
- The filter updates automatically when holdings are added, edited, or deleted.
- The UI remains intuitive and easy to use.

## Dependencies
- Existing DataGrid and filter implementation.
