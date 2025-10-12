using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.UI;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Welcome(Stand stand) : Menu_StateBase(stand)
    {
        public override void Enter()
        {
            base.Enter();
            Stand.welcomeMenuRoot.SetActive(true);
        }

        public override void OnButtonPress(EButtonType button)
        {
            base.OnButtonPress(button);
            ShirtManager.Instance.Initialize();
        }

        public override void Exit()
        {
            base.Exit();
            Stand.welcomeMenuRoot.SetActive(false);
        }
    }
}
