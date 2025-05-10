using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using GorillaShirts.Models;

namespace GorillaShirts.Buttons
{
    internal class Information : IStandButton
    {
        public EButtonType ButtonType => EButtonType.Info;

        public void ButtonActivation()
        {
            Singleton<Main>.Instance.UseInfoPanel ^= true;
            Singleton<Main>.Instance.SetInfoVisibility.Invoke(Singleton<Main>.Instance.UseInfoPanel);
        }
    }
}
