using UnityEngine;

namespace GorillaShirts.Behaviours.Cosmetic
{
    [DisallowMultipleComponent]
    [AddComponentMenu("GorillaShirts/Shirt Descriptor")]
    public class ShirtDescriptor : MonoBehaviour
    {
        public string ShirtName;

        public string PackName;

        /*
        public bool UsePackDescriptor = true;

        public PackDescriptor Pack;

        public EPreDefinedPack PackType;
        */

        public string Author;

        [TextArea(1, 12)]
        public string Description;

        // Objects
        public GameObject Head;

        public GameObject Body;

        public GameObject LeftUpperArm;

        public GameObject RightUpperArm;

        public GameObject LeftLowerArm;

        public GameObject RightLowerArm;

        public GameObject LeftHand;

        public GameObject RightHand;

        public AudioClip CustomWearSound;

        public AudioClip CustomRemoveSound;

#if PLUGIN

#endif
    }
}
