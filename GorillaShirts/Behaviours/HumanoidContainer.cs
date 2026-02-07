using GorillaExtensions;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Patches;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    [RequireComponent(typeof(RigContainer), typeof(VRRig))]
    [DisallowMultipleComponent]
    internal class HumanoidContainer : ShirtHumanoid
    {
        public static HumanoidContainer LocalHumanoid;

        public VRRig Rig;

        public GorillaIK IK;

        public float NameTagZOffset = 0f;

        public void Awake()
        {
            Rig = GetComponent<VRRig>();
            Root = gameObject;

            IK = Rig.myIk ?? gameObject.GetOrAddComponent<GorillaIK>();

            Head = IK.headBone ?? Rig.headMesh.transform;
            Body = IK.bodyBone.Find("body");
            LeftHand = IK.leftHand;
            RightHand = IK.rightHand;
            LeftUpper = IK.leftUpperArm ?? IK.bodyBone.Find("shoulder.L");
            RightUpper = IK.rightUpperArm ?? IK.bodyBone.Find("shoulder.R");
            LeftLower = IK.leftLowerArm ?? IK.bodyBone.Find("shoulder.L/forearm.L");
            RightLower = IK.rightLowerArm ?? IK.bodyBone.Find("shoulder.R/forearm.R");

            MainSkin = Rig.mainSkin;
            FaceSkin = Rig.faceSkin;

            PlayerNameTags = [Rig.playerText1];
            NameTagZOffset = Rig.playerText1.transform.localPosition.z;

            if (LocalHumanoid == null && (Rig.isOfflineVRRig || Rig.isLocal || (Rig.Creator is NetPlayer creator && creator.IsLocal)))
            {
                LayerOverrides.TryAdd(EShirtObject.Head, UnityLayer.MirrorOnly);
                LocalHumanoid = this;
            }

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer += OnLocalInvisibilityChanged;
        }

        public new void OnDestroy()
        {
            ClearShirts();
            OffsetNameTag(0);
            RemoveObjects();

            RigLocalInvisiblityPatch.OnSetInvisibleToLocalPlayer -= OnLocalInvisibilityChanged;
        }

        public override void OnShirtWorn()
        {
            RefreshBodyRenderer();
            MoveNameTag();
        }

        public override void OnShirtRemoved()
        {
            RefreshBodyRenderer();
            MoveNameTag();
        }

        public override void MoveNameTagTransform(Transform transform, float offset)
        {
            Vector3 offsetVector = transform.localPosition;
            offsetVector.z = NameTagZOffset + ((float)offset * 0.02f);
            transform.localPosition = offsetVector;
        }

        public override void MoveNameTag()
        {
            if (Rig.TryGetComponent(out VRRigAnchorOverrides anchorOverrides))
            {
                anchorOverrides.UpdateName();
                anchorOverrides.UpdateBadge();
            }

            base.MoveNameTag();
        }

        private void OnLocalInvisibilityChanged(VRRig targetRig, bool isInvisible)
        {
            if (targetRig is null || targetRig != Rig)
                return;

            InvisibleToLocalPlayer = isInvisible;

            var shirts = Shirts;
            foreach (var shirtAsset in shirts)
            {
                if (Objects.TryGetValue(shirtAsset, out var objects))
                {
                    objects.ForEach(gameObject => gameObject.SetActive(!isInvisible));
                }
            }
        }

        private void RefreshBodyRenderer()
        {
            if (Rig is null || Rig.bodyRenderer is not GorillaBodyRenderer bodyRenderer) return;
            bodyRenderer.Refresh();
        }
    }
}
