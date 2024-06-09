using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaShirts.Buttons
{
    internal class TagDecrease : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.TagDecrease;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            Configuration config = constructor.Config;
            PhysicalRig localRig = constructor.LocalRig;

            if (config.CurrentTagOffset.Value > 0)
            {
                Networking networking = constructor.Networking;

                config.CurrentTagOffset.Value--;
                networking.UpdateProperties(networking.GenerateHashtable(localRig.Rig.CurrentShirt, config.CurrentTagOffset.Value));
            }

            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.SetTagOffset(config.CurrentTagOffset.Value);
            shirtStand.Display.SetTag(config.CurrentTagOffset.Value);

            if (localRig.Rig.CurrentShirt != null) localRig.Rig.SetTagOffset(config.CurrentTagOffset.Value);
        };
    }
}
