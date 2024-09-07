using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;

namespace GorillaShirts.Buttons
{
    internal class ShirtEquip : IStandButton
    {
        public ButtonType Type => ButtonType.ShirtEquip;
        public Action<Main> Function => (Main constructor) =>
        {
            Shirt selectedShirt = constructor.SelectedShirt;
            ShirtRig localRig = constructor.LocalRig;

            constructor.SetShirt(selectedShirt);
            constructor.Stand.Display.SetEquipped(selectedShirt, localRig.Rig.CurrentShirt);
        };
    }
}
