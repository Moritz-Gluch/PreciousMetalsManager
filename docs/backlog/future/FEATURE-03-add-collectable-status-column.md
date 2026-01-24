# FEATURE-03: Add Collectable Status Column


## User Story
As a user, I want to specify whether a coin or bar is bullion, semi-numismatic or numismatic, so that I can better categorize and analyze my precious metal holdings.


## Acceptance Criteria / UI Flow

### Collectable Type Column
- The DataGrid displays a new column "Type" for each holding.
- The "Type" can be one of the following:
    - Bullion
    - Semi-numismatic
    - Numismatic
- The type is selected when adding or editing a holding (dropdown in dialog).
- The value is stored and displayed for each holding.

### Dependencies
- Requires CRUD and display features from previous stories.