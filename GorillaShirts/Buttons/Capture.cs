using System;
using System.IO;
using GorillaShirts.Behaviours;
using GorillaShirts.Extensions;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using UnityEngine;

namespace GorillaShirts.Buttons
{
    internal class Capture : IStandButton
    {
        private bool photoSnapped = false;

        public EButtonType ButtonType => EButtonType.Capture;

        public void ButtonActivation()
        {
            if (photoSnapped) return;
            photoSnapped = true;

            var corutine = Singleton<Main>.Instance.Stand.Camera.SnapPhoto(OnPhotoSnapped);
            Singleton<Main>.Instance.StartCoroutine(corutine);

            Singleton<Main>.Instance.PlaySound(EShirtAudio.CameraShutter);
        }

        public void OnPhotoSnapped(Texture2D texture)
        {
            if (!photoSnapped) return;
            photoSnapped = false;

            string directory = Path.Combine(Path.GetDirectoryName(typeof(Main).Assembly.Location), "Photos");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string file = Path.Combine(directory, $"{DateTime.Now:yy-MM-dd-HH-mm-ss-ff}.png");
            File.WriteAllBytes(file, texture.EncodeToPNG());
        }
    }
}
