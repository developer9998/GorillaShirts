using BoingKit;
using GorillaExtensions;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Extensions;
using GorillaShirts.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using static BoingKit.BoingBones;
using Object = UnityEngine.Object;

namespace GorillaShirts.Models.Cosmetic
{
    internal class LegacyGorillaShirt : IGorillaShirt
    {
        public string ShirtId { get; private set; }
        public FileInfo FileInfo { get; private set; }
        public ShirtDescriptor Descriptor { get; private set; }
        public GameObject Template { get; private set; }
        public EShirtObject Objects { get; private set; }
        public EShirtFeature Features { get; private set; }

        private static readonly Dictionary<string, EShirtObject> shirtObjectFromNameDict = new()
        {
            { "BodyObject", EShirtObject.Body         },
            { "HeadObject", EShirtObject.Head         },
            { "LUpperArm", EShirtObject.LeftUpper     },
            { "LLowerArm", EShirtObject.LeftLower     },
            { "LHand", EShirtObject.LeftHand          },
            { "RUpperArm", EShirtObject.RightUpper    },
            { "RLowerArm", EShirtObject.RightLower    },
            { "RHand", EShirtObject.RightHand         },
        };

        private static Material fur_material;

        public async Task CreateShirt(FileInfo file)
        {
            FileInfo = file;

            AssetBundle assetBundle = null;
            ShirtJSON dataJson = null;

            using ZipArchive archive = ZipFile.OpenRead(FileInfo.FullName);
            try
            {
                ZipArchiveEntry dataEntry = archive.Entries.FirstOrDefault(i => i.Name == "ShirtData.json");
                if (dataEntry is null) return;

                using StreamReader stream = new(dataEntry.Open(), Encoding.UTF8);
                string dataContents = await stream.ReadToEndAsync();
                dataJson = JsonConvert.DeserializeObject<ShirtJSON>(dataContents);
                stream.Close();

                ZipArchiveEntry assetEntry = archive.Entries.FirstOrDefault(i => i.Name == dataJson.assetName);
                if (assetEntry is null) return;

                using MemoryStream memStream = new();
                await assetEntry.Open().CopyToAsync(memStream);

                assetBundle = await LoadFromStream(memStream);
                memStream.Close();
            }
            catch (Exception ex)
            {
                Logging.Fatal($"Could not parse file: {FileInfo.FullName}");
                Logging.Error(ex);
                return;
            }

            try
            {
                Template = await LoadAsset<GameObject>(assetBundle, "ExportShirt");

                Descriptor = Template.AddComponent<ShirtDescriptor>();

                Descriptor.PackName = dataJson.packName;

                Descriptor.ShirtName = dataJson.infoDescriptor.shirtName;
                Descriptor.Author = dataJson.infoDescriptor.shirtAuthor.Trim('"');
                Descriptor.Description = dataJson.infoDescriptor.shirtDescription;

                if (dataJson.infoConfig.customColors) Features |= EShirtFeature.CustomColours;

                if (dataJson.infoConfig.invisibility)
                {
                    Features |= EShirtFeature.Invisibility;
                    Descriptor.BodyType = EShirtBodyType.Invisible;
                }
                // else Descriptor.BodyType = EShirtBodyType.Default;

                Template.name = $"{Descriptor.ShirtName} Legacy Asset";
                ShirtId = Encoding.UTF8.GetString(Encoding.Default.GetBytes($"{Descriptor.PackName}/{Descriptor.ShirtName}"));

                foreach (Transform child in Template.transform)
                {
                    if (child.gameObject.name == "OverrideWearClip" && child.TryGetComponent(out AudioSource customWearDevice))
                    {
                        Descriptor.WearSound = customWearDevice.clip;
                        continue;
                    }

                    if (child.gameObject.name == "OverrideRemoveClip" && child.TryGetComponent(out AudioSource customRemoveDevice))
                    {
                        Descriptor.RemoveSound = customRemoveDevice.clip;
                        continue;
                    }
                }

                async Task AssembleObject(string sectorName, EShirtObject sectorType)
                {
                    Transform body_part_tform = Template.transform.transform.Find(sectorName);
                    if (body_part_tform && body_part_tform.gameObject is GameObject body_part)
                    {
                        if (body_part.GetComponents(typeof(Component)).Length <= 1 && body_part_tform.childCount == 0)
                        {
                            // empty object
                            Logging.Info($"{Descriptor.ShirtName} has empty {sectorType}");
                            body_part.name = string.Concat(sectorType.ToString(), "Empty");
                            return;
                        }

                        Logging.Info($"{Descriptor.ShirtName} (legacy) has {sectorType}");

                        Objects |= sectorType;
                        body_part.name = sectorType.ToString();

                        /*
                        tempSector.gameObject.SetActive(false);
                        Sector newSector = new()
                        {
                            Object = tempSector.gameObject,
                            Type = sectorType,
                            Position = tempSector.localPosition,
                            Euler = tempSector.localEulerAngles,
                            Scale = tempSector.localScale
                        };
                        Descriptor.SectorList.Add(newSector);
                        */

                        ShirtColourProfile visualParent = body_part.GetOrAddComponent<ShirtColourProfile>();
                        visualParent.enabled = false;

                        List<Transform> bones = [], ignoreBones = [];

                        void FindFeature<T>(EShirtFeature feature, Action<T> handleFeature = null) where T : Component
                        {
                            var components = body_part.GetComponentsInChildren<T>();
                            if (components.Length > 0)
                            {
                                Features |= feature;
                                if (handleFeature != null) components.ForEach(component => handleFeature(component));
                            }
                        }

                        FindFeature<AudioSource>(EShirtFeature.Audio, audioSource =>
                        {
                            if (audioSource.spatialBlend == 0)
                            {
                                audioSource.spatialBlend = 1f;
                                audioSource.rolloffMode = AudioRolloffMode.Linear;
                                audioSource.minDistance = 2f;
                                audioSource.maxDistance = 10f;
                                audioSource.volume = 0.5f;
                            }
                        });
                        FindFeature<ParticleSystem>(EShirtFeature.Particles);
                        FindFeature<Light>(EShirtFeature.Light);

                        foreach (var decendant in body_part.GetComponentsInChildren<Transform>(true))
                        {
                            bool colourSupport = false;
                            bool customColour = dataJson.infoConfig.customColors;

                            if (decendant.TryGetComponent(out Renderer renderer))
                            {
                                colourSupport = true;
                                if (renderer.materials != null && renderer.materials.Length != 0)
                                {
                                    renderer.materials = [.. renderer.materials.Select(material => material.CreateUberMaterial())];
                                }
                            }

                            if (decendant.childCount > 0)
                            {
                                for (int i = 0; i < decendant.childCount; i++)
                                {
                                    Transform child = decendant.GetChild(i);
                                    if (child.name == "Wobble0")
                                    {
                                        bones.Add(decendant);
                                    }
                                    else if (child.name == "Wobble1")
                                    {
                                        ignoreBones.Add(decendant);
                                    }
                                    if (child.name.StartsWith("G_Fur") && colourSupport)
                                    {
                                        if (!fur_material)
                                        {
                                            Texture2D furTexture = await AssetLoader.LoadAsset<Texture2D>("lightfur");
                                            Shader uberShader = Shader.Find("GorillaTag/UberShader");

                                            string[] keywords = (GorillaTagger.Instance.offlineVRRig && GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance) ? GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance.shaderKeywords : ["_USE_TEXTURE", "_ENVIRONMENTREFLECTIONS_OFF", "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF"];
                                            keywords = [.. keywords.Except(["_GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY", "_USE_TEX_ARRAY_ATLAS"])];

                                            fur_material = new Material(uberShader)
                                            {
                                                mainTexture = furTexture,
                                                shaderKeywords = keywords,
                                                enabledKeywords = [.. keywords.Select(keyword => new LocalKeyword(uberShader, keyword))]
                                            };
                                        }

                                        ShirtCustomMaterial gorillaFur = decendant.gameObject.GetOrAddComponent<ShirtCustomMaterial>();
                                        gorillaFur.Appearance = (ShirtCustomMaterial.EAppearanceType)Convert.ToInt32(decendant.GetChild(decendant.childCount - 1).name[^1]);
                                        // gorillaFur.Source = EMaterialSource.Skin;
                                        gorillaFur.BaseFurMaterial = fur_material;
                                        gorillaFur.ShirtProfile = visualParent;
                                    }
                                    if (child.name.StartsWith("G_BB"))
                                    {
                                        Features |= EShirtFeature.Billboard;
                                        decendant.gameObject.GetOrAddComponent<ShirtBillboard>();
                                    }
                                }
                            }

                            if (colourSupport && customColour && !decendant.GetComponent<ShirtCustomMaterial>())
                            {
                                decendant.gameObject.AddComponent<ShirtCustomColour>().ShirtProfile = visualParent;
                            }
                        }

                        if (bones.Count > 0)
                        {
                            BoingBones boneComponent = body_part.AddComponent<BoingBones>();
                            boneComponent.LockTranslationX = dataJson.infoConfig.wobbleLockHorizontal;
                            boneComponent.LockTranslationY = dataJson.infoConfig.wobbleLockVertical;
                            boneComponent.LockTranslationZ = dataJson.infoConfig.wobbleLockHorizontal;

                            List<Chain> boneChainList = [];
                            foreach (var bone in bones)
                            {
                                Chain chain = !dataJson.infoConfig.wobbleLoose ? new()
                                {
                                    Root = bone,
                                    Exclusion = ignoreBones.ToArray(),
                                    PoseStiffnessCurveType = Chain.CurveType.Custom,
                                    PoseStiffnessCustomCurve = AnimationCurve.Linear(0f, 0.9f, 1f, 0.7f),
                                    BendAngleCapCurveType = Chain.CurveType.Custom,
                                    BendAngleCapCustomCurve = AnimationCurve.Linear(0f, 0.1f, 1f, 0.06f),
                                    SquashAndStretchCurveType = Chain.CurveType.ConstantZero,
                                    LengthStiffnessCurveType = Chain.CurveType.ConstantOne,
                                    LooseRoot = !dataJson.infoConfig.wobbleLockRoot
                                } : new()
                                {
                                    Root = bone,
                                    Exclusion = ignoreBones.ToArray(),
                                    SquashAndStretchCurveType = Chain.CurveType.ConstantZero,
                                    LengthStiffnessCurveType = Chain.CurveType.ConstantOne,
                                    LooseRoot = !dataJson.infoConfig.wobbleLockRoot
                                };

                                SharedBoingParams boingParams = ScriptableObject.CreateInstance<SharedBoingParams>();
                                boingParams.Params = boneComponent.Params;
                                chain.ParamsOverride = boingParams;

                                boneChainList.Add(chain);
                            }

                            Features |= EShirtFeature.Wobble;
                            boneComponent.BoneChains = [.. boneChainList];
                        }
                    }
                }

                shirtObjectFromNameDict.ForEach(async pair => await AssembleObject(pair.Key, pair.Value));
            }
            catch (Exception ex)
            {
                Descriptor = null;
                Logging.Fatal($"Could not assemble LegacyGorillaShirt: {FileInfo.FullName}");
                Logging.Error(ex);
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

        public override string ToString() => Descriptor == null ? "n/a" : $"{Descriptor.Author}: {Descriptor.ShirtName}";

        [Serializable]
        public class ShirtJSON
        {
            public string assetName;
            public string packName;

            public SDescriptor infoDescriptor;
            public SConfig infoConfig;
        }

        [Serializable]
        public class SDescriptor
        {
            public string shirtName;
            public string shirtAuthor;
            public string shirtDescription;
        }

        [Serializable]
        public class SConfig
        {
            public bool customColors;
            public bool invisibility;
            public bool wobbleLoose;
            public bool wobbleLockHorizontal;
            public bool wobbleLockVertical;
            public bool wobbleLockRoot = true;
        }
    }
}
