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

        public async Task<List<Pack<IShirtAsset>>> GetAllPacks()
        {
            shirtsLoaded = 0;

            List<IShirtAsset> shirts = [];

            var directories = Directory.GetDirectories(BasePath, "*", SearchOption.AllDirectories);

            var legacy_files = Directory.GetFiles(BasePath, "*.shirt", SearchOption.TopDirectoryOnly).ToList();
            directories.ForEach(directory => legacy_files.AddRange(Directory.GetFiles(directory, "*.shirt", SearchOption.TopDirectoryOnly)));

            shirtsToLoad = legacy_files.Count;
            ShirtLoadStart?.Invoke();

            shirts.AddRange(await LoadShirts<LegacyShirtAsset>(legacy_files));

            Dictionary<string, Pack<IShirtAsset>> packs = [];

            foreach(var shirt in shirts)
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
