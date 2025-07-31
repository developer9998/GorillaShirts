using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.UI;
using System.Linq;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Info(Stand stand, Menu_StateBase previousState) : Menu_SubState(stand, previousState)
    {
        public override void Enter()
        {
            base.Enter();

            Stand.Character.WearSignatureShirt();
            Stand.mainMenuRoot.SetActive(true);
            Stand.mainContentRoot.SetActive(false);
            Stand.infoContentRoot.SetActive(true);
            SetSidebarState(SidebarState.Info);

            string build_config = "Unrecognized";
#if DEBUG
            build_config = "Debug";
#elif RELEASE
            build_config = "Release";
#endif

            Stand.playerInfoText.text = Stand.playerInfoFormat
                .Replace("[SHIRTCOUNT]", Main.Instance.Packs.Select(pack => pack.Shirts.Count).Sum().ToString())
                .Replace("[PACKCOUNT]", Main.Instance.Packs.Count(pack => pack != Main.Instance.FavouritePack).ToString())
                .Replace("[BUILDCONFIG]", build_config)
                .Replace("[VERSION]", Constants.Version)
                .Replace("[PLAYERNAME]", NetworkSystem.Instance.GetMyNickName());
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button != EButtonType.Info) return;
            Main.Instance.MenuStateMachine.SwitchState(PreviousState);
        }

        public override void Exit()
        {
            base.Exit();
            Stand.mainMenuRoot.SetActive(false);
            Stand.mainContentRoot.SetActive(true);
            Stand.infoContentRoot.SetActive(false);
        }
    }
}
