using BepInEx.Configuration;
using Bepinject;
using UnityEngine;

namespace GorillaShirts.Behaviours.Tools
{
    public class Configuration
    {
        private readonly BepInConfig _config;

        public ConfigEntry<string> CurrentShirt { get; set; }
        public ConfigEntry<int> CurrentTagOffset { get; set; }

        public enum PreviewTypes { Silly, Steady }
        public ConfigEntry<PreviewTypes> CurrentPreview { get; set; }

        public ConfigEntry<bool> RemoveBaseItem { get; set; }

        public Configuration(BepInConfig config)
        {
            _config = config;

            CurrentShirt = _config.Config.Bind("Prior Data", "Current Shirt", "None", "The currently used shirt.");
            CurrentTagOffset = _config.Config.Bind("Prior Data", "Current Tag Offset", 1, "The used offset on nametags.");
            CurrentTagOffset.Value = Mathf.Clamp(CurrentTagOffset.Value, 0, Constants.TagOffsetLimit);

            var randomizedPreview = (PreviewTypes)Random.Range(0, 2);
            CurrentPreview = _config.Config.Bind("Prior Data", "Current Preview Character", randomizedPreview, "The currently used preview character.");

            RemoveBaseItem = _config.Config.Bind("General Data", "Remove Game Item", true, "If enabled, the player cannot have a badge item and shirt on at the same time");
        }

        public void SetCurrentShirt(string shirtName)
        {
            CurrentShirt.Value = shirtName;
            _config.Config.Save();
        }

        public void SetCurrentPreview(bool isSilly, bool opposite)
        {
            CurrentPreview.Value = isSilly ? opposite ? PreviewTypes.Steady : PreviewTypes.Silly : opposite ? PreviewTypes.Silly : PreviewTypes.Steady;
            _config.Config.Save();
        }
    }
}
