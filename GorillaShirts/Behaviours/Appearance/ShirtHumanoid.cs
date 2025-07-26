using UnityEngine;

#if PLUGIN
using GorillaShirts.Extensions;
using GorillaShirts.Models.Cosmetic;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
#endif

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtHumanoid : MonoBehaviour
    {
        public GameObject Root;

        public Transform Body, Head, LeftUpper, RightUpper, LeftLower, RightLower, LeftHand, RightHand;

        public SkinnedMeshRenderer MainSkin;

        public MeshRenderer FaceSkin;

#if PLUGIN

        public IGorillaShirt SingleShirt => Shirts.ElementAtOrDefault(0);

        public event Action OnShirtWornEvent, OnShirtRemovedEvent;

        public TMP_Text[] PlayerNameTags;

        public Dictionary<IGorillaShirt, List<GameObject>> Objects = [];

        public bool Invisible = false;

        public Dictionary<EShirtObject, UnityLayer> LayerOverrides = [];

        public readonly List<IGorillaShirt> Shirts = [];

        public int NameTagOffset;

        public bool ApplyInvisibility = false;

        public void SetShirt(IGorillaShirt shirt) => SetShirts(shirt == null ? [] : [shirt]);

        public void ClearShirts() => SetShirts([]);

        public void SetShirts(List<IGorillaShirt> shirts)
        {
            List<IGorillaShirt> currentShirts = [.. Shirts];

            foreach (IGorillaShirt shirt in currentShirts)
            {
                if (!shirts.Contains(shirt))
                {
                    NegateShirt(shirt);
                }
            }

            foreach (IGorillaShirt shirt in shirts)
            {
                if (!Shirts.Contains(shirt))
                {
                    UnionShirt(shirt);
                }
            }
        }

        public void UnionShirt(IGorillaShirt myShirt)
        {
            if (Shirts.Contains(myShirt)) return;

            if (Objects.TryGetValue(myShirt, out var shirtComponentObjects))
            {
                shirtComponentObjects.ForEach(gameObject =>
                {
                    if (gameObject.TryGetComponent(out ShirtProfile profile))
                    {
                        profile.Humanoid = this;
                        profile.enabled = true;
                    }
                    gameObject.SetActive(!Invisible);
                });

                goto ApplyCheck;
            }

            var newObjects = new List<GameObject>();

            foreach (EShirtObject type in Enum.GetValues(typeof(EShirtObject)).Cast<EShirtObject>())
            {
                if (myShirt.Objects.HasFlag(type) && myShirt.Template.transform.Find(type.ToString()) is Transform child)
                {
                    var parentObject = type switch
                    {
                        EShirtObject.Body => Body,
                        EShirtObject.Head => Head,
                        EShirtObject.LeftUpper => LeftUpper,
                        EShirtObject.LeftLower => LeftLower,
                        EShirtObject.LeftHand => LeftHand,
                        EShirtObject.RightUpper => RightUpper,
                        EShirtObject.RightLower => RightLower,
                        EShirtObject.RightHand => RightHand,
                        _ => null
                    };

                    if (!parentObject) continue;

                    var newSectorObject = Instantiate(child.gameObject);
                    newSectorObject.name = child.gameObject.name;
                    newSectorObject.transform.SetParent(parentObject, false);
                    newSectorObject.transform.localPosition = child.localPosition;
                    newSectorObject.transform.localEulerAngles = child.localEulerAngles;
                    newSectorObject.transform.localScale = child.localScale;
                    newObjects.Add(newSectorObject);
                    newSectorObject.SetActive(!Invisible);

                    if (newSectorObject.TryGetComponent(out ShirtProfile profile))
                    {
                        profile.Humanoid = this;
                        profile.enabled = true;
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

            var finalShirts = myShirt.Concat(Shirts);
            var currentShirtList = new List<IGorillaShirt>(Shirts);
            for (int i = 0; i < currentShirtList.Count; i++)
            {
                var currentShirt = currentShirtList[i];
                if (!finalShirts.Contains(currentShirt) && Shirts.Contains(currentShirt))
                {
                    NegateShirt(currentShirt);
                }
            }

            if (!Shirts.Contains(myShirt)) Shirts.Add(myShirt);

            CheckShirts();

            OnShirtWorn();
            OnShirtWornEvent?.Invoke();
        }

        public void NegateShirt(IGorillaShirt shirt)
        {
            if (shirt == null || !Shirts.Contains(shirt)) return;

            if (Objects.TryGetValue(shirt, out var shirtComponentObjects))
            {
                shirtComponentObjects.ForEach(gameObject =>
                {
                    if (gameObject.TryGetComponent(out ShirtProfile profile))
                    {
                        profile.enabled = false;
                    }
                    gameObject.SetActive(false);
                });
            }

            Shirts.Remove(shirt);

            CheckShirts();

            OnShirtRemoved();
            OnShirtRemovedEvent?.Invoke();
        }

        public void CheckShirts()
        {
            ApplyInvisibility = Shirts.Any(shirt => shirt.Features.HasFlag(EShirtFeature.Invisibility));
            MoveNameTag();
        }

        public void OffsetNameTag(int offset)
        {
            NameTagOffset = offset;
            MoveNameTag();
        }

        public virtual void OnShirtWorn()
        {

        }

        public virtual void OnShirtRemoved()
        {

        }

        public virtual void MoveNameTag()
        {
            int offset = Shirts.Count > 0 ? NameTagOffset : 0;

            if (PlayerNameTags != null)
            {
                foreach (var nametag in PlayerNameTags)
                {
                    if (nametag != null)
                    {
                        MoveNameTagTransform(nametag.transform, offset);
                    }
                }
            }
        }

        internal void MoveNameTagTransform(Transform transform, float offset)
        {
            Vector3 offsetVector = transform.localPosition;
            offsetVector.z = (float)-offset * 5;
            transform.localPosition = offsetVector;
        }

        public void RemoveObjects()
        {
            if (Objects == null || Objects.Count == 0) return;

            GameObject[] allShirtObjects = Objects?.SelectMany(selector => selector.Value).ToArray();
            for (int i = 0; i < allShirtObjects.Length; i++)
            {
                Destroy(allShirtObjects[i]);
            }

            Objects.Clear();
        }
#endif
    }
}
