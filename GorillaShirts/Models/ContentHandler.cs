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
    internal class ContentHandler(string directory)
    {
        public event Action<int, int> LoadStageCallback;

        private readonly string directory = directory;

        private int contentLoaded, contentCount;

        private readonly Dictionary<string, string> hardcodedDescriptions = new()
        {
            {
                "Default", "Default contains a variety of iconic shirts, such as hoodies, turtlenecks, croptops, and more."
            },
            {
                "Custom", "Custom contains plenty of diverse shirts assorted into a singular pack."
            }
        };

        public async Task<List<PackDescriptor>> LoadContent()
        {
            DirectoryInfo directoryInfo = new(directory);
            Logging.Message($"LoadShirts: {directoryInfo.FullName}");

            FileInfo[] files = directoryInfo.GetFiles("*.gshirt*", SearchOption.AllDirectories);
            Logging.Info($"{files.Length} files: {string.Join(", ", files.Select(file => file.Name))}");
            FileInfo[] legacyFiles = directoryInfo.GetFiles("*.shirt*", SearchOption.AllDirectories);
            Logging.Info($"{legacyFiles.Length} legacy files: {string.Join(", ", legacyFiles.Select(file => file.Name))}");

            contentLoaded = 0;
            contentCount = files.Length + legacyFiles.Length;
            LoadStageCallback?.Invoke(contentLoaded, contentCount);

            List<IGorillaShirt> shirts = [];
            shirts.AddRange(await LoadShirts<GorillaShirt>(files));
            shirts.AddRange(await LoadShirts<LegacyGorillaShirt>(legacyFiles));

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

                if (hardcodedDescriptions.TryGetValue(pack.PackName, out string description))
                    pack.Description = description;
                else
                    pack.Description = $"{pack.PackName} is a pack containing {pack.Shirts.Count} shirts by {pack.Author}";

                int legacyShirtCount = pack.Shirts.Count(shirt => shirt is LegacyGorillaShirt);
                if (legacyShirtCount == pack.Shirts.Count) pack.AdditionalNote = "All shirts in pack were made for an earlier version of GorillaShirts";
                else if (legacyShirtCount > 0) pack.AdditionalNote = "Some shirts in pack were made for an earlier version of GorillaShirts";
            }

            Logging.Info($"ShirtLoader loaded {contentLoaded} out of {contentCount} shirts");

            LoadStageCallback = null;

            return [.. packs.Values];
        }

        private async Task<List<T>> LoadShirts<T>(FileInfo[] files) where T : IGorillaShirt
        {
            List<T> shirts = [];

            for (int i = 0; i < files.Length; i++)
            {
                FileInfo file = files[i];

                T asset = Activator.CreateInstance<T>();
                await asset.CreateShirt(file);

                contentLoaded++;
                LoadStageCallback?.Invoke(contentLoaded, contentCount);

                if (asset.Descriptor == null)
                {
                    Logging.Warning($"Shirt: {file.Name} is broken");
                    File.Move(file.FullName, string.Concat(file, ".broken"));
                    continue;
                }

                shirts.Add(asset);
            }

            LoadStageCallback?.Invoke(contentLoaded, contentCount);

            return shirts;
        }

        public async Task DownloadZip(string url, string zipPath, string extractPath)
        {
            Logging.Info($"Downloading zip file at {url}");

            UnityWebRequest request = new(url)
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

            ZipFile.ExtractToDirectory(zipPath, extractPath);
            File.Delete(zipPath);
        }
    }
}
