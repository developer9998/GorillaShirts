using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using UnityEngine;

namespace GorillaShirts.Tools
{
    public class Events
    {
        public static event Action<NetPlayer, VRRig> RigAdded, RigRemoved;
        public static event Action<Player, Hashtable> CustomPropUpdate;

        public static event Action<VRRig, int, float> PlayShirtAudio;
        public static event Action<VRRig, AudioClip, float> PlayCustomAudio;

        public virtual void TriggerRigAdded(NetPlayer player, VRRig vrRig) => RigAdded?.Invoke(player, vrRig);
        public virtual void TriggerRigRemoved(NetPlayer player, VRRig vrRig) => RigRemoved?.Invoke(player, vrRig);
        public virtual void TriggerCustomPropUpdate(Player player, Hashtable hashtable) => CustomPropUpdate?.Invoke(player, hashtable);

        public virtual void TriggerPlayShirtAudio(VRRig vrRig, int index, float volume) => PlayShirtAudio?.Invoke(vrRig, index, volume);
        public virtual void TriggerPlayCustomAudio(VRRig vrRig, AudioClip clip, float volume) => PlayCustomAudio?.Invoke(vrRig, clip, volume);
    }
}
