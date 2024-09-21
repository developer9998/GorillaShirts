using GorillaShirts.Behaviours;
using GorillaShirts.Models;
using System;

namespace GorillaShirts.Interfaces
{
    public interface IStandButton
    {
        public ButtonType Type { get; }
        public Action<Main> Function { get; }
    }
}
