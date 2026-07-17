using System;

namespace GeckoOut.Core.Board
{
    /// <summary>
    /// Immutable board coordinate. (0,0) is the bottom-left cell of the grid.
    /// </summary>
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public int X { get; }
        public int Y { get; }

        public GridPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static readonly GridPosition Up = new GridPosition(0, 1);
        public static readonly GridPosition Down = new GridPosition(0, -1);
        public static readonly GridPosition Left = new GridPosition(-1, 0);
        public static readonly GridPosition Right = new GridPosition(1, 0);

        /// <summary>Orthogonal neighbours only; diagonals do not count.</summary>
        public bool IsAdjacentTo(GridPosition other)
        {
            int dx = Math.Abs(X - other.X);
            int dy = Math.Abs(Y - other.Y);
            return dx + dy == 1;
        }

        /// <summary>Returns the cell you reach by applying the given offset.</summary>
        public GridPosition Add(GridPosition offset)
        {
            return new GridPosition(X + offset.X, Y + offset.Y);
        }

        public bool Equals(GridPosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is GridPosition other && Equals(other);
        }

        // Equal positions must produce equal hashes, otherwise
        // HashSet/Dictionary lookups break. Never skip this when Equals are overridden.
        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }
}