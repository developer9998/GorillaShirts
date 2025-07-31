using GorillaShirts.Behaviours.UI;
using System;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_SubState(Stand stand, Menu_StateBase previousState) : Menu_StateBase(stand)
    {
        public Menu_StateBase PreviousState = previousState ?? throw new ArgumentNullException(nameof(previousState));
    }
}