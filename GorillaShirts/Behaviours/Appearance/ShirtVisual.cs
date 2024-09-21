using GorillaShirts.Models;
using System;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtVisual : MonoBehaviour
    {
        public Rig Rig;

        public VRRig PlayerRig => Rig.RigParent?.GetComponent<VRRig>();

        public Color PlayerColor;

        public Color SkinColor;

        public Action OnColourApplied;

        public void Awake()
        {
            SkinColor = Rig.RigSkin.material.color;
            if (PlayerRig)
            {
                PlayerColor = PlayerRig.myDefaultSkinMaterialInstance.color;
                PlayerRig.OnColorChanged += OnColorChanged;
            }
        }

        public void OnColorChanged(Color colour)
        {
            PlayerColor = colour;
            SkinColor = Rig.RigSkin.material.color;
            OnColourApplied?.Invoke();
        }
    }
}
