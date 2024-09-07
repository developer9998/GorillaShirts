using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;

namespace GorillaShirts.Buttons
{
    internal class TagIncrease : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.TagIncrease;
        public Action<Main> Function => (Main constructor) =>
        {
            ShirtRig localRig = constructor.LocalRig;

            if (Configuration.CurrentTagOffset.Value < Constants.TagOffsetLimit)
            {
                Networking networking = constructor.Networking;

                Configuration.CurrentTagOffset.Value++;
                networking.UpdateProperties(networking.GenerateHashtable(localRig.Rig.CurrentShirt, Configuration.CurrentTagOffset.Value));
            }

            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.SetTagOffset(Configuration.CurrentTagOffset.Value);
            shirtStand.Display.SetTag(Configuration.CurrentTagOffset.Value);

            if (localRig.Rig.CurrentShirt != null) localRig.Rig.SetTagOffset(Configuration.CurrentTagOffset.Value);
        };
    }
}
