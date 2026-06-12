# Contributing to API

API is maintained as a package-quality Unity library. Keep changes focused,
practical, and consistent with the existing `IApiClient`-based architecture.

## Documentation Maintenance

Documentation must be updated whenever public APIs are added, changed,
deprecated, or removed.

Update the relevant README sections, XML documentation, sample scripts/scenes,
and obsolete migration messages in the same change as the API change.

## Package Polish Checklist

- Prefer `IApiClient` for new runtime workflows.
- Keep `ApiServices` as a legacy/convenience facade only.
- Keep production defaults safe, especially certificate handling.
- Keep asmdef references readable with assembly names when Unity/package
  compatibility allows it. Document any required GUID references.
- Keep runtime code under `Runtime`, editor code under `Editor`, tests under
  `Tests/Editor`, and samples under `Samples~`.
- Do not commit Unity-generated `Library`, `Temp`, `Obj`, build output, zip, or
  `.unitypackage` artifacts.
- Add examples when a public workflow would otherwise require reading source.
- Run the API EditMode tests after changes.

## Release Channels

- `develop` is the beta channel.
- `main` is the stable channel.
- Tags such as `v1.0.0` are stable release installs.
- Prerelease tags such as `v1.1.0-beta.1` may be used for beta release installs.

GitHub Actions validates package structure on PRs and pushes. The manual Unity
EditMode test workflow requires Unity/GameCI secrets before it can run in CI.
