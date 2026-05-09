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
            Stand.mainSideBar.SetSidebarActive(false);

            string build_config = "Unrecognized";
#if DEBUG
            build_config = "Debug";
#elif RELEASE
            build_config = "Release";
#endif

            Stand.playerInfoText.text = Stand.playerInfoFormat
                .Replace("[SHIRTCOUNT]", ShirtManager.Instance.Packs.Select(pack => pack.Shirts.Count).Sum().ToString())
                .Replace("[PACKCOUNT]", ShirtManager.Instance.Packs.Count(pack => pack != ShirtManager.Instance.FavouritePack).ToString())
                .Replace("[RELEASECOUNT]", ShirtManager.Instance.Packs.Count(pack => pack.Release is not null).ToString())
                .Replace("[BUILDCONFIG]", build_config)
                .Replace("[VERSION]", Constants.Version)
                .Replace("[PLAYERNAME]", GorillaTagger.Instance.offlineVRRig.NormalizeName(true, NetworkSystem.Instance.GetMyNickName()));
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button != EButtonType.Return) return;
            ShirtManager.Instance.MenuStateMachine.SwitchState(PreviousState);
        }

        public override void Exit()
        {
            base.Exit();
            Stand.mainMenuRoot.SetActive(false);
            Stand.mainContentRoot.SetActive(true);
            Stand.infoContentRoot.SetActive(false);
            Stand.mainSideBar.SetSidebarActive(true);
        }
    }
}
