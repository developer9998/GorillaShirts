using GorillaShirts.Behaviours.Appearance;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class Rig
    {
        private bool Cooldown;
        public event Action OnShirtWorn, OnShirtRemoved;

        public Transform RigParent, Body, Head, LeftUpper, RightUpper, LeftLower, RightLower, LeftHand, RightHand;
        public SkinnedMeshRenderer RigSkin;
        public TMP_Text[] PlayerNameTags = new TMP_Text[2];

        public Dictionary<Shirt, List<GameObject>> Objects = [];

        public Shirt Shirt;

        public int NameTagOffset;

        internal GameObject[] ShirtGameObjectArray => Objects?.SelectMany(selector => selector.Value).ToArray();

        public void WearShirt(Shirt myShirt, out Shirt oldShirt)
        {
            oldShirt = Shirt;
            WearShirt(myShirt);
        }

        public void WearShirt(Shirt myShirt)
        {
            if (Shirt == myShirt) return;

            if (Shirt != myShirt && !Cooldown)
            {
                RemoveShirt();
            }

            Shirt = myShirt;

            if (Objects.ContainsKey(myShirt))
            {
                var setObjects = Objects[myShirt];
                setObjects.ForEach(a =>
                {
                    a.SetActive(true);
                    a.GetComponent<ShirtVisual>().Rig = this;
                });
                OnShirtWorn?.Invoke();
                return;
            }

            var newObjects = new List<GameObject>();
            Cooldown = true;
            foreach (var sector in myShirt.SectorList)
            {
                var sectorParent = sector.Type switch
                {
                    SectorType.Body => Body,
                    SectorType.Head => Head,
                    SectorType.LeftUpper => LeftUpper,
                    SectorType.LeftLower => LeftLower,
                    SectorType.LeftHand => LeftHand,
                    SectorType.RightUpper => RightUpper,
                    SectorType.RightLower => RightLower,
                    SectorType.RightHand => RightHand,
                    _ => throw new IndexOutOfRangeException()
                };
                var newSectorObject = UnityEngine.Object.Instantiate(sector.Object);
                newSectorObject.SetActive(false);
                newSectorObject.transform.SetParent(sectorParent, false);
                newSectorObject.transform.localPosition = sector.Position;
                newSectorObject.transform.localEulerAngles = sector.Euler;
                newSectorObject.transform.localScale = sector.Scale;
                newSectorObject.GetComponent<ShirtVisual>().Rig = this;
                newObjects.Add(newSectorObject);
                newSectorObject.SetActive(true);
            }
            Cooldown = false;
            Objects.Add(myShirt, newObjects);
            OnShirtWorn?.Invoke();
        }

        public void RemoveShirt()
        {
            if (Shirt == null) return;

            if (Shirt != null && Objects.ContainsKey(Shirt))
            {
                var setObjects = Objects[Shirt];
                setObjects.ForEach(a => a.SetActive(false));
                OnShirtRemoved?.Invoke();
            }
            Shirt = null;
        }

        public void OffsetNameTag(int offset)
        {
            NameTagOffset = offset;
            MoveNameTag();
        }

        public virtual void MoveNameTag()
        {
            int offset = Shirt != null ? NameTagOffset : 0;

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
