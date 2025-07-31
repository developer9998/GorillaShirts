using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Extensions;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_PackBrowser(Stand stand, Menu_StateBase previousState) : Menu_SubState(stand, previousState)
    {
        private ReleaseInfo CurrentInfo => releases[releaseIndex];

        private ReleaseInfo[] releases;
        private static int releaseIndex;

        private static readonly Dictionary<ReleaseInfo, ReleaseState> releaseStates = [];
        private static readonly Dictionary<ReleaseInfo, PackDescriptor> packReleases = [];
        private bool isProcessing;

        private bool doRotation;
        private float rotationTimer = 0;
        private List<IGorillaShirt> shirtsToRotate = null;
        private readonly Stack<IGorillaShirt> rotationStack = [];

        public override void Enter()
        {
            releases = [.. Main.Instance.Releases.OrderBy(info => info.Rank)];
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

            stand.shirtStatusText.text = GetState(info) switch
            {
                ReleaseState.None => "Install",
                ReleaseState.Processing => "Processing",
                ReleaseState.HasRelease => "<color=green>Installed</color>",
                _ => "what"
            };

            StringBuilder str = new();
            str.AppendLine(info.Description.EnforceLength(256));

            if (info.AlsoKnownAs is string[] alternativeNames && alternativeNames.Length > 0)
                str.AppendLine().Append("<color=#FF4C4C><size=4>AKA: ").Append(string.Join(", ", alternativeNames)).Append("</size></color>");

            stand.descriptionText.text = str.ToString();

            if (packReleases.TryGetValue(info, out PackDescriptor pack))
            {
                doRotation = true;

                if (shirtsToRotate == null || shirtsToRotate != pack.Shirts)
                {
                    shirtsToRotate = pack.Shirts;
                    rotationStack.Clear();
                    Rotate();
                }
            }
            else
            {
                doRotation = false;
                shirtsToRotate = null;
                rotationStack.Clear();

                stand.Character.WearSignatureShirt();
            }
        }

        public ReleaseState GetState(ReleaseInfo info)
        {
            if (info is null) return ReleaseState.None;

            if (releaseStates.ContainsKey(info)) return releaseStates[info];

            if (GetPackFromRelease(info) is PackDescriptor pack && pack)
            {
                SetState(info, ReleaseState.HasRelease);
                return ReleaseState.HasRelease;
            }

            return ReleaseState.None;
        }

        private static PackDescriptor GetPackFromRelease(ReleaseInfo info)
        {
            if (packReleases.ContainsKey(info)) return packReleases[info];

            if (Main.Instance.Packs is not null)
            {
                List<string> names = [info.Title];
                if (info.AlsoKnownAs is not null && info.AlsoKnownAs.Length != 0) names.AddRange(info.AlsoKnownAs);

                foreach (var pack in Main.Instance.Packs)
                {
                    if (names.Contains(pack.PackName))
                    {
                        if (!packReleases.ContainsKey(info)) packReleases.Add(info, pack);
                        return pack;
                    }
                }
            }

            return null;
        }

        public void SetState(ReleaseInfo info, ReleaseState state)
        {
            if (releaseStates.ContainsKey(info)) releaseStates[info] = state;
            else releaseStates.Add(info, state);

            if (state == ReleaseState.HasRelease && !packReleases.ContainsKey(info))
                GetPackFromRelease(info);

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

                    await Main.Instance.Content.InstallRelease(info, (step, progress) =>
                    {
                        if (!stand.packBrowserMenuRoot.activeSelf)
                        {
                            stand.Character.WearSignatureShirt();
                            stand.packBrowserMenuRoot.SetActive(true);
                            stand.mainMenuRoot.SetActive(false);
                            stand.packBrowserLabel.text = string.Format("Name: {0}<br>Author: {1}<line-height=120%><br><color=#FF4C4C>Please refrain from closing Gorilla Tag at this time!", info.Title, info.Author);
                        }

                        string stepTitle = step switch
                        {
                            0 => "Downloading Pack",
                            1 => "Installing Pack",
                            2 => "Loading Shirts",
                            _ => "huh, stop playing with me!"
                        };
                        stand.packBrowserStatus.text = string.Format("<size=60%>{0} / 3</size><br>{1}", (step + 1).ToString(), stepTitle);
                        stand.packBrowserRadial.fillAmount = progress;
                        stand.packBrowserPercent.text = $"{Mathf.FloorToInt(progress * 100)}%";
                    });
                    SetState(info, ReleaseState.HasRelease);
                    stand.packBrowserMenuRoot.SetActive(false);
                    stand.mainMenuRoot.SetActive(true);
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
            if (rotationTimer >= 1f) Rotate();
        }

        public void Rotate()
        {
            if (shirtsToRotate == null || shirtsToRotate.Count == 0)
            {
                doRotation = false;
                return;
            }

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
