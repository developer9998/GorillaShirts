using UnityEngine;

#if PLUGIN
using GorillaShirts.Behaviours.Cosmetic;
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

        public Dictionary<IGorillaShirt, Dictionary<EShirtAnchor, Transform>> Anchors = [];

        public GameObject NameTagAnchor;

        public GameObject BadgeAnchor;

        public bool InvisibleToLocalPlayer = false;

        public EShirtBodyType BodyType = EShirtBodyType.Default;

        public Dictionary<EShirtObject, UnityLayer> LayerOverrides = [];

        public readonly List<IGorillaShirt> Shirts = [];

        public int NameTagOffset;

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
                    if (gameObject.TryGetComponent(out ShirtColourProfile profile))
                    {
                        profile.Humanoid = this;
                        profile.enabled = true;
                    }
                    gameObject.SetActive(!InvisibleToLocalPlayer);
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
                    newSectorObject.name = $"{myShirt.Descriptor.ShirtName}: {child.gameObject.name}";
                    newSectorObject.transform.SetParent(parentObject, false);
                    newSectorObject.transform.localPosition = child.localPosition;
                    newSectorObject.transform.localEulerAngles = child.localEulerAngles;
                    newSectorObject.transform.localScale = child.localScale;
                    newObjects.Add(newSectorObject);
                    newSectorObject.SetActive(!InvisibleToLocalPlayer);

                    if (newSectorObject.TryGetComponent(out ShirtColourProfile profile))
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

            foreach(EShirtAnchor anchor in Enum.GetValues(typeof(EShirtAnchor)).Cast<EShirtAnchor>())
            {
                if (myShirt.Anchors.HasFlag(anchor) && myShirt.Template.transform.Find(anchor.GetName()) is Transform child)
                {
                    Transform anchorParent = Body;
                    if (anchorParent is null || !anchorParent) continue;

                    GameObject anchorGameObject = Instantiate(child.gameObject);
                    anchorGameObject.name = $"{myShirt.Descriptor.ShirtName}: {child.gameObject.name}";
                    anchorGameObject.transform.SetParent(anchorParent, false);
                    anchorGameObject.transform.localPosition = child.localPosition;
                    anchorGameObject.transform.localEulerAngles = child.localEulerAngles;
                    anchorGameObject.transform.localScale = child.localScale;

                    if (!Anchors.ContainsKey(myShirt)) Anchors.Add(myShirt, []);

                    if (!Anchors[myShirt].ContainsKey(anchor)) Anchors[myShirt].Add(anchor, anchorGameObject.transform);
                }
            }

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
                    if (gameObject.TryGetComponent(out ShirtColourProfile profile))
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
            BodyType = EShirtBodyType.Default;

            foreach (ShirtDescriptor descriptor in Shirts.Select(shirt => shirt.Descriptor))
            {
                if (descriptor.BodyType == EShirtBodyType.Invisible)
                {
                    BodyType = EShirtBodyType.Invisible;
                    break;
                }
                if (descriptor.BodyType > BodyType) BodyType = descriptor.BodyType;
            }

            NameTagAnchor = null;
            BadgeAnchor = null;

            var anchors = Anchors.Where(pair => Shirts.Contains(pair.Key)).Select(pair => pair.Value).ToArray();

            foreach(var dictionary in anchors)
            {
                if (dictionary.TryGetValue(EShirtAnchor.NameTagAnchor, out Transform nameTagAnchor) && (NameTagAnchor == null || nameTagAnchor.localPosition.z > NameTagAnchor.transform.localPosition.z))
                {
                    NameTagAnchor = nameTagAnchor.gameObject;
                }

                if (dictionary.TryGetValue(EShirtAnchor.BadgeAnchor, out Transform badgeAnchor) && (BadgeAnchor == null || badgeAnchor.localPosition.z > BadgeAnchor.transform.localPosition.z))
                {
                    BadgeAnchor = badgeAnchor.gameObject;
                }
            }

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
