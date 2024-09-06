using GorillaShirts.Tools;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch]
    public class RigPatches
    {
        public static void AddPatch(NetPlayer player, VRRig vrrig) => Events.RigEnabled?.Invoke(player is PunNetPlayer punPlayer ? punPlayer.PlayerRef : null, vrrig);

        public static void RemovePatch(NetPlayer player, VRRig vrrig) => Events.RigDisabled?.Invoke(player is PunNetPlayer punPlayer ? punPlayer.PlayerRef : null, vrrig);
    }
}
