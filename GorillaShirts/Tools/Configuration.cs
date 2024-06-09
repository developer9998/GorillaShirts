using BepInEx.Configuration;
using UnityEngine;

namespace GorillaShirts.Tools
{
    public class Configuration
    {
        private ConfigFile File;

        public ConfigEntry<string> CurrentShirt;
        public ConfigEntry<int> CurrentTagOffset;

        public ConfigEntry<PreviewTypes> CurrentPreview;
        public enum PreviewTypes
        {
            Silly,
            Steady
        }

        public ConfigEntry<bool> RemoveBaseItem;

        public Configuration(ConfigFile file)
        {
            File = file;

            CurrentShirt = File.Bind("Prior Data", "Current Shirt", "None", "The currently used shirt.");
            CurrentTagOffset = File.Bind("Prior Data", "Current Tag Offset", 1, "The used offset on nametags.");
            CurrentTagOffset.Value = Mathf.Clamp(CurrentTagOffset.Value, 0, Constants.TagOffsetLimit);

            PreviewTypes defaultPreview = (PreviewTypes)Random.Range(0, 2);
            CurrentPreview = File.Bind("Prior Data", "Current Preview Character", defaultPreview, "The currently used preview character.");

            RemoveBaseItem = File.Bind("General Data", "Remove Game Item", true, "If enabled, the player cannot have a badge item and shirt on at the same time");
        }

        public void SetCurrentShirt(string shirtName)
        {
            CurrentShirt.Value = shirtName;
            File.Save();
        }

        public void SetCurrentPreview(bool isSilly, bool opposite)
        {
            CurrentPreview.Value = isSilly ? opposite ? PreviewTypes.Steady : PreviewTypes.Silly : opposite ? PreviewTypes.Silly : PreviewTypes.Steady;
            File.Save();
        }
    }
}
