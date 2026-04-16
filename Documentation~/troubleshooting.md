# Troubleshooting

## Problem: "Figma PAT token not configured" error during import

**Cause:** The Figma Personal Access Token has not been set in Unity Preferences, or it was cleared after an editor update.

**Fix:**
1. Open **Edit > Preferences > Figma Importer**.
2. Paste your Figma PAT into the token field.
3. If you do not have a PAT, generate one at **figma.com > Account Settings > Personal Access Tokens**.

---

## Problem: Import completes but all assets are skipped

**Cause:** All assets in the manifest already exist at their expected paths. The importer only downloads assets with `ImportStatus.Missing`.

**Fix:**
1. If you want to re-download existing assets, delete the local files first or use a fresh import list.
2. Check that the `AssetPathRule` target folders match where you expect the files. If rules changed since the last import, assets may be at old paths while the importer looks at new paths.

---

## Problem: 403 Forbidden from Figma API

**Cause:** The PAT does not have access to the target Figma file, or the token has expired.

**Fix:**
1. Verify the PAT has read access to the file. Open the Figma file in a browser while logged in with the same account that generated the PAT.
2. Generate a fresh PAT if the current one has expired.
3. Check that the Figma file key in the import call matches the actual file URL (`https://figma.com/file/<FILE_KEY>/...`).

---

## Problem: Downloaded images are low resolution

**Cause:** The `scale` field in the matching `AssetPathRule` is set to 1. By default, icons use scale 1 (SVG vectors do not need upscaling), but raster categories like illustrations should use scale 2 or higher.

**Fix:**
1. Open your `FigmaImporterSettings` asset.
2. Find the path rule for the affected category.
3. Increase the `scale` value (2x or 3x for high-DPI assets).
4. Re-import the affected assets.

---

## Problem: SVG files are not rendering in UI Toolkit

**Cause:** Unity's UI Toolkit supports SVG rendering via the `com.unity.vectorgraphics` package, which must be installed separately. Without it, SVG files appear as raw XML in the project.

**Fix:**
1. Install `com.unity.vectorgraphics` via Package Manager.
2. After installation, reimport the SVG files so Unity generates the vector asset representation.
3. If SVG support is not needed, change the export format in the path rule from SVG to PNG.

---

## Problem: Asset names have unexpected characters or formatting

**Cause:** The `namingConvention` in `FigmaImporterSettings` does not match your project's convention, or the Figma layer names contain special characters that the naming formatter cannot transliterate.

**Fix:**
1. Check `FigmaImporterSettings.namingConvention` and set it to your preferred format (KebabCase, SnakeCase, PascalCase, or CamelCase).
2. In Figma, rename layers to use ASCII characters. The importer strips non-alphanumeric characters during name formatting.

---

## Problem: Addressables group is not created after import

**Cause:** `useAddressables` is enabled in `FigmaImporterSettings` but the `com.unity.addressables` package is not installed in the project.

**Fix:**
1. Install `com.unity.addressables` via Package Manager.
2. Ensure the Addressables system is initialized (open **Window > Asset Management > Addressables > Groups** at least once).
3. Re-run the import.
