using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Core.Rules
{
    /// <summary>Base game rule: a gecko may only use an exit of its own color.</summary>
    public class ColorMatchExitRule : IExitRule
    {
        public bool CanExit(GeckoBody gecko, ExitPoint exit)
        {
            return gecko.Color == exit.Color;
        }
    }
}