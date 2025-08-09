using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtColourProfile : MonoBehaviour
    {
        public Color Colour => customColour.GetValueOrDefault(playerColour);
        public Color? CustomColour => customColour;

        public ShirtHumanoid Humanoid;

        public VRRig Rig;

        [SerializeField]
        private Color? customColour;

        private Color playerColour;

        private readonly List<(MonoBehaviour behaviour, Action<Color> callback)> recievers = [];

        public void CheckForVRRig()
        {
            if (Rig == null) Rig = Humanoid?.Root?.GetComponent<VRRig>();// ?? GetComponentInParent<VRRig>();
        }

        public void OnEnable()
        {
            //LogMessage("ShirtColourProfile Enable");

            CheckForVRRig();
            if (Rig != null)
            {
                Rig.OnColorChanged += SetPlayerColour;
                playerColour = Rig.playerColor;
            }
            else
            {
                playerColour = Humanoid.MainSkin.material.color;
            }

            foreach (var (behaviour, callback) in recievers)
            {
                //LogInfo(behaviour.GetType().Name);
                behaviour.enabled = true;
                callback?.Invoke(Colour);
            }
        }

        public void OnDisable()
        {
            //LogMessage("ShirtColourProfile Disable");
            CheckForVRRig();

            if (Rig != null) Rig.OnColorChanged -= SetPlayerColour;
            playerColour = Color.white;

            foreach (var (behaviour, callback) in recievers)
            {
                //LogInfo(behaviour.GetType().Name);
                behaviour.enabled = false;
            }
        }

        public void SetPlayerColour(Color colour)
        {
            //LogMessage("ShirtColourProfile SetColour");

            playerColour = colour;

            if (isActiveAndEnabled)
            {
                foreach (var (behaviour, callback) in recievers)
                {
                    //LogInfo(behaviour.GetType().Name);
                    callback?.Invoke(Colour);
                }
            }
        }

        public void SetCustomColour(Color? colour)
        {
            customColour = colour;

            if (isActiveAndEnabled)
            {
                foreach (var (behaviour, callback) in recievers)
                {
                    //LogInfo(behaviour.GetType().Name);
                    callback?.Invoke(Colour);
                }
            }
        }

        public void AddRecipient(MonoBehaviour behaviour, Action<Color> callback)
        {
            if (behaviour == null || callback == null || recievers.Exists(pair => pair.behaviour == behaviour)) return;
            //LogInfo($"Added recipient: {behaviour.GetType().Name} with callback {callback.Method.Name}");
            recievers.Add((behaviour, callback));
            if (isActiveAndEnabled) callback?.Invoke(Colour);
        }
    }
}
