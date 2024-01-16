using BepInEx.Configuration;
using Bepinject;
using UnityEngine;

namespace GorillaShirts.Tools
{
    public class Configuration
    {
        private readonly BepInConfig Config;

        public ConfigEntry<string> CurrentShirt;
        public ConfigEntry<int> CurrentTagOffset;

        public ConfigEntry<PreviewTypes> CurrentPreview;
        public enum PreviewTypes
        {
            Silly,
            Steady
        }

        public ConfigEntry<bool> RemoveBaseItem;

        public Configuration(BepInConfig config)
        {
            Config = config;

            CurrentShirt = Config.Config.Bind("Prior Data", "Current Shirt", "None", "The currently used shirt.");
            CurrentTagOffset = Config.Config.Bind("Prior Data", "Current Tag Offset", 1, "The used offset on nametags.");
            CurrentTagOffset.Value = Mathf.Clamp(CurrentTagOffset.Value, 0, Constants.TagOffsetLimit);

            PreviewTypes defaultPreview = (PreviewTypes)Random.Range(0, 2);
            CurrentPreview = Config.Config.Bind("Prior Data", "Current Preview Character", defaultPreview, "The currently used preview character.");

            RemoveBaseItem = Config.Config.Bind("General Data", "Remove Game Item", true, "If enabled, the player cannot have a badge item and shirt on at the same time");
        }

        public void SetCurrentShirt(string shirtName)
        {
            CurrentShirt.Value = shirtName;
            Config.Config.Save();
        }

        public void SetCurrentPreview(bool isSilly, bool opposite)
        {
            CurrentPreview.Value = isSilly ? opposite ? PreviewTypes.Steady : PreviewTypes.Silly : opposite ? PreviewTypes.Silly : PreviewTypes.Steady;
            Config.Config.Save();
        }
    }
}
