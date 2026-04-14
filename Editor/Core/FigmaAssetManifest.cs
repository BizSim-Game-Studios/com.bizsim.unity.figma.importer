using System;
using System.Collections.Generic;

namespace BizSim.Unity.Figma.Importer.Editor {
    [Serializable]
    public class FigmaAssetManifest {
        public string version;
        public ManifestSource source;
        public List<ManifestAsset> assets;
        public List<ManifestFont> fonts;
    }

    [Serializable]
    public class ManifestSource {
        public string fileName;
        public string frameName;
        public string frameNodeId;
        public string exportDate;
    }

    [Serializable]
    public class ManifestFont {
        public string family;
        public string style;
        public string weight;
        public List<string> usedInNodes;
    }

    [Serializable]
    public class ManifestAsset {
        public string nodeId;
        public string name;
        public string originalName;
        public string category;
        public string format;
        public int scale;
        public int width;
        public int height;
        public bool isVector;
        public string parentFrame;
    }

    public enum ImportStatus {
        Missing,
        Exists,
        SizeChanged,
        Error
    }

    [Serializable]
    public class ImportItem {
        public ManifestAsset asset;
        public ImportStatus status;
        public string resolvedPath;
        public string existingPath;
        public bool selected;
    }

    public class ImportResult {
        public int totalAssets;
        public int imported;
        public int skipped;
        public int failed;
        public List<string> errors = new();
        public List<string> importedPaths = new();
    }
}
