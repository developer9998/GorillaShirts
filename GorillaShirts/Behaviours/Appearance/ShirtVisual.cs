using GorillaShirts.Models;
using System;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtVisual : MonoBehaviour
    {
        public Rig Rig;

        public VRRig PlayerRig => playerRig;

        private VRRig playerRig;

        public Color Colour => playerColour;

        private Color playerColour;

        public Action OnColourChanged;

        public void Awake()
        {
            playerRig = Rig.RigParent?.GetComponent<VRRig>();
        }

        public void Start()
        {
            UpdateColour();
        }

        public void OnEnable()
        {
            UpdateColour();
        }

        public void OnDisable()
        {
            playerColour = Color.white;

            if (playerRig)
            {
                playerRig.OnColorChanged -= TriggerColourChanged;
            }
        }

        private void UpdateColour()
        {
            if (playerRig)
            {
                UpdateColour(playerRig.playerColor);
                playerRig.OnColorChanged += TriggerColourChanged;
            }
            else
            {
                UpdateColour(Rig.RigSkin.material.color);
            }
        }

        private void UpdateColour(Color colour)
        {
            playerColour = colour;
        }

        public void TriggerColourChanged(Color colour)
        {
            UpdateColour(colour);

            OnColourChanged?.Invoke();
        }
    }
}
