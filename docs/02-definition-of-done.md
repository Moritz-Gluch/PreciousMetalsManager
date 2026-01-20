# Definition of Done (DoD): 

A story is considered done when all of the following criteria are met:

## Functional Criteria
- All acceptance criteria for the story are implemented and verified.
- All UI flows function as specified in the story.
- All calculations (Current Value, Total Value) are correct.
- Data persistence works as specified (for stories that include storage).

## Code Quality
- Code compiles without errors and warnings.
- Code follows project style conventions (C# / WPF / MVVM).
- No unused code or resources.
- Code is structured for maintainability (separation of concerns, MVVM pattern).

## Testing
- Unit tests cover critical logic (e.g., calculations, CRUD operations).
- All unit tests pass successfully.
- Any bugs found during development are resolved.

## Documentation
- Story and acceptance criteria are documented.
- Relevant code sections contain XML comments if necessary.
- UI changes are documented in the project overview or backlog notes.

## Review & Deployment
- Story has been self-reviewed.
- Changes are committed to Git with meaningful messages.
- No broken builds; application runs successfully in Visual Studio/VS Code.