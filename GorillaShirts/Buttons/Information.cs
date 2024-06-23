using GorillaShirts.Behaviours;
using GorillaShirts.Interfaces;
using System;

namespace GorillaShirts.Buttons
{
    internal class Information : IStandButton
    {
        public Interaction.ButtonType Type => Interaction.ButtonType.Info;
        public Action<ShirtConstructor> Function => (ShirtConstructor constructor) =>
        {
            constructor.UseInfoPanel ^= true;
            constructor.SetInfoVisibility.Invoke(constructor.UseInfoPanel);
        };
    }
}
