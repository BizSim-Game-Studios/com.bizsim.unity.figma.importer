using System;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    [Serializable]
    public class AssetPathRule {
        public string category;
        [Tooltip("Target folder relative to project root, e.g. Assets/Sprites/Icons")]
        public string targetFolder;
        [Tooltip("Optional prefix for file names, e.g. icon_")]
        public string namePrefix;
        public ExportFormat format;
        [Range(1, 4)]
        public int scale = 2;
    }

    public enum ExportFormat {
        PNG,
        SVG,
        JPG
    }

    public enum NamingConvention {
        KebabCase,
        SnakeCase,
        PascalCase,
        Original
    }
}
