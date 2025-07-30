﻿using BoingKit;
using GorillaExtensions;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Extensions;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace GorillaShirts.Models.Cosmetic
{
    internal class GorillaShirt : IGorillaShirt
    {
        public string ShirtId { get; private set; }
        public FileInfo FileInfo { get; private set; }
        public AssetBundle Bundle { get; private set; }
        public ShirtDescriptor Descriptor { get; private set; }
        public GameObject Template { get; private set; }
        public EShirtObject Objects { get; private set; }
        public EShirtFeature Features { get; private set; }

        private static Material furMaterial = null;

        public async Task CreateShirt(FileInfo file)
        {
            FileInfo = file;
            Bundle = null;

            try
            {
                AssetBundle bundleFromFile = await LoadFromFile(file.FullName);
                Bundle = bundleFromFile;
            }
            catch (Exception ex)
            {
                Logging.Fatal("AssetBundle could not be loaded");
                Logging.Error(ex);
                return;
            }

            try
            {
                Template = await LoadAsset<GameObject>(Bundle, "GorillaShirtAsset");
                Template.SetActive(false);

                if (Template.TryGetComponent(out ShirtDescriptor shirtDescriptor))
                {
                    Descriptor = shirtDescriptor;
                    Logging.Message($"{shirtDescriptor.ShirtName} ({shirtDescriptor.PackName})");

                    ShirtId = Encoding.UTF8.GetString(Encoding.Default.GetBytes($"{Descriptor.PackName}/{Descriptor.ShirtName}"));

                    Template.name = $"{Descriptor.ShirtName} Asset";
                    Template.SanitizeObjectRecursive();

                    var objectTypes = Enum.GetValues(typeof(EShirtObject)).Cast<EShirtObject>().ToArray();
                    for (int i = 0; i < objectTypes.Length; i++)
                    {
                        Transform child = Template.transform.Find(objectTypes[i].ToString());
                        if (child != null && child)
                        {
                            Logging.Info($"{Descriptor.ShirtName} has {objectTypes[i]}");
                            Objects |= objectTypes[i];

                            ShirtColourProfile shirtProfile = child.gameObject.GetOrAddComponent<ShirtColourProfile>();
                            shirtProfile.enabled = false;

                            List<ShirtWobbleRoot> wobbleList = [];

                            void FindFeature<T>(EShirtFeature feature, Action<T> handleFeature = null) where T : Component
                            {
                                var components = child.GetComponentsInChildren<T>();
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
                                    audioSource.maxDistance = 30f;
                                    audioSource.volume = 0.5f;
                                }
                            });
                            FindFeature<ParticleSystem>(EShirtFeature.Particles);
                            FindFeature<Light>(EShirtFeature.Light);

                            foreach (Transform decendant in child.GetComponentsInChildren<Transform>(true))
                            {
                                if (decendant.TryGetComponent(out ShirtWobbleRoot wobbleRoot))
                                {
                                    wobbleList.Add(wobbleRoot);
                                }

                                if (decendant.TryGetComponent(out Renderer renderer))
                                {
                                    Logging.Info($"{decendant.gameObject.name} ({renderer.GetType().Name})");
                                    renderer.materials = [.. renderer.materials.Select(material => new Material(material)).Select(material => material.ResolveUberMaterial())];

                                    if (decendant.TryGetComponent(out ShirtCustomColour customColour))
                                    {
                                        if (!Features.HasFlag(EShirtFeature.CustomColours)) Features |= EShirtFeature.CustomColours;
                                        Logging.Info("Assigned ShirtProfile to ShirtCustomColour");
                                        Logging.Info(decendant.GetPath().TrimStart('/'));
                                        customColour.ShirtProfile = shirtProfile;
                                    }
                                    else if (decendant.TryGetComponent(out ShirtCustomMaterial customMaterial))
                                    {
                                        Logging.Info("Assigned ShirtProfile to ShirtCustomMaterial");
                                        Logging.Info(decendant.GetPath().TrimStart('/'));

                                        if (furMaterial == null)
                                        {
                                            Texture2D furTexture = await AssetLoader.LoadAsset<Texture2D>("lightfur");
                                            Shader uberShader = UberShader.GetShader();

                                            string[] keywords = (GorillaTagger.Instance.offlineVRRig && GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance) ? GorillaTagger.Instance.offlineVRRig.myDefaultSkinMaterialInstance.shaderKeywords : ["_USE_TEXTURE", "_ENVIRONMENTREFLECTIONS_OFF", "_GLOSSYREFLECTIONS_OFF", "_SPECULARHIGHLIGHTS_OFF"];
                                            keywords = [.. keywords.Except(["_GT_BASE_MAP_ATLAS_SLICE_SOURCE__PROPERTY", "_USE_TEX_ARRAY_ATLAS"])];

                                            furMaterial = new Material(uberShader)
                                            {
                                                mainTexture = furTexture,
                                                shaderKeywords = keywords,
                                                enabledKeywords = [.. keywords.Select(keyword => new LocalKeyword(uberShader, keyword))]
                                            };
                                        }

                                        customMaterial.ShirtProfile = shirtProfile;
                                        customMaterial.BaseFurMaterial = furMaterial;
                                    }
                                }
                            }

                            if (wobbleList.Count > 0)
                            {
                                Logging.Message("Has ShirtWobbleRoot");
                                Dictionary<(bool LockTranslationX, bool LockTranslationY, bool LockTranslationZ), List<ShirtWobbleRoot>> wobbleDict = [];

                                static BoingBones.Chain.CurveType ToNativeCurveType(ShirtWobbleRoot.CurveType curveType) => (BoingBones.Chain.CurveType)(int)curveType;

                                foreach (ShirtWobbleRoot wobbleRoot in wobbleList)
                                {
                                    var tuple = (wobbleRoot.LockTranslationX, wobbleRoot.LockTranslationY, wobbleRoot.LockTranslationZ);
                                    if (!wobbleDict.ContainsKey(tuple)) wobbleDict.Add(tuple, []);
                                    wobbleDict[tuple].Add(wobbleRoot);
                                }

                                foreach (var (tuple, wobbleListPerLock) in wobbleDict)
                                {
                                    Logging.Info($"BoingBones: {tuple.LockTranslationX}, {tuple.LockTranslationY}, {tuple.LockTranslationZ}");
                                    BoingBones boingBones = child.AddComponent<BoingBones>();
                                    boingBones.LockTranslationX = tuple.LockTranslationX;
                                    boingBones.LockTranslationY = tuple.LockTranslationY;
                                    boingBones.LockTranslationZ = tuple.LockTranslationZ;
                                    List<BoingBones.Chain> chainList = [];
                                    foreach (ShirtWobbleRoot wobbleRoot in wobbleListPerLock)
                                    {
                                        Logging.Info(wobbleRoot.gameObject.name);
                                        var chain = new BoingBones.Chain()
                                        {
                                            Root = wobbleRoot.transform,
                                            Exclusion = wobbleRoot.Exclusion,
                                            LooseRoot = wobbleRoot.LooseRoot,
                                            AnimationBlendCurveType = ToNativeCurveType(wobbleRoot.AnimationBlendCurveType),
                                            AnimationBlendCustomCurve = wobbleRoot.AnimationBlendCustomCurve,
                                            LengthStiffnessCurveType = ToNativeCurveType(wobbleRoot.LengthStiffnessCurveType),
                                            LengthStiffnessCustomCurve = wobbleRoot.LengthStiffnessCustomCurve,
                                            PoseStiffnessCurveType = ToNativeCurveType(wobbleRoot.PoseStiffnessCurveType),
                                            PoseStiffnessCustomCurve = wobbleRoot.PoseStiffnessCustomCurve,
                                            BendAngleCapCurveType = ToNativeCurveType(wobbleRoot.BendAngleCapCurveType),
                                            BendAngleCapCustomCurve = wobbleRoot.BendAngleCapCustomCurve,
                                            SquashAndStretchCurveType = ToNativeCurveType(wobbleRoot.SquashAndStretchCurveType),
                                            SquashAndStretchCustomCurve = wobbleRoot.SquashAndStretchCustomCurve
                                        };

                                        SharedBoingParams boingParams = ScriptableObject.CreateInstance<SharedBoingParams>();
                                        boingParams.Params = boingBones.Params;
                                        chain.ParamsOverride = boingParams;
                                        chainList.Add(chain);
                                    }

                                    if (!Features.HasFlag(EShirtFeature.Wobble)) Features |= EShirtFeature.Wobble;
                                    boingBones.BoneChains = [.. chainList];
                                    if (boingBones.isActiveAndEnabled) boingBones.RescanBoneChains();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Descriptor = null;
                Logging.Fatal($"Could not assemble GorillaShirt: {FileInfo.FullName}");
                Logging.Error(ex);
            }
        }

        private static async Task<AssetBundle> LoadFromFile(string path)
        {
            var taskCompletionSource = new TaskCompletionSource<AssetBundle>();
            var request = AssetBundle.LoadFromFileAsync(path);
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
    }
}
