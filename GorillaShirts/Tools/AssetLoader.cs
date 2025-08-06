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

        private static readonly Dictionary<string, Object> loadedAssets = [];

        private static Task bundleLoadTask;

        private static readonly Dictionary<string, TaskCompletionSource<Texture2D>> textureCompletionSources = [];

        private static async Task LoadAssetBundle()
        {
            TaskCompletionSource<AssetBundle> completionSource = new();

            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream("GorillaShirts.Content.legacyshirtbundle");

            AssetBundleCreateRequest request = AssetBundle.LoadFromStreamAsync(stream);
            request.completed += _ => completionSource.SetResult(request.assetBundle);

            loadedBundle = await completionSource.Task;
            stream.Close();
        }

        public static async Task<T> LoadAsset<T>(string assetName) where T : Object
        {
            if (loadedAssets.TryGetValue(assetName, out Object asset) && asset is T) return (T)asset;

            if (loadedBundle is null)
            {
                bundleLoadTask ??= LoadAssetBundle();
                await bundleLoadTask;
            }

            TaskCompletionSource<T> completionSource = new();

            AssetBundleRequest request = loadedBundle.LoadAssetAsync<T>(assetName);
            request.completed += _ => completionSource.SetResult(request.asset is Object asset ? (T)asset : null);

            T result = await completionSource.Task;
            loadedAssets.Add(assetName, result);
            return result;
        }

        public static async Task<Texture2D> LoadTexture(string url)
        {
            if (textureCompletionSources.TryGetValue(url, out TaskCompletionSource<Texture2D> completionSource))
            {
                Texture2D completedTex = completionSource.Task.IsCompleted ? completionSource.Task.Result : await completionSource.Task;
                return completedTex;
            }

            completionSource = new();
            textureCompletionSources.Add(url, completionSource);

            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

                Texture2D texture2D = Texture2D.CreateExternalTexture(texture.width, texture.height, TextureFormat.RGB24, false, false, texture.GetNativeTexturePtr());

                completionSource.TrySetResult(texture2D);
                return texture2D;
            }

            return null;
        }
    }
}
