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

        public event Action<int, int, int> ContentProcessCallback;

        public event Action<List<PackDescriptor>> OnPacksLoaded;

        public event Action<IGorillaShirt> OnShirtUnloaded;

        public event Action<PackDescriptor> OnPackUnloaded;

        private int contentProcessed, contentCount, errorCount;

        public async void LoadContent() => await LoadContent(RootLocation);

        private async Task LoadContent(string directory)
        {
            DirectoryInfo directoryInfo = new(directory);
            Logging.Message($"LoadShirts: {directoryInfo.FullName}");

            FileInfo[] files = directoryInfo.GetFiles("*.gshirt", SearchOption.AllDirectories);
            Logging.Info($"{files.Length} files: {string.Join(", ", files.Select(file => file.Name))}");
            FileInfo[] legacyFiles = directoryInfo.GetFiles("*.shirt", SearchOption.AllDirectories);
            Logging.Info($"{legacyFiles.Length} legacy files: {string.Join(", ", legacyFiles.Select(file => file.Name))}");

            contentProcessed = 0;
            contentCount = files.Length + legacyFiles.Length;
            errorCount = 0;
            ContentProcessCallback?.Invoke(contentProcessed, contentCount, errorCount);

            List<IGorillaShirt> shirts = [];
            shirts.AddRange(await LoadShirts<LegacyGorillaShirt>(legacyFiles));
            shirts.AddRange(await LoadShirts<GorillaShirt>(files));

            if (shirts.Count == 0)
            {
                await LoadDefaultContent(false);
                return;
            }

            Dictionary<string, PackDescriptor> packs = [];

            foreach (IGorillaShirt shirt in shirts)
            {
                string packName = shirt.Descriptor.PackName;

                if (!packs.TryGetValue(packName, out PackDescriptor pack))
                {
                    Logging.Info($"Created PackDescriptor: {packName}");
                    pack = ScriptableObject.CreateInstance<PackDescriptor>();
                    pack.PackName = packName;
                    pack.Shirts = [];
                    packs.Add(packName, pack);

                    foreach (ReleaseInfo info in Main.Instance.Releases)
                    {
                        List<string> names = [info.Title];
                        if (info.AlsoKnownAs is not null && info.AlsoKnownAs.Length != 0) names.AddRange(info.AlsoKnownAs);

                        if (names.Contains(pack.PackName))
                        {
                            pack.Release = info;
                            info.Pack = pack;
                            Logging.Info($"{pack.PackName} has release: {info.Title} by {info.Author} at {info.PackArchiveLink}");
                            break;
                        }
                    }
                }

                Logging.Info($"Added to {packName}: {shirt.Descriptor.ShirtName}");
                pack.Shirts.Add(shirt);
            }

            foreach (PackDescriptor pack in packs.Values)
            {
                if (pack.Release is not null)
                {
                    pack.Author = pack.Release.Author;
                    pack.Description = pack.Release.Description;
                }
                else
                {
                    pack.Description = $"{pack.PackName} is a pack containing {pack.Shirts.Count} shirts by {pack.Author}";
                    pack.Author = string.Join(", ", pack.Shirts.Select(shirt => shirt.Descriptor.Author).Distinct().OrderBy(author => author, StringComparer.CurrentCultureIgnoreCase));
                }

                int legacyShirtCount = pack.Shirts.Count(shirt => shirt is LegacyGorillaShirt);
                if (legacyShirtCount == pack.Shirts.Count) pack.AdditionalNote = "All shirts in pack were made in an earlier editor version";
                else if (legacyShirtCount > 0) pack.AdditionalNote = "Some shirts in pack were made in an earlier editor version";
            }

            Logging.Info($"ShirtLoader loaded {contentProcessed} out of {contentCount} shirts");

            OnPacksLoaded?.Invoke([.. packs.Values]);
            ContentProcessCallback = null;

            await LoadDefaultContent(true);
        }

        public async Task LoadDefaultContent(bool notInstalledExclusive)
        {
            if (Main.Instance.Releases is not null && Array.Find(Main.Instance.Releases, info => info.Title == "Default" && info.Rank == 0) is ReleaseInfo defaultRelease && (!notInstalledExclusive || defaultRelease.Pack == null))
            {
                await InstallRelease(defaultRelease, (step, progress) =>
                {
                    Logging.Info($"Default Pack Installation: {Mathf.FloorToInt(progress * 100f)}%");
                });
            }
        }

        private async Task<List<T>> LoadShirts<T>(FileInfo[] files) where T : IGorillaShirt
        {
            List<T> shirts = [];

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                if (Array.Find(Main.Instance.Shirts.Values.ToArray(), shirt => shirt.FileInfo.FullName == file.FullName) is IGorillaShirt shirt && shirt is T existingAsset && existingAsset.Bundle)
                {
                    await UnloadShirt(existingAsset, existingAsset is LegacyGorillaShirt && typeof(T) == typeof(GorillaShirt));
                }

                T asset = Activator.CreateInstance<T>();
                await asset.CreateShirt(file);

                if (asset.Descriptor is null)
                {
                    Logging.Warning($"Shirt {file.Name}: broken");
                    File.Move(file.FullName, string.Concat(file, ".broken"));

                    errorCount++;
                    Main.Instance.PlayOhNoAudio();
                }
                else
                {
                    shirts.Add(asset);
                    //Main.Instance.ShirtStand.Character.SetShirt(asset);
                }

                contentProcessed++;
                ContentProcessCallback?.Invoke(contentProcessed, contentCount, errorCount);
            }
            return shirts;
        }

        public async Task UnloadContent(PackDescriptor content)
        {
            if (content is null) throw new ArgumentNullException(nameof(content));

            contentProcessed = 0;
            contentCount = content.Shirts.Count;
            errorCount = 0;

            foreach (IGorillaShirt shirt in new List<IGorillaShirt>(content.Shirts))
            {
                try
                {
                    await UnloadShirt(shirt);
                }
                catch
                {
                    errorCount++;
                }

                contentProcessed++;
                ContentProcessCallback?.Invoke(contentProcessed, contentCount, errorCount);
            }

            OnPackUnloaded?.Invoke(content);
        }

        public async Task UnloadShirt(IGorillaShirt shirt, bool removeFile = true)
        {
            if (shirt is null) throw new ArgumentNullException(nameof(shirt));
            if (shirt.Bundle is not AssetBundle bundle || !shirt.Bundle) return;

            string directory = shirt.FileInfo.DirectoryName;

            TaskCompletionSource<object> completionSource = new();
            AssetBundleUnloadOperation unloadOperation = bundle.UnloadAsync(true);
            unloadOperation.completed += _ => completionSource.TrySetResult(null);
            await completionSource.Task;

            Logging.Info($"Unloaded shirt bundle: {shirt.ShirtId}"); // descriptor is no more
            UnityEngine.Object.Destroy(shirt.Bundle);
            OnShirtUnloaded?.Invoke(shirt);

            if (removeFile)
            {
                ThreadingHelper.Instance.StartAsyncInvoke(() =>
                {
                    try
                    {
                        File.Delete(shirt.FileInfo.FullName);
                        if (!Directory.EnumerateFileSystemEntries(directory).Any())
                        {
                            Directory.Delete(directory, false);
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {

                    }
                    catch (DirectoryNotFoundException)
                    {

                    }
                    return null;
                });
            }

        }

        public async Task UninstallRelease(ReleaseInfo info, Action<float> callback)
        {
            callback.Invoke(0);

            if (info.Pack is not PackDescriptor pack || !pack) return;

            ContentProcessCallback += (assetsLoaded, assetCount, errorCount) =>
            {
                callback.Invoke((float)assetsLoaded / assetCount);
            };

            await UnloadContent(pack);
        }

        public async Task InstallRelease(ReleaseInfo info, Action<int, float> callback, bool loadContent = true)
        {
            string link = info.PackArchiveLink;
            string archiveDestination = Path.Combine(RootLocation, $"{info.Title}.zip");
            string folderDestination = Path.Combine(RootLocation, info.Title);
            callback.Invoke(0, 0);

            UnityWebRequest request = new(link)
            {
                downloadHandler = new DownloadHandlerFile(archiveDestination)
            };

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();

            float donwloadProgress = 0f;
            while (!operation.isDone)
            {
                await Task.Delay(4);
                if (donwloadProgress != request.downloadProgress)
                {
                    donwloadProgress = request.downloadProgress;
                    callback.Invoke(0, Mathf.Clamp01(donwloadProgress));
                }
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Logging.Error($"Failed to download zip: {request.error}");
                return;
            }

            request.Dispose();

            Logging.Info($"Extracting zip file to {folderDestination}");

            using (ZipArchive archive = ZipFile.OpenRead(archiveDestination))
            {
                callback.Invoke(1, 0);

                int totalEntries = archive.Entries.Count;
                int entriesExtracted = 0;

                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string path = Path.Combine(folderDestination, entry.FullName);
                    string directory = Path.GetDirectoryName(path);

                    if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        using Stream stream = entry.Open();
                        using FileStream fileStream = File.Create(path);
                        stream.CopyTo(fileStream);
                    }

                    entriesExtracted++;
                    callback.Invoke(1, (float)entriesExtracted / totalEntries);
                    await Task.Delay(8);
                }
            }

            if (File.Exists(archiveDestination)) File.Delete(archiveDestination);

            if (!loadContent) return;

            ContentProcessCallback += (assetsLoaded, assetCount, errorCount) =>
            {
                callback.Invoke(2, (float)assetsLoaded / assetCount);
            };

            await LoadContent(folderDestination);

            List<string> names = [info.Title];
            if (info.AlsoKnownAs is not null && info.AlsoKnownAs.Length != 0) names.AddRange(info.AlsoKnownAs);

            foreach (PackDescriptor pack in Enumerable.Reverse(Main.Instance.Packs))
            {
                if (pack.Release is not null) continue;

                if (names.Contains(pack.PackName))
                {
                    pack.Release = info;
                    info.Pack = pack;
                    Logging.Info($"{pack.PackName} has release: {info.Title} by {info.Author} at {info.PackArchiveLink}");
                    break;
                }
            }
        }
    }
}
