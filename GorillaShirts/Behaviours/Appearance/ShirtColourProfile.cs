using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    public class ShirtColourProfile : MonoBehaviour
    {
        public readonly bool ProvideLogging = false;

        public ShirtHumanoid Humanoid;

        public VRRig Rig;

        public Color Colour => playerColour;

        private Color playerColour;

        private readonly List<(MonoBehaviour behaviour, Action<Color> callback)> recievers = [];

        public void CheckForVRRig()
        {
            if (Rig == null) Rig = Humanoid?.Root?.GetComponent<VRRig>();// ?? GetComponentInParent<VRRig>();
        }

        public void OnEnable()
        {
            LogMessage("ShirtColourProfile Enable");
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

            foreach(var (behaviour, callback) in recievers)
            {
                LogInfo(behaviour.GetType().Name);
                behaviour.enabled = true;
                callback?.Invoke(playerColour);
            }

            //Logging.Info($"ShirtVisual OnEnable {Mathf.RoundToInt(playerColour.r * 255)}, {Mathf.RoundToInt(playerColour.g * 255)}, {Mathf.RoundToInt(playerColour.b * 255)}");
        }

        public void OnDisable()
        {
            LogMessage("ShirtColourProfile Disable");
            CheckForVRRig();

            if (Rig != null) Rig.OnColorChanged -= SetColour;
            playerColour = Color.white;

            foreach (var (behaviour, callback) in recievers)
            {
                LogInfo(behaviour.GetType().Name);
                behaviour.enabled = false;
            }
        }

        public void SetColour(Color colour)
        {
            LogMessage("ShirtColourProfile SetColour");

            playerColour = colour;

            foreach (var (behaviour, callback) in recievers)
            {
                LogInfo(behaviour.GetType().Name);
                callback?.Invoke(playerColour);
            }
        }

        public void AddRecipient(MonoBehaviour behaviour, Action<Color> callback)
        {
            if (behaviour == null || callback == null || recievers.Exists(pair => pair.behaviour == behaviour)) return;
            LogInfo($"Added recipient: {behaviour.GetType().Name} with callback {callback.Method.Name}");
            recievers.Add((behaviour, callback));
            if (isActiveAndEnabled) callback?.Invoke(playerColour);
        }

        public void LogMessage(object data)
        {
            if (!ProvideLogging) return;
            Logging.Message(data);
        }

        public void LogInfo(object data)
        {
            if (!ProvideLogging) return;
            Logging.Info(data);
        }
    }
}
