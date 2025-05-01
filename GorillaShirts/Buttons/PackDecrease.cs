using System;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class PackDecrease : IStandButton
    {
        public ButtonType Type => ButtonType.PackDecrease;
        public Action<Main> Function => (Main constructor) =>
        {
            constructor.SelectedPackIndex = constructor.SelectedPackIndex == 0 ? constructor.Packs.Count - 1 : constructor.SelectedPackIndex - 1;

            Pack<IShirtAsset> selectedPack = constructor.SelectedPack;

            constructor.SetPackInfo(selectedPack, selectedPack.Items[selectedPack.Selection]);

            var selectedShirt = constructor.SelectedShirt;
            ShirtRig localRig = constructor.LocalRig;
            Stand shirtStand = constructor.Stand;

            shirtStand.Rig.WearShirt(selectedShirt);
            shirtStand.Display.UpdateDisplay(selectedShirt, localRig.Rig.Shirt, selectedPack);
        };
    }
}
