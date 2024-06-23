using ExitGames.Client.Photon;
using Photon.Realtime;
using System;
using UnityEngine;

namespace GorillaShirts.Tools
{
    public class Events
    {
        public static Action<Player, VRRig> RigEnabled, RigDisabled;
        public static Action<Player, Hashtable> CustomPropUpdate;

        public static Action<VRRig, int, float> PlayShirtAudio;
        public static Action<VRRig, AudioClip, float> PlayCustomAudio;
    }
}
