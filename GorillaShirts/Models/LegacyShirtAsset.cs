using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoingKit;
using GorillaExtensions;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Extensions;
using GorillaShirts.Tools;
using UnityEngine;
using UnityEngine.Rendering;
using static BoingKit.BoingBones;
using static GorillaShirts.Behaviours.Appearance.PlayerMaterialAppearance;
using Object = UnityEngine.Object;

namespace GorillaShirts.Models
{
    public class LegacyShirtAsset : IShirtAsset
    {
        public string FilePath { get; private set; }

        public ShirtDescriptor Descriptor { get; private set; }

        public GameObject Template { get; private set; }

        public List<EShirtComponentType> ComponentTypes { get; private set; }

        public List<bool> TemplateData =>
        [
            Template.GetComponentInChildren<AudioSource>(),
            Descriptor.Billboard,
            Descriptor.CustomColours,
            Descriptor.Invisiblity,
            Template.GetComponentInChildren<Light>(),
            Template.GetComponentInChildren<ParticleSystem>(),
            Descriptor.Wobble
        ];

        private readonly Dictionary<string, EShirtComponentType> sector_dict = new()
        {
            { "BodyObject", EShirtComponentType.Body         },
            { "HeadObject", EShirtComponentType.Head         },
            { "LUpperArm", EShirtComponentType.LeftUpper     },
            { "LLowerArm", EShirtComponentType.LeftLower     },
            { "LHand", EShirtComponentType.LeftHand          },
            { "RUpperArm", EShirtComponentType.RightUpper    },
            { "RLowerArm", EShirtComponentType.RightLower    },
            { "RHand", EShirtComponentType.RightHand         },
        };

        private static Material fur_material;

        public async Task<IShirtAsset> Construct(string filePath)
        {
            FilePath = filePath;

            AssetBundle shirtResourceBundle = null;
            ShirtJSON shirtDataJSON = null;

            Logging.Info($"Opening file '{Path.GetFileName(filePath)}'");
            using var archive = ZipFile.OpenRead(filePath);
            try
            {
                var packageEntry = archive.Entries.FirstOrDefault(i => i.Name == "ShirtData.json");
                if (packageEntry == null) throw new MissingFieldException(nameof(packageEntry));

                using var stream = new StreamReader(packageEntry.Open(), Encoding.UTF8);

                string packageReadContents = await stream.ReadToEndAsync();
                shirtDataJSON = Newtonsoft.Json.JsonConvert.DeserializeObject<ShirtJSON>(packageReadContents);

                var shirtResourceEntry = archive.Entries.FirstOrDefault(i => i.Name == shirtDataJSON.assetName);
                if (shirtResourceEntry == null) throw new MissingFieldException(nameof(shirtResourceEntry));

                using var SeekableStream = new MemoryStream();
                await shirtResourceEntry.Open().CopyToAsync(SeekableStream);

                shirtResourceBundle = await LoadFromStream(SeekableStream);
            }
            catch (Exception ex)
            {
                Logging.Warning($"Failed to parse file '{Path.GetFileName(filePath)}' as a shirt for the mod: {ex}");
                return null;
            }

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                Template = await LoadAsset<GameObject>(shirtResourceBundle, "ExportShirt");

                Descriptor = Template.AddComponent<ShirtDescriptor>();

                Descriptor.Pack = shirtDataJSON.packName;

                Descriptor.Name = $"{shirtDataJSON.packName}/{shirtDataJSON.infoDescriptor.shirtName}";
                Descriptor.DisplayName = shirtDataJSON.infoDescriptor.shirtName;
                Descriptor.Author = shirtDataJSON.infoDescriptor.shirtAuthor.Trim('"');
                Descriptor.Description = shirtDataJSON.infoDescriptor.shirtDescription;

                Descriptor.CustomColours = shirtDataJSON.infoConfig.customColors;
                Descriptor.Invisiblity = shirtDataJSON.infoConfig.invisibility;

                Transform WearOverride = Template.transform.Find("OverrideWearClip");
                Transform RemoveOverride = Template.transform.Find("OverrideRemoveClip");
                if (WearOverride) Descriptor.CustomWearSound = WearOverride.GetComponent<AudioSource>().clip;
                if (RemoveOverride) Descriptor.CustomRemoveSound = RemoveOverride.GetComponent<AudioSource>().clip;

                ComponentTypes = [];

                async Task PrepareSectorAsync(string sectorName, EShirtComponentType sectorType)
                {
                    Transform body_part_tform = Template.transform.transform.Find(sectorName);
                    if (body_part_tform && body_part_tform.gameObject is GameObject body_part)
                    {
                        if (body_part.GetComponents(typeof(Component)).Length <= 1 && body_part_tform.childCount == 0)
                        {
                            // empty object
                            Logging.Info($"{Descriptor.DisplayName} has empty {sectorType}");
                            body_part.name = string.Concat(sectorType.ToString(), "Empty");
                            return;
                        }

                        Logging.Info($"{Descriptor.DisplayName} has {sectorType}");

                        ComponentTypes.Add(sectorType);
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

                        ShirtVisual visualParent = body_part.GetOrAddComponent<ShirtVisual>();
                        visualParent.enabled = false;

                        List<Transform> bones = [], ignoreBones = [];

                        foreach (var decendant in body_part.GetComponentsInChildren<Transform>(true))
                        {
                            bool colourSupport = decendant.GetComponent<Renderer>();
                            bool customColour = shirtDataJSON.infoConfig.customColors;

                            if (decendant.TryGetComponent(out Renderer renderer))
                            {
                                if (renderer.materials != null && renderer.materials.Length != 0)
                                {
                                    renderer.materials = renderer.materials.Select(material => material.CreateUberShaderVariant()).ToArray();
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

                                        PlayerMaterialAppearance gorillaFur = decendant.gameObject.GetOrAddComponent<PlayerMaterialAppearance>();
                                        gorillaFur.Appearance = (EAppearanceType)Convert.ToInt32(decendant.GetChild(decendant.childCount - 1).name[^1]);
                                        gorillaFur.Source = EMaterialSource.Skin;
                                        gorillaFur.BaseFurMaterial = fur_material;
                                        gorillaFur.ShirtVisual = visualParent;
                                    }
                                    if (child.name.StartsWith("G_BB"))
                                    {
                                        Descriptor.Billboard = true;
                                        decendant.gameObject.GetOrAddComponent<Billboard>();
                                    }
                                }
                            }
                            bool isFur = decendant.GetComponent<PlayerMaterialAppearance>();
                            if (colourSupport && customColour && !isFur && decendant.GetComponent<Renderer>().material.HasProperty("_BaseColor"))
                            {
                                decendant.gameObject.AddComponent<GorillaColour>().ShirtVisual = visualParent;
                            }
                        }

                        if (bones.Count > 0)
                        {
                            BoingBones boneComponent = body_part.AddComponent<BoingBones>();
                            boneComponent.LockTranslationX = shirtDataJSON.infoConfig.wobbleLockHorizontal;
                            boneComponent.LockTranslationY = shirtDataJSON.infoConfig.wobbleLockVertical;
                            boneComponent.LockTranslationZ = shirtDataJSON.infoConfig.wobbleLockHorizontal;

                            List<Chain> boneChainList = [];
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

                            Descriptor.Wobble = true;
                            boneComponent.BoneChains = [.. boneChainList];
                        }
                    }
                }
                sector_dict.ForEach(async pair => await PrepareSectorAsync(pair.Key, pair.Value));
            }
            catch (Exception ex)
            {
                Logging.Warning($"Exception thrown when loading shirt {Path.GetFileName(filePath)}: {ex}");
                return null;
            }

            return this;
        }

        public (string name, string author, string description, string type, string source, string note) GetNavigationInfo()
        {
            return
            (
                Descriptor.DisplayName,
                Descriptor.Author,
                Descriptor.Description,
                "Shirt",
                Descriptor.Pack,
                "This shirt was made for an earlier version of GorillaShirts, and may not have the latest features."
            );
        }

        public override string ToString()
        {
            return $"{Descriptor.Name} / {Descriptor.Version}";
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

        [Serializable]
        public class ShirtJSON
        {
            public string assetName;
            public string packName;
            public int version = 1;

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
