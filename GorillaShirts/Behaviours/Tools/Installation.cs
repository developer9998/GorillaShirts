using BoingKit;
using GorillaExtensions;
using GorillaNetworking;
using GorillaShirts.Behaviours.Data;
using GorillaShirts.Behaviours.Editor;
using GorillaShirts.Behaviours.Visuals;
using GorillaShirts.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static BoingKit.BoingBones;
using Object = UnityEngine.Object;

namespace GorillaShirts.Behaviours.Tools
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
                                        itemObject.gameObject.GetOrAddComponent<GorillaFur>()._visualParent = visualParent;
                                    }
                                }
                            }

                            bool isFur = itemObject.GetComponent<GorillaFur>();

                            if (colourSupport && customColour && !isFur && itemObject.GetComponent<Renderer>().material.HasProperty("_BaseColor"))
                            {
                                itemObject.gameObject.AddComponent<GorillaColour>()._visualParent = visualParent;
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
                                    PoseStiffnessCustomCurve = AnimationCurve.Linear(0f, 0.7f, 1f, 0.6f),
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

                            boneComponent.BoneChains = boneChainList.ToArray();
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
