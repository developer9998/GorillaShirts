using GorillaShirts.Models.Cosmetic;
using UnityEngine;
using UnityEngine.Serialization;

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

        public EShirtBodyType BodyType;

        [FormerlySerializedAs("CustomWearSound")]
        public AudioClip WearSound;

        [FormerlySerializedAs("CustomRemoveSound")]
        public AudioClip RemoveSound;

        public EShirtFallback Fallback;

#if PLUGIN

#endif
    }
}
