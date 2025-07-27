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
    internal class Menu_ShirtCollection(Stand stand, Menu_StateBase previousState, PackDescriptor pack) : Menu_SubState(stand, previousState)
    {
        protected PackDescriptor pack = pack;

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
            IGorillaShirt shirt = pack.Shirts[pack.Selection];

            stand.headerText.text = string.Format(stand.headerFormat, shirt.Descriptor.ShirtName.EnforceLength(20), "Shirt", shirt.Descriptor.Author.EnforceLength(30));

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

            for(int i = 0; i < features.Count; i++)
            {
                if (stand.featureObjects.ElementAtOrDefault(i) is GameObject featureObject)
                    featureObject.SetActive(shirt.Features.HasFlag(features[i]));
            }

            stand.Character.SetShirt(shirt);
        }

        public void ConfigureSidebar()
        {
            stand.sillyHeadObject.SetActive(stand.Character.Preference == ECharacterPreference.Feminine);
            stand.steadyHeadObject.SetActive(stand.Character.Preference == ECharacterPreference.Masculine);
            stand.tagOffsetText.text = HumanoidContainer.LocalHumanoid.NameTagOffset.ToString();
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button == EButtonType.Info)
            {
                Main.Instance.MenuStateMachine.SwitchState(new Menu_Info(stand, this));
                return;
            }

            if (button == EButtonType.Return)
            {
                Main.Instance.MenuStateMachine.SwitchState(previousState);
                return;
            }

            if (button == EButtonType.RigToggle)
            {
                stand.Character.SetAppearence(stand.Character.Preference switch
                {
                    ECharacterPreference.Masculine => ECharacterPreference.Feminine,
                    ECharacterPreference.Feminine => ECharacterPreference.Masculine,
                    _ => stand.Character.Preference
                });
                ConfigureSidebar();
                return;
            }

            if (button == EButtonType.TagIncrease)
            {
                Main.Instance.AdjustTagOffset(Mathf.Min(HumanoidContainer.LocalHumanoid.NameTagOffset + 1, 8));
                ConfigureSidebar();
                return;
            }

            if (button == EButtonType.TagDecrease)
            {
                Main.Instance.AdjustTagOffset(Mathf.Max(HumanoidContainer.LocalHumanoid.NameTagOffset - 1, 0));
                ConfigureSidebar();
                return;
            }

            switch (button)
            {
                case EButtonType.ShirtEquip:
                    Main.Instance.HandleShirt(pack.Shirts[pack.Selection]);
                    break;
                case EButtonType.ShirtIncrease:
                    pack.Selection = (pack.Selection + 1) % pack.Shirts.Count;
                    break;
                case EButtonType.ShirtDecrease:
                    pack.Selection = pack.Selection <= 0 ? (pack.Selection + pack.Shirts.Count - 1) : (pack.Selection - 1);
                    break;
                default:
                    return;
            }

            ViewShirt();
        }
    }
}
