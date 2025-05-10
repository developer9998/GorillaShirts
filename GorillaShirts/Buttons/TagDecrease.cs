using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using UnityEngine;

namespace GorillaShirts.Buttons
{
    internal class TagDecrease : IStandButton
    {
        public EButtonType ButtonType => EButtonType.TagDecrease;

        public void ButtonActivation()
        {
            Configuration.CurrentTagOffset.Value = Mathf.Max(0, Configuration.CurrentTagOffset.Value - 1);
            Singleton<Main>.Instance.UpdateTagOffset();

            Stand stand = Singleton<Main>.Instance.Stand;
            stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
            stand.Display.SetTag(Configuration.CurrentTagOffset.Value);
        }
    }
}
