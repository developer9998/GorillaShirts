using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;


#if PLUGIN
using System.Text;
using GorillaShirts.Extensions;
using GorillaShirts.Models;
using System.Linq;
#endif

namespace GorillaShirts.Behaviours
{
    public class MainMenu : MonoBehaviour
    {
        public GameObject TextParent;

        public GameObject ButtonParent;

        public TMP_Text Head;

        public TMP_Text Body;

        public TMP_Text EquipText;

        public GameObject Source;

        [FormerlySerializedAs("Context")]
        public TMP_Text SourceText;

        public Text TagText;

        public GameObject SteadyIcon;

        public GameObject SillyIcon;

        public List<GameObject> SlotItems;

#if PLUGIN

        private IStandNavigationInfo navigationInfo;

        private IEnumerable<IShirtAsset> wornShirts;

        public void SetContent(string head, string body)
        {
            Head.text = head;
            Body.text = body;
        }

        public void SetContent(IStandNavigationInfo info)
        {
            (string name, string author, string description, string type, string source, string note) = info.GetNavigationInfo();

            StringBuilder str = new();

            str.Append("<size=12.5>").Append(name.LimitString(20)).Append("</size>");

            if (!string.IsNullOrEmpty(author))
            {
                str.AppendLine();
                str.Append($"<size=6>{type} by <size=7>").Append(author.LimitString(30)).Append("</size></size>");
            }

            string headText = str.ToString();

            str.Clear();

            str.AppendLine(description.LimitString(258));

            if (!string.IsNullOrEmpty(note))
            {
                str.AppendLine().Append("<color=#FF4C4C><size=4>NOTE: ").Append(note).Append("</size></color>");
            }

            string bodyText = str.ToString();

            SetContent(headText, bodyText);

            Source.SetActive(!string.IsNullOrEmpty(source) && !string.IsNullOrWhiteSpace(source));
            SourceText.text = source;
        }

        public void SetEquipped(IStandNavigationInfo selection, IEnumerable<IShirtAsset> wornShirts)
        {
            if (selection is Pack<IShirtAsset> pack)
            {
                EquipText.text = "View";
                return;
            }

            if (selection is IShirtAsset selectedShirt)
            {
                // EquipText.text = shirt_names.Contains(myShirt.Descriptor.Name) ? "Remove" : "Wear";

                if (wornShirts.Contains(selectedShirt))
                {
                    EquipText.text = "Remove";
                    return;
                }

                EquipText.text = wornShirts.All(shirt => shirt.ComponentTypes.All(type => !selectedShirt.ComponentTypes.Contains(type))) ? "Wear" : "Swap";
                return;
            }
        }

        public void SetTag(int tagOffset) => TagText.text = tagOffset.ToString();

        public void SetSlots(List<bool> slotList)
        {
            for (int i = 0; i < SlotItems.Count; i++)
            {
                var currentSlot = SlotItems[i];
                currentSlot.SetActive(slotList != null && slotList[i]);
            }
        }

        public void UpdateDisplay(IStandNavigationInfo navigationInfo = null, IEnumerable<IShirtAsset> wornShirts = null)
        {
            if (navigationInfo is null)
                navigationInfo = this.navigationInfo;
            else
                this.navigationInfo = navigationInfo;

            if (wornShirts is null)
                wornShirts = this.wornShirts;
            else
                this.wornShirts = wornShirts;

            SetContent(navigationInfo);
            SetEquipped(navigationInfo, wornShirts);
            SetSlots(navigationInfo is IShirtAsset myShirt ? myShirt.TemplateData : null);
        }
#endif
    }
}
