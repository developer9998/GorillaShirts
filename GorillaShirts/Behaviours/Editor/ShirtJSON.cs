using System;

namespace GorillaShirts.Behaviours.Editor
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
