using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;

namespace GorillaShirts.Buttons
{
    internal class RigToggle : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.RigToggle;
        public Action<Main> Function => (Main constructor) =>
        {
            Configuration config = constructor.Config;
            Stand shirtStand = constructor.Stand;

            config.SetCurrentPreview(config.CurrentPreview.Value == Configuration.PreviewTypes.Silly, true);
            shirtStand.Rig.SetAppearance(config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);
        };
    }
}
