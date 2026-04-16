# Configuration

## FigmaImporterSettings (Project Settings)

Create via **Create > BizSim > Figma Importer Settings** in the Project window. This ScriptableObject is committed to version control and shared across the team.

### Asset Organization

| Field | Default | Description |
|-------|---------|-------------|
| `defaultAssetBasePath` | `Assets/Sprites` | Base path for assets when no category rule matches |

### Path Rules

Path rules map asset categories (from the Figma plugin manifest) to Unity project folders, export formats, and scale factors.

Default rules:

| Category | Target Folder | Format | Scale |
|----------|---------------|--------|-------|
| `background` | `Assets/Sprites/Backgrounds` | PNG | 1x |
| `icon` | `Assets/Sprites/Icons` | SVG | 1x |
| `logo` | `Assets/Sprites/Logos` | PNG | 2x |
| `illustration` | `Assets/Sprites/Illustrations` | PNG | 2x |
| `avatar` | `Assets/Sprites/Avatars` | PNG | 2x |
| `element` | `Assets/Sprites/UI` | PNG | 2x |

Add custom rules for project-specific categories. The `category` field must match the category string in the Figma plugin's Asset Manifest.

### Naming Convention

Controls how downloaded file names are formatted:

- **KebabCase** (default): `my-asset-name.png`
- **SnakeCase**: `my_asset_name.png`
- **PascalCase**: `MyAssetName.png`
- **CamelCase**: `myAssetName.png`

### Addressables Integration

| Field | Default | Description |
|-------|---------|-------------|
| `useAddressables` | `false` | Register imported assets with Unity Addressables |
| `addressableGroupName` | `FigmaAssets` | Addressables group name when enabled |

When enabled, each imported asset is added to the specified Addressable group after download. Requires the `com.unity.addressables` package to be installed.

## FigmaUserSettings (Per-User)

Accessed via **Edit > Preferences > Figma Importer**. These settings are stored in `EditorPrefs` (per-user, per-machine) and are NOT committed to version control.

| Field | Description |
|-------|-------------|
| `Personal Access Token` | Your Figma PAT. Required for API access. |

### Generating a Figma PAT

1. Open [figma.com](https://figma.com) and sign in.
2. Go to **Account Settings** (click your avatar, top-left).
3. Scroll to **Personal Access Tokens**.
4. Click **Generate new token**, give it a descriptive name.
5. Copy the token and paste it into the Unity Preferences field.

The token is stored securely in EditorPrefs and never committed to your project.

## Multiple Settings Assets

You can create multiple `FigmaImporterSettings` assets for different workflows (e.g., one for UI sprites, one for marketing assets). Pass the desired settings to `FigmaImportOrchestrator` at import time.
