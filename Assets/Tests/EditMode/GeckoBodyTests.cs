using System.Collections.Generic;
using GeckoOut.Core.Board;
using GeckoOut.Core.Gecko;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class GeckoBodyTests
    {
        // Straight 3-cell red gecko: head (2,2), middle (2,3), tail (2,4).
        private GeckoBody CreateGecko()
        {
            return new GeckoBody(ColorId.Red, new List<GridPosition>
            {
                new GridPosition(2, 2),
                new GridPosition(2, 3),
                new GridPosition(2, 4)
            });
        }

        [Test]
        public void Constructor_EmptyCellList_Throws()
        {
            Assert.Throws<System.ArgumentException>(delegate
            {
                new GeckoBody(ColorId.Red, new List<GridPosition>());
            });
        }

        [Test]
        public void Constructor_BrokenChain_Throws()
        {
            Assert.Throws<System.ArgumentException>(delegate
            {
                new GeckoBody(ColorId.Red, new List<GridPosition>
                {
                    new GridPosition(0, 0),
                    new GridPosition(0, 2)   // gap: not adjacent to (0,0)
                });
            });
        }

        [Test]
        public void Constructor_DuplicateCell_Throws()
        {
            Assert.Throws<System.ArgumentException>(delegate
            {
                new GeckoBody(ColorId.Red, new List<GridPosition>
                {
                    new GridPosition(0, 0),
                    new GridPosition(0, 1),
                    new GridPosition(0, 0)   // revisits the first cell
                });
            });
        }

        [Test]
        public void HeadTailAndLength_MatchConstructorCells()
        {
            GeckoBody gecko = CreateGecko();

            Assert.That(gecko.Head, Is.EqualTo(new GridPosition(2, 2)));
            Assert.That(gecko.Tail, Is.EqualTo(new GridPosition(2, 4)));
            Assert.That(gecko.Length, Is.EqualTo(3));
        }

        [Test]
        public void Occupies_BodyCell_ReturnsTrue()
        {
            Assert.That(CreateGecko().Occupies(new GridPosition(2, 3)), Is.True);
        }

        [Test]
        public void Occupies_FreeCell_ReturnsFalse()
        {
            Assert.That(CreateGecko().Occupies(new GridPosition(0, 0)), Is.False);
        }

        [Test]
        public void Step_HeadToFreeCell_SlidesBody()
        {
            GeckoBody gecko = CreateGecko();

            GridPosition freed = gecko.Step(GeckoEnd.Head, new GridPosition(2, 1));

            Assert.That(gecko.Head, Is.EqualTo(new GridPosition(2, 1)));
            Assert.That(gecko.Tail, Is.EqualTo(new GridPosition(2, 3)));
            Assert.That(gecko.Length, Is.EqualTo(3));
            Assert.That(freed, Is.EqualTo(new GridPosition(2, 4)));
        }

        [Test]
        public void Step_TailToFreeCell_SlidesBodyBackwards()
        {
            GeckoBody gecko = CreateGecko();

            GridPosition freed = gecko.Step(GeckoEnd.Tail, new GridPosition(2, 5));

            Assert.That(gecko.Tail, Is.EqualTo(new GridPosition(2, 5)));
            Assert.That(gecko.Head, Is.EqualTo(new GridPosition(2, 3)));
            Assert.That(freed, Is.EqualTo(new GridPosition(2, 2)));
        }

        [Test]
        public void Step_ToNonAdjacentCell_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(delegate
            {
                CreateGecko().Step(GeckoEnd.Head, new GridPosition(4, 4));
            });
        }

        [Test]
        public void Step_IntoOwnBodyCell_Throws()
        {
            Assert.Throws<System.InvalidOperationException>(delegate
            {
                CreateGecko().Step(GeckoEnd.Head, new GridPosition(2, 3));
            });
        }

        [Test]
        public void Step_IntoOwnTailCell_IsAllowed()
        {
            // U-shaped gecko: head (0,0) and tail (0,1) are adjacent.
            var gecko = new GeckoBody(ColorId.Red, new List<GridPosition>
            {
                new GridPosition(0, 0),
                new GridPosition(1, 0),
                new GridPosition(1, 1),
                new GridPosition(0, 1)
            });

            gecko.Step(GeckoEnd.Head, new GridPosition(0, 1));

            Assert.That(gecko.Head, Is.EqualTo(new GridPosition(0, 1)));
            Assert.That(gecko.Length, Is.EqualTo(4));
        }
    }
}