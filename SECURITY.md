# Security policy

## Supported versions

Security fixes are considered for the current `main` package channel. `develop` is a preview channel and may receive a fix before promotion. Older commits and locally modified copies are not guaranteed to receive backports.

## Report a vulnerability privately

Use [GitHub private vulnerability reporting](https://github.com/Deucarian/API/security/advisories/new). Do not open a public issue for a suspected vulnerability.

Include the affected package version or commit, Unity version and platform, impact, reproduction steps or proof of concept, and any known mitigations. Remove live credentials and personal data; use synthetic examples wherever possible.

The maintainers will triage the report in GitHub's private advisory, may ask for additional evidence, and will coordinate disclosure after a fix or mitigation is available. No response or remediation deadline is guaranteed.

Security scope includes request construction, authentication handling, response parsing, serialization, transport, and package-supplied samples. Vulnerabilities in Unity or separately resolved dependencies should also be reported to their respective maintainers.
