namespace GeckoOut.Core.Commands
{
    /// <summary>
    /// An action that can be applied and then taken back.
    /// Undo must restore the exact state that existed before Execute.
    /// </summary>
    public interface IReversibleCommand
    {
        void Execute();
        void Undo();
    }
}