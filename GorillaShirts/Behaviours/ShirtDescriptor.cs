using System;
using System.Collections.Generic;
using GorillaShirts.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaShirts.Behaviours
{
    [AddComponentMenu("GorillaShirts/Shirt Descriptor")]
    public class ShirtDescriptor : MonoBehaviour
    {
        [NonSerialized, HideInInspector]
        public EShirtVersion Version;

        [NonSerialized, HideInInspector]
        public string Name;

        [Tooltip("The pack given to the shirt, think of it like seperating clothing into a wardrobe")]
        public string Pack;

        [FormerlySerializedAs("Name"), Tooltip("The name of the shirt")]
        public string DisplayName;

        [Tooltip("The author of the shirt")]
        public string Author;

        [FormerlySerializedAs("Info"), Tooltip("Any additional information regarding your shirt a player wouldn't otherwise know (e.g., history, attribution)")]
        public string Description;

        public GameObject Head;
        public GameObject Body;
        public GameObject LeftUpperArm;
        public GameObject RightUpperArm;
        public GameObject LeftLowerArm;
        public GameObject RightLowerArm;
        public GameObject LeftHand;
        public GameObject RightHand;

        // Settings
        public bool CustomColours;
        public bool Invisiblity;
        public bool Billboard;
        public bool Wobble;
        public List<GameObject> FurTextures;

        public bool wobbleLoose = true;
        public bool wobbleLockHorizontal;
        public bool wobbleLockVertical;
        public bool wobbleLockRoot;

        [FormerlySerializedAs("ShirtSound1")]
        public AudioClip CustomWearSound;

        [FormerlySerializedAs("ShirtSound2")]
        public AudioClip CustomRemoveSound;

        public bool HideRelativeCosmetic;

#if PLUGIN
        public List<Sector> SectorList = [];
#endif
    }
}
