using System;

namespace GorillaShirts.Behaviours.Editor
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
