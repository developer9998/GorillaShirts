using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaShirts.Tools
{
    internal class AssetLoader
    {
        public static AssetBundle Bundle => is_bundle_loaded ? asset_bundle : null;

        private static bool is_bundle_loaded;
        private static AssetBundle asset_bundle;
        private static Task bundle_load_task = null;

        private static readonly Dictionary<string, object> loaded_assets = [];

        private static async Task LoadBundle()
        {
            Stream stream = typeof(Plugin).Assembly.GetManifestResourceStream("GorillaShirts.Content.legacyshirtbundle");
            var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(stream);

            var taskCompletionSource = new TaskCompletionSource<AssetBundle>();

            bundleLoadRequest.completed += operation =>
            {
                var outRequest = operation as AssetBundleCreateRequest;
                taskCompletionSource.SetResult(outRequest.assetBundle);
            };

            asset_bundle = await taskCompletionSource.Task;
            is_bundle_loaded = true;
            stream.Close();
        }

        public static async Task<T> LoadAsset<T>(string name) where T : Object
        {
            if (loaded_assets.ContainsKey(name) && loaded_assets[name] is Object _loadedObject) return _loadedObject as T;

            if (!is_bundle_loaded)
            {
                bundle_load_task ??= LoadBundle();
                await bundle_load_task;
            }

            var taskCompletionSource = new TaskCompletionSource<T>();
            var assetLoadRequest = asset_bundle.LoadAssetAsync<T>(name);

            assetLoadRequest.completed += operation =>
            {
                var outRequest = operation as AssetBundleRequest;
                if (outRequest.asset == null)
                {
                    taskCompletionSource.SetResult(null);
                    return;
                }

                taskCompletionSource.SetResult(outRequest.asset as T);
            };

            var asset = await taskCompletionSource.Task;
            loaded_assets.AddOrUpdate(name, asset);
            return asset;
        }
    }
}
