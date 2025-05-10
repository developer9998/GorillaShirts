using GorillaShirts.Models;
using GorillaShirts.Patches;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    [RequireComponent(typeof(VRRig))]
    public class PlayerShirtRig : MonoBehaviour
    {
        public VRRig PlayerRig;

        public BaseRigHandler RigHandler;

        public void Awake()
        {
            PlayerRig = GetComponent<VRRig>();
        }

        public void Start()
        {
            RigHandler = new();

            RigHandler.OnShirtWorn += OnShirtWorn;
            RigHandler.OnShirtRemoved += OnShirtRemoved;

            RigHandler.RigObject = gameObject;

            RigHandler.Head = PlayerRig.headMesh.transform;
            RigHandler.Body = RigHandler.Head.parent;
            RigHandler.LeftHand = PlayerRig.leftHandTransform.parent;
            RigHandler.RightHand = PlayerRig.rightHandTransform.parent;
            RigHandler.LeftLower = RigHandler.LeftHand.parent;
            RigHandler.RightLower = RigHandler.RightHand.parent;
            RigHandler.LeftUpper = RigHandler.LeftLower.parent;
            RigHandler.RightUpper = RigHandler.RightLower.parent;

            RigHandler.MainSkin = PlayerRig.mainSkin;
            RigHandler.FaceSkin = PlayerRig.faceSkin;

            RigHandler.PlayerNameTags = [PlayerRig.playerText1, PlayerRig.playerText2];

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer += OnLocalInvisibilityChanged;
        }

        public void OnShirtWorn()
        {
            SetInvisiblityState(RigHandler.ApplyInvisibility);
            RigHandler.MoveNameTag();
        }

        public void OnShirtRemoved()
        {
            SetInvisiblityState(false);
            RigHandler.MoveNameTag();
        }

        public void OnDestroy()
        {
            RigHandler.Shirts = [];
            RigHandler.OffsetNameTag(0);
            RigHandler.ClearObjects();

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer -= OnLocalInvisibilityChanged;
        }

        private void SetInvisiblityState(bool invisible)
        {
            RigHandler.MainSkin.forceRenderingOff = invisible;
            RigHandler.FaceSkin.forceRenderingOff = invisible;
        }

        private void OnLocalInvisibilityChanged(VRRig targetRig, bool isInvisible)
        {
            if (targetRig is null || targetRig != PlayerRig)
                return;

            RigHandler.Invisible = isInvisible;

            var shirts = RigHandler.Shirts;
            foreach (var shirtAsset in shirts)
            {
                if (RigHandler.Objects.TryGetValue(shirtAsset, out var objects))
                {
                    objects.ForEach(gameObject => gameObject.SetActive(!isInvisible));
                }
            }
        }
    }
}
