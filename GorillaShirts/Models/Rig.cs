using GorillaExtensions;
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

        public Shirt ActiveShirt;
        public Dictionary<Shirt, List<GameObject>> CachedObjects = new();

        public bool Toggle = true;

        public void Wear(Shirt myShirt, out Shirt preShirt, out Shirt postShirt)
        {
            preShirt = ActiveShirt;
            Wear(myShirt);
            postShirt = ActiveShirt;
        }

        public void Wear(Shirt myShirt)
        {
            if (ActiveShirt == myShirt && Toggle)
            {
                Remove();
                return;
            }

            if (ActiveShirt != myShirt && !Cooldown) Remove();
            ActiveShirt = myShirt;

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

        public void Remove()
        {
            if (ActiveShirt != null && CachedObjects.ContainsKey(ActiveShirt))
            {
                var setObjects = CachedObjects[ActiveShirt];
                setObjects.ForEach(a => a.SetActive(false));
                OnShirtRemoved?.Invoke();
            }
            ActiveShirt = null;
        }

        public void SetTagOffset(int offset) => Nametag.transform.localPosition = Nametag.transform.localPosition.WithZ((float)-offset * 5);
    }
}
