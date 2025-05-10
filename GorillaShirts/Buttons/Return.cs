using System.Linq;
using GorillaShirts.Behaviours;
using GorillaShirts.Extensions;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    public class Return : IStandButton
    {
        public EButtonType ButtonType => EButtonType.Return;

        public void ButtonActivation()
        {
            if (Main.Instance.HasPack)
            {
                Main.Instance.CurrentPack = null; // property, setter will only pop the stack responsible for the getter, so may not be null after such

                var stand = Main.Instance.Stand;

                stand.Display.UpdateDisplay(Main.Instance.Selection);

                if (Main.Instance.HasPack)
                {
                    stand.Rig.StopCycle();
                    stand.Rig.Shirts = Main.Instance.SelectedShirt.WithShirts(Main.Instance.LocalRig.RigHandler.Shirts);
                }
                else
                {
                    stand.Rig.StartCycle(Main.Instance.SelectedPack.Items);
                }
            }
        }
    }
}
