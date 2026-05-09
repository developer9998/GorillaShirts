using GorillaShirts.Behaviours;
using GorillaShirts.Models.Cosmetic;
using HarmonyLib;
using System;

namespace GorillaShirts.Patches
{
    [HarmonyPatch(typeof(GorillaBodyRenderer), nameof(GorillaBodyRenderer.GetActiveBodyType))]
    internal class RigBodyRendererPatch
    {
        private static GorillaBodyType cosmeticBodyType;
        private static bool appliedCustomBodyType = false;

        public static void Prefix(GorillaBodyRenderer __instance)
        {
            if (__instance.rig is VRRig rig && rig.TryGetComponent(out HumanoidContainer humanoid))
            {
                EShirtBodyType shirtBodyType = humanoid.BodyType;
                if (shirtBodyType != EShirtBodyType.Default && Enum.TryParse(shirtBodyType.GetName(), out GorillaBodyType bodyType))
                {
                    appliedCustomBodyType = true;
                    cosmeticBodyType = __instance.cosmeticBodyType;
                    __instance.cosmeticBodyType = bodyType;
                }
            }
        }

        public static void Postfix(GorillaBodyRenderer __instance)
        {
            if (appliedCustomBodyType)
            {
                appliedCustomBodyType = false;
                __instance.cosmeticBodyType = cosmeticBodyType;
            }
        }
    }
}
