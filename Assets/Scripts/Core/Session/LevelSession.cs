using System;
using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Commands;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Rules;

namespace GeckoOut.Core.Session
{
    /// <summary>
    /// Runs one level: owns the geckos, applies the rules through the
    /// validator, tracks the timer and announces everything that happens
    /// through events. The outside world only talks to this class.
    /// </summary>
    public class LevelSession
    {
        private readonly BoardGrid _board;
        private readonly List<GeckoBody> _activeGeckos;
        private readonly MoveValidator _validator;
        private readonly PathResolver _pathResolver;
        private readonly MoveHistory _history;

        public SessionState State { get; private set; }
        public float RemainingSeconds { get; private set; }

        public event Action<GeckoBody> GeckoStepped;
        public event Action<GeckoBody, ExitPoint> GeckoExited;
        public event Action LevelWon;
        public event Action LevelLost;
        

        public LevelSession(BoardGrid board, IEnumerable<GeckoBody> geckos,
                            MoveValidator validator, PathResolver pathResolver,
                            float timeLimitSeconds)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (geckos == null)
            {
                throw new ArgumentNullException(nameof(geckos));
            }

            if (validator == null)
            {
                throw new ArgumentNullException(nameof(validator));
            }

            if (pathResolver == null)
            {
                throw new ArgumentNullException(nameof(pathResolver));
            }

            _activeGeckos = new List<GeckoBody>(geckos);

            if (_activeGeckos.Count == 0)
            {
                throw new ArgumentException("A level needs at least one gecko.");
            }

            if (timeLimitSeconds <= 0f)
            {
                throw new ArgumentException("Time limit must be positive.");
            }

            _board = board;
            _validator = validator;
            _pathResolver = pathResolver;
            _history = new MoveHistory();

            State = SessionState.Playing;
            RemainingSeconds = timeLimitSeconds;
        }

        public IReadOnlyList<GeckoBody> ActiveGeckos
        {
            get { return _activeGeckos; }
        }

        /// <summary>Advances the level timer. Called once per frame by the presentation layer.</summary>
        public void Tick(float deltaSeconds)
        {
            if (State != SessionState.Playing)
            {
                return;
            }

            RemainingSeconds -= deltaSeconds;

            if (RemainingSeconds <= 0f)
            {
                RemainingSeconds = 0f;
                State = SessionState.Lost;

                if (LevelLost != null)
                {
                    LevelLost();
                }
            }
        }

        /// <summary>
        /// Tries to move one end of a gecko into an adjacent cell.
        /// Dragging back onto the cell the end just came from undoes
        /// the last step instead.
        /// </summary>
        public bool TryStepTo(GeckoBody gecko, GeckoEnd movingEnd, GridPosition target)
        {
            if (State != SessionState.Playing)
            {
                return false;
            }

            if (!_activeGeckos.Contains(gecko))
            {
                return false;
            }

            if (IsBackwardsDrag(gecko, movingEnd, target))
            {
                return TryUndoLastStep(gecko);
            }

            if (!_validator.CanStep(gecko, movingEnd, target, _activeGeckos))
            {
                return false;
            }

            var command = new StepCommand(gecko, movingEnd, target);
            _history.ExecuteAndRecord(command);

            if (GeckoStepped != null)
            {
                GeckoStepped(gecko);
            }

            ResolveExitIfReached(gecko, movingEnd);
            return true;
        }

        /// <summary>
        /// Tries to bring one end of a gecko to a possibly distant cell by
        /// walking a path of single steps. Stops at the first blocked step.
        /// Returns true if at least one step was made.
        /// </summary>
        public bool TryDragTo(GeckoBody gecko, GeckoEnd movingEnd, GridPosition target)
        {
            if (State != SessionState.Playing)
            {
                return false;
            }

            if (!_activeGeckos.Contains(gecko))
            {
                return false;
            }

            GridPosition start = gecko.GetEnd(movingEnd);

            if (start.Equals(target))
            {
                return false;
            }

            if (start.IsAdjacentTo(target))
            {
                return TryStepTo(gecko, movingEnd, target);
            }

            if (!_pathResolver.TryFindPath(start, target, IsFreeCell,
                    out List<GridPosition> path))
            {
                return false;
            }

            bool anyStepMade = false;

            foreach (GridPosition cell in path)
            {
                if (!TryStepTo(gecko, movingEnd, cell))
                {
                    break;
                }

                anyStepMade = true;
            }

            return anyStepMade;
        }

        private bool IsBackwardsDrag(GeckoBody gecko, GeckoEnd movingEnd, GridPosition target)
        {
            StepCommand last = _history.PeekLast() as StepCommand;

            if (last == null)
            {
                return false;
            }

            return last.Gecko == gecko
                && last.MovingEnd == movingEnd
                && target.Equals(last.PreviousEndCell);
        }

        private bool TryUndoLastStep(GeckoBody gecko)
        {
            StepCommand last = (StepCommand)_history.PeekLast();

            // Another gecko may have taken the freed cell in the meantime;
            // in that case the body has no room to slide back.
            GeckoEnd oppositeEnd = GeckoBody.Opposite(last.MovingEnd);

            if (!_validator.CanStep(gecko, oppositeEnd, last.FreedCell, _activeGeckos))
            {
                return false;
            }

            _history.UndoLast();

            if (GeckoStepped != null)
            {
                GeckoStepped(gecko);
            }

            return true;
        }

        private void ResolveExitIfReached(GeckoBody gecko, GeckoEnd movingEnd)
        {
            GridPosition endCell = gecko.GetEnd(movingEnd);

            if (!_board.TryGetExitAt(endCell, out ExitPoint exit))
            {
                return;
            }

            // The validator only allows stepping onto a matching exit,
            // so reaching this point means the gecko leaves the board.
            _activeGeckos.Remove(gecko);
            _history.Clear();

            if (GeckoExited != null)
            {
                GeckoExited(gecko, exit);
            }

            if (_activeGeckos.Count == 0)
            {
                State = SessionState.Won;

                if (LevelWon != null)
                {
                    LevelWon();
                }
            }
        }

        private bool IsFreeCell(GridPosition cell)
        {
            if (!_board.IsInside(cell))
            {
                return false;
            }

            if (_board.IsWall(cell))
            {
                return false;
            }

            if (_board.HasExitAt(cell))
            {
                return false;
            }

            foreach (GeckoBody gecko in _activeGeckos)
            {
                if (gecko.Occupies(cell))
                {
                    return false;
                }
            }

            return true;
        }
    }
}