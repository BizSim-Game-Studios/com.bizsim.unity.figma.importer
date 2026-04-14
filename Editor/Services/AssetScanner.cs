using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    public class AssetScanner {
        public List<ImportItem> BuildImportList(
            FigmaAssetManifest manifest,
            PathResolver pathResolver
        ) {
            var items = new List<ImportItem>();

            foreach (var asset in manifest.assets) {
                string resolvedPath = pathResolver.Resolve(asset);
                string fullPath = Path.Combine(
                    Application.dataPath.Replace("/Assets", ""),
                    resolvedPath.Replace("/", Path.DirectorySeparatorChar.ToString())
                );

                var item = new ImportItem {
                    asset = asset,
                    resolvedPath = resolvedPath,
                    selected = true
                };

                if (File.Exists(fullPath)) {
                    var fileInfo = new FileInfo(fullPath);
                    item.status = ImportStatus.Exists;
                    item.existingPath = resolvedPath;
                    item.selected = false;
                } else {
                    string existingPath = FindExistingAssetByName(asset.name);
                    if (existingPath != null) {
                        item.status = ImportStatus.Exists;
                        item.existingPath = existingPath;
                        item.selected = false;
                    } else {
                        item.status = ImportStatus.Missing;
                        item.selected = true;
                    }
                }

                items.Add(item);
            }

            return items;
        }

        private string FindExistingAssetByName(string assetName) {
            string searchPattern = $"{assetName}.*";
            string[] guids = UnityEditor.AssetDatabase.FindAssets(assetName);

            foreach (string guid in guids) {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                if (fileName.Equals(assetName, System.StringComparison.OrdinalIgnoreCase)) {
                    return path;
                }
            }

            return null;
        }
    }
}
