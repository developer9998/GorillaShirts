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
    internal class Menu_PackCollection(Stand stand, List<PackDescriptor> packs) : Menu_StateBase(stand)
    {
        public List<PackDescriptor> Packs = packs;

        private List<PackDescriptor> shownPacks;

        // selection
        private int packIndex = 0;
        private PackDescriptor lastPack;

        // cycle
        private float previewCycleTimer = 0;
        private readonly Stack<IGorillaShirt> shirtStack = [];

        protected readonly Dictionary<PackDescriptor, Menu_ShirtCollection> menuPerPack = [];

        public override void Enter()
        {
            shownPacks = [.. Packs.Where(pack => pack.Shirts.Count != 0)];

            base.Enter();

            Stand.mainMenuRoot.SetActive(true);
            Stand.navigationRoot.SetActive(false);

            SetSidebarState(SidebarState.PackNavigation);

            PreviewPack();
        }

        public override void Resume()
        {
            base.Resume();

            if (lastPack != null && shownPacks.ElementAtOrDefault(packIndex) != lastPack && shownPacks.Contains(lastPack))
            {
                packIndex = shownPacks.IndexOf(lastPack);
            }
        }

        public override void Exit()
        {
            base.Exit();
            Stand.mainMenuRoot.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            previewCycleTimer += Time.unscaledDeltaTime;
            if (previewCycleTimer >= 1f) PerformShirtCycle();
        }

        public void PreviewPack()
        {
            packIndex = packIndex.Wrap(0, shownPacks.Count);
            PackDescriptor pack = shownPacks[packIndex];
            lastPack = pack;

            Stand.favouriteButtonObject.SetActive(pack != Main.Instance.FavouritePack && shownPacks.Contains(Main.Instance.FavouritePack));
            Stand.favouriteButtonSymbol.color = Color.white;

            Stand.headerText.text = pack.Author == null ? pack.PackName.EnforceLength(20) : string.Format(Stand.headerFormat, pack.PackName.EnforceLength(20), "Pack", pack.Author.EnforceLength(30));
            Stand.shirtStatusText.text = "View";

            StringBuilder str = new();
            str.AppendLine(pack.Description.EnforceLength(256));

            if (!string.IsNullOrEmpty(pack.AdditionalNote))
                str.AppendLine().Append("<color=#FF4C4C><size=4>NOTE: ").Append(pack.AdditionalNote).Append("</size></color>");

            Stand.descriptionText.text = str.ToString();

            for (int i = 0; i < Stand.featureObjects.Length; i++)
            {
                if (Stand.featureObjects.ElementAtOrDefault(i) is GameObject featureObject && featureObject.activeSelf)
                    featureObject.SetActive(false);
            }

            shirtStack.Clear();
            PerformShirtCycle();
        }

        public void PerformShirtCycle()
        {
            previewCycleTimer = 0f;

            IGorillaShirt single;

            if (shirtStack.Count == 0)
            {
                PackDescriptor pack = shownPacks[packIndex];

                single = Stand.Character.SingleShirt;
                if (single != null && pack.Shirts.Contains(single)) shirtStack.Push(single);

                pack.Shirts.Where(shirt => !shirtStack.Contains(shirt)).OrderBy(shirt => Random.value).ForEach(shirtStack.Push);
            }

            if (shirtStack.TryPop(out single)) Stand.Character.SetShirt(single);
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button == EButtonType.Info)
            {
                Main.Instance.MenuStateMachine.SwitchState(new Menu_Info(Stand, this));
                return;
            }

            if (button == EButtonType.PackBrowser)
            {
                Main.Instance.MenuStateMachine.SwitchState(new Menu_PackBrowser(Stand, this));
                return;
            }

            if (button == EButtonType.NavigateSelect)
            {
                PackDescriptor pack = shownPacks[packIndex];
                if (!menuPerPack.ContainsKey(pack)) menuPerPack.Add(pack, new Menu_ShirtCollection(Stand, this, pack));
                Main.Instance.MenuStateMachine.SwitchState(menuPerPack[pack]);
                return;
            }

            switch (button)
            {
                case EButtonType.NavigateIncrease:
                    packIndex++;
                    break;
                case EButtonType.NavigateDecrease:
                    packIndex--;
                    break;
                case EButtonType.Favourite:
                    packIndex = shownPacks.IndexOf(Main.Instance.FavouritePack);
                    break;
                default:
                    return;
            }

            PreviewPack();
        }
    }
}
