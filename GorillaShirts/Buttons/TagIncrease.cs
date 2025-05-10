using GorillaShirts.Behaviours;
using GorillaShirts.Behaviours.UI;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;
using UnityEngine;

namespace GorillaShirts.Buttons
{
    internal class TagIncrease : IStandButton
    {
        public EButtonType ButtonType => EButtonType.TagIncrease;

        public void ButtonActivation()
        {
            Configuration.CurrentTagOffset.Value = Mathf.Min(Constants.TagOffsetLimit, Configuration.CurrentTagOffset.Value + 1);
            Singleton<Main>.Instance.UpdateTagOffset();

            Stand stand = Singleton<Main>.Instance.Stand;
            stand.Rig.OffsetNameTag(Configuration.CurrentTagOffset.Value);
            stand.Display.SetTag(Configuration.CurrentTagOffset.Value);
        }
    }
}
