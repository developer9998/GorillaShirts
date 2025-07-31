using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using System;
using UnityEngine;

namespace GorillaShirts.Models
{
    // Represents a release in the form of a pack
    public class ReleaseInfo
    {
        public string Title;

        public string[] AlsoKnownAs = null;

        public int Rank = 10;

        public string Author;

        public string Description;

        public int Version;

        public string PackArchiveLink;

        public string PackPreviewLink = null;

        [NonSerialized]
        public Sprite PackPreviewSprite;

        [NonSerialized]
        public PackDescriptor Pack;

        public int GetInstalledVersion()
        {
            if (DataManager.Instance is null) return -1;
            return DataManager.Instance.GetItem($"{Title}_Version", -1);
        }

        public void UpdateInstalledVersion()
        {
            if (DataManager.Instance is null) return;
            DataManager.Instance.SetItem($"{Title}_Version", Version);
        }
    }
}
