using System.Linq;
using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Extensions;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class ShirtEquip : IStandButton
    {
        public EButtonType ButtonType => EButtonType.ShirtEquip;

        public void ButtonActivation()
        {
            if (Main.Instance.Stand is not Stand stand)
                return;

            if (Main.Instance.HasPack)
            {
                Main.Instance.UpdateWornShirt();
            }
            else
            {
                Main.Instance.CurrentPack = Main.Instance.SelectedPack;
                stand.Rig.StopCycle();
            }

            stand.Rig.Shirts = Main.Instance.SelectedShirt.WithShirts(Main.Instance.LocalRig.RigHandler.Shirts);
            stand.Display.UpdateDisplay(navigationInfo: Main.Instance.Selection, wornShirts: Main.Instance.LocalRig.RigHandler.Shirts);
        }
    }
}
