using UnityEngine;

namespace GorillaShirts.Behaviours.Appearance
{
    [DisallowMultipleComponent]
    [AddComponentMenu("GorillaShirts/Appearance/Wobble Root")]
    public class ShirtWobbleRoot : MonoBehaviour
    {
        public bool LockTranslationX;

        public bool LockTranslationY;

        public bool LockTranslationZ;

        public Transform[] Exclusion;

        public bool LooseRoot;

        public CurveType AnimationBlendCurveType = CurveType.RootOneTailZero;

        public AnimationCurve AnimationBlendCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

        public CurveType LengthStiffnessCurveType;

        public AnimationCurve LengthStiffnessCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public CurveType PoseStiffnessCurveType;

        public AnimationCurve PoseStiffnessCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public CurveType BendAngleCapCurveType;

        public AnimationCurve BendAngleCapCustomCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        public CurveType SquashAndStretchCurveType = CurveType.ConstantZero;

        public AnimationCurve SquashAndStretchCustomCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        public enum CurveType
        {
            ConstantOne,
            ConstantHalf,
            ConstantZero,
            RootOneTailHalf,
            RootOneTailZero,
            RootHalfTailOne,
            RootZeroTailOne,
            Custom
        }
    }
}
