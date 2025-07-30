using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Extensions;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_PackBrowser(Stand stand, Menu_StateBase previousState) : Menu_SubState(stand, previousState)
    {
        private ReleaseInfo CurrentInfo => releases[releaseIndex];

        private ReleaseInfo[] releases;
        private static int releaseIndex;

        private static readonly Dictionary<ReleaseInfo, ReleaseState> releaseStates = [];
        private bool isProcessing;

        private bool doRotation;
        private float rotationTimer = 0;
        private List<IGorillaShirt> shirtsToRotate = [];
        private readonly Stack<IGorillaShirt> rotationStack = [];

        public override void Enter()
        {
            releases = Main.Instance.Releases;
            stand.mainMenuRoot.SetActive(true);
            stand.navigationRoot.SetActive(false);
            SetSidebarState(SidebarState.PackBrowser);

            DisplayRelease();
        }

        public void DisplayRelease()
        {
            releaseIndex = releaseIndex.Wrap(0, releases.Length);
            ReleaseInfo info = CurrentInfo;

            stand.headerText.text = string.Format(stand.headerFormat, info.Title.EnforceLength(20), "Pack", info.Author.EnforceLength(30));
            stand.descriptionText.text = info.Description.EnforceLength(256);

            stand.shirtStatusText.text = GetState(info) switch
            {
                ReleaseState.None => "Install",
                ReleaseState.Processing => "Processing",
                ReleaseState.HasRelease => "<color=green>Installed</color>",
                _ => "what"
            };

            if (Main.Instance.Packs.Find(pack => pack.PackName == info.Title) is PackDescriptor pack)
            {
                doRotation = true;
                if (shirtsToRotate != pack.Shirts)
                {
                    shirtsToRotate = pack.Shirts;
                    rotationStack.Clear();
                    PerformShirtCycle();
                }
            }
            else
            {
                doRotation = false;
                rotationStack.Clear();

                stand.Character.ClearShirts(); // TODO: make stump character wear suitable shirt
            }
        }

        public ReleaseState GetState(ReleaseInfo info)
        {
            if (info is null) return ReleaseState.None;

            if (releaseStates.ContainsKey(info)) return releaseStates[info];

            if (Main.Instance.Packs is var packs && packs.Exists(pack => pack.PackName == info.Title))
            {
                SetState(info, ReleaseState.HasRelease);
                return ReleaseState.HasRelease;
            }

            return ReleaseState.None;
        }

        public void SetState(ReleaseInfo info, ReleaseState state)
        {
            if (releaseStates.ContainsKey(info)) releaseStates[info] = state;
            else releaseStates.Add(info, state);

            isProcessing = releaseStates.Values.Any(value => value == ReleaseState.Processing);
        }

        public override async void OnButtonPress(EButtonType button)
        {
            if (isProcessing) return;

            if (button == EButtonType.PackBrowser)
            {
                Main.Instance.MenuStateMachine.SwitchState(previousState);
                return;
            }

            if (button == EButtonType.NavigateSelect)
            {
                ReleaseInfo info = CurrentInfo;
                if (GetState(info) == 0)
                {
                    SetState(info, ReleaseState.Processing);
                    DisplayRelease();
                    await Main.Instance.Content.InstallRelease(info);
                    SetState(info, ReleaseState.HasRelease);
                    DisplayRelease();
                }
                return;
            }

            releaseIndex += button.GetNavDirection();
            DisplayRelease();
        }

        public override void Update()
        {
            if (!doRotation) return;

            rotationTimer += Time.unscaledDeltaTime;
            if (rotationTimer >= 1f) PerformShirtCycle();
        }

        public void PerformShirtCycle()
        {
            rotationTimer = 0f;

            IGorillaShirt single;

            if (rotationStack.Count == 0)
            {
                single = stand.Character.SingleShirt;
                if (single != null && shirtsToRotate.Contains(single)) rotationStack.Push(single);

                shirtsToRotate.Where(shirt => !rotationStack.Contains(shirt)).OrderBy(shirt => Random.value).ForEach(rotationStack.Push);
            }

            if (rotationStack.TryPop(out single)) stand.Character.SetShirt(single);
        }

        public override void Exit()
        {
            stand.mainMenuRoot.SetActive(false);
        }

        internal enum ReleaseState
        {
            None,
            Processing,
            HasRelease
        }
    }
}
