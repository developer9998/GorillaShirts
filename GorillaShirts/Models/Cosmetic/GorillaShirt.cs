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
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GorillaShirts.Models.Cosmetic
{
    internal class GorillaShirt : IGorillaShirt
    {
        public string ShirtId { get; private set; }
        public FileInfo FileInfo { get; private set; }
        public ShirtDescriptor Descriptor { get; private set; }
        public GameObject Template { get; private set; }
        public EShirtObject Objects { get; private set; }
        public EShirtFeature Features { get; private set; }

        private static readonly List<Type> allowedTypeList =
        [
            typeof(Transform),
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(Light),
            typeof(ReflectionProbe),
            typeof(AudioSource),
            typeof(Animator),
            typeof(TextMesh),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
            typeof(RectTransform),
            typeof(SpriteRenderer),
            typeof(BillboardRenderer),
            typeof(Canvas),
            typeof(CanvasRenderer),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(TrailRenderer),
            typeof(Camera),
            typeof(Text),
            typeof(ShirtDescriptor),
        ];

        public async Task CreateShirt(FileInfo file)
        {
            FileInfo = file;

            AssetBundle assetBundle;

            try
            {
                assetBundle = await LoadFromFile(file.FullName);
            }
            catch(Exception ex)
            {
                Logging.Fatal("AssetBundle could not be loaded");
                Logging.Error(ex);
                return;
            }

            try
            {
                Template = await LoadAsset<GameObject>(assetBundle, "GorillaShirtAsset");

                if (Template.TryGetComponent(out ShirtDescriptor shirtDescriptor))
                {
                    Descriptor = shirtDescriptor;

                    ShirtId = Encoding.UTF8.GetString(Encoding.Default.GetBytes($"{Descriptor.PackName}/{Descriptor.ShirtName}"));

                    var objectTypes = Enum.GetValues(typeof(EShirtObject)).Cast<EShirtObject>().ToArray();
                    for(int i = 0; i < objectTypes.Length; i++)
                    {
                        Transform child = Template.transform.Find(objectTypes[i].ToString());
                        if (child != null && child)
                        {
                            Objects |= objectTypes[i];

                            ShirtProfile visualParent = child.gameObject.GetOrAddComponent<ShirtProfile>();
                            visualParent.enabled = false;

                            foreach (Transform decendant in child.GetComponentsInChildren<Transform>(true))
                            {
                                if (decendant.TryGetComponent(out MeshRenderer renderer))
                                {
                                    renderer.materials = [.. renderer.materials.Where(material => material.UpdateUberShaderMaterial())];
                                }
                            }
                        }
                    }
                }

                Template.SetActive(false);
                SanitizeObjectRecursive(Template);
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

        private static void SanitizeObjectRecursive(GameObject gameObject)
        {
            SanitizeObject(gameObject);
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                GameObject child = gameObject.transform.GetChild(i).gameObject;
                if (child != null && child)
                {
                    SanitizeObjectRecursive(child);
                }
            }
        }

        private static void SanitizeObject(GameObject gameObject)
        {
            if (gameObject == null || !gameObject) return;

            Component[] components = gameObject.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (allowedTypeList.Contains(components[i].GetType())) continue;
                Object.DestroyImmediate(components[i]);
            }
        }

        public override string ToString() => Descriptor == null ? "n/a" : $"{Descriptor.Author}: {Descriptor.ShirtName}";
    }
}
