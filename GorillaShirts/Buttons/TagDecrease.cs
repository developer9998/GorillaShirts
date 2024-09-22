using GorillaShirts.Behaviours;
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
            if (Configuration.CurrentTagOffset.Value > 0)
            {
                Configuration.CurrentTagOffset.Value--;
                Main.Instance.UpdatePlayerHash();
            }
            Main.Instance.Stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
        };
    }
}
