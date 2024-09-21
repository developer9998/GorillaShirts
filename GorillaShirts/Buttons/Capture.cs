using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;
using UnityEngine;

namespace GorillaShirts.Buttons
{
    internal class Capture : IStandButton
    {
        public ButtonType Type => ButtonType.Capture;
        public Action<Main> Function => (Main constructor) =>
        {
            Camera camera = constructor.Camera;

            camera.gameObject.SetActive(true);

            constructor.PlaySound(ShirtAudio.Shutter);

            constructor.StartCoroutine(constructor.Capture(camera));
            camera.gameObject.SetActive(false);
        };
    }
}
