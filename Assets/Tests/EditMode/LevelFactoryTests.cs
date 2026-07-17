using GeckoOut.Core.Board;
using GeckoOut.Data;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class LevelFactoryTests
    {
        private LevelFactory _factory;

        [SetUp]
        public void SetUp()
        {
            _factory = new LevelFactory();
        }

        private LevelDefinition CreateDefinition()
        {
            var definition = new LevelDefinition();
            definition.levelId = 3;
            definition.timeLimitSeconds = 45f;
            definition.gridWidth = 6;
            definition.gridHeight = 5;
            definition.walls.Add(new CellDefinition { x = 2, y = 2 });
            definition.exits.Add(new ExitDefinition { x = 5, y = 0, color = "Green" });

            var gecko = new GeckoDefinition { color = "Green" };
            gecko.cells.Add(new CellDefinition { x = 0, y = 0 });
            gecko.cells.Add(new CellDefinition { x = 0, y = 1 });
            definition.geckos.Add(gecko);

            return definition;
        }

        [Test]
        public void Create_BuildsBoardWithWallsAndExits()
        {
            LoadedLevel level = _factory.Create(CreateDefinition());

            Assert.That(level.Board.Width, Is.EqualTo(6));
            Assert.That(level.Board.IsWall(new GridPosition(2, 2)), Is.True);
            Assert.That(level.Board.TryGetExitAt(new GridPosition(5, 0), out ExitPoint exit), Is.True);
            Assert.That(exit.Color, Is.EqualTo(ColorId.Green));
        }

        [Test]
        public void Create_BuildsGeckosWithParsedColorAndCells()
        {
            LoadedLevel level = _factory.Create(CreateDefinition());

            Assert.That(level.Geckos.Count, Is.EqualTo(1));
            Assert.That(level.Geckos[0].Color, Is.EqualTo(ColorId.Green));
            Assert.That(level.Geckos[0].Head, Is.EqualTo(new GridPosition(0, 0)));
            Assert.That(level.Geckos[0].Length, Is.EqualTo(2));
        }

        [Test]
        public void Create_CarriesLevelIdAndTimeLimit()
        {
            LoadedLevel level = _factory.Create(CreateDefinition());

            Assert.That(level.LevelId, Is.EqualTo(3));
            Assert.That(level.TimeLimitSeconds, Is.EqualTo(45f));
        }
    }
}