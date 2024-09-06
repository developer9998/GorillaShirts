using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;

namespace GorillaShirts.Buttons
{
    internal class ShirtIncrease : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.ShirtDecrease;
        public Action<Main> Function => (Main constructor) =>
        {
            Pack selectedPack = constructor.SelectedPack;
            int currentItem = selectedPack.CurrentItem;

            selectedPack.CurrentItem = (currentItem + 1) % selectedPack.PackagedShirts.Count;

            Shirt selectedShirt = constructor.SelectedShirt;
            PhysicalRig localRig = constructor.LocalRig;
            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.Wear(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.CurrentShirt, selectedPack);
        };
    }
}
