using GorillaLibrary.Extensions;
using GorillaLibrary.Models;
using GorillaShirts.Behaviours.Appearance;
using GorillaShirts.Models.Cosmetic;
using GorillaShirts.Patches;
using HarmonyLib;
using UnityEngine;

namespace GorillaShirts.Behaviours
{
    [RequireComponent(typeof(RigContainer), typeof(VRRig))]
    [DisallowMultipleComponent]
    internal class HumanoidContainer : ShirtHumanoid
    {
        public static HumanoidContainer LocalHumanoid;

        public VRRig Rig;

        public float NameTagZOffset = 0f;

        public void Awake()
        {
            Rig = GetComponent<VRRig>();
            Root = gameObject;

            Head = Rig.GetBone(GorillaRigBone.Head);
            Body = Rig.GetBone(GorillaRigBone.Body);
            LeftHand = Rig.GetBone(GorillaRigBone.LeftHand);
            RightHand = Rig.GetBone(GorillaRigBone.RightHand);
            LeftUpper = Rig.GetBone(GorillaRigBone.LeftUpperArm);
            RightUpper = Rig.GetBone(GorillaRigBone.RightUpperArm);
            LeftLower = Rig.GetBone(GorillaRigBone.LeftLowerArm);
            RightLower = Rig.GetBone(GorillaRigBone.RightLowerArm);

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
                AccessTools.Method(anchorOverrides.GetType(), "UpdateName").Invoke(anchorOverrides, null);
                AccessTools.Method(anchorOverrides.GetType(), "UpdateBadge").Invoke(anchorOverrides, null);
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
            AccessTools.Method(bodyRenderer.GetType(), "Refresh").Invoke(bodyRenderer, null);
        }
    }
}
