using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GorillaShirts.Tools;

namespace GorillaShirts.Models
{
    public class ShirtLoader
    {
        public string BasePath;

        public event Action ShirtLoadStart;
        public event Action<(int shirtsLoaded, int shirtsToLoad)> ShirtLoadChanged;

        private int shirtsLoaded = 0;
        private int shirtsToLoad = 0;

        private readonly Dictionary<string, string> hardcodedDescriptions = new()
        {
            {
                "Default", "Default contains a variety of iconic shirts, such as hoodies, turtlenecks, croptops, and more."
            },
            {
                "Custom", "Custom contains plenty of diverse shirts assorted into a singular pack."
            }
        };

        public async Task<List<Pack<IShirtAsset>>> GetAllPacks(Action initCallback = null, Action<(int shirtsLoaded, int shirtsToLoad)> updateCallback = null)
        {
            List<IShirtAsset> shirts = [];

            var directories = Directory.GetDirectories(BasePath, "*", SearchOption.AllDirectories);

            var legacy_files = Directory.GetFiles(BasePath, "*.shirt", SearchOption.TopDirectoryOnly).ToList();
            directories.ForEach(directory => legacy_files.AddRange(Directory.GetFiles(directory, "*.shirt", SearchOption.TopDirectoryOnly)));

            shirtsLoaded = 0;
            shirtsToLoad = legacy_files.Count;

            if (initCallback != null && updateCallback != null)
            {
                ShirtLoadStart += initCallback;
                ShirtLoadChanged += updateCallback;
            }

            ShirtLoadStart?.Invoke();

            shirts.AddRange(await LoadShirts<LegacyShirtAsset>(legacy_files));

            if (initCallback != null && updateCallback != null)
            {
                ShirtLoadStart -= initCallback;
                ShirtLoadChanged -= updateCallback;
            }

            Dictionary<string, Pack<IShirtAsset>> packs = [];

            foreach (var shirt in shirts)
            {
                var pack_name = shirt.Descriptor.Pack;

                if (!packs.TryGetValue(pack_name, out var pack))
                {
                    pack = new Pack<IShirtAsset>
                    {
                        Name = pack_name,
                        Items = []
                    };
                    packs.Add(pack_name, pack);
                }

                pack.Items.Add(shirt);
            }

            foreach (var pack in packs.Values)
            {
                pack.Author = string.Join(", ", pack.Items.Select(shirt => shirt.Descriptor.Author).Distinct().OrderBy(author => author, StringComparer.CurrentCultureIgnoreCase));

                if (hardcodedDescriptions.TryGetValue(pack.Name, out string description))
                    pack.Description = description;
                else
                    pack.Description = $"{pack.Name} is a pack containing {pack.Items.Count} shirts by {pack.Author}";

                var legacy_shirt_count = pack.Items.Count(shirt => shirt.Descriptor.Version == EShirtVersion.Legacy);
                if (legacy_shirt_count == pack.Items.Count)
                    pack.Note = "All shirts in pack were made for an earlier version of GorillaShirts";
                else if (legacy_shirt_count > 0)
                    pack.Note = "Some shirts in pack were made for an earlier version of GorillaShirts";
            }

            Logging.Info($"ShirtLoader loaded {shirtsLoaded} out of {shirtsToLoad} shirts");

            return [.. packs.Values];
        }

        public async Task<List<T>> LoadShirts<T>(List<string> files) where T : IShirtAsset
        {
            List<T> shirts = [];

            for (int i = 0; i < files.Count; i++)
            {
                var file = files[i];

                var asset = Activator.CreateInstance<T>();
                var shirt = await asset.Construct(file);

                shirtsLoaded += 1;
                ShirtLoadChanged?.Invoke((shirtsLoaded, shirtsToLoad));

                if (shirt == null)
                {
                    File.Move(file, $"{file}.broken");
                    continue;
                }

                shirts.Add(asset);
            }

            shirtsLoaded += files.Count;

            return shirts;
        }
    }
}
