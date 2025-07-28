using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.UI;
using System;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_StateBase(Stand stand) : State
    {
        public Stand Stand => stand;

        protected Stand stand = stand ?? throw new ArgumentNullException(nameof(stand));

        public virtual void OnButtonPress(EButtonType button)
        {

        }

        public void SetSidebarState(bool active)
        {
            stand.rigButtonObject.SetActive(active);
            stand.captureButtonObject.SetActive(active);
            stand.shuffleButtonObject.SetActive(active);
            stand.tagOffsetControlObject.SetActive(active);
            stand.favouriteButtonObject.SetActive(active);
        }
    }
}
