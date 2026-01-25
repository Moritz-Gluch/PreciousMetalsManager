# FEATURE-05: Add Multi-User Login

## User Story
As a user, I want to create and log in with my own account, so that multiple people can securely manage their own portfolios within the application.

## Acceptance Criteria / UI Flow

### Account Management
- On application start, a login screen is displayed before accessing the main window.               
- Users can register a new account with a unique username and password.
- Users can log in with their credentials to access their personal portfolio.

### User Data Isolation
- Each user account has its own separate portfolio data.
- The application only loads and displays data for the currently logged-in user.

### Security
- The application does not load or display any portfolio data until the user is authenticated.
- Passwords are stored securely.

### Error Handling
- If authentication or registration fails, an error message is shown to the user.

### Dependencies
- Requires CRUD and display features from previous stories.
