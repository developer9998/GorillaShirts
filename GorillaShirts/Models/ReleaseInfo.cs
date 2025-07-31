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

        public string PackArchiveLink;

        public string PackPreviewLink = null;

        [NonSerialized]
        public Sprite PackPreviewSprite;
    }
}
