using GorillaShirts.Extensions;
using System.Collections.Generic;
using System.Text;
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

        public void SetDisplay(Shirt myShirt)
        {
            StringBuilder str = new StringBuilder();
            str.Append("<size=20>").Append(myShirt.DisplayName.LimitString(17)).Append("</size>").AppendLines(2);
            str.Append("Author: <size=7>").Append(myShirt.Author.LimitString(20)).Append("</size>");

            SetDisplay(str.ToString(), myShirt.Description.LimitString(258));
        }

        public void SetDisplay(Shirt myShirt, Pack myPack)
        {
            SetDisplay(myShirt);
            SetPack(myPack);
        }

        public void SetVersion(string myVersion) => Version.text = myVersion;

        public void SetPack(Pack myPack) => Pack.text = myPack.DisplayName ?? "Unknown";

        public void SetEquipped(Shirt myShirt, Shirt currentShirt) => Equip.text = currentShirt == myShirt ? "Remove" : "Wear";

        public void SetTag(int tagOffset) => Tag.text = tagOffset.ToString();

        public void SetSlots(List<bool> slotList)
        {
            for (int i = 0; i < SlotItems.Count; i++)
            {
                var currentSlot = SlotItems[i];
                currentSlot.SetActive(slotList != null && slotList[i]);
            }
        }

        public void UpdateDisplay(Shirt myShirt, Shirt activeShirt, Pack myPack)
        {
            SetDisplay(myShirt, myPack);
            SetSlots(myShirt.GetSlotData());
            SetEquipped(myShirt, activeShirt);
        }
    }
}
