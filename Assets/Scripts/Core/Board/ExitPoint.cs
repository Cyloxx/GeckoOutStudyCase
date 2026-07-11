namespace GeckoOut.Core.Board
{
    /// <summary>A colored hole on the board. A gecko of the same color exits through it.</summary>
    public class ExitPoint
    {
        public GridPosition Position { get; }
        public ColorId Color { get; }

        public ExitPoint(GridPosition position, ColorId color)
        {
            Position = position;
            Color = color;
        }
    }
}