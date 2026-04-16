# Data Safety

## Data Flow Information

This document describes what data flows through `com.bizsim.unity.figma.importer`.

## Editor-Only Package

This package is editor-only. It does NOT ship any code in player builds (APK, AAB, IPA, desktop, or WebGL). All assemblies are excluded from builds via `includePlatforms: ["Editor"]` in the asmdef definition.

## Data Collected at Edit Time

| Data Type | Collected | Persisted | Transmitted | Purpose |
|-----------|-----------|-----------|-------------|---------|
| Figma Personal Access Token | Yes | Yes (EditorPrefs, per-user) | Yes (to Figma REST API) | Authentication with Figma |
| Figma file key | Yes | No (passed at call time) | Yes (to Figma REST API) | Identify which file to download from |
| Downloaded image/font files | Yes | Yes (Unity project assets) | No | UI rendering in Unity |

## How Data Flows

1. **Figma PAT** is stored in `EditorPrefs` (local to the user's machine, not committed to version control). It is sent as a Bearer token in HTTPS requests to `api.figma.com`.

2. **Figma file key** is used in API URLs to identify the source file. It is not persisted by this package.

3. **Downloaded assets** (images, icons, fonts) are written to the Unity project's `Assets/` folder. These are project files managed by version control, not user data.

## Runtime Impact

- No code from this package exists in compiled player builds
- No network calls are made at runtime
- No data is written to the player device
- No analytics events are sent

## Play Store / App Store Forms

When filling out the Play Store Data Safety form or App Store privacy nutrition label, you do NOT need to declare anything for this package. It has zero runtime presence.

## Third-Party Services

This package communicates with:
- **Figma REST API** (`api.figma.com`) -- for downloading images and fonts. Figma's own privacy policy applies to data processed by their servers.
