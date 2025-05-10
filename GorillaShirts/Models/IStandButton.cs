using GorillaShirts.Models;

namespace GorillaShirts.Interfaces
{
    public interface IStandButton
    {
        public EButtonType ButtonType { get; }
        public void ButtonActivation();
    }
}
