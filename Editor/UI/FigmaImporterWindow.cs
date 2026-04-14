using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace BizSim.Unity.Figma.Importer.Editor {
    public class FigmaImporterWindow : EditorWindow {
        private FigmaImporterSettings _settings;
        private FigmaImportOrchestrator _orchestrator;

        private string _manifestJson = "";
        private string _figmaFileKey = "";
        private FigmaAssetManifest _manifest;
        private List<ImportItem> _importItems;
        private ImportResult _lastResult;
        private List<FontScanResult> _fontResults;

        private float _progress;
        private string _progressMessage = "";
        private bool _isImporting;

        private Vector2 _scrollPosition;
        private Vector2 _compareScrollPosition;
        private int _selectedTab;
        private readonly string[] _tabNames = { "Import", "Compare", "Settings", "User Settings" };

        private string _screenshotBase64Input = "";
        private Texture2D _figmaScreenshot;
        private Texture2D _unityScreenshot;
        private float _compareZoom = 1f;

        [MenuItem("Window/Figma Asset Importer")]
        public static void ShowWindow() {
            var window = GetWindow<FigmaImporterWindow>("Figma Asset Importer");
            window.minSize = new Vector2(500, 400);
        }

        private void OnEnable() {
            _figmaFileKey = FigmaUserSettings.FigmaFileKey;
        }

        private void OnDestroy() {
            if (_figmaScreenshot != null) DestroyImmediate(_figmaScreenshot);
            if (_unityScreenshot != null) DestroyImmediate(_unityScreenshot);
        }

        private void OnGUI() {
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);
            EditorGUILayout.Space(8);

            switch (_selectedTab) {
                case 0: DrawImportTab(); break;
                case 1: DrawCompareTab(); break;
                case 2: DrawSettingsTab(); break;
                case 3: DrawUserSettingsTab(); break;
            }
        }

        #region Compare Tab
        private void DrawCompareTab() {
            EditorGUILayout.LabelField("Figma vs Unity Visual Comparison", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Figma Screenshot", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load from File (MCP/Saved)")) {
                LoadFigmaScreenshotFromFile();
            }
            if (GUILayout.Button("Paste Base64")) {
                LoadFigmaScreenshot();
            }
            if (GUILayout.Button("Clear", GUILayout.Width(50))) {
                ClearCompare();
            }
            EditorGUILayout.EndHorizontal();

            _screenshotBase64Input = EditorGUILayout.TextField("Or paste base64 data URI:", _screenshotBase64Input);

            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("Unity Screenshot", EditorStyles.miniLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Capture Game View")) {
                CaptureUnityScreenshot();
            }
            if (GUILayout.Button("Load from File")) {
                LoadUnityScreenshotFromFile();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Workflow:\n" +
                "1. Figma MCP: get_screenshot ile frame'in PNG'sini alın\n" +
                "2. PNG dosyasını projeye kaydedin (Assets/Screenshots/)\n" +
                "3. 'Load from File' ile yükleyin\n" +
                "4. Unity Game View'u yakalayın veya dosyadan yükleyin\n" +
                "5. Yan yana karşılaştırın",
                MessageType.None
            );

            if (_figmaScreenshot == null && _unityScreenshot == null) return;

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zoom:", GUILayout.Width(45));
            _compareZoom = EditorGUILayout.Slider(_compareZoom, 0.25f, 2f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            _compareScrollPosition = EditorGUILayout.BeginScrollView(_compareScrollPosition);
            EditorGUILayout.BeginHorizontal();

            if (_figmaScreenshot != null) {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Figma Design", EditorStyles.boldLabel);
                float w = _figmaScreenshot.width * _compareZoom;
                float h = _figmaScreenshot.height * _compareZoom;
                Rect rect = GUILayoutUtility.GetRect(w, h);
                GUI.DrawTexture(rect, _figmaScreenshot, ScaleMode.ScaleToFit);
                EditorGUILayout.LabelField($"{_figmaScreenshot.width}x{_figmaScreenshot.height}", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }

            if (_unityScreenshot != null) {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Unity Implementation", EditorStyles.boldLabel);
                float w = _unityScreenshot.width * _compareZoom;
                float h = _unityScreenshot.height * _compareZoom;
                Rect rect = GUILayoutUtility.GetRect(w, h);
                GUI.DrawTexture(rect, _unityScreenshot, ScaleMode.ScaleToFit);
                EditorGUILayout.LabelField($"{_unityScreenshot.width}x{_unityScreenshot.height}", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4);
            EditorGUILayout.BeginHorizontal();
            if (_figmaScreenshot != null && GUILayout.Button("Save Figma Screenshot")) {
                SaveTexture(_figmaScreenshot, "FigmaScreenshot");
            }
            if (_unityScreenshot != null && GUILayout.Button("Save Unity Screenshot")) {
                SaveTexture(_unityScreenshot, "UnityScreenshot");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void LoadFigmaScreenshot() {
            if (string.IsNullOrEmpty(_screenshotBase64Input)) return;

            string base64 = _screenshotBase64Input.Trim();
            if (base64.StartsWith("data:image/png;base64,")) {
                base64 = base64["data:image/png;base64,".Length..];
            }
            if (base64.StartsWith("//")) {
                int dataIndex = base64.IndexOf("data:image/png;base64,");
                if (dataIndex >= 0) {
                    base64 = base64[(dataIndex + "data:image/png;base64,".Length)..];
                }
            }

            try {
                byte[] bytes = Convert.FromBase64String(base64.Trim());
                if (_figmaScreenshot != null) DestroyImmediate(_figmaScreenshot);
                _figmaScreenshot = new Texture2D(2, 2);
                _figmaScreenshot.LoadImage(bytes);
                Debug.Log($"[FigmaImporter] Loaded Figma screenshot: {_figmaScreenshot.width}x{_figmaScreenshot.height}");
            } catch (Exception ex) {
                EditorUtility.DisplayDialog("Error", $"Failed to decode base64 image:\n{ex.Message}", "OK");
            }
        }

        private void CaptureUnityScreenshot() {
            string tempPath = Path.Combine(Application.temporaryCachePath, "figma_compare_unity.png");
            ScreenCapture.CaptureScreenshot(tempPath);
            EditorApplication.delayCall += () => {
                EditorApplication.delayCall += () => {
                    if (File.Exists(tempPath)) {
                        byte[] bytes = File.ReadAllBytes(tempPath);
                        if (_unityScreenshot != null) DestroyImmediate(_unityScreenshot);
                        _unityScreenshot = new Texture2D(2, 2);
                        _unityScreenshot.LoadImage(bytes);
                        Debug.Log($"[FigmaImporter] Captured Unity screenshot: {_unityScreenshot.width}x{_unityScreenshot.height}");
                        Repaint();
                    }
                };
            };
        }

        private void LoadUnityScreenshotFromFile() {
            string path = EditorUtility.OpenFilePanel("Select Unity Screenshot", "", "png,jpg");
            if (string.IsNullOrEmpty(path)) return;

            byte[] bytes = File.ReadAllBytes(path);
            if (_unityScreenshot != null) DestroyImmediate(_unityScreenshot);
            _unityScreenshot = new Texture2D(2, 2);
            _unityScreenshot.LoadImage(bytes);
            Repaint();
        }

        private void SaveTexture(Texture2D tex, string defaultName) {
            string path = EditorUtility.SaveFilePanel("Save Screenshot", "", defaultName, "png");
            if (string.IsNullOrEmpty(path)) return;
            File.WriteAllBytes(path, tex.EncodeToPNG());
            Debug.Log($"[FigmaImporter] Saved: {path}");
        }

        private void LoadFigmaScreenshotFromFile() {
            string path = EditorUtility.OpenFilePanel("Select Figma Screenshot", "Assets", "png,jpg");
            if (string.IsNullOrEmpty(path)) return;

            byte[] bytes = File.ReadAllBytes(path);
            if (_figmaScreenshot != null) DestroyImmediate(_figmaScreenshot);
            _figmaScreenshot = new Texture2D(2, 2);
            _figmaScreenshot.LoadImage(bytes);
            Debug.Log($"[FigmaImporter] Loaded Figma screenshot: {_figmaScreenshot.width}x{_figmaScreenshot.height} from {path}");
            Repaint();
        }

        private void ClearCompare() {
            _screenshotBase64Input = "";
            if (_figmaScreenshot != null) DestroyImmediate(_figmaScreenshot);
            if (_unityScreenshot != null) DestroyImmediate(_unityScreenshot);
            _figmaScreenshot = null;
            _unityScreenshot = null;
        }
        #endregion

        private void DrawFontStatus() {
            EditorGUILayout.LabelField("Fonts", EditorStyles.boldLabel);

            foreach (var fr in _fontResults) {
                EditorGUILayout.BeginHorizontal();

                string icon;
                GUIStyle style;
                switch (fr.status) {
                    case FontStatus.Found:
                        icon = "[OK]";
                        style = EditorStyles.label;
                        break;
                    case FontStatus.PartialMatch:
                        icon = "[PARTIAL]";
                        style = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1f, 0.8f, 0.2f) } };
                        break;
                    default:
                        icon = "[MISSING]";
                        style = new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1f, 0.4f, 0.3f) } };
                        break;
                }

                EditorGUILayout.LabelField(icon, GUILayout.Width(70));
                EditorGUILayout.LabelField(fr.searchedPattern, style);

                if (fr.foundPath != null) {
                    EditorGUILayout.LabelField(fr.foundPath, EditorStyles.miniLabel);
                } else {
                    EditorGUILayout.LabelField("Not found in Assets/Fonts/", EditorStyles.miniLabel);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        #region Import Tab
        private void DrawImportTab() {
            EditorGUILayout.LabelField("Step 1: Select Settings", EditorStyles.boldLabel);
            _settings = (FigmaImporterSettings)EditorGUILayout.ObjectField(
                "Settings Asset", _settings, typeof(FigmaImporterSettings), false
            );

            if (_settings == null) {
                EditorGUILayout.HelpBox(
                    "Create settings via: Right-click in Project > Create > BizSim > Figma Importer Settings",
                    MessageType.Info
                );
                return;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Step 2: Figma File Key", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _figmaFileKey = EditorGUILayout.TextField("File Key", _figmaFileKey);
            if (EditorGUI.EndChangeCheck()) {
                FigmaUserSettings.FigmaFileKey = _figmaFileKey;
            }
            EditorGUILayout.HelpBox(
                "From your Figma URL: figma.com/design/{FILE_KEY}/...",
                MessageType.None
            );

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Step 3: Paste Asset Manifest JSON", EditorStyles.boldLabel);
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(120));
            _manifestJson = EditorGUILayout.TextArea(_manifestJson, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Parse Manifest", GUILayout.Height(30))) {
                ParseManifest();
            }
            if (GUILayout.Button("Clear", GUILayout.Width(60), GUILayout.Height(30))) {
                _manifestJson = "";
                _manifest = null;
                _importItems = null;
                _lastResult = null;
            }
            EditorGUILayout.EndHorizontal();

            if (_fontResults != null && _fontResults.Count > 0) {
                EditorGUILayout.Space(12);
                DrawFontStatus();
            }

            if (_importItems != null && _importItems.Count > 0) {
                EditorGUILayout.Space(12);
                DrawAssetTable();
            }

            if (_isImporting) {
                EditorGUILayout.Space(8);
                EditorGUI.ProgressBar(
                    EditorGUILayout.GetControlRect(false, 20),
                    _progress,
                    _progressMessage
                );
            }

            if (_lastResult != null) {
                EditorGUILayout.Space(8);
                DrawImportResult();
            }
        }

        private void DrawAssetTable() {
            EditorGUILayout.LabelField(
                $"Assets ({_importItems.Count} found)",
                EditorStyles.boldLabel
            );

            int missing = 0, existing = 0;
            foreach (var item in _importItems) {
                if (item.status == ImportStatus.Missing) missing++;
                else existing++;
            }

            EditorGUILayout.LabelField($"Missing: {missing}  |  Existing: {existing}");
            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Select All Missing")) {
                foreach (var item in _importItems)
                    item.selected = item.status == ImportStatus.Missing;
            }
            if (GUILayout.Button("Select None")) {
                foreach (var item in _importItems)
                    item.selected = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            foreach (var item in _importItems) {
                EditorGUILayout.BeginHorizontal();

                item.selected = EditorGUILayout.Toggle(item.selected, GUILayout.Width(20));

                string statusIcon = item.status switch {
                    ImportStatus.Missing => "[MISSING]",
                    ImportStatus.Exists => "[EXISTS]",
                    ImportStatus.SizeChanged => "[CHANGED]",
                    _ => "[ERROR]"
                };

                var style = item.status == ImportStatus.Missing
                    ? new GUIStyle(EditorStyles.label) { normal = { textColor = new Color(1f, 0.6f, 0.2f) } }
                    : EditorStyles.label;

                EditorGUILayout.LabelField(statusIcon, GUILayout.Width(75));
                EditorGUILayout.LabelField(item.asset.category, GUILayout.Width(80));
                EditorGUILayout.LabelField(item.asset.name, style);
                EditorGUILayout.LabelField(item.resolvedPath, EditorStyles.miniLabel);

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(8);
            GUI.enabled = !_isImporting && !string.IsNullOrEmpty(_figmaFileKey);
            if (GUILayout.Button("Import Selected Assets", GUILayout.Height(35))) {
                ImportSelected();
            }
            GUI.enabled = true;

            if (string.IsNullOrEmpty(_figmaFileKey)) {
                EditorGUILayout.HelpBox("Enter Figma File Key above to enable import.", MessageType.Warning);
            }
            if (!FigmaUserSettings.HasValidToken) {
                EditorGUILayout.HelpBox("Configure Figma PAT in User Settings tab.", MessageType.Error);
            }
        }

        private void DrawImportResult() {
            var result = _lastResult;
            string message = $"Imported: {result.imported} | Skipped: {result.skipped} | Failed: {result.failed}";
            MessageType type = result.failed > 0 ? MessageType.Warning : MessageType.Info;
            EditorGUILayout.HelpBox(message, type);

            foreach (string error in result.errors) {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }
        }
        #endregion

        #region Settings Tabs
        private void DrawSettingsTab() {
            if (_settings == null) {
                EditorGUILayout.HelpBox("Select a settings asset in the Import tab first.", MessageType.Info);
                return;
            }

            var editor = Editor.CreateEditor(_settings);
            editor.OnInspectorGUI();
            DestroyImmediate(editor);
        }

        private void DrawUserSettingsTab() {
            EditorGUILayout.LabelField("Personal Settings (NOT saved to git)", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Figma Personal Access Token");
            string pat = EditorGUILayout.PasswordField(FigmaUserSettings.PersonalAccessToken);
            if (pat != FigmaUserSettings.PersonalAccessToken) {
                FigmaUserSettings.PersonalAccessToken = pat;
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Get your PAT from: Figma > Settings > Account > Personal access tokens\n" +
                "This token is stored in EditorPrefs (local machine only, never committed to git).",
                MessageType.Info
            );

            EditorGUILayout.Space(8);
            bool cache = EditorGUILayout.Toggle("Enable Cache", FigmaUserSettings.CacheEnabled);
            if (cache != FigmaUserSettings.CacheEnabled) {
                FigmaUserSettings.CacheEnabled = cache;
            }
        }
        #endregion

        #region Actions
        private void ParseManifest() {
            if (string.IsNullOrEmpty(_manifestJson)) return;

            _orchestrator = new FigmaImportOrchestrator(_settings);
            _manifest = _orchestrator.ParseManifest(_manifestJson);

            if (_manifest?.assets == null || _manifest.assets.Count == 0) {
                EditorUtility.DisplayDialog("Figma Importer", "No assets found in manifest.", "OK");
                return;
            }

            _importItems = _orchestrator.ScanProject(_manifest);
            _lastResult = null;

            if (_manifest.fonts != null && _manifest.fonts.Count > 0) {
                var fontScanner = new FontScanner();
                _fontResults = fontScanner.ScanFonts(_manifest.fonts);
            } else {
                _fontResults = null;
            }
        }

        private async void ImportSelected() {
            if (_orchestrator == null || _importItems == null) return;

            _isImporting = true;
            _orchestrator.OnProgressChanged += (progress, message) => {
                _progress = progress;
                _progressMessage = message;
                Repaint();
            };

            _lastResult = await _orchestrator.Import(_importItems, _figmaFileKey);
            _isImporting = false;

            if (_lastResult.imported > 0) {
                _importItems = _orchestrator.ScanProject(_manifest);
            }

            Repaint();
        }
        #endregion
    }
}
