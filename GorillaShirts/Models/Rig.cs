using GorillaShirts.Behaviours.Visuals;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShirts.Models
{
    public class Rig
    {
        private bool Cooldown;
        public event Action OnShirtWorn, OnShirtRemoved;

        public Transform RigParent, Body, Head, LeftUpper, RightUpper, LeftLower, RightLower, LeftHand, RightHand;
        public SkinnedMeshRenderer RigSkin;
        public Text Nametag;
        public Dictionary<Shirt, List<GameObject>> CachedObjects = new();

        public bool Toggle = true;

        public Shirt Shirt;

        public int NameTagOffset;

        public void WearShirt(Shirt myShirt, out Shirt oldShirt)
        {
            oldShirt = Shirt;
            WearShirt(myShirt);
        }

        public void WearShirt(Shirt myShirt)
        {
            if (Shirt == myShirt && Toggle)
            {
                RemoveShirt();
                return;
            }

            if (Shirt != myShirt && !Cooldown) RemoveShirt();
            Shirt = myShirt;

            if (CachedObjects.ContainsKey(myShirt))
            {
                var setObjects = CachedObjects[myShirt];
                setObjects.ForEach(a =>
                {
                    a.SetActive(true);
                    a.GetComponent<VisualParent>().Rig = this;
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
                var newSectorObject = UnityEngine.Object.Instantiate(sector.Object, sectorParent);
                newSectorObject.transform.localPosition = sector.Position;
                newSectorObject.transform.localEulerAngles = sector.Euler;
                newSectorObject.transform.localScale = sector.Scale;
                newSectorObject.GetComponent<VisualParent>().Rig = this;
                newObjects.Add(newSectorObject);
            }
            Cooldown = false;
            CachedObjects.Add(myShirt, newObjects);
            OnShirtWorn?.Invoke();
        }

        public void RemoveShirt()
        {
            if (Shirt != null && CachedObjects.ContainsKey(Shirt))
            {
                var setObjects = CachedObjects[Shirt];
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

        public void MoveNameTag()
        {
            int offset = Shirt != null || GetType() == typeof(StandRig) ? NameTagOffset : 0;

            Vector3 offsetVector = Nametag.transform.localPosition;
            offsetVector.z = (float)-offset * 5;

            Nametag.transform.localPosition = offsetVector;
        }
    }
}
