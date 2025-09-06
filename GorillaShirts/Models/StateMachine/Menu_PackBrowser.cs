using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Extensions;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using System;
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

        private bool isProcessing;
        private ReleaseFlags flags;

        private bool doRotation;
        private float rotationTimer = 0;
        private List<IGorillaShirt> shirtsToRotate = null;
        private readonly Stack<IGorillaShirt> rotationStack = [];

        public override void Enter()
        {
            releases = [.. ShirtManager.Instance.Releases.OrderBy(info => info.Rank)];

            Stand.mainMenuRoot.SetActive(true);
            Stand.navigationRoot.SetActive(false);

            Stand.navigationRoot.SetActive(true);
            Stand.navigationText.text = "Return to Main Menu";

            Stand.mainSideBar.packBrowserButtonNewSymbol.SetActive(false);
            Stand.mainSideBar.SetSidebarState(Sidebar.SidebarState.ReleaseView);
            Stand.mainSideBar.UpdateSidebar();

            DisplayRelease();
        }

        public void DisplayRelease()
        {
            releaseIndex = releaseIndex.Wrap(0, releases.Length);
            ReleaseInfo info = CurrentInfo;

            Stand.packBrowserNewSymbol.SetActive(info.Version > info.GetVersion(EReleaseVersion.Viewed) || info.GetVersion(EReleaseVersion.Viewed) == -1);
            info.UpdateVersion(EReleaseVersion.Viewed);

            flags = (info.Version > info.GetVersion(EReleaseVersion.Installed) && info.Pack is not null) ? ReleaseFlags.Update : ((info.Title == "Default" && info.Rank == 0) ? ReleaseFlags.Reinstall : ReleaseFlags.None);
            if (info.IsOutdated) flags |= ReleaseFlags.Outdated;

            Stand.headerText.text = string.Format(Stand.headerFormat, info.Title.EnforceLength(20), "Pack", info.Author.EnforceLength(48));

            Stand.shirtStatusText.text = flags.HasFlag(ReleaseFlags.Outdated) ? "<color=red>GorillaShirts update required</color>" : GetState(info) switch
            {
                ReleaseState.None => flags.HasFlag(ReleaseFlags.Update) ? "Update" : "Install",
                ReleaseState.Processing => "Processing",
                ReleaseState.HasRelease => flags.HasFlag(ReleaseFlags.Reinstall) ? "Reinstall" : (flags.HasFlag(ReleaseFlags.Update) ? "Update" : "Remove"),
                _ => "tell me what it is"
            };

            StringBuilder str = new();
            str.AppendLine(info.Description.EnforceLength(256));

            if (info.AlsoKnownAs is string[] alternativeNames && alternativeNames.Length > 0)
                str.AppendLine().Append("<color=#FF4C4C><size=4>AKA: ").Append(string.Join(", ", alternativeNames)).Append("</size></color>");

            Stand.descriptionText.text = str.ToString();

            Sprite releasePreview = info.PreviewSprite;
            bool hasPreview = releasePreview is not null;

            if (Stand.previewImage.gameObject.activeSelf != hasPreview)
                Stand.previewImage.gameObject.SetActive(hasPreview);

            if (hasPreview) Stand.previewImage.sprite = releasePreview;

            if (info.Pack is not null)
            {
                PackDescriptor pack = info.Pack;
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

            if (releaseStates.ContainsKey(info))
            {
                if (releaseStates[info] == ReleaseState.HasRelease && info.Pack is null)
                {
                    SetState(info, ReleaseState.None);
                    return ReleaseState.None;
                }
                return releaseStates[info];
            }

            if (info.Pack is not null)
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

            if (button == EButtonType.Return)
            {
                ShirtManager.Instance.MenuStateMachine.SwitchState(PreviousState);
                return;
            }

            switch (button)
            {
                case EButtonType.RigToggle:
                    Stand.Character.SetAppearence(Stand.Character.Preference switch
                    {
                        ECharacterPreference.Masculine => ECharacterPreference.Feminine,
                        ECharacterPreference.Feminine => ECharacterPreference.Masculine,
                        _ => Stand.Character.Preference
                    });
                    Stand.mainSideBar.UpdateSidebar();
                    return;
                case EButtonType.TagIncrease:
                    ShirtManager.Instance.AdjustTagOffset(Mathf.Min(HumanoidContainer.LocalHumanoid.NameTagOffset + 1, 8));
                    Stand.mainSideBar.UpdateSidebar();
                    return;
                case EButtonType.TagDecrease:
                    ShirtManager.Instance.AdjustTagOffset(Mathf.Max(HumanoidContainer.LocalHumanoid.NameTagOffset - 1, 0));
                    Stand.mainSideBar.UpdateSidebar();
                    return;
            }

            if (button == EButtonType.NavigateSelect)
            {
                if (flags.HasFlag(ReleaseFlags.Outdated))
                {
                    ShirtManager.Instance.PlayOhNoAudio();
                    return;
                }

                ReleaseInfo info = CurrentInfo;

                ReleaseState initialState = GetState(info);
                if (initialState != ReleaseState.Processing)
                {
                    SetState(info, ReleaseState.Processing);

                    doRotation = false;
                    shirtsToRotate = null;
                    rotationStack.Clear();
                    Stand.Character.WearSignatureShirt();
                    Stand.packBrowserMenuRoot.SetActive(true);
                    Stand.mainMenuRoot.SetActive(false);

                    bool uninstallRelease = initialState == ReleaseState.HasRelease && !flags.HasFlag(ReleaseFlags.Update);
                    bool installRelease = initialState == ReleaseState.None || flags.HasFlag(ReleaseFlags.Update) || flags.HasFlag(ReleaseFlags.Reinstall);

                    int stepOffset = uninstallRelease ? 1 : 0;
                    int stepCount = new int[] { installRelease ? 3 : 0, stepOffset }.Sum();

                    if (initialState == ReleaseState.HasRelease && flags.HasFlag(ReleaseFlags.Update) && info.Pack is not null)
                    {
                        // bandaid fix to duplicate packs after update with existing install
                        ShirtManager.Instance.Packs.Remove(info.Pack);
                    }

                    if (uninstallRelease)
                    {
                        await ShirtManager.Instance.Content.UninstallRelease(info, (progress) =>
                        {
                            Stand.packBrowserStatus.text = string.Format("<size=60%>{0} / {1}</size><br>{2}", "1", stepCount.ToString(), "Removing Pack");
                            Stand.packBrowserRadial.fillAmount = progress;
                            Stand.packBrowserPercent.text = $"{Mathf.FloorToInt(progress * 100)}%";
                        });

                        SetState(info, ReleaseState.None);
                    }

                    if (installRelease)
                    {
                        Stand.packBrowserLabel.text = string.Format("Name: {0}<br>Version: {1}<line-height=120%><br><color=#FF4C4C>Please refrain from closing Gorilla Tag at this time!", info.Title, info.Version);

                        await ShirtManager.Instance.Content.InstallRelease(info, (step, progress) =>
                        {
                            Stand.packBrowserStatus.text = string.Format("<size=60%>{0} / {1}</size><br>{2}", (step + 1 + stepOffset).ToString(), stepCount.ToString(), step switch
                            {
                                0 => "Downloading Pack",
                                1 => "Installing Pack",
                                2 => "Loading Shirts",
                                _ => "huh, stop playing with me"
                            });
                            Stand.packBrowserRadial.fillAmount = progress;
                            Stand.packBrowserPercent.text = $"{Mathf.FloorToInt(progress * 100)}%";
                        });

                        info.UpdateVersion(EReleaseVersion.Installed);
                        SetState(info, ReleaseState.HasRelease);
                    }

                    Stand.packBrowserMenuRoot.SetActive(false);
                    Stand.mainMenuRoot.SetActive(true);
                    Stand.mainSideBar.SetSidebarState(Sidebar.SidebarState.ReleaseView);
                    DisplayRelease();
                    ShirtManager.Instance.CheckPlayerProperties();
                    return;
                }

                ShirtManager.Instance.PlayOhNoAudio();

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

                shirtsToRotate.Where(shirt => !rotationStack.Contains(shirt)).OrderBy(shirt => UnityEngine.Random.value).ForEach(rotationStack.Push);
            }

            if (rotationStack.TryPop(out single)) Stand.Character.SetShirt(single);
        }

        public override void Exit()
        {
            Stand.mainMenuRoot.SetActive(false);
            Stand.navigationRoot.SetActive(false);

            if (Stand.previewImage.gameObject.activeSelf != false)
                Stand.previewImage.gameObject.SetActive(false);

            if (Stand.packBrowserNewSymbol.activeSelf != false)
                Stand.packBrowserNewSymbol.SetActive(false);
        }

        internal enum ReleaseState
        {
            None,
            Processing,
            HasRelease
        }

        [Flags]
        internal enum ReleaseFlags
        {
            None = 1 << 0,
            Reinstall = 1 << 1,
            Update = 1 << 2,
            Outdated = 1 << 3
        }
    }
}
