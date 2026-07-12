using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using GeckoOut.Core.Rules;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class MoveValidatorTests
    {
        // Shared scene for most tests. 6x6 board.
        //
        //   y5  .  .  .  .  .  .
        //   y4  .  .  G  g  U  u
        //   y3 GE  R  r  .  u  u      R/r = red gecko (head at r? no, see below)
        //   y2  .  .  R  W  .  .      G/g = green gecko, U/u = blue U-gecko
        //   y1  .  .RE  .  .  .      W = wall, RE = red exit, GE = green exit
        //   y0  .  .  .  .  .  .
        //       x0 x1 x2 x3 x4 x5
        //
        // Red gecko cells:  head (2,2) -> (2,3) -> tail (1,3)
        // Green gecko cells: head (2,4) -> tail (3,4)
        // Blue U gecko:     head (4,4) -> (5,4) -> (5,5)... (see CreateUGecko)

        private BoardGrid _board;
        private GeckoBody _red;
        private GeckoBody _green;
        private List<GeckoBody> _allGeckos;
        private MoveValidator _validator;

        [SetUp]
        public void SetUp()
        {
            var walls = new List<GridPosition> { new GridPosition(3, 2) };
            var exits = new List<ExitPoint>
            {
                new ExitPoint(new GridPosition(2, 1), ColorId.Red),
                new ExitPoint(new GridPosition(0, 3), ColorId.Green)
            };
            _board = new BoardGrid(6, 6, walls, exits);

            _red = new GeckoBody(ColorId.Red, new List<GridPosition>
            {
                new GridPosition(2, 2),
                new GridPosition(2, 3),
                new GridPosition(1, 3)
            });

            _green = new GeckoBody(ColorId.Green, new List<GridPosition>
            {
                new GridPosition(2, 4),
                new GridPosition(3, 4)
            });

            _allGeckos = new List<GeckoBody> { _red, _green };
            _validator = new MoveValidator(_board, new ColorMatchExitRule());
        }

        [Test]
        public void CanStep_ToFreeCell_ReturnsTrue()
        {
            bool allowed = _validator.CanStep(_red, GeckoEnd.Head,
                new GridPosition(1, 2), _allGeckos);

            Assert.That(allowed, Is.True);
        }

        [Test]
        public void CanStep_ToNonAdjacentCell_ReturnsFalse()
        {
            bool allowed = _validator.CanStep(_red, GeckoEnd.Head,
                new GridPosition(5, 5), _allGeckos);

            Assert.That(allowed, Is.False);
        }

        [Test]
        public void CanStep_OffTheBoard_ReturnsFalse()
        {
            var edgeGecko = new GeckoBody(ColorId.Blue, new List<GridPosition>
            {
                new GridPosition(0, 0),
                new GridPosition(0, 1)
            });

            bool allowed = _validator.CanStep(edgeGecko, GeckoEnd.Head,
                new GridPosition(-1, 0), new List<GeckoBody> { edgeGecko });

            Assert.That(allowed, Is.False);
        }

        [Test]
        public void CanStep_IntoWall_ReturnsFalse()
        {
            bool allowed = _validator.CanStep(_red, GeckoEnd.Head,
                new GridPosition(3, 2), _allGeckos);

            Assert.That(allowed, Is.False);
        }

        [Test]
        public void CanStep_IntoOwnBody_ReturnsFalse()
        {
            // Head (2,2) tries to move onto its own middle cell (2,3).
            bool allowed = _validator.CanStep(_red, GeckoEnd.Head,
                new GridPosition(2, 3), _allGeckos);

            Assert.That(allowed, Is.False);
        }

        [Test]
        public void CanStep_IntoOwnTailCell_ReturnsTrue()
        {
            // U-shaped gecko: head (4,4) and tail (4,5) are adjacent,
            // and the tail cell is freed in the same step.
            var uGecko = new GeckoBody(ColorId.Blue, new List<GridPosition>
            {
                new GridPosition(4, 4),
                new GridPosition(5, 4),
                new GridPosition(5, 5),
                new GridPosition(4, 5)
            });

            bool allowed = _validator.CanStep(uGecko, GeckoEnd.Head,
                new GridPosition(4, 5), new List<GeckoBody> { uGecko });

            Assert.That(allowed, Is.True);
        }

        [Test]
        public void CanStep_IntoOtherGecko_ReturnsFalse()
        {
            // Green head (2,4) tries to move onto red's middle cell (2,3).
            bool allowed = _validator.CanStep(_green, GeckoEnd.Head,
                new GridPosition(2, 3), _allGeckos);

            Assert.That(allowed, Is.False);
        }

        [Test]
        public void CanStep_IntoMatchingExit_ReturnsTrue()
        {
            // Red head (2,2) steps onto the red exit at (2,1).
            bool allowed = _validator.CanStep(_red, GeckoEnd.Head,
                new GridPosition(2, 1), _allGeckos);

            Assert.That(allowed, Is.True);
        }

        [Test]
        public void CanStep_IntoWrongColorExit_ReturnsFalse()
        {
            // Red TAIL (1,3) steps onto the green exit at (0,3).
            // Also proves tail moves go through the same validation.
            bool allowed = _validator.CanStep(_red, GeckoEnd.Tail,
                new GridPosition(0, 3), _allGeckos);

            Assert.That(allowed, Is.False);
        }
    }
}