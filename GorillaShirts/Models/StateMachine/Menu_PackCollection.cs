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
    internal class Menu_PackCollection(Stand stand, List<PackDescriptor> collection) : Menu_StateBase(stand)
    {
        protected List<PackDescriptor> packs;

        // selection
        private int packIndex = 0;
        private PackDescriptor lastPack;

        // cycle
        private float previewCycleTimer = 0;
        private readonly Stack<IGorillaShirt> shirtStack = [];

        protected readonly Dictionary<PackDescriptor, Menu_ShirtCollection> menuPerPack = [];

        public override void Enter()
        {
            packs = [.. collection.Where(pack => pack.Shirts.Count > 0)];

            base.Enter();

            stand.mainMenuRoot.SetActive(true);
            stand.navigationRoot.SetActive(false);

            SetSidebarState(false);
            stand.favouriteButtonObject.SetActive(packs.Contains(Main.Instance.FavouritePack));
            stand.favouriteButtonSymbol.color = Color.white;

            PreviewPack();
        }

        public override void Resume()
        {
            base.Resume();

            if (lastPack != null && packs.ElementAtOrDefault(packIndex) != lastPack && packs.Contains(lastPack))
            {
                packIndex = packs.IndexOf(lastPack);
            }
        }

        public override void Exit()
        {
            base.Exit();
            stand.mainMenuRoot.SetActive(false);
        }

        public override void Update()
        {
            base.Update();
            previewCycleTimer += Time.unscaledDeltaTime;
            if (previewCycleTimer >= 1f) PerformShirtCycle();
        }

        public void PreviewPack()
        {
            packIndex = packIndex.Wrap(0, packs.Count);
            PackDescriptor pack = packs[packIndex];
            lastPack = pack;

            stand.headerText.text = pack.Author == null ? pack.PackName.EnforceLength(20) : string.Format(stand.headerFormat, pack.PackName.EnforceLength(20), "Pack", pack.Author.EnforceLength(30));
            stand.shirtStatusText.text = "View";

            StringBuilder str = new();
            str.AppendLine(pack.Description.EnforceLength(256));

            if (!string.IsNullOrEmpty(pack.AdditionalNote))
                str.AppendLine().Append("<color=#FF4C4C><size=4>NOTE: ").Append(pack.AdditionalNote).Append("</size></color>");

            stand.descriptionText.text = str.ToString();

            for (int i = 0; i < stand.featureObjects.Length; i++)
            {
                if (stand.featureObjects.ElementAtOrDefault(i) is GameObject featureObject && featureObject.activeSelf)
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
                PackDescriptor pack = packs[packIndex];

                single = stand.Character.SingleShirt;
                if (single != null && pack.Shirts.Contains(single)) shirtStack.Push(single);

                pack.Shirts.Where(shirt => !shirtStack.Contains(shirt)).OrderBy(shirt => Random.value).ForEach(shirtStack.Push);
            }

            if (shirtStack.TryPop(out single)) stand.Character.SetShirt(single);
        }

        public override void OnButtonPress(EButtonType button)
        {
            if (button == EButtonType.Info)
            {
                Main.Instance.MenuStateMachine.SwitchState(new Menu_Info(stand, this));
                return;
            }

            if (button == EButtonType.NavigateSelect)
            {
                PackDescriptor pack = packs[packIndex];
                if (!menuPerPack.ContainsKey(pack)) menuPerPack.Add(pack, new Menu_ShirtCollection(stand, this, pack));
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
                    packIndex = packs.IndexOf(Main.Instance.FavouritePack);
                    break;
                default:
                    return;
            }

            PreviewPack();
        }
    }
}
