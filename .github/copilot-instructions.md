# Copilot Instructions

## Project Guidelines
- Use the repository's Verify.* extension functions for argument validation in SelectParallelAsync instead of built-in guards such as ArgumentOutOfRangeException.ThrowIfNegativeOrZero or ArgumentNullException.ThrowIfNull. This supersedes the earlier preference for built-in guards.

## Database Project Guidelines
- Keep the existing Visual Studio SSDT format for database projects.
- Do not use Python automation for database tasks.
- Place stored procedures in the App schema (the database API).
- Place views in the AppDbo schema.
- Organize tables, indexes, procedures, and views by domain.
- Include maintenance procedures and read views for every domain.