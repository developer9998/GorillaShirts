using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    [AddComponentMenu("GorillaShirts/ShirtDescriptor")]
    public class Descriptor : MonoBehaviour
    {
        // Pack
        public string Pack;

        // Base data
        public string Name;
        public string Author;
        public string Info;

        // Objects 
        public GameObject Head;
        public GameObject Body;
        public GameObject LeftUpperArm;
        public GameObject RightUpperArm;
        public GameObject LeftLowerArm;
        public GameObject RightLowerArm;
        public GameObject LeftHand;
        public GameObject RightHand;

        // Settings
        public bool customColors;
        public bool invisibility;
        public List<GameObject> FurTextures;

        public bool wobbleLoose = true;
        public bool wobbleLockHorizontal;
        public bool wobbleLockVertical;
        public bool wobbleLockRoot;

        public AudioClip ShirtSound1;
        public AudioClip ShirtSound2;
    }
}
