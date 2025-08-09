using UnityEngine;

#if PLUGIN
using GorillaShirts.Tools;
#endif

namespace GorillaShirts.Behaviours.UI
{
    [RequireComponent(typeof(Collider))]
    public class StandProximityTrigger : MonoBehaviour
    {
#if PLUGIN

        // BodyTrigger name for others
        // GTPlayer.bodyCollider for local

        public void OnTriggerEnter(Collider other)
        {

        }

        public void OnTriggerExit(Collider other)
        {

        }
#endif
    }
}
