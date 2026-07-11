using System;
using System.Collections.Generic;
using GeckoOut.Core.Board;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class BoardGridTests
    {
        // 5x4 board, (2,2) wall, (0,3) red ve (4,0) green
        private BoardGrid CreateBoard()
        {
            var walls = new List<GridPosition> { new GridPosition(2, 2) };
            var exits = new List<ExitPoint>
            {
                new ExitPoint(new GridPosition(0, 3), ColorId.Red),
                new ExitPoint(new GridPosition(4, 0), ColorId.Green)
            };

            return new BoardGrid(5, 4, walls, exits);
        }

        [TestCase(0, 0)]
        [TestCase(4, 3)]
        [TestCase(2, 1)]
        public void IsInside_CellWithinBounds_ReturnsTrue(int x, int y)
        {
            Assert.That(CreateBoard().IsInside(new GridPosition(x, y)), Is.True);
        }

        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        [TestCase(5, 0)]   
        [TestCase(0, 4)]  
        public void IsInside_CellOutsideBounds_ReturnsFalse(int x, int y)
        {
            Assert.That(CreateBoard().IsInside(new GridPosition(x, y)), Is.False);
        }

        [Test]
        public void IsWall_WallCell_ReturnsTrue()
        {
            Assert.That(CreateBoard().IsWall(new GridPosition(2, 2)), Is.True);
        }

        [Test]
        public void IsWall_EmptyCell_ReturnsFalse()
        {
            Assert.That(CreateBoard().IsWall(new GridPosition(1, 1)), Is.False);
        }

        [Test]
        public void TryGetExitAt_ExitCell_ReturnsTrueWithCorrectColor()
        {
            BoardGrid board = CreateBoard();

            bool found = board.TryGetExitAt(new GridPosition(0, 3), out ExitPoint exit);

            Assert.That(found, Is.True);
            Assert.That(exit.Color, Is.EqualTo(ColorId.Red));
        }

        [Test]
        public void TryGetExitAt_EmptyCell_ReturnsFalse()
        {
            bool found = CreateBoard().TryGetExitAt(new GridPosition(1, 1), out ExitPoint exit);

            Assert.That(found, Is.False);
            Assert.That(exit, Is.Null);
        }

        [Test]
        public void Constructor_ZeroWidth_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>( () =>
            {
                new BoardGrid(0, 4, new List<GridPosition>(), new List<ExitPoint>());
            });
        }
    }
}