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
            // Check our current directory
            await FindShirtsFromPackDirectory(myDirectory);

            var shirtPackDirectories = Directory.GetDirectories(myDirectory, "*", SearchOption.AllDirectories);
            if (shirtPackDirectories.Length > 0)
            {
                shirtPackDirectories = shirtPackDirectories.OrderBy(a => Path.GetFileName(a) == "Default Shirts" ? 0 : 1).ToArray();
                foreach (var folderDirectory in shirtPackDirectories)
                {
                    // Check this new directory
                    await FindShirtsFromPackDirectory(folderDirectory);
                }
            }

            return _packDictionary.Values.ToList();
        }


        private async Task FindShirtsFromPackDirectory(string path)
        {
            // Check to see if a directory exists at our current path
            if (!Directory.Exists(path)) return;

            var dirInfo = new DirectoryInfo(path);
            FileInfo[] fileInfoArray = dirInfo.GetFiles("*.shirt");

            // Check to see if this directory has any shirt files
            if (fileInfoArray.Length == 0) return;

            Pack currentPack = null;
            foreach (var fileInfo in fileInfoArray)
            {
                string fileDirectory = Path.GetFileNameWithoutExtension(fileInfo.Name);
                string filePath = Path.Combine(path, fileInfo.Name);

                AssetBundle shirtResourceBundle = null;
                ShirtJSON shirtDataJSON = null;

                using var archive = ZipFile.OpenRead(filePath);
                try
                {
                    var packageEntry = archive.Entries.FirstOrDefault(i => i.Name == "ShirtData.json");
                    if (packageEntry == null) continue;

                    using var stream = new StreamReader(packageEntry.Open(), Encoding.UTF8);

                    string packageReadContents = await stream.ReadToEndAsync();
                    shirtDataJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<ShirtJSON>(packageReadContents);

                    var shirtResourceEntry = archive.Entries.FirstOrDefault(i => i.Name == shirtDataJSON.assetName);
                    if (shirtResourceEntry == null) continue;

                    using var SeekableStream = new MemoryStream();
                    await shirtResourceEntry.Open().CopyToAsync(SeekableStream);
                    shirtResourceBundle = await LoadFromStream(SeekableStream);
                }
                catch (Exception ex)
                {
                    Logging.Error("Failed to load archive from path " + filePath + ": " + ex.ToString());
                    continue;
                }

                Shirt newShirt = new(string.Concat(shirtDataJSON.packName, "/", shirtDataJSON.infoDescriptor.shirtName), shirtDataJSON.infoDescriptor.shirtName, fileDirectory);
                ShirtPair newPair = new(newShirt, shirtDataJSON);

                newShirt.Pair = newPair;
                newShirt.Author = shirtDataJSON.infoDescriptor.shirtAuthor;
                newShirt.Description = shirtDataJSON.infoDescriptor.shirtDescription;
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
                        Sector newSector = new();
                        newSector.Object = tempSector.gameObject;
                        newSector.Type = sectorType;
                        newSector.Position = tempSector.localPosition;
                        newSector.Euler = tempSector.localEulerAngles;
                        newSector.Scale = tempSector.localScale;
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
