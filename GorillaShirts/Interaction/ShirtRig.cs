using GorillaShirts.Behaviours;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaShirts.Interaction
{
    public class ShirtRig : MonoBehaviour
    {
        public VRRig PlayerRig => GetComponent<VRRig>();

        public SkinnedMeshRenderer Skin => PlayerRig.mainSkin;

        public MeshRenderer Face => PlayerRig.faceSkin;

        public Rig Rig;
        public Player Player;

        public void Start()
        {
            if (Player == null && PlayerRig.isOfflineVRRig)
            {
                Player = PhotonNetwork.LocalPlayer;
            }
            else if (Player == null && !PlayerRig.isOfflineVRRig && PlayerRig.Creator != null)
            {
                Player player = PhotonNetwork.CurrentRoom.GetPlayer(PlayerRig.Creator.ActorNumber);
                if (player == null)
                {
                    Logging.Error("ShirtRig has no player");
                    Destroy(this);
                    return;
                }
                Player = player;
                Logging.Warning($"ShirtRig has assigned NetPlayer {player.NickName} in place of null player");
            }

            if (!Player.IsLocal && !PhotonNetwork.InRoom)
            {
                Logging.Error($"ShirtRig of player {Player.NickName} is to not be used when not in a room");
                Destroy(this);
                return;
            }

            Rig = new();
            Rig.Head = PlayerRig.headMesh.transform;
            Rig.Body = Rig.Head.parent;
            Rig.RigParent = PlayerRig.transform;
            Rig.RigSkin = PlayerRig.mainSkin;
            Rig.Nametag = PlayerRig.playerText;

            Rig.LeftHand = PlayerRig.leftHandTransform.parent;
            Rig.RightHand = PlayerRig.rightHandTransform.parent;
            Rig.LeftLower = Rig.LeftHand.parent;
            Rig.RightLower = Rig.RightHand.parent;
            Rig.LeftUpper = Rig.LeftLower.parent;
            Rig.RightUpper = Rig.RightLower.parent;

            Rig.OnShirtWorn += OnShirtWorn;
            Rig.OnShirtRemoved += OnShirtRemoved;

            ForceUpdateProperties();
        }

        public async void ForceUpdateProperties()
        {
            if (Player != null && !Player.IsLocal)
            {
                await Task.Delay(PhotonNetwork.NetworkingClient != null ? Mathf.Max(PhotonNetwork.GetPing(), Constants.NetworkOffset) : Constants.NetworkOffset);

                Main.Instance.OnPlayerPropertiesUpdate(Player, Player.CustomProperties);
            }
        }

        public void OnShirtWorn()
        {
            SetInvisiblityState(Rig.Shirt.Invisibility);
            Rig.MoveNameTag();
        }

        public void OnShirtRemoved()
        {
            SetInvisiblityState(false);
            Rig.MoveNameTag();
        }

        private void SetInvisiblityState(bool invisible)
        {
            Skin.forceRenderingOff = invisible;
            Face.forceRenderingOff = invisible;
        }

        public void OnDestroy()
        {
            Rig.RemoveShirt();
            Rig.OffsetNameTag(0);
            Rig.ClearObjects();
        }
    }
}
