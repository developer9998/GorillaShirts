using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;

namespace GorillaShirts.Buttons
{
    internal class PackIncrease : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.PackIncrease;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            constructor.SelectedPackIndex = (constructor.SelectedPackIndex + 1) % constructor.ConstructedPacks.Count;

            Pack selectedPack = constructor.SelectedPack;

            constructor.SetPackInfo(selectedPack, selectedPack.PackagedShirts[selectedPack.CurrentItem]);

            Shirt selectedShirt = constructor.SelectedShirt;
            PhysicalRig localRig = constructor.LocalRig;
            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.Wear(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.CurrentShirt, selectedPack);
        };
    }
}
