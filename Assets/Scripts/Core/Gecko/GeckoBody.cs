using System;
using System.Collections.Generic;
using GeckoOut.Core.Board;

namespace GeckoOut.Core.Gecko
{
    /// <summary>
    /// A gecko occupying a chain of grid cells. Index 0 is the head,
    /// the last index is the tail. Moving inserts a cell at one end and
    /// frees the cell at the other end, so the body "slides".
    /// </summary>
    public class GeckoBody
    {
        public ColorId Color { get; }

        private readonly List<GridPosition> _cells;

        public GeckoBody(ColorId color, IEnumerable<GridPosition> cells)
        {
            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            _cells = new List<GridPosition>(cells);

            if (_cells.Count == 0)
            {
                throw new ArgumentException("A gecko needs at least one cell.");
            }

            for (int i = 1; i < _cells.Count; i++)
            {
                if (!_cells[i].IsAdjacentTo(_cells[i - 1]))
                {
                    throw new ArgumentException(
                        "Body cells must form a connected chain. Break between "
                        + _cells[i - 1] + " and " + _cells[i]);
                }
            }

            var seenCells = new HashSet<GridPosition>();
            foreach (GridPosition cell in _cells)
            {
                if (!seenCells.Add(cell))
                {
                    throw new ArgumentException("Body cells must be unique. Duplicate: " + cell);
                }
            }

            Color = color;
        }

        public int Length
        {
            get { return _cells.Count; }
        }

        public GridPosition Head
        {
            get { return _cells[0]; }
        }

        public GridPosition Tail
        {
            get { return _cells[_cells.Count - 1]; }
        }

        public IReadOnlyList<GridPosition> Cells
        {
            get { return _cells; }
        }

        public GridPosition GetEnd(GeckoEnd end)
        {
            if (end == GeckoEnd.Head)
            {
                return Head;
            }

            return Tail;
        }

        public bool Occupies(GridPosition cell)
        {
            return _cells.Contains(cell);
        }

        /// <summary>
        /// Slides the body one step: the given end moves into newCell and the
        /// opposite end leaves its cell. Returns the cell that was freed.
        /// </summary>
        public GridPosition Step(GeckoEnd movingEnd, GridPosition newCell)
        {
            GridPosition movingCell = GetEnd(movingEnd);

            if (!movingCell.IsAdjacentTo(newCell))
            {
                throw new InvalidOperationException(
                    "Step target must be adjacent. " + movingCell + " -> " + newCell);
            }

            GridPosition freedCell = GetEnd(Opposite(movingEnd));

            if (Occupies(newCell) && !newCell.Equals(freedCell))
            {
                throw new InvalidOperationException(
                    "Step target overlaps the body: " + newCell);
            }

            if (movingEnd == GeckoEnd.Head)
            {
                _cells.RemoveAt(_cells.Count - 1);
                _cells.Insert(0, newCell);
            }
            else
            {
                _cells.RemoveAt(0);
                _cells.Add(newCell);
            }

            return freedCell;
        }

        /// <summary>
        /// Removes the cell at the given end. Used while the gecko is sinking
        /// into an exit hole, one cell per tick. Returns the freed cell.
        /// </summary>
        public GridPosition ShrinkFrom(GeckoEnd end)
        {
            if (_cells.Count == 0)
            {
                throw new InvalidOperationException("Body is already empty.");
            }

            GridPosition freedCell;

            if (end == GeckoEnd.Head)
            {
                freedCell = _cells[0];
                _cells.RemoveAt(0);
            }
            else
            {
                freedCell = _cells[_cells.Count - 1];
                _cells.RemoveAt(_cells.Count - 1);
            }

            return freedCell;
        }

        public static GeckoEnd Opposite(GeckoEnd end)
        {
            if (end == GeckoEnd.Head)
            {
                return GeckoEnd.Tail;
            }

            return GeckoEnd.Head;
        }
    }
}