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
            Stand shirtStand = constructor.Stand;

            Configuration.UpdatePreviewGorilla((int)Configuration.PreviewGorillaEntry.Value, 1);
            shirtStand.Rig.SetAppearance(Configuration.PreviewGorillaEntry.Value == Configuration.PreviewGorilla.Silly);
        };
    }
}
