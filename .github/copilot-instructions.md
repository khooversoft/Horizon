# Copilot Instructions

## Project Guidelines
- Use the repository's Verify.* extension functions for argument validation in SelectParallelAsync instead of built-in guards such as ArgumentOutOfRangeException.ThrowIfNegativeOrZero or ArgumentNullException.ThrowIfNull. This supersedes the earlier preference for built-in guards.