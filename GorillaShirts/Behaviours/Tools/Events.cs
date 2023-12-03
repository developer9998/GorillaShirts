using ExitGames.Client.Photon;
using Photon.Realtime;
using System;

namespace GorillaShirts.Behaviours.Tools
{
    public class Events
    {
        public static event Action<Player, VRRig> RigAdded;
        public static event Action<Player, VRRig> RigRemoved;
        public static event Action<Player, Hashtable> CustomPropUpdate;
        public static event Action<VRRig, int, float> PlayShirtAudio;

        public virtual void TriggerRigAdded(Player player, VRRig vrRig) => RigAdded?.Invoke(player, vrRig);
        public virtual void TriggerRigRemoved(Player player, VRRig vrRig) => RigRemoved?.Invoke(player, vrRig);
        public virtual void TriggerCustomPropUpdate(Player player, Hashtable hashtable) => CustomPropUpdate?.Invoke(player, hashtable);
        public virtual void TriggerPlayShirtAudio(VRRig vrRig, int index, float volume) => PlayShirtAudio?.Invoke(vrRig, index, volume);
    }
}
