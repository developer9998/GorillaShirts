using UnityEngine;
using GorillaShirts.Models;


#if PLUGIN
using GorillaShirts.Models.Cosmetic;
using System.Collections.Generic;
using Random = System.Random;
#endif

namespace GorillaShirts.Behaviours.Cosmetic
{
    [CreateAssetMenu(fileName = "Pack", menuName = "GorillaShirts/Pack Descriptor")]
    public class PackDescriptor : ScriptableObject
    {
        public string PackName;

        public string Author;

        [TextArea(1, 12)]
        public string Description;

#if PLUGIN

        public int Selection;

        public List<IGorillaShirt> Shirts = [];

        public string AdditionalNote;

        public ReleaseInfo Release;

        public void Shuffle()
        {
            Random random = new();

            int shirtsToShuffle = Shirts.Count;
            while (shirtsToShuffle > 1)
            {
                shirtsToShuffle--;
                int index = random.Next(shirtsToShuffle + 1);
                (Shirts[shirtsToShuffle], Shirts[index]) = (Shirts[index], Shirts[shirtsToShuffle]);
            }
        }



#endif
    }
}
