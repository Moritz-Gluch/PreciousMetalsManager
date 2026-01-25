# FEATURE-02: Add Tax-Free Status Column


## User Story
As a user, I want to see at a glance whether my precious metal holdings are eligible for tax-free sale, so that I can make informed decisions about selling.

## Acceptance Criteria / UI Flow

## Tax-Free Status Calculation
- The "Tax-Free" status is automatically determined:
    - If the holding period (Purchase Date to today) is at least 1 year, the status is "Yes".
    - Otherwise, the status is "No. of days left".
- The status updates automatically on application start or if the purchase date is edited.
  
## UI
- The DataGrid displays a new column "Tax-Free" for each holding.
- The "Tax-Free" column is read-only.
- Optionally, tax-free holdings can be highlighted (e.g., with a color or icon).
  
## Filtering (Optional)
- User can filter holdings by tax-free status ("Yes"/"No" or Checkbox).
  
## Dependencies
- Requires correct handling of purchase dates and display features from previous stories.