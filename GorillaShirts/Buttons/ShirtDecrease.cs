using System;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class ShirtDecrease : IStandButton
    {
        public ButtonType Type => ButtonType.ShirtDecrease;
        public Action<Main> Function => (Main constructor) =>
        {
            var selectedPack = constructor.SelectedPack;
            selectedPack.Navigate(-1);

            var selectedShirt = constructor.SelectedShirt;
            ShirtRig localRig = constructor.LocalRig;
            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.WearShirt(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.Shirt, selectedPack);
        };
    }
}
