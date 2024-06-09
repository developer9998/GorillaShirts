using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GorillaShirts.Buttons
{
    internal class Randomize : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.Randomize;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            Pack selectedPack = constructor.SelectedPack;

            selectedPack.Randomize();

            Stand shirtStand = constructor.Stand;
            PhysicalRig localRig = constructor.LocalRig;
            Shirt selectedShirt = constructor.SelectedShirt;

            shirtStand.Rig.Wear(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.CurrentShirt, selectedPack);

            constructor.PlaySound(ShirtAudio.DiceRoll);
        };
    }
}
