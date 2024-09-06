using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Buttons
{
    internal class Information : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.Info;
        public Action<Main> Function => (Main constructor) =>
        {
            constructor.UseInfoPanel ^= true;
            constructor.SetInfoVisibility.Invoke(constructor.UseInfoPanel);
        };
    }
}
