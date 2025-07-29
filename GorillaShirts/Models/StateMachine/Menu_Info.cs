using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.UI;
using System.Linq;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Info(Stand stand, Menu_StateBase previousState) : Menu_StateBase(stand)
    {
        protected Menu_StateBase previousState = previousState;

        public override void Enter()
        {
            base.Enter();

            // TODO: make stump character wear appropriate shirt using plugin exclusive fallbacks
            // i.e., silly wears her croptop, steady wears his hoodie

            stand.mainMenuRoot.SetActive(true);
            stand.mainContentRoot.SetActive(false);
            stand.infoContentRoot.SetActive(true);
            SetSidebarState(false);

            string build_config = "Unrecognized";
#if DEBUG
            build_config = "Debug";
#elif RELEASE
            build_config = "Release";
#endif

            stand.playerInfoText.text = stand.playerInfoFormat
                .Replace("[SHIRTCOUNT]", Main.Instance.Packs.Select((a) => a.Shirts.Count).Sum().ToString())
                .Replace("[PACKCOUNT]", Main.Instance.Packs.Count.ToString())
                .Replace("[BUILDCONFIG]", build_config)
                .Replace("[VERSION]", Constants.Version)
                .Replace("[PLAYERNAME]", NetworkSystem.Instance.GetMyNickName());
        }

        public override void Exit()
        {
            base.Exit();
            stand.mainMenuRoot.SetActive(false);
            stand.mainContentRoot.SetActive(true);
            stand.infoContentRoot.SetActive(false);
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button != EButtonType.Info) return;
            Main.Instance.MenuStateMachine.SwitchState(previousState);
        }
    }
}
