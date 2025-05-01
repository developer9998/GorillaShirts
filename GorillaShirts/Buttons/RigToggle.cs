using System;
using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;

namespace GorillaShirts.Buttons
{
    internal class RigToggle : IStandButton
    {
        public ButtonType Type => ButtonType.RigToggle;
        public Action<Main> Function => (Main constructor) =>
        {
            Stand shirtStand = constructor.Stand;

            Configuration.UpdatePreviewGorilla((int)Configuration.PreviewGorillaEntry.Value, 1);
            shirtStand.Rig.SetAppearance(Configuration.PreviewGorillaEntry.Value == Configuration.PreviewGorilla.Silly);
        };
    }
}
