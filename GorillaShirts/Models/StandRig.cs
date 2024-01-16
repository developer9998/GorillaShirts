using GorillaShirts.Tools;
using System;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class StandRig : Rig
    {
        public event Action<Configuration.PreviewTypes> OnAppearanceChange;

        public Color
            SteadyColour = new(0.8f, 0.2f, 0.2f),
            SillyColour = new(1f, 0.5f, 0.9f);

        public MeshRenderer
            SteadyHat, SillyHat;

        public void SetAppearance(bool isSilly)
        {
            SteadyHat.enabled = !isSilly;
            SillyHat.enabled = isSilly;
            RigSkin.material.color = isSilly ? SillyColour : SteadyColour;
            Nametag.text = isSilly ? "SILLY" : "STEADY";

            OnAppearanceChange?.Invoke(isSilly ? Configuration.PreviewTypes.Silly : Configuration.PreviewTypes.Steady);
        }
    }
}
