using BepInEx.Configuration;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GorillaShirts.Tools
{
    public static class Configuration
    {
        private static ConfigFile File;

        public static ConfigEntry<string> CurrentShirt;
        public static ConfigEntry<int> CurrentTagOffset;

        public static ConfigEntry<PreviewGorilla> PreviewGorillaEntry;

        private static int PreviewGorillaLength => Enum.GetNames(typeof(PreviewGorilla)).Length;

        public static void Initialize(ConfigFile file)
        {
            File = file;

            CurrentShirt = File.Bind("Prior Data", "Current Shirt", "None", "The currently used shirt.");
            CurrentTagOffset = File.Bind("Prior Data", "Current Tag Offset", 1, "The used offset on nametags.");
            CurrentTagOffset.Value = Mathf.Clamp(CurrentTagOffset.Value, 0, Constants.TagOffsetLimit);

            PreviewGorilla defaultPreview = (PreviewGorilla)Random.Range(0, PreviewGorillaLength);
            PreviewGorillaEntry = File.Bind("Appearence", "Preview Gorilla", defaultPreview, "The gorilla character that is shown when previewing a shirt.");
        }

        public static void UpdateGorillaShirt(string shirtName)
        {
            CurrentShirt.Value = shirtName;
            File.Save();
        }

        public static void UpdatePreviewGorilla(int value, int offset)
        {
            value += offset;
            int number = value % PreviewGorillaLength;
            PreviewGorillaEntry.Value = (PreviewGorilla)number;
            File.Save();
        }

        public enum PreviewGorilla
        {
            Silly,
            Steady
        }
    }
}
