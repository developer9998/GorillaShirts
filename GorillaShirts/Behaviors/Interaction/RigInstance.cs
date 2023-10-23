using GorillaShirts.Behaviors.Models;
using GorillaShirts.Behaviors.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Behaviors.Interaction
{
    public class RigInstance : MonoBehaviour
    {
        public Rig Rig;
        public Player Player;
        public bool IsNetwork;

        private Events _Events;
        private bool _Initialized;

        private SkinnedMeshRenderer Skin;
        private Renderer Face, Chest;

        public async void Start()
        {
            if (_Initialized) return;
            _Initialized = true;

            VRRig vrRig = GetComponent<VRRig>();
            if (Rig == null)
            {
                Rig ??= new Rig
                {
                    Head = vrRig.headMesh.transform,
                    Toggle = !IsNetwork
                };

                Rig.Body = Rig.Head.parent;
                Rig.RigParent = vrRig.transform;
                Rig.RigSkin = vrRig.mainSkin;
                Rig.Nametag = vrRig.playerText;

                Rig.LeftHand = vrRig.leftHandTransform.parent;
                Rig.RightHand = vrRig.rightHandTransform.parent;
                Rig.LeftLower = Rig.LeftHand.parent;
                Rig.RightLower = Rig.RightHand.parent;
                Rig.LeftUpper = Rig.LeftLower.parent;
                Rig.RightUpper = Rig.RightLower.parent;
            }

            try
            {
                Rig.ShirtWorn += OnShirtWorn;
                Rig.ShirtRemoved += OnShirtRemoved;
            }
            catch
            {

            }

            if (Player != null && !Player.IsLocal)
            {
                await Task.Delay(PhotonNetwork.NetworkingClient != null ? Mathf.Max(PhotonNetwork.GetPing(), Constants.NetworkOffset) : Constants.NetworkOffset);

                _Events ??= new Events();
                _Events.TriggerCustomPropUpdate(Player, Player.CustomProperties);
            }
        }

        public void OnShirtWorn()
        {
            SetInvisiblityState(Rig.ActiveShirt.Invisibility);
        }

        public void OnShirtRemoved()
        {
            SetInvisiblityState(false);
        }

        private void SetInvisiblityState(bool isActivated)
        {
            Skin ??= Rig.RigSkin;
            Face ??= Rig.Head.Find("gorillaface").GetComponent<Renderer>();
            Chest ??= Rig.Body.Find("gorillachest").GetComponent<Renderer>();

            // Yep relying on all of them is the path I'm choosing to go down
            if (Skin == null || Face == null || Chest == null) return;
            Skin.forceRenderingOff = isActivated;
            Face.forceRenderingOff = isActivated;
            Chest.forceRenderingOff = isActivated;
        }
    }
}
