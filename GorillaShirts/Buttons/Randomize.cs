using System;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class Randomize : IStandButton
    {
        public ButtonType Type => ButtonType.Randomize;
        public Action<Main> Function => (Main constructor) =>
        {
            var selectedPack = constructor.SelectedPack;

            //selectedPack.Randomize();

            Stand shirtStand = constructor.Stand;
            ShirtRig localRig = constructor.LocalRig;
            var selectedShirt = constructor.SelectedShirt;

            shirtStand.Rig.WearShirt(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.Shirt, selectedPack);

            constructor.PlaySound(ShirtAudio.DiceRoll);
        };
    }
}
