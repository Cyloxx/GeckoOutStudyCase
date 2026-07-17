using System;
using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Data
{
    /// <summary>
    /// Converts a validated LevelDefinition into real domain objects.
    /// Run the LevelValidator first; this class assumes clean input and
    /// relies on the domain constructors as a final safety net.
    /// </summary>
    public class LevelFactory
    {
        public LoadedLevel Create(LevelDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var walls = new List<GridPosition>();
            foreach (CellDefinition wall in definition.walls)
            {
                walls.Add(new GridPosition(wall.x, wall.y));
            }

            var exits = new List<ExitPoint>();
            foreach (ExitDefinition exit in definition.exits)
            {
                exits.Add(new ExitPoint(
                    new GridPosition(exit.x, exit.y),
                    ParseColorOrThrow(exit.color)));
            }

            var board = new BoardGrid(definition.gridWidth, definition.gridHeight,
                walls, exits);

            var geckos = new List<GeckoBody>();
            foreach (GeckoDefinition gecko in definition.geckos)
            {
                var cells = new List<GridPosition>();
                foreach (CellDefinition cell in gecko.cells)
                {
                    cells.Add(new GridPosition(cell.x, cell.y));
                }

                geckos.Add(new GeckoBody(ParseColorOrThrow(gecko.color), cells));
            }

            return new LoadedLevel(definition.levelId, definition.timeLimitSeconds,
                board, geckos);
        }

        private ColorId ParseColorOrThrow(string text)
        {
            if (!ColorIdParser.TryParse(text, out ColorId color))
            {
                throw new InvalidOperationException(
                    "Unknown color '" + text + "'. Was the level validated?");
            }

            return color;
        }
    }
}