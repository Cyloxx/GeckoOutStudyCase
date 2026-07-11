using GeckoOut.Core.Board;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class GridPositionTests
    {
        [Test]
        public void Equals_SameCoordinates_AreEqual()
        {
            Assert.That(new GridPosition(2, 3), Is.EqualTo(new GridPosition(2, 3)));
        }

        [Test]
        public void Equals_DifferentCoordinates_AreNotEqual()
        {
            Assert.That(new GridPosition(2, 3), Is.Not.EqualTo(new GridPosition(3, 2)));
        }

        [TestCase(1, 0)]
        [TestCase(-1, 0)]
        [TestCase(0, 1)]
        [TestCase(0, -1)]
        public void IsAdjacentTo_OrthogonalNeighbour_ReturnsTrue(int dx, int dy)
        {
            var origin = new GridPosition(5, 5);

            Assert.That(origin.IsAdjacentTo(new GridPosition(5 + dx, 5 + dy)), Is.True);
        }

        [TestCase(0, 0)]  
        [TestCase(1, 1)]  
        [TestCase(2, 0)]  
        public void IsAdjacentTo_NonNeighbour_ReturnsFalse(int dx, int dy)
        {
            var origin = new GridPosition(5, 5);

            Assert.That(origin.IsAdjacentTo(new GridPosition(5 + dx, 5 + dy)), Is.False);
        }

        [Test]
        public void Add_WithUpDirection_MovesOneCellUp()
        {
            GridPosition result = new GridPosition(2, 3).Add(GridPosition.Up);

            Assert.That(result, Is.EqualTo(new GridPosition(2, 4)));
        }

        [Test]
        public void GetHashCode_EqualValues_Match()
        {
            Assert.That(new GridPosition(4, 7).GetHashCode(),
                Is.EqualTo(new GridPosition(4, 7).GetHashCode()));
        }
    }
}