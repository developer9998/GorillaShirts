using GorillaShirts.Models;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Interaction
{
    public class ShirtRig : MonoBehaviour
    {
        public static Dictionary<VRRig, Rig> RigCache = [];

        public bool Local => Player.IsLocal;

        public Rig Rig;
        public Player Player;

        private SkinnedMeshRenderer Skin;
        private Renderer Face, Chest;

        private bool initialized;

        public void Start()
        {
            if (initialized) return;
            initialized = true;

            VRRig vrRig = GetComponent<VRRig>();
            if (Rig == null && RigCache.ContainsKey(vrRig))
            {
                Rig = RigCache.TryGetValue(vrRig, out Rig instance) ? instance : null;
            }
            else if (Rig == null && !RigCache.ContainsKey(vrRig))
            {
                Rig = new Rig
                {
                    Head = vrRig.headMesh.transform,
                    Toggle = Player.IsLocal
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

                RigCache.Add(vrRig, Rig);
            }

            Rig.OnShirtWorn += OnShirtWorn;
            Rig.OnShirtRemoved += OnShirtRemoved;

            ForceUpdateProperties();
        }

        public async void ForceUpdateProperties()
        {
            if (Player != null && !Player.IsLocal)
            {
                await Task.Delay(PhotonNetwork.NetworkingClient != null ? Mathf.Max(PhotonNetwork.GetPing(), Constants.NetworkOffset) : Constants.NetworkOffset);

                Networking.Instance.OnPlayerPropertiesUpdate(Player, Player.CustomProperties);
            }
        }

        public void OnShirtWorn()
        {
            SetInvisiblityState(Rig.Shirt.Invisibility);
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

            if (Skin == null || Face == null || Chest == null) return;

            Skin.forceRenderingOff = isActivated;
            Face.forceRenderingOff = isActivated;
            Chest.forceRenderingOff = isActivated;
        }
    }
}
