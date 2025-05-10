using GorillaShirts.Behaviours;
using GorillaShirts.Extensions;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class ShirtIncrease : IStandButton
    {
        public EButtonType ButtonType => EButtonType.ShirtIncrease;

        public void ButtonActivation()
        {
            if (Main.Instance.HasPack)
            {
                var pack = Main.Instance.CurrentPack;
                pack.Selection = MathEx.Wrap(pack.Selection + 1, 0, pack.Items.Count);
                Main.Instance.Stand.Rig.Shirts = Main.Instance.SelectedShirt.WithShirts(Main.Instance.LocalRig.RigHandler.Shirts);
                Main.Instance.Stand.Display.UpdateDisplay(navigationInfo: Main.Instance.Selection, wornShirts: Main.Instance.LocalRig.RigHandler.Shirts);
            }
            else
            {
                Main.Instance.SelectedPackIndex = MathEx.Wrap(Main.Instance.SelectedPackIndex + 1, 0, Main.Instance.Packs.Count);
                Main.Instance.Stand.Rig.StartCycle(Main.Instance.SelectedPack.Items);
                Main.Instance.Stand.Display.UpdateDisplay(navigationInfo: Main.Instance.Selection, wornShirts: Main.Instance.LocalRig.RigHandler.Shirts);
            }
        }
    }
}
