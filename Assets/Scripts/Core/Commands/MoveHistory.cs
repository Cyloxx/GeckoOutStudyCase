using System.Collections.Generic;

namespace GeckoOut.Core.Commands
{
    /// <summary>
    /// Executes commands and keeps them in order, so the most recent
    /// one can be undone (backwards dragging).
    /// </summary>
    public class MoveHistory
    {
        private readonly List<IReversibleCommand> _executedCommands
            = new List<IReversibleCommand>();

        public int Count
        {
            get { return _executedCommands.Count; }
        }

        public void ExecuteAndRecord(IReversibleCommand command)
        {
            command.Execute();
            _executedCommands.Add(command);
        }

        public IReversibleCommand PeekLast()
        {
            if (_executedCommands.Count == 0)
            {
                return null;
            }

            return _executedCommands[_executedCommands.Count - 1];
        }

        public void UndoLast()
        {
            IReversibleCommand last = PeekLast();

            if (last == null)
            {
                return;
            }

            last.Undo();
            _executedCommands.RemoveAt(_executedCommands.Count - 1);
        }

        public void Clear()
        {
            _executedCommands.Clear();
        }
    }
}