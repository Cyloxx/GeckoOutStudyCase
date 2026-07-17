using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;

namespace GeckoOut.Data
{
    /// <summary>
    /// Checks a parsed level definition against the game's data rules.
    /// Collects every problem instead of stopping at the first one, so a
    /// level designer can fix a broken file in a single pass.
    /// </summary>
    public class LevelValidator
    {
        public bool IsValid(LevelDefinition definition, out List<string> errors)
        {
            errors = new List<string>();

            if (definition == null)
            {
                errors.Add("Level definition is null.");
                return false;
            }

            CheckBasics(definition, errors);
            CheckCellsInsideGrid(definition, errors);
            CheckGeckoBodies(definition, errors);
            CheckOverlaps(definition, errors);
            CheckColorsAndExitCoverage(definition, errors);

            return errors.Count == 0;
        }

        private void CheckBasics(LevelDefinition definition, List<string> errors)
        {
            if (definition.gridWidth <= 0 || definition.gridHeight <= 0)
            {
                errors.Add("Grid size must be positive. Got: "
                    + definition.gridWidth + "x" + definition.gridHeight);
            }

            if (definition.timeLimitSeconds <= 0f)
            {
                errors.Add("Time limit must be positive.");
            }

            if (definition.geckos.Count == 0)
            {
                errors.Add("Level has no geckos.");
            }
        }

        private void CheckCellsInsideGrid(LevelDefinition definition, List<string> errors)
        {
            foreach (CellDefinition wall in definition.walls)
            {
                CheckInside(definition, wall.x, wall.y, "Wall", errors);
            }

            foreach (ExitDefinition exit in definition.exits)
            {
                CheckInside(definition, exit.x, exit.y, "Exit", errors);
            }

            for (int i = 0; i < definition.geckos.Count; i++)
            {
                foreach (CellDefinition cell in definition.geckos[i].cells)
                {
                    CheckInside(definition, cell.x, cell.y, "Gecko " + i, errors);
                }
            }
        }

        private void CheckInside(LevelDefinition definition, int x, int y,
                                 string owner, List<string> errors)
        {
            if (!BoardGrid.IsInside(definition.gridWidth, definition.gridHeight,
                    new GridPosition(x, y)))
            {
                errors.Add(owner + " cell (" + x + ", " + y + ") is outside the grid.");
            }
        }

        private void CheckGeckoBodies(LevelDefinition definition, List<string> errors)
        {
            for (int i = 0; i < definition.geckos.Count; i++)
            {
                GeckoDefinition gecko = definition.geckos[i];

                if (gecko.cells.Count == 0)
                {
                    errors.Add("Gecko " + i + " has no cells.");
                    continue;
                }

                List<GridPosition> positions = ToPositions(gecko.cells);
                int breakIndex = GeckoBody.FindChainBreakIndex(positions);

                if (breakIndex >= 0)
                {
                    errors.Add("Gecko " + i + " chain is broken between "
                        + positions[breakIndex - 1] + " and " + positions[breakIndex]);
                }
            }
        }

        private void CheckOverlaps(LevelDefinition definition, List<string> errors)
        {
            var usedCells = new HashSet<GridPosition>();

            foreach (CellDefinition wall in definition.walls)
            {
                AddOrReport(usedCells, new GridPosition(wall.x, wall.y), "Wall", errors);
            }

            foreach (ExitDefinition exit in definition.exits)
            {
                AddOrReport(usedCells, new GridPosition(exit.x, exit.y), "Exit", errors);
            }

            for (int i = 0; i < definition.geckos.Count; i++)
            {
                foreach (GridPosition cell in ToPositions(definition.geckos[i].cells))
                {
                    AddOrReport(usedCells, cell, "Gecko " + i, errors);
                }
            }
        }

        private void AddOrReport(HashSet<GridPosition> usedCells, GridPosition cell,
                                 string owner, List<string> errors)
        {
            if (!usedCells.Add(cell))
            {
                errors.Add(owner + " overlaps another element at " + cell);
            }
        }

        private void CheckColorsAndExitCoverage(LevelDefinition definition, List<string> errors)
        {
            var exitColors = new HashSet<ColorId>();

            foreach (ExitDefinition exit in definition.exits)
            {
                if (ColorIdParser.TryParse(exit.color, out ColorId exitColor))
                {
                    exitColors.Add(exitColor);
                }
                else
                {
                    errors.Add("Exit at (" + exit.x + ", " + exit.y
                        + ") has unknown color: '" + exit.color + "'");
                }
            }

            for (int i = 0; i < definition.geckos.Count; i++)
            {
                if (!ColorIdParser.TryParse(definition.geckos[i].color, out ColorId geckoColor))
                {
                    errors.Add("Gecko " + i + " has unknown color: '"
                        + definition.geckos[i].color + "'");
                    continue;
                }

                if (!exitColors.Contains(geckoColor))
                {
                    errors.Add("Gecko " + i + " (" + geckoColor + ") has no exit of its color.");
                }
            }
        }

        private List<GridPosition> ToPositions(List<CellDefinition> cells)
        {
            var positions = new List<GridPosition>(cells.Count);

            foreach (CellDefinition cell in cells)
            {
                positions.Add(new GridPosition(cell.x, cell.y));
            }

            return positions;
        }
    }
}