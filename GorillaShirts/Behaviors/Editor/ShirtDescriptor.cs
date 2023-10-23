using System.Collections.Generic;
using UnityEngine;

namespace GorillaShirts.Behaviors.Editor
{
    [AddComponentMenu("GorillaShirts/ShirtDescriptor")]
    public class ShirtDescriptor : MonoBehaviour
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
        public GameObject Boobs;
        public GameObject LeftUpperArm;
        public GameObject RightUpperArm;
        public GameObject LeftLowerArm;
        public GameObject RightLowerArm;
        public GameObject LeftHand;
        public GameObject RightHand;

        // Additional data
        public bool customColors;
        public bool invisibility;
        public List<GameObject> FurTextures;
    }
}
