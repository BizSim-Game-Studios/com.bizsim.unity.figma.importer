# API Reference

Namespace: `BizSim.Unity.Figma.Importer.Editor`

Assembly: `BizSim.Unity.Figma.Importer.Editor`

All types in this package are editor-only.

---

## FigmaImportOrchestrator

`public class FigmaImportOrchestrator`

Coordinates the full import pipeline: manifest parsing, project scanning, and asset downloading.

### Constructor

| Parameter | Type | Description |
|-----------|------|-------------|
| `settings` | `FigmaImporterSettings` | Project-wide import configuration |

### Methods

| Method | Return | Description |
|--------|--------|-------------|
| `ParseManifest(string json)` | `FigmaAssetManifest` | Deserializes the JSON manifest from the Figma plugin |
| `ScanProject(FigmaAssetManifest manifest)` | `List<ImportItem>` | Builds the import list by checking which assets are missing |
| `Import(List<ImportItem> items, string figmaFileKey)` | `Task<ImportResult>` | Downloads selected missing assets from Figma |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnProgressChanged` | `Action<float, string>` | Fired during import with progress (0-1) and status message |
| `OnImportCompleted` | `Action<ImportResult>` | Fired when the import finishes |

---

## FigmaImporterSettings

`public class FigmaImporterSettings : ScriptableObject`

Project-wide configuration for asset organization and naming.

### Fields

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `defaultAssetBasePath` | `string` | `"Assets/Sprites"` | Fallback path when no rule matches |
| `pathRules` | `List<AssetPathRule>` | 6 default rules | Per-category path, format, and scale rules |
| `namingConvention` | `NamingConvention` | `KebabCase` | File naming convention for downloaded assets |
| `useAddressables` | `bool` | `false` | Register imported assets with Unity Addressables |
| `addressableGroupName` | `string` | `"FigmaAssets"` | Addressables group name when enabled |

### Methods

| Method | Return | Description |
|--------|--------|-------------|
| `GetRuleForCategory(string category)` | `AssetPathRule` | Returns the rule for a category, or a default rule if none matches |

---

## AssetPathRule

`public class AssetPathRule`

Defines how assets in a specific category are stored.

| Field | Type | Description |
|-------|------|-------------|
| `category` | `string` | Asset category from the manifest (e.g., "icon", "background") |
| `targetFolder` | `string` | Unity project path for downloaded files |
| `format` | `ExportFormat` | Export format (PNG, SVG, JPG, PDF) |
| `scale` | `int` | Export scale multiplier |

---

## FigmaAssetManifest

`public class FigmaAssetManifest`

Deserialized JSON manifest produced by the Figma UI Toolkit Generator plugin. Contains the list of assets referenced by generated UXML/USS files.

---

## FigmaUserSettings

Per-user settings stored outside the project (not committed to version control).

| Property | Type | Description |
|----------|------|-------------|
| `PersonalAccessToken` | `string` | Figma PAT for API authentication |
| `HasValidToken` | `bool` | Whether a non-empty token is configured |

---

## ImportResult

| Field | Type | Description |
|-------|------|-------------|
| `totalAssets` | `int` | Total number of assets in the manifest |
| `imported` | `int` | Number of assets successfully downloaded |
| `skipped` | `int` | Number of assets already present |
| `errors` | `List<string>` | Error messages for failed downloads |

---

## NamingConvention (enum)

| Value | Description |
|-------|-------------|
| `KebabCase` | `my-asset-name` (default) |
| `SnakeCase` | `my_asset_name` |
| `PascalCase` | `MyAssetName` |
| `CamelCase` | `myAssetName` |

---

## ExportFormat (enum)

| Value | Description |
|-------|-------------|
| `PNG` | Rasterized PNG |
| `SVG` | Vector SVG |
| `JPG` | Compressed JPEG |
| `PDF` | Vector PDF |
