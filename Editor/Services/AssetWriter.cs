using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    public class AssetWriter {
        private readonly FigmaApiClient _apiClient;

        public AssetWriter(FigmaApiClient apiClient) {
            _apiClient = apiClient;
        }

        public async Task<bool> DownloadAndSave(string imageUrl, string assetPath, ManifestAsset assetInfo) {
            byte[] data = await _apiClient.DownloadImage(imageUrl);
            if (data == null || data.Length == 0) {
                Debug.LogError($"[FigmaImporter] Empty data for {assetInfo.name}");
                return false;
            }

            string fullPath = Path.Combine(
                Application.dataPath.Replace("/Assets", ""),
                assetPath.Replace("/", Path.DirectorySeparatorChar.ToString())
            );

            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullPath, data);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            ConfigureTextureImporter(assetPath, assetInfo);

            Debug.Log($"[FigmaImporter] Imported: {assetPath} ({data.Length / 1024}KB)");
            return true;
        }

        private void ConfigureTextureImporter(string assetPath, ManifestAsset assetInfo) {
            if (assetInfo.format == "svg") return;

            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.isReadable = false;

            if (assetInfo.category == "icon") {
                importer.filterMode = FilterMode.Trilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
            } else if (assetInfo.category == "background") {
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.maxTextureSize = 2048;
            } else {
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Compressed;
            }

            importer.SaveAndReimport();
        }
    }
}
