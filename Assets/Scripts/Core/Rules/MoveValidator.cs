using System;
using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Core.Rules
{
    /// <summary>
    /// Answers one question: is this single step legal by the game rules?
    /// Returns false for illegal moves instead of throwing, because an
    /// illegal move is a normal part of gameplay, not a programming error.
    /// </summary>
    public class MoveValidator
    {
        private readonly BoardGrid _board;
        private readonly IExitRule _exitRule;

        public MoveValidator(BoardGrid board, IExitRule exitRule)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (exitRule == null)
            {
                throw new ArgumentNullException(nameof(exitRule));
            }

            _board = board;
            _exitRule = exitRule;
        }

        public bool CanStep(GeckoBody gecko, GeckoEnd movingEnd, GridPosition target,
                            IReadOnlyList<GeckoBody> allGeckos)
        {
            if (!gecko.GetEnd(movingEnd).IsAdjacentTo(target))
            {
                return false;
            }

            if (!_board.IsInside(target))
            {
                return false;
            }

            if (_board.IsWall(target))
            {
                return false;
            }

            if (gecko.WouldOverlapSelf(movingEnd, target))
            {
                return false;
            }

            foreach (GeckoBody other in allGeckos)
            {
                if (other != gecko && other.Occupies(target))
                {
                    return false;
                }
            }

            if (_board.TryGetExitAt(target, out ExitPoint exit))
            {
                return _exitRule.CanExit(gecko, exit);
            }

            return true;
        }
    }
}