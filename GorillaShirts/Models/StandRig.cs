using GorillaShirts.Tools;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShirts.Models
{
    public class StandRig : Rig
    {
        public event Action<Configuration.PreviewGorilla> OnAppearanceChange;

        public Color
            SteadyColour = new(0.8f, 0.2f, 0.2f),
            SillyColour = new(1f, 0.5f, 0.9f);

        public MeshRenderer
            SteadyHat, SillyHat;

        public Text StandNameTag;

        public void SetAppearance(bool isSilly)
        {
            SteadyHat.enabled = !isSilly;
            SillyHat.enabled = isSilly;
            RigSkin.material.color = isSilly ? SillyColour : SteadyColour;
            StandNameTag.text = isSilly ? "SILLY" : "STEADY";

            OnAppearanceChange?.Invoke(isSilly ? Configuration.PreviewGorilla.Silly : Configuration.PreviewGorilla.Steady);
        }

        public override void MoveNameTag()
        {
            int offset = NameTagOffset;
            MoveNameTagTransform(StandNameTag.transform, offset);
        }
    }
}
