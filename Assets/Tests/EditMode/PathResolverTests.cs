using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Rules;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class PathResolverTests
    {
        private PathResolver _resolver;
        private HashSet<GridPosition> _walls;

        [SetUp]
        public void SetUp()
        {
            _resolver = new PathResolver();
            _walls = new HashSet<GridPosition>();
        }

        // Walkable = inside a 5x5 board and not a wall.
        private bool IsWalkable(GridPosition cell)
        {
            bool inside = cell.X >= 0 && cell.X < 5 && cell.Y >= 0 && cell.Y < 5;
            return inside && !_walls.Contains(cell);
        }

        [Test]
        public void TryFindPath_OpenBoard_FindsShortestPath()
        {
            bool found = _resolver.TryFindPath(new GridPosition(0, 0),
                new GridPosition(3, 0), IsWalkable, out List<GridPosition> path);

            Assert.That(found, Is.True);
            Assert.That(path, Is.EqualTo(new List<GridPosition>
            {
                new GridPosition(1, 0),
                new GridPosition(2, 0),
                new GridPosition(3, 0)
            }));
        }

        [Test]
        public void TryFindPath_WallInTheWay_GoesAround()
        {
            // Vertical wall segment blocks the direct route:
            //   y1  .  W  .
            //   y0  S  W  T      S=(0,0)  T=(2,0)  W=walls at x1
            _walls.Add(new GridPosition(1, 0));
            _walls.Add(new GridPosition(1, 1));

            bool found = _resolver.TryFindPath(new GridPosition(0, 0),
                new GridPosition(2, 0), IsWalkable, out List<GridPosition> path);

            Assert.That(found, Is.True);
            // Direct distance is 2; the detour over y2 costs 6 steps.
            Assert.That(path.Count, Is.EqualTo(6));
            Assert.That(path[path.Count - 1], Is.EqualTo(new GridPosition(2, 0)));
        }

        [Test]
        public void TryFindPath_EnclosedTarget_ReturnsFalse()
        {
            // Target (4,4) is boxed in by its two orthogonal neighbours.
            _walls.Add(new GridPosition(3, 4));
            _walls.Add(new GridPosition(4, 3));

            bool found = _resolver.TryFindPath(new GridPosition(0, 0),
                new GridPosition(4, 4), IsWalkable, out List<GridPosition> path);

            Assert.That(found, Is.False);
        }

        [Test]
        public void TryFindPath_TargetEqualsStart_ReturnsTrueWithEmptyPath()
        {
            bool found = _resolver.TryFindPath(new GridPosition(2, 2),
                new GridPosition(2, 2), IsWalkable, out List<GridPosition> path);

            Assert.That(found, Is.True);
            Assert.That(path, Is.Empty);
        }

        [Test]
        public void TryFindPath_ResultCells_AreAllAdjacentInOrder()
        {
            _walls.Add(new GridPosition(1, 1));
            _walls.Add(new GridPosition(2, 1));

            _resolver.TryFindPath(new GridPosition(0, 0),
                new GridPosition(3, 3), IsWalkable, out List<GridPosition> path);

            GridPosition previous = new GridPosition(0, 0);
            foreach (GridPosition cell in path)
            {
                Assert.That(cell.IsAdjacentTo(previous), Is.True,
                    "Path is broken between " + previous + " and " + cell);
                previous = cell;
            }
        }
    }
}