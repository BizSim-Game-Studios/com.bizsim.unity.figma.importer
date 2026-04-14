namespace BizSim.Unity.Figma.Importer.Editor {
    public interface IAssetPostProcessor {
        int Priority { get; }
        void OnAssetImported(string assetPath, ManifestAsset assetInfo, FigmaImporterSettings settings);
    }
}
