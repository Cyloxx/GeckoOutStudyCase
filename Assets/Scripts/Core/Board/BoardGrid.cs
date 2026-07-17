using System;
using System.Collections.Generic;

namespace GeckoOut.Core.Board
{
    /// <summary>
    /// The static part of a level: size, walls and exit holes.
    /// Knows nothing about geckos; occupancy is tracked by the session.
    /// </summary>
    public class BoardGrid
    {
        public int Width { get; }
        public int Height { get; }

        private readonly HashSet<GridPosition> _walls;
        private readonly Dictionary<GridPosition, ExitPoint> _exitsByPosition;

        public BoardGrid(int width, int height,
                         IEnumerable<GridPosition> walls,
                         IEnumerable<ExitPoint> exits)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException(
                    "Board dimensions must be positive. Got: " + width + "x" + height);
            }

            if (walls == null)
            {
                throw new ArgumentNullException(nameof(walls));
            }

            if (exits == null)
            {
                throw new ArgumentNullException(nameof(exits));
            }

            Width = width;
            Height = height;

            _walls = new HashSet<GridPosition>(walls);

            _exitsByPosition = new Dictionary<GridPosition, ExitPoint>();
            foreach (ExitPoint exit in exits)
            {
                _exitsByPosition.Add(exit.Position, exit);
            }
        }

        public IReadOnlyCollection<ExitPoint> Exits
        {
            get { return _exitsByPosition.Values; }
        }

        /// <summary>Bounds check that also works before a board instance exists.</summary>
        public static bool IsInside(int width, int height, GridPosition position)
        {
            return position.X >= 0 && position.X < width
                                   && position.Y >= 0 && position.Y < height;
        }

        public bool IsInside(GridPosition position)
        {
            return IsInside(Width, Height, position);
        }

        public bool IsWall(GridPosition position)
        {
            return _walls.Contains(position);
        }

        public bool HasExitAt(GridPosition position)
        {
            return _exitsByPosition.ContainsKey(position);
        }

        public bool TryGetExitAt(GridPosition position, out ExitPoint exit)
        {
            return _exitsByPosition.TryGetValue(position, out exit);
        }
    }
}