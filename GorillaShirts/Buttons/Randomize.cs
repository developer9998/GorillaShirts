using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;

namespace GorillaShirts.Buttons
{
    internal class Randomize : IStandButton
    {
        public ButtonType Type => ButtonType.Randomize;
        public Action<Main> Function => (Main constructor) =>
        {
            Pack selectedPack = constructor.SelectedPack;

            selectedPack.Randomize();

            Stand shirtStand = constructor.Stand;
            ShirtRig localRig = constructor.LocalRig;
            Shirt selectedShirt = constructor.SelectedShirt;

            shirtStand.Rig.WearShirt(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.Shirt, selectedPack);

            constructor.PlaySound(ShirtAudio.DiceRoll);
        };
    }
}
