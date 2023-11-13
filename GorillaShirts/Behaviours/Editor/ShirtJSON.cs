using System;

namespace GorillaShirts.Behaviors.Editor
{
    [Serializable]
    public class ShirtJSON
    {
        public string assetName;
        public string packName;

        public SDescriptor infoDescriptor;
        public SConfig infoConfig;
    }
}
