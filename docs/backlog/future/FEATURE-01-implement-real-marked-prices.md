# FEATURE-01: Implement Real Market Prices

## User Story
As a user, I want the application to fetch current market prices for precious metals from an online API, so that my portfolio values are always up to date.

## Acceptance Criteria / UI Flow

### API Integration
- On application start and on user request, the app fetches current market prices for all supported metals from a public API.
- The fetched prices are displayed in the UI.
- The "Current Value" and "Total Value" columns in the DataGrid are recalculated using the latest prices.
  
### Manual/Automatic Update
- User can trigger a manual price update via a "Refresh Prices" button.
- Optionally, prices can be refreshed automatically at a set interval.

### Error Handling
- If the API call fails, an error message is shown to the user.
- The app falls back to the last known prices or allows manual entry if the API is unavailable.

### Dependencies
- Requires CRUD, display, and calculation features from previous stories.