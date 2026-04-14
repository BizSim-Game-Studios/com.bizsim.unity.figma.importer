using System.Collections.Generic;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    [CreateAssetMenu(fileName = "FigmaImporterSettings", menuName = "BizSim/Figma Importer Settings")]
    public class FigmaImporterSettings : ScriptableObject {
        [Header("Asset Organization")]
        [Tooltip("Base path for assets when no specific rule matches")]
        public string defaultAssetBasePath = "Assets/Sprites";

        [Tooltip("Rules for organizing assets by category into folders")]
        public List<AssetPathRule> pathRules = new() {
            new() { category = "background", targetFolder = "Assets/Sprites/Backgrounds", format = ExportFormat.PNG, scale = 1 },
            new() { category = "icon", targetFolder = "Assets/Sprites/Icons", format = ExportFormat.SVG, scale = 1 },
            new() { category = "logo", targetFolder = "Assets/Sprites/Logos", format = ExportFormat.PNG, scale = 2 },
            new() { category = "illustration", targetFolder = "Assets/Sprites/Illustrations", format = ExportFormat.PNG, scale = 2 },
            new() { category = "avatar", targetFolder = "Assets/Sprites/Avatars", format = ExportFormat.PNG, scale = 2 },
            new() { category = "element", targetFolder = "Assets/Sprites/UI", format = ExportFormat.PNG, scale = 2 },
        };

        [Header("Naming")]
        public NamingConvention namingConvention = NamingConvention.KebabCase;

        [Header("Addressables")]
        public bool useAddressables;
        public string addressableGroupName = "FigmaAssets";

        public AssetPathRule GetRuleForCategory(string category) {
            foreach (var rule in pathRules) {
                if (rule.category == category) return rule;
            }
            return new AssetPathRule {
                category = category,
                targetFolder = defaultAssetBasePath,
                format = ExportFormat.PNG,
                scale = 2
            };
        }
    }
}
