namespace BizSim.Unity.Figma.Importer.Editor {
    public interface IAssetPathResolver {
        bool CanResolve(ManifestAsset asset);
        string ResolvePath(ManifestAsset asset, FigmaImporterSettings settings);
    }
}
