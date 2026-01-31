# FEATURE-07: Localization (German and English)

## Goal
Enable localization for the application, supporting both German and English languages. Currently all UI texts are hardcoded in English.

## Acceptance Criteria
- All UI texts are loaded from resource files, not hardcoded in place.
- Two language resources exist: German (de) and English (en).
- Users can switch the application language at runtime (e.g., via a menu or settings).
- The selected language is persisted and restored on next launch.
- Fallback to English if a translation is missing.


## Dependencies
- Requires CRUD operations from STORY-01 (for managing holdings and UI updates)
- Requires Display functionality from STORY-02 (for UI text rendering)
- Requires Local Storage from STORY-04 (for persisting language selection)

## Notes
- Use standard .NET/WPF localization mechanisms (e.g., .resx files, resource binding).
- This feature does not include translation of documentation or other files outside the application.