using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;
using GorillaShirts.Tools;

namespace GorillaShirts.Buttons
{
    internal class RigToggle : IStandButton
    {
        public EButtonType ButtonType => EButtonType.RigToggle;

        public void ButtonActivation()
        {
            Configuration.UpdatePreviewGorilla((int)Configuration.PreviewGorillaEntry.Value, 1);
            Singleton<Main>.Instance.Stand.Rig.SetAppearance(Configuration.PreviewGorillaEntry.Value == Configuration.PreviewGorilla.Silly);
        }
    }
}
