using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    public enum FontStatus {
        Found,
        Missing,
        PartialMatch
    }

    public class FontScanResult {
        public ManifestFont font;
        public FontStatus status;
        public string foundPath;
        public string searchedPattern;
    }

    public class FontScanner {
        private static readonly string[] FONT_EXTENSIONS = { ".ttf", ".otf" };
        private static readonly string[] SEARCH_DIRS = { "Assets/Fonts", "Assets/TextMesh Pro/Fonts" };

        public List<FontScanResult> ScanFonts(List<ManifestFont> requiredFonts) {
            var results = new List<FontScanResult>();
            if (requiredFonts == null) return results;

            var existingFonts = DiscoverProjectFonts();

            foreach (var font in requiredFonts) {
                var result = new FontScanResult { font = font };

                string exactKey = $"{font.family}_{font.weight}".ToLowerInvariant();
                string familyKey = font.family.ToLowerInvariant().Replace(" ", "");
                string styleKey = $"{font.family}_{font.style}".ToLowerInvariant().Replace(" ", "");

                if (existingFonts.TryGetValue(exactKey, out string exactPath)) {
                    result.status = FontStatus.Found;
                    result.foundPath = exactPath;
                } else if (TryFindByPattern(existingFonts, familyKey, font.style, out string partialPath)) {
                    result.status = FontStatus.Found;
                    result.foundPath = partialPath;
                } else if (TryFindByPattern(existingFonts, familyKey, "", out string familyPath)) {
                    result.status = FontStatus.PartialMatch;
                    result.foundPath = familyPath;
                } else {
                    result.status = FontStatus.Missing;
                }

                result.searchedPattern = $"{font.family} {font.style} (weight {font.weight})";
                results.Add(result);
            }

            return results;
        }

        private Dictionary<string, string> DiscoverProjectFonts() {
            var fonts = new Dictionary<string, string>();

            foreach (string dir in SEARCH_DIRS) {
                if (!Directory.Exists(dir)) continue;

                foreach (string ext in FONT_EXTENSIONS) {
                    string[] files = Directory.GetFiles(dir, $"*{ext}", SearchOption.AllDirectories);
                    foreach (string file in files) {
                        string name = Path.GetFileNameWithoutExtension(file).ToLowerInvariant();
                        string unityPath = file.Replace("\\", "/");
                        fonts[name.Replace(" ", "").Replace("-", "_")] = unityPath;
                        fonts[name.Replace(" ", "")] = unityPath;
                        fonts[name] = unityPath;
                    }
                }
            }

            return fonts;
        }

        private bool TryFindByPattern(
            Dictionary<string, string> fonts,
            string familyKey,
            string style,
            out string path
        ) {
            string combined = string.IsNullOrEmpty(style)
                ? familyKey
                : $"{familyKey}{style.ToLowerInvariant().Replace(" ", "")}";

            foreach (var kvp in fonts) {
                if (kvp.Key.Contains(combined) || combined.Contains(kvp.Key)) {
                    path = kvp.Value;
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(style)) {
                string weightSuffix = style.ToLowerInvariant().Replace(" ", "");
                foreach (var kvp in fonts) {
                    if (kvp.Key.Contains(familyKey) && kvp.Key.Contains(weightSuffix)) {
                        path = kvp.Value;
                        return true;
                    }
                }
            }

            path = null;
            return false;
        }
    }
}
