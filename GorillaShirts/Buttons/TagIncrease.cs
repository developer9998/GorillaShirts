using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;

namespace GorillaShirts.Buttons
{
    internal class TagIncrease : IStandButton
    {
        public ButtonType Type => ButtonType.TagIncrease;

        public Action<Main> Function => (Main constructor) =>
        {
            if (Configuration.CurrentTagOffset.Value < Constants.TagOffsetLimit)
            {
                Configuration.CurrentTagOffset.Value++;
                Main.Instance.UpdatePlayerHash();
            }
            Main.Instance.Stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
        };
    }
}
