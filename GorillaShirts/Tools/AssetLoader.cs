using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace GorillaShirts.Tools
{
    internal class AssetLoader
    {
        private static AssetBundle loadedBundle;

        private static Task bundleLoadTask;

        private static readonly Dictionary<string, TaskCompletionSource<Object>> assetCache = [];

        private static readonly Dictionary<string, TaskCompletionSource<Texture2D>> textureCache = [];

        private static async Task LoadAssetBundle()
        {
            TaskCompletionSource<AssetBundle> completionSource = new();

            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream(Constants.AssetBundleName);

            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
            request.completed += _ => completionSource.SetResult(request.assetBundle);

            loadedBundle = await completionSource.Task;
            stream.Close();
        }

        public static async Task<T> LoadAsset<T>(string assetName) where T : Object
        {
            Logging.Message("LoadAsset");
            Logging.Info($"{assetName} of {typeof(T).FullName}");

            if (assetCache.TryGetValue(assetName, out TaskCompletionSource<Object> completionSource))
            {
                Object completedAsset = completionSource.Task.IsCompleted ? completionSource.Task.Result : await completionSource.Task;
                return (T)completedAsset;
            }

            completionSource = new();
            assetCache.Add(assetName, completionSource);

            if (loadedBundle is null)
            {
                bundleLoadTask ??= LoadAssetBundle();
                await bundleLoadTask;
            }

            AssetBundleRequest request = loadedBundle.LoadAssetAsync<T>(assetName);
            request.completed += _ => completionSource.TrySetResult(request.asset);

            Object result = await completionSource.Task;
            return (T)result;
        }

        public static async Task<Texture2D> LoadTexture(string url)
        {
            Logging.Message("LoadTexture");
            Logging.Info(url);

            if (textureCache.TryGetValue(url, out TaskCompletionSource<Texture2D> completionSource))
            {
                Texture2D texture = completionSource.Task.IsCompleted ? completionSource.Task.Result : await completionSource.Task;
                return texture;
            }

            completionSource = new();
            textureCache.Add(url, completionSource);

            using UnityWebRequest request = UnityWebRequest.Get(url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = new(2, 2, TextureFormat.RGB24, false);
                texture.LoadImage(request.downloadHandler.data);
                completionSource.TrySetResult(texture);
                return texture;
            }

            Logging.Fatal($"Result for web request: {request.result}");
            Logging.Error(request.downloadHandler.error);

            return null;
        }
    }
}
