using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Patches;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    [RequireComponent(typeof(RigContainer))]
    [DisallowMultipleComponent]
    internal class HumanoidContainer : ShirtHumanoid
    {
        public static HumanoidContainer LocalHumanoid;

        public VRRig Rig;

        public void Awake()
        {
            Rig = GetComponent<VRRig>();
            Root = gameObject;

            Head = Rig.headMesh.transform;
            Body = Head.parent;
            LeftHand = Rig.leftHandTransform.parent;
            RightHand = Rig.rightHandTransform.parent;
            LeftLower = LeftHand.parent;
            RightLower = RightHand.parent;
            LeftUpper = LeftLower.parent;
            RightUpper = RightLower.parent;

            MainSkin = Rig.mainSkin;
            FaceSkin = Rig.faceSkin;

            PlayerNameTags = [Rig.playerText1, Rig.playerText2];

            if (Rig.isOfflineVRRig || Rig.isLocal || (Rig.Creator is NetPlayer creator && creator.IsLocal))
            {
                LayerOverrides.TryAdd(EShirtObject.Head, UnityLayer.MirrorOnly);
                LocalHumanoid = this;
            }

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer += OnLocalInvisibilityChanged;
        }

        public void OnDestroy()
        {
            ClearShirts();
            OffsetNameTag(0);
            RemoveObjects();

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer -= OnLocalInvisibilityChanged;
        }

        public override void OnShirtWorn()
        {
            SetInvisiblityState(ApplyInvisibility);
            MoveNameTag();
        }

        public override void OnShirtRemoved()
        {
            SetInvisiblityState(false);
            MoveNameTag();
        }

        private void SetInvisiblityState(bool invisible)
        {
            MainSkin.forceRenderingOff = invisible;
            FaceSkin.forceRenderingOff = invisible;
        }

        private void OnLocalInvisibilityChanged(VRRig targetRig, bool isInvisible)
        {
            if (targetRig is null || targetRig != Rig)
                return;

            Invisible = isInvisible;

            var shirts = Shirts;
            foreach (var shirtAsset in shirts)
            {
                if (Objects.TryGetValue(shirtAsset, out var objects))
                {
                    objects.ForEach(gameObject => gameObject.SetActive(!isInvisible));
                }
            }
        }
    }
}
