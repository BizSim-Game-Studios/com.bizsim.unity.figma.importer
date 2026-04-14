using UnityEditor;

namespace BizSim.Unity.Figma.Importer.Editor {
    public static class FigmaUserSettings {
        private const string PAT_KEY = "BizSim_FigmaImporter_PAT";
        private const string FILE_KEY = "BizSim_FigmaImporter_FileKey";
        private const string CACHE_KEY = "BizSim_FigmaImporter_CacheEnabled";

        public static string PersonalAccessToken {
            get => EditorPrefs.GetString(PAT_KEY, "");
            set => EditorPrefs.SetString(PAT_KEY, value);
        }

        public static string FigmaFileKey {
            get => EditorPrefs.GetString(FILE_KEY, "");
            set => EditorPrefs.SetString(FILE_KEY, value);
        }

        public static bool CacheEnabled {
            get => EditorPrefs.GetBool(CACHE_KEY, true);
            set => EditorPrefs.SetBool(CACHE_KEY, value);
        }

        public static bool HasValidToken => !string.IsNullOrEmpty(PersonalAccessToken);
    }
}
