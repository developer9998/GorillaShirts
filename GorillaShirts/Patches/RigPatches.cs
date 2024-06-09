using GorillaShirts.Tools;
using HarmonyLib;

namespace GorillaShirts.Patches
{
    [HarmonyPatch]
    public class RigPatches
    {
        private static Events _Events;

        public static void Prepare() => _Events = new Events();

        public static void AddPatch(NetPlayer player, VRRig vrrig) => _Events.TriggerRigAdded(player, vrrig);

        public static void RemovePatch(NetPlayer player, VRRig vrrig) => _Events.TriggerRigRemoved(player, vrrig);
    }
}
