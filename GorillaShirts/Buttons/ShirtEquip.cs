using System;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class ShirtEquip : IStandButton
    {
        public ButtonType Type => ButtonType.ShirtEquip;

        public Action<Main> Function => (Main constructor) =>
        {
            var selectedShirt = constructor.SelectedShirt;
            ShirtRig localRig = constructor.LocalRig;

            constructor.UpdatePlayerHash(true);
            constructor.Stand.Display.SetEquipped(selectedShirt, localRig.Rig.Shirt);
        };
    }
}
