using System;
using System.Collections.Generic;
using GeckoOut.Core.Board;

namespace GeckoOut.Core.Rules
{
    /// <summary>
    /// Finds the shortest orthogonal path between two cells using
    /// breadth-first search. Which cells are walkable is decided by the
    /// caller through a predicate, so this class knows nothing about
    /// geckos, walls or board bounds.
    /// </summary>
    public class PathResolver
    {
        private static readonly GridPosition[] Directions =
        {
            GridPosition.Up,
            GridPosition.Down,
            GridPosition.Left,
            GridPosition.Right
        };

        /// <summary>
        /// Returns true if a path exists. The path contains every cell to
        /// step through in order, ending with the target. The start cell
        /// is not included. An empty path means "already at the target".
        /// </summary>
        public bool TryFindPath(GridPosition start, GridPosition target,
                                Func<GridPosition, bool> isWalkable,
                                out List<GridPosition> path)
        {
            if (isWalkable == null)
            {
                throw new ArgumentNullException(nameof(isWalkable));
            }

            path = new List<GridPosition>();

            if (start.Equals(target))
            {
                return true;
            }

            var frontier = new Queue<GridPosition>();
            var visited = new HashSet<GridPosition>();
            var cameFrom = new Dictionary<GridPosition, GridPosition>();

            frontier.Enqueue(start);
            visited.Add(start);

            while (frontier.Count > 0)
            {
                GridPosition current = frontier.Dequeue();

                foreach (GridPosition direction in Directions)
                {
                    GridPosition next = current.Add(direction);

                    if (visited.Contains(next))
                    {
                        continue;
                    }

                    if (!isWalkable(next))
                    {
                        continue;
                    }

                    visited.Add(next);
                    cameFrom[next] = current;

                    if (next.Equals(target))
                    {
                        BuildPath(start, target, cameFrom, path);
                        return true;
                    }

                    frontier.Enqueue(next);
                }
            }

            return false;
        }

        private void BuildPath(GridPosition start, GridPosition target,
                               Dictionary<GridPosition, GridPosition> cameFrom,
                               List<GridPosition> path)
        {
            GridPosition current = target;

            while (!current.Equals(start))
            {
                path.Add(current);
                current = cameFrom[current];
            }

            path.Reverse();
        }
    }
}