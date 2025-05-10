using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class Randomize : IStandButton
    {
        public EButtonType ButtonType => EButtonType.Randomize;

        public void ButtonActivation()
        {
            if (Main.Instance.HasPack)
            {
                Main.Instance.CurrentPack.Shuffle();
                Main.Instance.Stand.Display.UpdateDisplay(navigationInfo: Main.Instance.Selection, wornShirts: Main.Instance.LocalRig.RigHandler.Shirts);
            }

            Singleton<Main>.Instance.PlaySound(EShirtAudio.DiceRoll);
        }
    }
}
