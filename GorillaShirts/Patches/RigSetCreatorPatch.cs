using GorillaShirts.Behaviours.Networking;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(RigContainer), "set_Creator")]
    public class RigSetCreatorPatch
    {
        [HarmonyWrapSafe]
        public static void Postfix(RigContainer __instance, NetPlayer value)
        {
            if (!__instance.GetComponent<NetworkedPlayer>())
            {
                NetworkedPlayer networked_player = __instance.gameObject.AddComponent<NetworkedPlayer>();
                networked_player.PlayerRig = __instance.Rig;
                networked_player.Owner = value;
            }
        }
    }
}
