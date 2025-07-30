using BepInEx;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace GorillaShirts.Models
{
    internal class ContentHandler(string rootLocation)
    {
        public readonly string RootLocation = rootLocation;

        public event Action<int, int, int> LoadStageCallback;

        public event Action<List<PackDescriptor>> OnContentLoaded;

        private int contentLoaded, contentCount, errorCount;

        private readonly Dictionary<string, string> hardcodedDescriptions = new()
        {
            {
                "Default", "Default contains a variety of iconic shirts, such as hoodies, turtlenecks, croptops, and more."
            },
            {
                "Custom", "Custom contains plenty of diverse shirts assorted into a singular pack."
            }
        };

        public async void LoadContent() => await LoadContent(RootLocation);

        private async Task LoadContent(string directory)
        {
            DirectoryInfo directoryInfo = new(directory);
            Logging.Message($"LoadShirts: {directoryInfo.FullName}");

            FileInfo[] files = directoryInfo.GetFiles("*.gshirt", SearchOption.AllDirectories);
            Logging.Info($"{files.Length} files: {string.Join(", ", files.Select(file => file.Name))}");
            FileInfo[] legacyFiles = directoryInfo.GetFiles("*.shirt", SearchOption.AllDirectories);
            Logging.Info($"{legacyFiles.Length} legacy files: {string.Join(", ", legacyFiles.Select(file => file.Name))}");

            contentLoaded = 0;
            contentCount = files.Length + legacyFiles.Length;
            errorCount = 0;
            LoadStageCallback?.Invoke(contentLoaded, contentCount, errorCount);

            List<IGorillaShirt> shirts = [];
            shirts.AddRange(await LoadShirts<GorillaShirt>(files));
            shirts.AddRange(await LoadShirts<LegacyGorillaShirt>(legacyFiles));

            if (shirts.Count == 0)
            {
                if (Main.Instance.Releases is not null && Array.Find(Main.Instance.Releases, info => info.Title == "Default" && info.Rank == 0) is ReleaseInfo defaultRelease)
                {
                    await InstallRelease(defaultRelease);
                    // await LoadContent(directory);
                    return;
                }

                return;
            }

            Dictionary<string, PackDescriptor> packs = [];

            foreach (IGorillaShirt shirt in shirts)
            {
                string packName = shirt.Descriptor.PackName;

                if (!packs.TryGetValue(packName, out PackDescriptor pack))
                {
                    pack = ScriptableObject.CreateInstance<PackDescriptor>();
                    pack.PackName = packName;
                    pack.Shirts = [];
                    packs.Add(packName, pack);
                }

                pack.Shirts.Add(shirt);
            }

            foreach (PackDescriptor pack in packs.Values)
            {
                pack.Author = string.Join(", ", pack.Shirts.Select(shirt => shirt.Descriptor.Author).Distinct().OrderBy(author => author, StringComparer.CurrentCultureIgnoreCase));

                if (hardcodedDescriptions.TryGetValue(pack.PackName, out string description)) pack.Description = description;
                else pack.Description = $"{pack.PackName} is a pack containing {pack.Shirts.Count} shirts by {pack.Author}";

                int legacyShirtCount = pack.Shirts.Count(shirt => shirt is LegacyGorillaShirt);
                if (legacyShirtCount == pack.Shirts.Count) pack.AdditionalNote = "All shirts in pack were made in an earlier editor version";
                else if (legacyShirtCount > 0) pack.AdditionalNote = "Some shirts in pack were made in an earlier editor version";
            }

            Logging.Info($"ShirtLoader loaded {contentLoaded} out of {contentCount} shirts");

            LoadStageCallback = null;
            OnContentLoaded?.Invoke([.. packs.Values]);
        }

        private async Task<List<T>> LoadShirts<T>(FileInfo[] files) where T : IGorillaShirt
        {
            List<T> shirts = [];

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                if (Main.Instance.Shirts is var shirtDictionary && shirtDictionary.Count != 0)
                {
                    string path = file.FullName;
                    if (Array.Find(shirtDictionary.Values.ToArray(), shirt => shirt.FileInfo.FullName == path) is IGorillaShirt shirt && shirt is T existingAsset)
                    {
                        shirts.Add(existingAsset);

                        contentLoaded++;
                        LoadStageCallback?.Invoke(contentLoaded, contentCount, errorCount);
                        
                        continue;
                    }
                }

                T asset = Activator.CreateInstance<T>();
                await asset.CreateShirt(file);

                contentLoaded++;

                if (asset.Descriptor == null)
                {
                    Logging.Warning($"Shirt {file.Name}: broken");
                    File.Move(file.FullName, string.Concat(file, ".broken"));

                    errorCount++;
                    LoadStageCallback?.Invoke(contentLoaded, contentCount, errorCount);
                    continue;
                }

                LoadStageCallback?.Invoke(contentLoaded, contentCount, errorCount);
                shirts.Add(asset);
            }

            LoadStageCallback?.Invoke(contentLoaded, contentCount, errorCount);

            return shirts;
        }

        public async Task InstallRelease(ReleaseInfo release)
        {
            string archiveDestination = Path.Combine(RootLocation, $"{release.Title}.zip");
            string folderDestination = Path.Combine(RootLocation, release.Title);
            await InstallArchive(release.Link, archiveDestination, folderDestination);
            await LoadContent(folderDestination);
        }

        private async Task InstallArchive(string link, string zipPath, string extractPath)
        {
            Logging.Message($"ContentHandler installing archive");
            Logging.Info(link);

            UnityWebRequest request = new(link)
            {
                downloadHandler = new DownloadHandlerFile(zipPath)
            };

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            await operation;

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to download zip: {request.error}");
                return;
            }
            request.Dispose();

            Logging.Info($"Extracting zip file to {extractPath}");

            TaskCompletionSource<object> completionSource = new();

            ThreadingHelper.Instance.StartAsyncInvoke(() =>
            {
                try
                {
                    ZipFile.ExtractToDirectory(zipPath, extractPath);
                }
                catch (Exception)
                {
                    return null;
                }

                if (File.Exists(zipPath)) File.Delete(zipPath);
                completionSource.TrySetResult(null);

                return null;
            });

            await completionSource.Task;
        }
    }
}
