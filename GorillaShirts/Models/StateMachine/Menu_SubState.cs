using GorillaShirts.Behaviours.UI;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_SubState(Stand stand, Menu_StateBase previousState) : Menu_StateBase(stand)
    {
        public Menu_StateBase previousState = previousState;
    }
}
