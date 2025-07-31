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
        private static readonly Dictionary<Texture2D, Sprite> releaseSpriteFromTex = [];
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
            Stand.mainMenuRoot.SetActive(true);
            Stand.navigationRoot.SetActive(false);
            SetSidebarState(SidebarState.PackBrowser);
            DisplayRelease();
        }

        public void DisplayRelease()
        {
            releaseIndex = releaseIndex.Wrap(0, releases.Length);
            ReleaseInfo info = CurrentInfo;

            Stand.headerText.text = string.Format(Stand.headerFormat, info.Title.EnforceLength(20), "Pack", info.Author.EnforceLength(30));

            Stand.shirtStatusText.text = GetState(info) switch
            {
                ReleaseState.None => "Install",
                ReleaseState.Processing => "Processing",
                ReleaseState.HasRelease => "<color=green>Installed</color>",
                _ => "tell me what it is"
            };

            StringBuilder str = new();
            str.AppendLine(info.Description.EnforceLength(256));

            if (info.AlsoKnownAs is string[] alternativeNames && alternativeNames.Length > 0)
                str.AppendLine().Append("<color=#FF4C4C><size=4>AKA: ").Append(string.Join(", ", alternativeNames)).Append("</size></color>");

            Stand.descriptionText.text = str.ToString();

            Sprite releasePreview = info.PackPreviewSprite;
            bool hasPreview = releasePreview is not null;

            if (Stand.previewImage.gameObject.activeSelf != hasPreview)
                Stand.previewImage.gameObject.SetActive(hasPreview);

            if (hasPreview) Stand.previewImage.sprite = releasePreview;

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

                Stand.Character.WearSignatureShirt();
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
                Main.Instance.MenuStateMachine.SwitchState(PreviousState);
                return;
            }

            if (button == EButtonType.NavigateSelect)
            {
                ReleaseInfo info = CurrentInfo;

                if (GetState(info) == ReleaseState.None)
                {
                    SetState(info, ReleaseState.Processing);

                    await Main.Instance.Content.InstallRelease(info, (step, progress) =>
                    {
                        if (!Stand.packBrowserMenuRoot.activeSelf)
                        {
                            Stand.Character.WearSignatureShirt();
                            Stand.packBrowserMenuRoot.SetActive(true);
                            Stand.mainMenuRoot.SetActive(false);
                            Stand.packBrowserLabel.text = string.Format("Name: {0}<br>Author: {1}<line-height=120%><br><color=#FF4C4C>Please refrain from closing Gorilla Tag at this time!", info.Title, info.Author);
                        }

                        Stand.packBrowserStatus.text = string.Format("<size=60%>{0} / 3</size><br>{1}", (step + 1).ToString(), step switch
                        {
                            0 => "Downloading Pack",
                            1 => "Installing Pack",
                            2 => "Loading Shirts",
                            _ => "huh, stop playing with me"
                        });
                        Stand.packBrowserRadial.fillAmount = progress;
                        Stand.packBrowserPercent.text = $"{Mathf.FloorToInt(progress * 100)}%";
                    });

                    SetState(info, ReleaseState.HasRelease);
                    Stand.packBrowserMenuRoot.SetActive(false);
                    Stand.mainMenuRoot.SetActive(true);
                    DisplayRelease();
                    return;
                }

                Main.Instance.PlayOhNoAudio();

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
                single = Stand.Character.SingleShirt;
                if (single != null && shirtsToRotate.Contains(single)) rotationStack.Push(single);

                shirtsToRotate.Where(shirt => !rotationStack.Contains(shirt)).OrderBy(shirt => Random.value).ForEach(rotationStack.Push);
            }

            if (rotationStack.TryPop(out single)) Stand.Character.SetShirt(single);
        }

        public override void Exit()
        {
            Stand.mainMenuRoot.SetActive(false);
        }

        internal enum ReleaseState
        {
            None,
            Processing,
            HasRelease
        }
    }
}
