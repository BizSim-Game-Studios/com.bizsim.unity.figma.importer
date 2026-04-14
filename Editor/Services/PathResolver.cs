using System.Text.RegularExpressions;

namespace BizSim.Unity.Figma.Importer.Editor {
    public class PathResolver {
        private readonly FigmaImporterSettings _settings;

        public PathResolver(FigmaImporterSettings settings) {
            _settings = settings;
        }

        public string Resolve(ManifestAsset asset) {
            var rule = _settings.GetRuleForCategory(asset.category);
            string folder = rule.targetFolder;
            string fileName = FormatName(asset.name, _settings.namingConvention);
            string extension = GetExtension(rule.format, asset.isVector);

            return $"{folder}/{rule.namePrefix}{fileName}.{extension}";
        }

        private static string FormatName(string name, NamingConvention convention) {
            switch (convention) {
                case NamingConvention.SnakeCase:
                    return ToSnakeCase(name);
                case NamingConvention.PascalCase:
                    return ToPascalCase(name);
                case NamingConvention.Original:
                    return name;
                case NamingConvention.KebabCase:
                default:
                    return name;
            }
        }

        private static string ToSnakeCase(string kebab) {
            return kebab.Replace("-", "_");
        }

        private static string ToPascalCase(string kebab) {
            var parts = kebab.Split('-');
            for (int i = 0; i < parts.Length; i++) {
                if (parts[i].Length > 0) {
                    parts[i] = char.ToUpper(parts[i][0]) + parts[i][1..];
                }
            }
            return string.Join("", parts);
        }

        private static string GetExtension(ExportFormat format, bool isVector) {
            if (isVector && format != ExportFormat.JPG) return "svg";
            return format switch {
                ExportFormat.SVG => "svg",
                ExportFormat.JPG => "jpg",
                _ => "png"
            };
        }
    }
}
