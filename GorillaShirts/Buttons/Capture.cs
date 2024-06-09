using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GorillaShirts.Buttons
{
    internal class Capture : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.Capture;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            Camera camera = constructor.Camera;

            camera.gameObject.SetActive(true);

            constructor.PlaySound(ShirtAudio.Shutter);

            constructor.StartCoroutine(constructor.Capture(camera));
            camera.gameObject.SetActive(false);
        };
    }
}
