using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.Cosmetic;
using System;
using UnityEngine;

namespace GorillaShirts.Models
{
    public class PackRelease
    {
        public string Title;

        public string[] AlsoKnownAs = null;

        public int Rank = 10;

        public string Author;

        public string Description;

        public int Version;

        public Version MinimumPluginVersion = null;

        public string PackArchiveLink;

        public string PackPreviewLink = null;

        [NonSerialized]
        public Sprite PreviewSprite;

        [NonSerialized]
        public PackDescriptor Pack;

        [NonSerialized]
        public bool IsOutdated = false;

        public string GetVersionKey(ReleaseSource versionType) => $"{Title}_{versionType.GetName()}Version";

        public int GetVersion(ReleaseSource versionType)
        {
            if (DataManager.Instance is null) return -1;
            return DataManager.Instance.GetItem(GetVersionKey(versionType), -1);
        }

        public void UpdateVersion(ReleaseSource versionType)
        {
            if (DataManager.Instance is null) return;
            DataManager.Instance.SetItem(GetVersionKey(versionType), Version);
        }

        public override string ToString() => $"{Author}: {Title} at {PackArchiveLink}";

        public enum ReleaseSource
        {
            Viewed,
            Installed
        }
    }
}
