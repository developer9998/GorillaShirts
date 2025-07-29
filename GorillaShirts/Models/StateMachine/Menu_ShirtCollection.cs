using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Extensions;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Models.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GorillaShirts.Models.StateMachine
{
    internal class Menu_ShirtCollection(Stand stand, Menu_StateBase previousState, PackDescriptor pack) : Menu_SubState(stand, previousState)
    {
        protected PackDescriptor pack = pack;

        private bool isWritingPhoto = false;

        public override void Enter()
        {
            base.Enter();
            stand.mainMenuRoot.SetActive(true);
            stand.navigationRoot.SetActive(true);
            stand.navigationText.text = pack.PackName;
            SetSidebarState(true);

            ViewShirt();
            ConfigureSidebar();
        }

        public override void Exit()
        {
            base.Exit();
            stand.mainMenuRoot.SetActive(false);
        }

        public void ViewShirt()
        {
            if (pack.Shirts.Count == 0)
            {
                Main.Instance.MenuStateMachine.SwitchState(previousState);
                return;
            }

            pack.Selection = pack.Selection.Wrap(0, pack.Shirts.Count);
            IGorillaShirt shirt = pack.Shirts[pack.Selection];

            stand.headerText.text = string.Format(stand.headerFormat, shirt.Descriptor.ShirtName.EnforceLength(50), "Shirt", shirt.Descriptor.Author.EnforceLength(30));

            List<IGorillaShirt> wornShirts = HumanoidContainer.LocalHumanoid.Shirts;
            if (wornShirts.Contains(shirt)) stand.shirtStatusText.text = "Remove";
            else stand.shirtStatusText.text = wornShirts
                    .All(wornShirt => Enum.GetValues(typeof(EShirtObject)).Cast<EShirtObject>()
                    .Where(shirtObject => wornShirt.Objects.HasFlag(shirtObject))
                    .All(shirtObject => !shirt.Objects.HasFlag(shirtObject))) ? "Wear" : "Swap";

            StringBuilder str = new();
            str.AppendLine(shirt.Descriptor.Description.EnforceLength(256));

            if (shirt is LegacyGorillaShirt)
                str.AppendLine().Append("<color=#FF4C4C><size=4>NOTE: ").Append("This shirt was made for an earlier version of GorillaShirts, and may not have the latest features.").Append("</size></color>");

            stand.descriptionText.text = str.ToString();

            var features = Enum.GetValues(typeof(EShirtFeature)).Cast<EShirtFeature>().ToList();

            for (int i = 0; i < features.Count; i++)
            {
                if (stand.featureObjects.ElementAtOrDefault(i) is GameObject featureObject)
                    featureObject.SetActive(shirt.Features.HasFlag(features[i]));
            }

            stand.Character.SetShirts(shirt.Concat(HumanoidContainer.LocalHumanoid.Shirts));

            ConfigureSidebar();
        }

        public void ConfigureSidebar()
        {
            stand.sillyHeadObject.SetActive(stand.Character.Preference == ECharacterPreference.Feminine);
            stand.steadyHeadObject.SetActive(stand.Character.Preference == ECharacterPreference.Masculine);
            stand.tagOffsetText.text = HumanoidContainer.LocalHumanoid.NameTagOffset.ToString();

            if (pack.Shirts.Count != 0)
            {
                IGorillaShirt shirt = pack.Shirts[pack.Selection];
                stand.favouriteButtonSymbol.color = Main.Instance.IsFavourite(shirt) ? Color.yellow : Color.white;
            }
        }

        public async override void OnButtonPress(EButtonType button)
        {
            // state machine handling
            switch (button)
            {
                case EButtonType.Info:
                    Main.Instance.MenuStateMachine.SwitchState(new Menu_Info(stand, this));
                    return;
                case EButtonType.Return:
                    Main.Instance.MenuStateMachine.SwitchState(previousState);
                    return;
            }

            // sidebar
            switch (button)
            {
                case EButtonType.RigToggle:
                    stand.Character.SetAppearence(stand.Character.Preference switch
                    {
                        ECharacterPreference.Masculine => ECharacterPreference.Feminine,
                        ECharacterPreference.Feminine => ECharacterPreference.Masculine,
                        _ => stand.Character.Preference
                    });
                    ConfigureSidebar();
                    return;
                case EButtonType.Capture:
                    if (isWritingPhoto) return;
                    isWritingPhoto = true;

                    Texture2D texture = await stand.Camera.GetTexture();
                    if (texture is not null)
                    {
                        Main.Instance.PlayShirtAudio(EAudioType.CameraShutter, 1f);

                        string nativePicturesDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                        string shirtsDirectoryName = "GorillaShirts";
                        if (string.IsNullOrEmpty(nativePicturesDirectory) || string.IsNullOrWhiteSpace(nativePicturesDirectory) || !Directory.Exists(nativePicturesDirectory))
                        {
                            nativePicturesDirectory = Path.GetDirectoryName(Plugin.Info.Location);
                            shirtsDirectoryName = "Photos";
                        }

                        string fileDirectory = Path.Combine(nativePicturesDirectory, shirtsDirectoryName);
                        if (!Directory.Exists(fileDirectory)) Directory.CreateDirectory(fileDirectory);

                        string fileName = string.Concat("GorillaShirts_", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff"), ".png");
                        byte[] fileBytes = texture.EncodeToPNG();
                        await File.WriteAllBytesAsync(Path.Combine(fileDirectory, fileName), fileBytes);
                    }

                    isWritingPhoto = false;
                    return;
                case EButtonType.Randomize:
                    Main.Instance.PlayShirtAudio(EAudioType.DiceRoll, 1f);
                    pack.Shuffle();
                    ViewShirt();
                    return;
                case EButtonType.TagIncrease:
                    Main.Instance.AdjustTagOffset(Mathf.Min(HumanoidContainer.LocalHumanoid.NameTagOffset + 1, 8));
                    ConfigureSidebar();
                    return;
                case EButtonType.TagDecrease:
                    Main.Instance.AdjustTagOffset(Mathf.Max(HumanoidContainer.LocalHumanoid.NameTagOffset - 1, 0));
                    ConfigureSidebar();
                    return;
                case EButtonType.Favourite:
                    Main.Instance.FavouriteShirt(pack.Shirts[pack.Selection]);
                    ViewShirt();
                    return;
            }

            // main
            switch (button)
            {
                case EButtonType.NavigateSelect:
                    Main.Instance.HandleShirt(pack.Shirts[pack.Selection]);
                    break;
                case EButtonType.NavigateIncrease:
                    pack.Selection++;
                    break;
                case EButtonType.NavigateDecrease:
                    pack.Selection--;
                    break;
                default:
                    return;
            }

            ViewShirt();
        }
    }
}
