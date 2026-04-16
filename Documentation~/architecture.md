# Architecture

## Package Role

The Figma Importer is an editor-only package that bridges the gap between the Figma UI Toolkit Generator plugin (which produces UXML/USS files with asset references) and the Unity Editor (which needs the actual image/font files to render those UXML/USS files).

## Assembly Structure

```
BizSim.Unity.Figma.Importer.Editor (Editor asmdef)
    includePlatforms: [Editor]
    references: (none)
```

There is no Runtime asmdef. All code compiles and executes only in the Unity Editor. No code from this package ships in player builds.

## Component Diagram

```
┌──────────────────────────────────────────────────┐
│                 Figma (Cloud)                    │
│  Figma File → Plugin generates Manifest JSON     │
└────────────────────┬─────────────────────────────┘
                     │ REST API (via PAT)
┌────────────────────▼─────────────────────────────┐
│              Unity Editor                        │
│                                                  │
│  FigmaImportOrchestrator                         │
│    ├── ParseManifest() → FigmaAssetManifest      │
│    ├── ScanProject()   → List<ImportItem>         │
│    │     └── AssetScanner (compares manifest      │
│    │          entries against existing files)      │
│    └── Import()        → ImportResult             │
│          ├── FigmaApiClient (HTTP GET images)     │
│          ├── AssetWriter (saves to disk)          │
│          └── PathResolver (applies path rules)    │
│                                                  │
│  FigmaImporterSettings (ScriptableObject)        │
│    └── AssetPathRule[] (category → folder map)    │
│                                                  │
│  FigmaUserSettings (EditorPrefs)                 │
│    └── Personal Access Token                     │
└──────────────────────────────────────────────────┘
```

## Import Pipeline

1. **Manifest Parsing** -- `ParseManifest()` deserializes the JSON Asset Manifest produced by the Figma plugin. The manifest lists every image, icon, and font referenced by the generated UXML/USS.

2. **Project Scanning** -- `ScanProject()` iterates the manifest entries and checks whether each asset already exists in the Unity project. The `AssetScanner` compares file paths resolved by `PathResolver` against the asset database. Items are marked as `Missing`, `Existing`, or `Modified`.

3. **Asset Download** -- `Import()` iterates the selected missing items, calls the Figma REST API via `FigmaApiClient` to download each asset in the specified format and scale, and writes the files to disk via `AssetWriter`. Progress events fire during download.

4. **Post-Import** -- Unity's asset pipeline detects the new files and imports them. If Addressables is enabled, assets are registered to the configured Addressable group.

## Extensibility

### Custom Path Rules

Add `AssetPathRule` entries to `FigmaImporterSettings.pathRules` for project-specific asset categories. Each rule maps a manifest category string to a Unity folder, export format, and scale.

### Custom Naming

Set `FigmaImporterSettings.namingConvention` to control downloaded file name formatting (kebab-case, snake_case, PascalCase, camelCase).

### Programmatic Usage

All public APIs are available for scripting. You can build custom editor windows or CI scripts that call `FigmaImportOrchestrator` directly.

## Thread Model

All operations run on the Unity main thread. HTTP requests to the Figma API use `async/await` (`Task`-based) but are initiated from the editor main thread. Progress callbacks fire on the main thread.
