using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace BizSim.Unity.Figma.Importer.Editor {
    public class FigmaApiClient {
        private const string BASE_URL = "https://api.figma.com/v1";
        private const int MAX_IDS_PER_REQUEST = 50;
        private const int RATE_LIMIT_DELAY_MS = 2100;

        private readonly string _token;
        private DateTime _lastRequestTime = DateTime.MinValue;

        public FigmaApiClient(string personalAccessToken) {
            _token = personalAccessToken;
        }

        public async Task<Dictionary<string, string>> GetImageUrls(
            string fileKey,
            List<string> nodeIds,
            string format = "png",
            int scale = 2
        ) {
            var allUrls = new Dictionary<string, string>();

            for (int i = 0; i < nodeIds.Count; i += MAX_IDS_PER_REQUEST) {
                int batchSize = Math.Min(MAX_IDS_PER_REQUEST, nodeIds.Count - i);
                var batch = nodeIds.GetRange(i, batchSize);
                string ids = string.Join(",", batch);

                await EnforceRateLimit();

                string url = $"{BASE_URL}/images/{fileKey}?ids={ids}&format={format}&scale={scale}";
                string response = await SendRequest(url);

                if (string.IsNullOrEmpty(response)) continue;

                var result = JsonUtility.FromJson<FigmaImagesResponse>(response);
                if (result?.images != null) {
                    foreach (var kvp in result.images) {
                        if (!string.IsNullOrEmpty(kvp.Value)) {
                            allUrls[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            return allUrls;
        }

        public async Task<byte[]> DownloadImage(string imageUrl) {
            using var request = UnityWebRequest.Get(imageUrl);
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"[FigmaImporter] Download failed: {request.error} — {imageUrl}");
                return null;
            }

            return request.downloadHandler.data;
        }

        private async Task<string> SendRequest(string url) {
            using var request = UnityWebRequest.Get(url);
            request.SetRequestHeader("X-Figma-Token", _token);

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.LogError($"[FigmaImporter] API error: {request.responseCode} {request.error}");
                return null;
            }

            _lastRequestTime = DateTime.Now;
            return request.downloadHandler.text;
        }

        private async Task EnforceRateLimit() {
            var elapsed = (DateTime.Now - _lastRequestTime).TotalMilliseconds;
            if (elapsed < RATE_LIMIT_DELAY_MS) {
                int waitMs = (int)(RATE_LIMIT_DELAY_MS - elapsed);
                await Task.Delay(waitMs);
            }
        }

        [Serializable]
        private class FigmaImagesResponse {
            public SerializableDictionary images;
        }

        [Serializable]
        private class SerializableDictionary : Dictionary<string, string>, ISerializationCallbackReceiver {
            [SerializeField] private List<string> keys = new();
            [SerializeField] private List<string> values = new();

            public void OnBeforeSerialize() {
                keys.Clear();
                values.Clear();
                foreach (var kvp in this) {
                    keys.Add(kvp.Key);
                    values.Add(kvp.Value);
                }
            }

            public void OnAfterDeserialize() {
                Clear();
                for (int i = 0; i < keys.Count; i++) {
                    Add(keys[i], i < values.Count ? values[i] : "");
                }
            }
        }
    }
}
