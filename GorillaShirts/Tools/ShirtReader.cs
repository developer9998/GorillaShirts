/*

using BoingKit;
using GorillaExtensions;
using GorillaNetworking;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Extensions;
using GorillaShirts.Models;
using GorillaShirts.Utilities;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using static BoingKit.BoingBones;
using Object = UnityEngine.Object;

namespace GorillaShirts.Tools
{
    public class ShirtReader
    {
        public event Action ShirtLoadStart;
        public event Action<(int shirtsLoaded, int shirtsToLoad)> ShirtLoadChanged;

        private int shirtsLoaded = 0;
        private int shirtsToLoad = 0;

        public AssetLoader AssetLoader;
        private Material _furMaterial;

        private readonly Dictionary<string, Pack> Ref_CreatedPacks = [];
        

        public async void Initialize()
        {
            Texture2D furTexture = await AssetLoader.LoadAsset<Texture2D>("lightfur");
            Shader uberShader = Shader.Find("GorillaTag/UberShader");

            string[] keywords = (GorillaTagger.Instance.offlineVRRig && GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance) ? GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance.shaderKeywords : ["_USE_TEXTURE", "_ENVIRONMENTREFLECTIONS_OFF", "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF"];

            _furMaterial = new Material(uberShader)
            {
                mainTexture = furTexture,
                shaderKeywords = keywords,
                enabledKeywords = [.. keywords.Select(keyword => new LocalKeyword(uberShader, keyword))]
            };
        }

        public FileInfo[] GetShirtInfo(string path)
        {
            FileInfo[] fileInfo = [];
            if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new(path);
                fileInfo = directoryInfo.GetFiles("*.shirt");
            }
            return fileInfo;
        }

        public async Task<List<Pack>> FindShirtsFromDirectory(string directoryPath, bool packCountCheck = true)
        {
            Logging.Info($"FindShirtsFromDirectory (directory name {Path.GetDirectoryName(directoryPath)} packCountCheck is {packCountCheck})");

            Ref_CreatedPacks.Clear();

            var baseDirectoryFiles = GetShirtInfo(directoryPath);
            bool hasFiles = baseDirectoryFiles.Length > 0;
            Dictionary<string, FileInfo[]> subDirectoryFiles = [];

            var directory_list = Directory.GetDirectories(directoryPath, "*", SearchOption.AllDirectories);
            foreach(var directory in directory_list)
            {
                subDirectoryFiles.Add(directory, GetShirtInfo(directory));
                hasFiles = hasFiles || subDirectoryFiles[directory].Length > 0;
            }

            int packCount = Ref_CreatedPacks.Count;
            if (packCountCheck && !hasFiles)
            {
                Logging.Warning($"no packs, downloading defaults now");
                string zipPath = directoryPath + "/DefaultGorillaShirts.zip";

                if (!File.Exists(zipPath))
                {
                    Logging.Warning("getting defaults zip");

                    UnityWebRequest request = new("https://github.com/developer9998/GorillaShirts/raw/refs/heads/main/DefaultGorillaShirts.zip");
                    request.downloadHandler = new DownloadHandlerFile(zipPath);

                    UnityWebRequestAsyncOperation operation = request.SendWebRequest();
                    await TaskYieldUtils.Yield(operation);

                    if ((int)request.result > 1) // this includes only errors
                    {
                        Logging.Error($"error when downloading default shirts: {request.error}");
                        return null;
                    }

                    Logging.Info("got defaults !!");
                    request.Dispose();
                }

                Logging.Info("extracting zips");
                ZipFile.ExtractToDirectory(zipPath, directoryPath);
                File.Delete(zipPath);
                return await FindShirtsFromDirectory(directoryPath, false);
            }

            shirtsLoaded = 0;
            shirtsToLoad = baseDirectoryFiles.Length + subDirectoryFiles.Values.Sum(files => files.Length);

            ShirtLoadStart?.Invoke();

            await FindShirtsFromPackDirectory(directoryPath, baseDirectoryFiles);

            foreach (var directory in directory_list)
            {
                Logging.Info($"Locating shirt files from directory '{Path.GetFileName(directory)}'");
                await FindShirtsFromPackDirectory(directory, subDirectoryFiles[directory]);
            }

            return [.. Ref_CreatedPacks.Values];
        }

        private async Task FindShirtsFromPackDirectory(string path, FileInfo[] fileInfo)
        {
            if (fileInfo.Length > 0)
            {
                int shirtsLoadedFromPack = 0;

                Pack currentPack = null;

                foreach (var file in fileInfo)
                {
                    string fileDirectory = Path.GetFileNameWithoutExtension(file.Name);
                    string filePath = Path.Combine(path, file.Name);

                    AssetBundle shirtResourceBundle = null;
                    ShirtJSON shirtDataJSON = null;

                    Logging.Info($"Opening file '{Path.GetFileName(filePath)}'");
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
                        Logging.Warning($"Failed to parse file '{Path.GetFileName(filePath)}' as a shirt for the mod: {ex}");
                        File.Move(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".broken"));
                        continue;
                    }

                    try
                    {
                        Shirt newShirt = new(string.Concat(shirtDataJSON.packName, "/", shirtDataJSON.infoDescriptor.shirtName), shirtDataJSON.infoDescriptor.shirtName, fileDirectory);

                        newShirt.Author = shirtDataJSON.infoDescriptor.shirtAuthor;
                        newShirt.Description = shirtDataJSON.infoDescriptor.shirtDescription;

                        newShirt.ImportedAsset = await LoadAsset<GameObject>(shirtResourceBundle, "ExportShirt");
                        shirtResourceBundle.Unload(false);

                        newShirt.CustomColor = shirtDataJSON.infoConfig.customColors;
                        newShirt.HasAudio = newShirt.ImportedAsset.GetComponentInChildren<AudioSource>() != null;
                        newShirt.HasLight = newShirt.ImportedAsset.GetComponentInChildren<Light>() != null;
                        newShirt.HasParticles = newShirt.ImportedAsset.GetComponentInChildren<ParticleSystem>() != null;
                        newShirt.Invisibility = shirtDataJSON.infoConfig.invisibility;

                        Transform WearOverride = newShirt.ImportedAsset.transform.Find("OverrideWearClip"), RemoveOverride = newShirt.ImportedAsset.transform.Find("OverrideRemoveClip");
                        if (WearOverride) newShirt.Wear = WearOverride.GetComponent<AudioSource>().clip;
                        if (RemoveOverride) newShirt.Remove = RemoveOverride.GetComponent<AudioSource>().clip;

                        void PrepareSector(string sectorName, SectorType sectorType)
                        {
                            Transform tempSector = newShirt.ImportedAsset.transform.Find(sectorName);
                            if (tempSector != null)
                            {
                                tempSector.gameObject.SetActive(false);
                                Sector newSector = new()
                                {
                                    Object = tempSector.gameObject,
                                    Type = sectorType,
                                    Position = tempSector.localPosition,
                                    Euler = tempSector.localEulerAngles,
                                    Scale = tempSector.localScale
                                };
                                newShirt.SectorList.Add(newSector);

                                ShirtVisual visualParent = newSector.Object.AddComponent<ShirtVisual>();
                                List<Transform> bones = new(), ignoreBones = new();
                                foreach (var itemObject in newSector.Object.GetComponentsInChildren<Transform>(true))
                                {
                                    bool colourSupport = itemObject.GetComponent<Renderer>();
                                    bool customColour = shirtDataJSON.infoConfig.customColors;
                                    if (itemObject.childCount > 0)
                                    {
                                        for (int i = 0; i < itemObject.childCount; i++)
                                        {
                                            Transform child = itemObject.GetChild(i);
                                            if (child.name == "Wobble0")
                                            {
                                                bones.Add(itemObject);
                                            }
                                            else if (child.name == "Wobble1")
                                            {
                                                ignoreBones.Add(itemObject);
                                            }
                                            if (child.name.StartsWith("G_Fur") && colourSupport)
                                            {
                                                GorillaFur gorillaFur = itemObject.gameObject.GetOrAddComponent<GorillaFur>();
                                                gorillaFur.BaseFurMaterial = _furMaterial;
                                                gorillaFur.ShirtVisual = visualParent;
                                            }
                                            if (child.name.StartsWith("G_BB"))
                                            {
                                                newShirt.Billboard = true;
                                                itemObject.gameObject.GetOrAddComponent<Billboard>();
                                            }
                                        }
                                    }
                                    bool isFur = itemObject.GetComponent<GorillaFur>();
                                    if (colourSupport && customColour && !isFur && itemObject.GetComponent<Renderer>().material.HasProperty("_BaseColor"))
                                    {
                                        itemObject.gameObject.AddComponent<GorillaColour>().ShirtVisual = visualParent;
                                    }
                                }

                                if (bones.Count > 0)
                                {
                                    BoingBones boneComponent = newSector.Object.AddComponent<BoingBones>();
                                    boneComponent.LockTranslationX = shirtDataJSON.infoConfig.wobbleLockHorizontal;
                                    boneComponent.LockTranslationY = shirtDataJSON.infoConfig.wobbleLockVertical;
                                    boneComponent.LockTranslationZ = shirtDataJSON.infoConfig.wobbleLockHorizontal;

                                    List<Chain> boneChainList = new();
                                    foreach (var bone in bones)
                                    {
                                        Chain chain = !shirtDataJSON.infoConfig.wobbleLoose ? new()
                                        {
                                            Root = bone,
                                            Exclusion = ignoreBones.ToArray(),
                                            PoseStiffnessCurveType = Chain.CurveType.Custom,
                                            PoseStiffnessCustomCurve = AnimationCurve.Linear(0f, 0.9f, 1f, 0.7f),
                                            BendAngleCapCurveType = Chain.CurveType.Custom,
                                            BendAngleCapCustomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 0.06f),
                                            SquashAndStretchCurveType = Chain.CurveType.ConstantZero,
                                            LengthStiffnessCurveType = Chain.CurveType.ConstantOne,
                                            ParamsOverride = new()
                                            {
                                                Params = boneComponent.Params
                                            },
                                            LooseRoot = !shirtDataJSON.infoConfig.wobbleLockRoot
                                        } : new()
                                        {
                                            Root = bone,
                                            Exclusion = ignoreBones.ToArray(),
                                            SquashAndStretchCurveType = Chain.CurveType.ConstantZero,
                                            LengthStiffnessCurveType = Chain.CurveType.ConstantOne,
                                            ParamsOverride = new()
                                            {
                                                Params = boneComponent.Params
                                            },
                                            LooseRoot = !shirtDataJSON.infoConfig.wobbleLockRoot
                                        };
                                        boneChainList.Add(chain);
                                    }

                                    newShirt.Wobble = true;
                                    boneComponent.BoneChains = boneChainList.ToArray();
                                }
                            }
                        }
                        Ref_SectorDict.Do(pair => PrepareSector(pair.Key, pair.Value));

                        if (Ref_CreatedPacks.TryGetValue(shirtDataJSON.packName, out Pack pack))
                        {
                            currentPack = pack;
                        }
                        else
                        {
                            currentPack = new()
                            {
                                Name = shirtDataJSON.packName,
                                DisplayName = shirtDataJSON.packName.NicknameFormat()
                            };
                        }

                        if (!Ref_CreatedPacks.ContainsKey(shirtDataJSON.packName)) Ref_CreatedPacks.Add(shirtDataJSON.packName, currentPack);

                        currentPack.PackagedShirts.Add(newShirt);
                        currentPack.ShirtNameDictionary.AddOrUpdate(newShirt.Name, newShirt);

                        shirtsLoadedFromPack++;
                        ShirtLoadChanged?.Invoke((shirtsLoaded + shirtsLoadedFromPack, shirtsToLoad));
                    }
                    catch (Exception ex)
                    {
                        Logging.Warning($"Exception thrown when loading shirt {Path.GetFileName(filePath)}: {ex}");
                        File.Move(filePath, Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + ".broken"));
                        continue;
                    }
                }

                if (currentPack.Name == "Default")
                {
                    var random = new System.Random();
                    currentPack.PackagedShirts = [.. currentPack.PackagedShirts.OrderBy(a => random.Next())];
                }

                shirtsLoaded += shirtsLoadedFromPack;
                ShirtLoadChanged?.Invoke((shirtsLoaded, shirtsToLoad));
            }
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

*/