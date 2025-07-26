using System;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtColourProfile : MonoBehaviour
    {
        public ShirtHumanoid Humanoid;

        public VRRig Rig;

        public Color Colour => playerColour;

        private Color playerColour;

        public Action OnColourChanged;

        public void CheckForVRRig()
        {
            if (Rig == null || !Rig) Rig = Humanoid?.Root?.GetComponent<VRRig>() ?? GetComponentInParent<VRRig>();
        }

        public void OnEnable()
        {
            CheckForVRRig();

            if (Rig != null)
            {
                Rig.OnColorChanged += SetColour;
                playerColour = Rig.playerColor;
            }
            else
            {
                playerColour = Humanoid.MainSkin.material.color;
            }

            //Logging.Info($"ShirtVisual OnEnable {Mathf.RoundToInt(playerColour.r * 255)}, {Mathf.RoundToInt(playerColour.g * 255)}, {Mathf.RoundToInt(playerColour.b * 255)}");
        }

        public void OnDisable()
        {
            CheckForVRRig();

            playerColour = Color.white;
            if (Rig != null) Rig.OnColorChanged -= SetColour;
        }

        public void SetColour(Color colour)
        {
            //Logging.Info($"ShirtVisual SetColour {Mathf.RoundToInt(colour.r * 255)}, {Mathf.RoundToInt(colour.g * 255)}, {Mathf.RoundToInt(colour.b * 255)}");

            playerColour = colour;
            OnColourChanged?.Invoke();
        }
    }
}
