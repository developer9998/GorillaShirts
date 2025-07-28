using System;
using UnityEngine.Serialization;

namespace GorillaShirts.Models.UI
{
    internal enum EButtonType
    {
        [FormerlySerializedAs("ShirtEquip")]
        NavigateSelect,
        [FormerlySerializedAs("ShirtIncrease")]
        NavigateIncrease,
        [FormerlySerializedAs("ShirtDecrease")]
        NavigateDecrease,
        [Obsolete]
        PackDecrease,
        [Obsolete]
        PackIncrease,
        Return,
        Info,
        RigToggle,
        Capture,
        Randomize,
        TagIncrease,
        TagDecrease,
        GeneralUse1,
        GeneralUse2
    }
}
