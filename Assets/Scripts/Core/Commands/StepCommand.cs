using System;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Core.Commands
{
    /// <summary>
    /// One single slide step of one gecko. Remembers which cell the
    /// opposite end freed, so the step can be reversed exactly.
    /// </summary>
    public class StepCommand : IReversibleCommand
    {
        private readonly GeckoBody _gecko;
        private readonly GeckoEnd _movingEnd;
        private readonly GridPosition _targetCell;

        private GridPosition _freedCell;
        private bool _executed;

        public StepCommand(GeckoBody gecko, GeckoEnd movingEnd, GridPosition targetCell)
        {
            if (gecko == null)
            {
                throw new ArgumentNullException(nameof(gecko));
            }

            _gecko = gecko;
            _movingEnd = movingEnd;
            _targetCell = targetCell;
        }

        public GeckoBody Gecko
        {
            get { return _gecko; }
        }

        public GeckoEnd MovingEnd
        {
            get { return _movingEnd; }
        }

        public void Execute()
        {
            if (_executed)
            {
                throw new InvalidOperationException("Command was already executed.");
            }

            _freedCell = _gecko.Step(_movingEnd, _targetCell);
            _executed = true;
        }

        public void Undo()
        {
            if (!_executed)
            {
                throw new InvalidOperationException("Command was not executed yet.");
            }

            GeckoEnd oppositeEnd = GeckoBody.Opposite(_movingEnd);
            _gecko.Step(oppositeEnd, _freedCell);
            _executed = false;
        }
    }
}