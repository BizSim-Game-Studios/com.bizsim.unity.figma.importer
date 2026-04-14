using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    public class FigmaImportOrchestrator {
        private readonly FigmaImporterSettings _settings;
        private readonly PathResolver _pathResolver;
        private readonly AssetScanner _scanner;
        private FigmaApiClient _apiClient;
        private AssetWriter _writer;

        public event Action<float, string> OnProgressChanged;
        public event Action<ImportResult> OnImportCompleted;

        public FigmaImportOrchestrator(FigmaImporterSettings settings) {
            _settings = settings;
            _pathResolver = new PathResolver(settings);
            _scanner = new AssetScanner();
        }

        public FigmaAssetManifest ParseManifest(string json) {
            return JsonUtility.FromJson<FigmaAssetManifest>(json);
        }

        public List<ImportItem> ScanProject(FigmaAssetManifest manifest) {
            return _scanner.BuildImportList(manifest, _pathResolver);
        }

        public async Task<ImportResult> Import(
            List<ImportItem> items,
            string figmaFileKey
        ) {
            var result = new ImportResult { totalAssets = items.Count };

            if (!FigmaUserSettings.HasValidToken) {
                result.errors.Add("Figma PAT token not configured. Go to Preferences.");
                return result;
            }

            _apiClient = new FigmaApiClient(FigmaUserSettings.PersonalAccessToken);
            _writer = new AssetWriter(_apiClient);

            var toImport = items.Where(i => i.selected && i.status == ImportStatus.Missing).ToList();
            if (toImport.Count == 0) {
                result.skipped = items.Count;
                return result;
            }

            ReportProgress(0f, "Fetching image URLs from Figma...");

            var nodeIds = toImport.Select(i => i.asset.nodeId).ToList();

            var formatGroups = toImport.GroupBy(i => {
                var rule = _settings.GetRuleForCategory(i.asset.category);
                return (format: GetFormatString(rule.format, i.asset.isVector), scale: rule.scale);
            });

            var allImageUrls = new Dictionary<string, string>();

            foreach (var group in formatGroups) {
                var groupNodeIds = group.Select(i => i.asset.nodeId).ToList();
                var urls = await _apiClient.GetImageUrls(
                    figmaFileKey,
                    groupNodeIds,
                    group.Key.format,
                    group.Key.scale
                );
                foreach (var kvp in urls) {
                    allImageUrls[kvp.Key] = kvp.Value;
                }
            }

            ReportProgress(0.3f, $"Downloading {toImport.Count} assets...");

            for (int i = 0; i < toImport.Count; i++) {
                var item = toImport[i];
                float progress = 0.3f + (0.65f * i / toImport.Count);
                ReportProgress(progress, $"Importing {item.asset.name}...");

                if (!allImageUrls.TryGetValue(item.asset.nodeId, out string imageUrl)) {
                    result.failed++;
                    result.errors.Add($"No image URL for {item.asset.name} (node {item.asset.nodeId})");
                    continue;
                }

                bool success = await _writer.DownloadAndSave(imageUrl, item.resolvedPath, item.asset);

                if (success) {
                    result.imported++;
                    result.importedPaths.Add(item.resolvedPath);
                } else {
                    result.failed++;
                    result.errors.Add($"Failed to download {item.asset.name}");
                }
            }

            RunPostProcessors(result);

            ReportProgress(1f, "Import complete.");
            AssetDatabase.Refresh();

            result.skipped = items.Count - toImport.Count;
            OnImportCompleted?.Invoke(result);
            return result;
        }

        private void RunPostProcessors(ImportResult result) {
            var processorTypes = TypeCache.GetTypesWithAttribute<FigmaPostProcessorAttribute>();
            var processors = new List<IAssetPostProcessor>();

            foreach (var type in processorTypes) {
                if (typeof(IAssetPostProcessor).IsAssignableFrom(type)) {
                    var instance = Activator.CreateInstance(type) as IAssetPostProcessor;
                    if (instance != null) processors.Add(instance);
                }
            }

            processors.Sort((a, b) => a.Priority.CompareTo(b.Priority));

            foreach (string path in result.importedPaths) {
                foreach (var processor in processors) {
                    processor.OnAssetImported(path, null, _settings);
                }
            }
        }

        private static string GetFormatString(ExportFormat format, bool isVector) {
            if (isVector) return "svg";
            return format switch {
                ExportFormat.SVG => "svg",
                ExportFormat.JPG => "jpg",
                _ => "png"
            };
        }

        private void ReportProgress(float progress, string message) {
            OnProgressChanged?.Invoke(progress, message);
        }
    }
}
