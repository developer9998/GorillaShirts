using GorillaShirts.Behaviours.Data;
using GorillaShirts.Extensions;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaShirts.Behaviours.UI
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
            StringBuilder str = new StringBuilder().Append("<size=10>").Append(myShirt.DisplayName.LimitString(17)).Append("</size>").AppendLines(2);
            str.Append("Author: <size=7>").Append(myShirt.Author.LimitString(20)).Append("</size>").AppendLines(3).Append("<size=10>Description:</size>");
            SetDisplay(str.ToString(), myShirt.Description.LimitString(140));
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
            slotList ??= new List<bool>() { false, false, false, false, false, false };
            for (int i = 0; i < SlotItems.Count; i++)
            {
                var currentSlot = SlotItems[i];
                currentSlot.SetActive(slotList[i]);
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
