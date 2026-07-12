using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Core.Rules
{
    /// <summary>
    /// Decides whether a gecko may enter the given exit hole.
    /// Extension point for special exit mechanics (frozen exits, toll gates):
    /// new rules plug in here without modifying MoveValidator (OCP).
    /// </summary>
    public interface IExitRule
    {
        bool CanExit(GeckoBody gecko, ExitPoint exit);
    }
}