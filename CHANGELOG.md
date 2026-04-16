# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2026-04-16

### Fixed
- Fully-qualify `UnityEditor.Editor.CreateEditor` call in `FigmaImporterWindow` to resolve ambiguous reference

## [1.0.0] - 2026-04-14

### Added

- Initial release of `com.bizsim.unity.figma.importer` — Unity Editor companion to the [Figma2UnityUIToolkit](https://github.com/BizSim-Game-Studios/Figma2UnityUIToolkit) Figma plugin.
- `FigmaImporterWindow` — Editor window at **Window → Figma Asset Importer** for browsing manifests, entering credentials, and running imports.
- `FigmaImportOrchestrator` — scans a `FigmaAssetManifest`, deduplicates requests, and drives image/font downloads via the Figma REST API.
- `FigmaApiClient` — REST client for the Figma v1 API (images, files, fonts).
- `AssetScanner` / `AssetWriter` — project-side diff and write pipeline respecting `AssetPathRule` configuration.
- `FigmaUserSettings` — per-machine `EditorPrefs`-backed storage for the Figma Personal Access Token and File Key (never written to source control).
- `FigmaImporterSettings` — project-level `ScriptableObject` for asset path rules and import defaults.
- Extensibility hooks: `IAssetPathResolver`, `IAssetPostProcessor`, `[FigmaPostProcessor]` attribute.

### Notes

- This is the first release under the new dot-separated package id `com.bizsim.unity.figma.importer`. The previous incarnation (`com.bizsim.unity-figma-importer`) at version 1.0.0 or 2.0.0 is archived and no longer maintained.
- Editor-only package — Unity 6.0 LTS (`6000.0`) floor.
- Required companion to the [Figma2UnityUIToolkit](https://github.com/BizSim-Game-Studios/Figma2UnityUIToolkit) Figma plugin which emits the Asset Manifest this package consumes.
