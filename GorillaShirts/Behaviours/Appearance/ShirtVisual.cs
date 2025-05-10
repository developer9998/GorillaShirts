using System;
using GorillaShirts.Models;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtVisual : MonoBehaviour
    {
        public BaseRigHandler RigHandler;

        public VRRig PlayerRig;

        public Color Colour => playerColour;

        private Color playerColour;

        public Action OnColourChanged;

        public void CheckForVRRig()
        {
            if (PlayerRig is null)
                PlayerRig = RigHandler?.RigObject?.GetComponent<VRRig>() ?? GetComponentInParent<VRRig>();
        }

        public void OnEnable()
        {
            CheckForVRRig();

            if (PlayerRig is VRRig vrRig)
            {
                vrRig.OnColorChanged += SetColour;
                playerColour = vrRig.playerColor;
            }
            else
            {
                playerColour = RigHandler.MainSkin.material.color;
            }

            //Logging.Info($"ShirtVisual OnEnable {Mathf.RoundToInt(playerColour.r * 255)}, {Mathf.RoundToInt(playerColour.g * 255)}, {Mathf.RoundToInt(playerColour.b * 255)}");
        }

        public void OnDisable()
        {
            CheckForVRRig();

            playerColour = Color.white;

            if (PlayerRig is VRRig vrRig)
                vrRig.OnColorChanged -= SetColour;
        }

        public void SetColour(Color colour)
        {
            //Logging.Info($"ShirtVisual SetColour {Mathf.RoundToInt(colour.r * 255)}, {Mathf.RoundToInt(colour.g * 255)}, {Mathf.RoundToInt(colour.b * 255)}");

            playerColour = colour;
            OnColourChanged?.Invoke();
        }
    }
}
