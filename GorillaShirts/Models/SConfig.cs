using System;

namespace GorillaShirts.Models
{
    [Serializable]
    public class SConfig
    {
        public bool customColors;
        public bool invisibility;
        public bool wobbleLoose;
        public bool wobbleLockHorizontal;
        public bool wobbleLockVertical;
        public bool wobbleLockRoot = true;
    }
}
