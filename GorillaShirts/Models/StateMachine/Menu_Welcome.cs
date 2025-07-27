using GorillaShirts.Behaviours.UI;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Welcome(Stand stand) : Menu_StateBase(stand)
    {
        public override void Enter()
        {
            base.Enter();
            stand.welcomeMenuRoot.SetActive(true);
        }

        public override void Exit()
        {
            base.Exit();
            stand.welcomeMenuRoot.SetActive(false);
        }
    }
}
