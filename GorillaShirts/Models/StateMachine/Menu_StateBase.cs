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

        public void SetSidebarState(SidebarState state)
        {
            stand.infoButtonObject.SetActive(state != SidebarState.PackBrowser);
            stand.packBrowserButtonObject.SetActive(state == SidebarState.PackNavigation || state == SidebarState.PackBrowser);
            stand.rigButtonObject.SetActive(state == SidebarState.ShirtNavigation);
            stand.captureButtonObject.SetActive(state == SidebarState.ShirtNavigation);
            stand.shuffleButtonObject.SetActive(state == SidebarState.ShirtNavigation);
            stand.tagOffsetControlObject.SetActive(state == SidebarState.ShirtNavigation);
            stand.favouriteButtonObject.SetActive(state == SidebarState.ShirtNavigation);
        }

        public enum SidebarState
        {
            Info,
            PackNavigation,
            PackBrowser,
            ShirtNavigation
        }
    }
}
