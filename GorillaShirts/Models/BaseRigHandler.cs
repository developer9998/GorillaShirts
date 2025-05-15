using System;
using System.Collections.Generic;
using System.Linq;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Extensions;
using TMPro;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class BaseRigHandler
    {
        public event Action OnShirtWorn, OnShirtRemoved;

        public GameObject RigObject;

        public Transform Body, Head, LeftUpper, RightUpper, LeftLower, RightLower, LeftHand, RightHand;

        public SkinnedMeshRenderer MainSkin;

        public MeshRenderer FaceSkin;

        public TMP_Text[] PlayerNameTags = new TMP_Text[2];

        public Dictionary<IShirtAsset, List<GameObject>> Objects = [];

        public bool Invisible = false;

        public Dictionary<EShirtComponentType, UnityLayer> LayerOverrides = [];

        public List<IShirtAsset> Shirts
        {
            get => shirts;
            set
            {
                var currentShirts = new List<IShirtAsset>(shirts);
                foreach (var shirt in currentShirts)
                {
                    if (!value.Contains(shirt))
                    {
                        RemoveShirt(shirt);
                    }
                }
                foreach (var shirt in value)
                {
                    if (!shirts.Contains(shirt))
                    {
                        WearShirt(shirt);
                    }
                }
            }
        }

        private readonly List<IShirtAsset> shirts = [];

        public readonly List<string> ShirtNames = [];

        public int NameTagOffset;

        public bool ApplyInvisibility = false;

        public int ShirtCapacity = (int)EShirtComponentType.Count;

        internal GameObject[] ShirtGameObjectArray => Objects?.SelectMany(selector => selector.Value).ToArray();

        public void CheckShirts()
        {
            ShirtNames.Clear();
            ApplyInvisibility = false;

            foreach (var shirt in shirts)
            {
                ShirtNames.Add(shirt.Descriptor.Name);
                if (shirt.Descriptor.Invisiblity) ApplyInvisibility = true;
            }

            MoveNameTag();
        }

        public void WearShirt(IShirtAsset myShirt)
        {
            if (shirts.Contains(myShirt)) return;

            if (Objects.TryGetValue(myShirt, out var shirtComponentObjects))
            {
                shirtComponentObjects.ForEach(gameObject =>
                {
                    if (gameObject.TryGetComponent(out ShirtVisual visual))
                    {
                        visual.RigHandler = this;
                        visual.enabled = true;
                    }
                    gameObject.SetActive(!Invisible);
                });

                goto ApplyCheck;
            }

            var newObjects = new List<GameObject>();

            foreach (var type in myShirt.ComponentTypes)
            {
                if (myShirt.Template.transform.Find(type.ToString()) is Transform child)
                {
                    var parentObject = type switch
                    {
                        EShirtComponentType.Body => Body,
                        EShirtComponentType.Head => Head,
                        EShirtComponentType.LeftUpper => LeftUpper,
                        EShirtComponentType.LeftLower => LeftLower,
                        EShirtComponentType.LeftHand => LeftHand,
                        EShirtComponentType.RightUpper => RightUpper,
                        EShirtComponentType.RightLower => RightLower,
                        EShirtComponentType.RightHand => RightHand,
                        _ => null
                    };

                    if (!parentObject) continue;

                    var newSectorObject = UnityEngine.Object.Instantiate(child.gameObject);
                    newSectorObject.name = child.gameObject.name;
                    newSectorObject.transform.SetParent(parentObject, false);
                    newSectorObject.transform.localPosition = child.localPosition;
                    newSectorObject.transform.localEulerAngles = child.localEulerAngles;
                    newSectorObject.transform.localScale = child.localScale;
                    newObjects.Add(newSectorObject);
                    newSectorObject.SetActive(!Invisible);

                    if (newSectorObject.TryGetComponent(out ShirtVisual visual))
                    {
                        visual.RigHandler = this;
                        visual.enabled = true;
                    }

                    if (LayerOverrides.TryGetValue(type, out UnityLayer layer))
                    {
                        newSectorObject.GetComponentsInChildren<Renderer>()
                            .Select(renderer => renderer.gameObject)
                            .Distinct()
                            .ForEach(gameObject => gameObject.SetLayer(layer));
                    }
                }
            }

            Objects.Add(myShirt, newObjects);

        ApplyCheck:

            var finalShirts = myShirt.WithShirts(shirts);
            var currentShirtList = new List<IShirtAsset>(shirts);
            for (int i = 0; i < currentShirtList.Count; i++)
            {
                var currentShirt = currentShirtList[i];
                if (!finalShirts.Contains(currentShirt) && shirts.Contains(currentShirt))
                {
                    RemoveShirt(currentShirt);
                }
            }

            shirts.Add(myShirt);

            CheckShirts();

            OnShirtWorn?.Invoke();
        }

        public void RemoveShirt(IShirtAsset shirt)
        {
            if (shirt == null || !shirts.Contains(shirt)) return;

            if (Objects.TryGetValue(shirt, out var shirtComponentObjects))
            {
                shirtComponentObjects.ForEach(gameObject =>
                {
                    if (gameObject.TryGetComponent(out ShirtVisual visual))
                    {
                        visual.enabled = false;
                    }
                    gameObject.SetActive(false);
                });
            }

            shirts.Remove(shirt);

            CheckShirts();

            OnShirtRemoved?.Invoke();
        }

        public void OffsetNameTag(int offset)
        {
            NameTagOffset = offset;
            MoveNameTag();
        }

        public virtual void MoveNameTag()
        {
            int offset = shirts.Count > 0 ? NameTagOffset : 0;

            foreach (var nametag in PlayerNameTags)
            {
                MoveNameTagTransform(nametag.transform, offset);
            }
        }

        internal void MoveNameTagTransform(Transform transform, float offset)
        {
            Vector3 offsetVector = transform.localPosition;
            offsetVector.z = (float)-offset * 5;
            transform.localPosition = offsetVector;
        }

        public void ClearObjects()
        {
            if (Objects == null || Objects.Count == 0) return;

            var shirtGameObjectArrayCosYouHaveToCacheThatFr = ShirtGameObjectArray;
            for (int i = 0; i < shirtGameObjectArrayCosYouHaveToCacheThatFr.Length; i++)
            {
                UnityEngine.Object.Destroy(shirtGameObjectArrayCosYouHaveToCacheThatFr[i]);
            }

            Objects.Clear();
        }
    }
}
