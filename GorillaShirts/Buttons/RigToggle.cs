using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaShirts.Buttons
{
    internal class RigToggle : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.RigToggle;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            Configuration config = constructor.Config;
            Stand shirtStand = constructor.Stand;

            config.SetCurrentPreview(config.CurrentPreview.Value == Configuration.PreviewTypes.Silly, true);
            shirtStand.Rig.SetAppearance(config.CurrentPreview.Value == Configuration.PreviewTypes.Silly);
        };
    }
}
