# Changelog

## 1.0.1 - 2026-06-15

- Improved README structure for overview, core concepts, public API, samples, integrations, versioning, and limitations.
- Moved the raw JSON debug toggle under `Tools > Deucarian > API`.

## 1.0.1 - 2026-06-15

- Standardized package logging on com.deucarian.logging.
- Added `ApiLog` package categories and removed the internal API logger abstraction.

## 1.0.0 - 2026-06-03

- Converted APIHelper into a standalone Unity Package Manager Git package.
- Migrated the refactored `IApiClient`-based APIHelper runtime, editor code,
  tests, documentation, and sample scene from the HoloHelmet project.
- Added package metadata, release-channel documentation, and GitHub Actions
  validation.
- Kept legacy `ApiServices` compatibility wrappers with obsolete migration
  guidance.
