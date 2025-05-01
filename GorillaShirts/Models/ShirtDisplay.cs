using System.Collections.Generic;
using System.Text;
using GorillaShirts.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShirts.Models
{
    public class ShirtDisplay
    {
        public Text
            Main,
            Body,
            Version;

        public Text
            Equip,
            Pack,
            Tag;

        public GameObject Steady, Silly;

        public GameObject SlotParent;
        public List<GameObject> SlotItems = new();

        public void SetDisplay(string myText, string myDescription)
        {
            Main.text = myText;
            Body.text = myDescription;
        }

        public void SetDisplay(IShirtAsset myShirt)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<size=20>").Append(myShirt.Descriptor.DisplayName.LimitString(17)).Append("</size>").AppendLines(2);
            str.Append("Author: <size=7>").Append(myShirt.Descriptor.Author.LimitString(20)).Append("</size>");

            SetDisplay(str.ToString(), myShirt.Descriptor.Description.LimitString(258));
        }

        public void SetDisplay(IShirtAsset myShirt, Pack<IShirtAsset> myPack)
        {
            SetDisplay(myShirt);
            SetPack(myPack);
        }

        public void SetVersion(string myVersion) => Version.text = myVersion;

        public void SetPack(Pack<IShirtAsset> myPack) => Pack.text = myPack.Name ?? "N/A";

        public void SetEquipped(IShirtAsset myShirt, IShirtAsset currentShirt) => Equip.text = currentShirt == myShirt ? "Remove" : "Wear";

        public void SetTag(int tagOffset) => Tag.text = tagOffset.ToString();

        public void SetSlots(List<bool> slotList)
        {
            for (int i = 0; i < SlotItems.Count; i++)
            {
                var currentSlot = SlotItems[i];
                currentSlot.SetActive(slotList != null && slotList[i]);
            }
        }

        public void UpdateDisplay(IShirtAsset myShirt, IShirtAsset activeShirt, Pack<IShirtAsset> myPack)
        {
            SetDisplay(myShirt, myPack);
            SetSlots(myShirt.TemplateData);
            SetEquipped(myShirt, activeShirt);
        }
    }
}
