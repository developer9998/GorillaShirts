using GorillaShirts.Behaviours.UI;
using GorillaShirts.Models.UI;
using System;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_StateBase(Stand stand) : State
    {
        protected Stand Stand = stand ?? throw new ArgumentNullException(nameof(stand));

        public virtual void OnButtonPress(EButtonType button)
        {

        }

        public void SetSidebarState(SidebarState state)
        {
            Stand.infoButtonObject.SetActive(state != SidebarState.PackBrowser);
            Stand.packBrowserButtonObject.SetActive(state == SidebarState.PackNavigation || state == SidebarState.PackBrowser);
            Stand.rigButtonObject.SetActive(state == SidebarState.ShirtNavigation);
            Stand.captureButtonObject.SetActive(state == SidebarState.ShirtNavigation);
            Stand.shuffleButtonObject.SetActive(state == SidebarState.ShirtNavigation);
            Stand.tagOffsetControlObject.SetActive(state == SidebarState.ShirtNavigation);
            Stand.favouriteButtonObject.SetActive(state == SidebarState.ShirtNavigation);
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
