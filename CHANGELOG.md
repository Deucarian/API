# Changelog

## 1.1.4 - 2026-07-17

- Documented the Example Scene sample and aligned the exact Logging dependency for the portfolio release.

## 1.1.3 - 2026-06-22

- Updated the exact `com.deucarian.logging` dependency to `1.0.1`.

## 1.1.2 - 2026-06-22

- Accepted the public release automation state for `com.deucarian.api` 1.1.2 on develop.

## 1.1.1 - 2026-06-22

- Promoted the prepared API 1.1.1 release metadata into develop.

## 1.1.0 - 2026-06-19

- Added `ApiResponseFormat.AssetBundle` with automatic `AssetBundle` response detection.
- Added `ApiAssetBundleRequestOptions` for CRC and Unity AssetBundle cache metadata.
- Added API transfer progress callbacks with normalized progress and byte counts.
- Routed AssetBundle responses through `UnityWebRequestAssetBundle` and `DownloadHandlerAssetBundle` without reading `downloadHandler.data`.

## 1.0.2 - 2026-06-17

- Renamed Session API package documentation from bridge terminology to integration terminology.

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
