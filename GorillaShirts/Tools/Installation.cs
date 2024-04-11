using BoingKit;
using GorillaExtensions;
using GorillaNetworking;
using GorillaShirts.Behaviours.Visuals;
using GorillaShirts.Extensions;
using GorillaShirts.Models;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using static BoingKit.BoingBones;
using Object = UnityEngine.Object;

namespace GorillaShirts.Tools
{
    public class Installation : IInitializable
    {
        private AssetLoader _assetLoader;
        private Material _furMaterial;

        private readonly Dictionary<string, Pack> Ref_CreatedPacks = new();
        private readonly Dictionary<string, SectorType> Ref_SectorDict = new()
        {
            { "BodyObject", SectorType.Body         },
            { "HeadObject", SectorType.Head         },
            { "LUpperArm", SectorType.LeftUpper     },
            { "LLowerArm", SectorType.LeftLower     },
            { "LHand", SectorType.LeftHand          },
            { "RUpperArm", SectorType.RightUpper    },
            { "RLowerArm", SectorType.RightLower    },
            { "RHand", SectorType.RightHand         },
        };

        [Inject]
        public void Construct(AssetLoader assetLoader)
        {
            _assetLoader = assetLoader;
        }

        public async void Initialize()
        {
            Texture2D furTexture = await _assetLoader.LoadAsset<Texture2D>("lightfur");
            Shader uberShader = Shader.Find("GorillaTag/UberShader");

            _furMaterial = new Material(uberShader)
            {
                mainTexture = furTexture,
                shaderKeywords = new string[] { "_USE_TEXTURE", "_ENVIRONMENTREFLECTIONS_OFF", "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF" },
                enabledKeywords = new UnityEngine.Rendering.LocalKeyword[] { new(uberShader, "_USE_TEXTURE") }
            };
        }

        public void TryCreateDirectory(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }

        public async Task<List<Pack>> FindShirtsFromDirectory(string myDirectory)
        {
            Ref_CreatedPacks.Clear();

            await FindShirtsFromPackDirectory(myDirectory);

            var shirtPackDirectories = Directory.GetDirectories(myDirectory, "*", SearchOption.AllDirectories);
            foreach (var directory in shirtPackDirectories)
            {
                Logging.Info($"Locating shirt files from directory '{Path.GetFileName(directory)}'");
                await FindShirtsFromPackDirectory(directory);
            }

            return Ref_CreatedPacks.Values.ToList();
        }

        private async Task FindShirtsFromPackDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            FileInfo[] fileInfos = directoryInfo.GetFiles("*.shirt");
            if (fileInfos.Length > 0)
            {
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

                    Transform WearOverride = newShirt.RawAsset.transform.Find("OverrideWearClip"), RemoveOverride = newShirt.RawAsset.transform.Find("OverrideRemoveClip");
                    if (WearOverride) newShirt.Wear = WearOverride.GetComponent<AudioSource>().clip;
                    if (RemoveOverride) newShirt.Remove = RemoveOverride.GetComponent<AudioSource>().clip;

                    void PrepareSector(string sectorName, SectorType sectorType)
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

                            VisualParent visualParent = newSector.Object.AddComponent<VisualParent>();
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
                                            gorillaFur.Fur_VisualParent = visualParent;
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
                                    itemObject.gameObject.AddComponent<GorillaColour>().Ref_VisualParent = visualParent;
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

                    currentPack = Ref_CreatedPacks.ContainsKey(shirtDataJSON.packName) ? Ref_CreatedPacks[shirtDataJSON.packName] : new()
                    {
                        Name = shirtDataJSON.packName,
                        DisplayName = shirtDataJSON.packName.NicknameFormat()
                    };

                    if (!Ref_CreatedPacks.ContainsKey(shirtDataJSON.packName)) Ref_CreatedPacks.Add(shirtDataJSON.packName, currentPack);

                    currentPack.PackagedShirts.Add(newShirt);
                    currentPack.ShirtNameDictionary.AddOrUpdate(newShirt.Name, newShirt);
                    Logging.Info($" > Completed, '{newShirt.DisplayName}' is included in pack '{shirtDataJSON.packName}'");
                }

                var random = new System.Random();
                currentPack.PackagedShirts = currentPack.Name == "Default" ? currentPack.PackagedShirts.OrderBy(a => random.Next()).ToList() : currentPack.PackagedShirts;
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
