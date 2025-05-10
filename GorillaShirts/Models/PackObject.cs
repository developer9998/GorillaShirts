using UnityEngine;

namespace GorillaShirts.Models
{
    [CreateAssetMenu(fileName = "Pack", menuName = "GorillaShirts/Pack", order = 1)]
    public class PackObject : ScriptableObject
    {
        public string Name;

        public string Author;

        public string Description;
    }
}
