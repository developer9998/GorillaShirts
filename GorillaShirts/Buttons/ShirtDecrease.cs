using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaShirts.Buttons
{
    internal class ShirtDecrease : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.ShirtIncrease;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            Pack selectedPack = constructor.SelectedPack;
            int currentItem = selectedPack.CurrentItem;

            selectedPack.CurrentItem = currentItem == 0 ? selectedPack.PackagedShirts.Count - 1 : currentItem - 1;

            Shirt selectedShirt = constructor.SelectedShirt;
            PhysicalRig localRig = constructor.LocalRig;
            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.Wear(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.CurrentShirt, selectedPack);
        };
    }
}
