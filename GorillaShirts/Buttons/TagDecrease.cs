using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;

namespace GorillaShirts.Buttons
{
    internal class TagDecrease : IStandButton
    {
        public ButtonType Type => ButtonType.TagDecrease;

        public Action<Main> Function => (Main constructor) =>
        {
            ShirtRig localRig = constructor.LocalRig;

            if (Configuration.CurrentTagOffset.Value > 0)
            {
                Configuration.CurrentTagOffset.Value--;
                Main.Instance.UpdatePlayerHash();
            }

            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
            shirtStand.Display.SetTag(Configuration.CurrentTagOffset.Value);

            if (localRig.Rig.Shirt != null) localRig.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
        };
    }
}
