using GorillaShirts.Behaviours;
using GorillaShirts.Interaction;
using System;

namespace GorillaShirts.Interfaces
{
    public interface IStandButton
    {
        public ButtonType Type { get; }
        public Action<ShirtConstructor> Function { get; }
    }
}
