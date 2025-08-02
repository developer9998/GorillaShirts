using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if PLUGIN
using GorillaShirts.Models.UI;
using GorillaShirts.Tools;
using System.Collections.Generic;
using System.Linq;
#endif

namespace GorillaShirts.Behaviours.UI
{
    public class Sidebar : MonoBehaviour
    {
        public GameObject Root;

        public GameObject Backdrop;

#if PLUGIN
        public PushButton[] Buttons = [];
#else
        public PushButton[] Buttons = new PushButton[0];
#endif

        public GameObject packBrowserButtonNewSymbol;

        public Image favouriteButtonSymbol;

        public GameObject sillyHeadObject, steadyHeadObject;

        public GameObject tagOffsetControlObject;

        public TMP_Text tagOffsetText;

#if PLUGIN

        private Dictionary<EButtonType, PushButton> buttonTypeDict;

        public void Awake()
        {
            buttonTypeDict = Buttons.ToDictionary(button => button.Type, button => button);
        }

        public void SetSidebarActive(bool active)
        {
            if (Root.activeSelf != active) Root.SetActive(active);
        }

        public void SetSidebarState(SidebarState state)
        {
            if (state == SidebarState.None)
            {
                SetSidebarActive(false);
                return;
            }

            SetSidebarActive(true);

            EButtonType[] activeButtonArray = state switch
            {
                SidebarState.MainMenu => [EButtonType.Info, EButtonType.PackBrowser],
                SidebarState.ShirtView => [EButtonType.Favourite, EButtonType.RigToggle, EButtonType.Capture, EButtonType.Randomize],
                SidebarState.ReleaseView => [EButtonType.RigToggle],
                _ => []
            };

            bool tagOffsetActive = state == SidebarState.ShirtView || state == SidebarState.ReleaseView;
            if (tagOffsetControlObject.activeSelf != tagOffsetActive) tagOffsetControlObject.SetActive(tagOffsetActive);

            EButtonType[] ignoreButtonArray = state switch
            {
                SidebarState.MainMenu => [EButtonType.Favourite],
                _ => []
            };

            buttonTypeDict.Keys.Except(ignoreButtonArray).ForEach(type => SetButtonActive(type, activeButtonArray.Contains(type)));
        }

        public void SetButtonActive(EButtonType type, bool active)
        {
            if (!buttonTypeDict.TryGetValue(type, out PushButton button))
            {
                Logging.Warning($"Sidebar {Root.name} does not encompass button of type {type.GetName()}");
                return;
            }

            if (button.Root.activeSelf != active) button.Root.SetActive(active);
        }

        public void UpdateSidebar()
        {
            sillyHeadObject.SetActive(Main.Instance.ShirtStand.Character.Preference == ECharacterPreference.Feminine);
            steadyHeadObject.SetActive(Main.Instance.ShirtStand.Character.Preference == ECharacterPreference.Masculine);
            tagOffsetText.text = HumanoidContainer.LocalHumanoid.NameTagOffset.ToString();
        }

        public enum SidebarState
        {
            None,
            MainMenu,
            ShirtView,
            ReleaseView
        }
#endif
    }
}
