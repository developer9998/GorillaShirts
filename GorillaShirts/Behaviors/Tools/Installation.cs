using GorillaNetworking;
using GorillaShirts.Behaviors.Data;
using GorillaShirts.Behaviors.Editor;
using GorillaShirts.Behaviors.Visuals;
using GorillaShirts.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GorillaShirts.Behaviors.Tools
{
    public class Installation
    {
        private readonly Dictionary<string, Pack> _packDictionary = new();

        public async Task<List<Pack>> FindShirtsFromDirectory(string myDirectory)
        {
            await FindShirtsFromPackDirectory(myDirectory);

            var shirtPackDirectories = Directory.GetDirectories(myDirectory, "*", SearchOption.AllDirectories);
            foreach (var directory in shirtPackDirectories)
            {
                Logging.Info($"Locating shirt files from directory '{Path.GetFileName(directory)}'");
                await FindShirtsFromPackDirectory(directory);
            }

            return _packDictionary.Values.ToList();
        }

        public void TryCreateDirectory(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }


        private async Task FindShirtsFromPackDirectory(string path)
        {
            if (!Directory.Exists(path)) return;
            var directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles("*.shirt");
            if (fileInfos.Length == 0) return;

            Pack currentPack = null;
            foreach (var fileInfo in fileInfos)
            {
                string fileDirectory = Path.GetFileNameWithoutExtension(fileInfo.Name);
                string filePath = Path.Combine(path, fileInfo.Name);

                AssetBundle shirtResourceBundle = null;
                ShirtJSON shirtDataJSON = null;

                Logging.Info($"Opening file '{Path.GetFileName(filePath)}'");
                using var archive = ZipFile.OpenRead(filePath);
                try
                {
                    var packageEntry = archive.Entries.FirstOrDefault(i => i.Name == "ShirtData.json");
                    if (packageEntry == null) continue;

                    Logging.Info(" > Reading entry");
                    using var stream = new StreamReader(packageEntry.Open(), Encoding.UTF8);

                    string packageReadContents = await stream.ReadToEndAsync();
                    shirtDataJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<ShirtJSON>(packageReadContents);

                    Logging.Info(" > Deserializing contents");
                    var shirtResourceEntry = archive.Entries.FirstOrDefault(i => i.Name == shirtDataJSON.assetName);
                    if (shirtResourceEntry == null) continue;

                    using var SeekableStream = new MemoryStream();
                    await shirtResourceEntry.Open().CopyToAsync(SeekableStream);

                    Logging.Info(" > Loading resource bundle");
                    shirtResourceBundle = await LoadFromStream(SeekableStream);
                }
                catch (Exception ex)
                {
                    Logging.Warning($"Failed to parse file '{Path.GetFileName(filePath)}' as a shirt for the mod: {ex}");
                    continue;
                }

                Shirt newShirt = new(string.Concat(shirtDataJSON.packName, "/", shirtDataJSON.infoDescriptor.shirtName), shirtDataJSON.infoDescriptor.shirtName, fileDirectory);
                ShirtPair newPair = new(newShirt, shirtDataJSON);

                newShirt.Pair = newPair;
                newShirt.Author = shirtDataJSON.infoDescriptor.shirtAuthor;
                newShirt.Description = shirtDataJSON.infoDescriptor.shirtDescription;

                Logging.Info(" > Loading shirt asset");
                newShirt.RawAsset = await LoadAsset<GameObject>(shirtResourceBundle, "ExportShirt");
                shirtResourceBundle.Unload(false);

                newShirt.CustomColor = shirtDataJSON.infoConfig.customColors;
                newShirt.HasAudio = newShirt.RawAsset.GetComponentInChildren<AudioSource>() != null;
                newShirt.HasLight = newShirt.RawAsset.GetComponentInChildren<Light>() != null;
                newShirt.HasParticles = newShirt.RawAsset.GetComponentInChildren<ParticleSystem>() != null;
                newShirt.Invisibility = shirtDataJSON.infoConfig.invisibility;

                void CreateShirtSector(string sectorName, SectorType sectorType)
                {
                    Transform tempSector = newShirt.RawAsset.transform.Find(sectorName);
                    if (tempSector != null)
                    {
                        Sector newSector = new()
                        {
                            Object = tempSector.gameObject,
                            Type = sectorType,
                            Position = tempSector.localPosition,
                            Euler = tempSector.localEulerAngles,
                            Scale = tempSector.localScale
                        };
                        newShirt.SectorList.Add(newSector);

                        var sectorVP = newSector.Object.AddComponent<VisualParent>();
                        foreach (var itemObject in newSector.Object.GetComponentsInChildren<Transform>(false))
                        {
                            if (itemObject.transform.GetComponentsInChildren<Transform>().FirstOrDefault(a => a.name.StartsWith("G_Fur")) != null && itemObject.GetComponent<Renderer>() != null)
                            {
                                itemObject.gameObject.AddComponent<GorillaFur>()._visualParent = sectorVP;
                                continue;
                            }

                            if (itemObject.GetComponent<Renderer>() != null && itemObject.GetComponent<Renderer>().material.HasProperty("_BaseColor"))
                            {
                                if (!shirtDataJSON.infoConfig.customColors) continue;
                                itemObject.gameObject.AddComponent<GorillaColour>()._visualParent = sectorVP;
                            }
                        }
                    }
                }

                CreateShirtSector("BodyObject", SectorType.Body);
                CreateShirtSector("HeadObject", SectorType.Head);
                CreateShirtSector("LUpperArm", SectorType.LeftUpper);
                CreateShirtSector("LLowerArm", SectorType.LeftLower);
                CreateShirtSector("LHand", SectorType.LeftHand);
                CreateShirtSector("RUpperArm", SectorType.RightUpper);
                CreateShirtSector("RLowerArm", SectorType.RightLower);
                CreateShirtSector("RHand", SectorType.RightHand);

                currentPack = _packDictionary.ContainsKey(shirtDataJSON.packName) ? _packDictionary[shirtDataJSON.packName] : new()
                {
                    Name = shirtDataJSON.packName,
                    DisplayName = shirtDataJSON.packName.NicknameFormat()
                };

                if (!_packDictionary.ContainsKey(shirtDataJSON.packName)) _packDictionary.Add(shirtDataJSON.packName, currentPack);

                currentPack.PackagedShirts.Add(newShirt);
                currentPack.ShirtNameDictionary.AddOrUpdate(newShirt.Name, newShirt);
                Logging.Info($" > Completed, '{newShirt.DisplayName}' is included in pack '{shirtDataJSON.packName}'");
            }

            var random = new System.Random();
            currentPack.PackagedShirts = currentPack.Name == "Default" ? currentPack.PackagedShirts.OrderBy(a => random.Next()).ToList() : currentPack.PackagedShirts;
        }

        private static async Task<AssetBundle> LoadFromStream(Stream str)
        {
            var taskCompletionSource = new TaskCompletionSource<AssetBundle>();
            var request = AssetBundle.LoadFromStreamAsync(str);
            request.completed += operation =>
            {
                var outRequest = operation as AssetBundleCreateRequest;
                taskCompletionSource.SetResult(outRequest.assetBundle);
            };
            return await taskCompletionSource.Task;
        }

        private static async Task<T> LoadAsset<T>(AssetBundle bundle, string name) where T : Object
        {
            var taskCompletionSource = new TaskCompletionSource<T>();
            var request = bundle.LoadAssetAsync<T>(name);
            request.completed += operation =>
            {
                var outRequest = operation as AssetBundleRequest;
                if (outRequest.asset == null)
                {
                    taskCompletionSource.SetResult(null);
                    return;
                }

                taskCompletionSource.SetResult(outRequest.asset as T);
            };
            return await taskCompletionSource.Task;
        }
    }
}
