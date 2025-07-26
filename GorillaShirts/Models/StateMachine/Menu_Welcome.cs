using GorillaShirts.Behaviours.UI;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_Welcome(Stand stand) : Menu_StateBase(stand)
    {
        private static readonly string[] facts = ["I'll add these later"];

        public override void Enter()
        {
            base.Enter();
            stand.welcomeMenuRoot.SetActive(true);
            stand.tipText.text = facts[UnityEngine.Random.Range(0, facts.Length)];
        }

        public override void Exit()
        {
            base.Exit();
            stand.welcomeMenuRoot.SetActive(false);
        }
    }
}
