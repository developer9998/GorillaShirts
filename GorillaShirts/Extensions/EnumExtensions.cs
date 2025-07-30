using GorillaShirts.Models.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GorillaShirts.Extensions
{
    internal static class EnumExtensions
    {
        public static int GetNavDirection(this EButtonType buttonType) => buttonType switch
        {
            EButtonType.NavigateIncrease => 1,
            EButtonType.NavigateDecrease => -1,
            _ => 0
        };
    }
}
