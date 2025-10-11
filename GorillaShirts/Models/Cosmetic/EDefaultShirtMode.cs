using System.ComponentModel;

namespace GorillaShirts.Models.Cosmetic
{
    internal enum EDefaultShirtMode
    {
        [Description("Default shirts are not used")]
        None,
        [Description("Default shirts are assigned to each player randomly")]
        IrrelevantPlayer,
        [Description("Default shirts are assigned to each player by their unique ID")]
        RelevantPlayer,
        [Description("Default shirts match with what is worn by the local player")]
        Shared
    }
}
