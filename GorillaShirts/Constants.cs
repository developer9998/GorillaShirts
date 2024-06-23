namespace GorillaShirts
{
    public class Constants
    {
        // General

        /// <summary>
        /// The GUID (globally unique identifier) used to identify the mod
        /// </summary>
        public const string Guid = "dev.gorillashirts";

        /// <summary>
        /// The name of the mod
        /// </summary>
        public const string Name = "GorillaShirts";

        /// <summary>
        /// The version of the mod utilizing semantic versioning (major.minor.patch)
        /// </summary>
        public const string Version = "1.1.1";

        // Assets

        /// <summary>
        /// The manifest path of the main asset bundle
        /// </summary>
        public const string BundlePath = "GorillaShirts.Resources.shirtbundle";

        /// <summary>
        /// The asset name of the shirt stand
        /// </summary>
        public const string StandName = "ShirtStand";

        // Networking

        /// <summary>
        /// The duration of how often the player's custom properties should be set (in seconds)
        /// </summary>
        public const float NetworkCooldown = 1.25f;

        /// <summary>
        /// The minimum amount of milliseconds used to check if a player has any GorillaShirts related data
        /// </summary>
        public const int NetworkOffset = 300;

        /// <summary>
        /// The custom property key used for the shirt a player is wearing
        /// </summary>
        public const string ShirtKey = "G_Shirt";

        /// <summary>
        /// The custom property key used for the nametag offset a player has
        /// </summary>
        public const string TagKey = "G_Tag";

        // Other

        /// <summary>
        /// The maximum amount of nametag spacing a shirt can have
        /// </summary>
        public const int TagOffsetLimit = 8;
    }
}
