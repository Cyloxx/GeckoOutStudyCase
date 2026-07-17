using System.Collections.Generic;
using GeckoOut.Data;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class LevelValidatorTests
    {
        private LevelValidator _validator;

        [SetUp]
        public void SetUp()
        {
            _validator = new LevelValidator();
        }

        // 5x5, one wall, red gecko with a matching exit. Fully valid.
        private LevelDefinition CreateValidDefinition()
        {
            var definition = new LevelDefinition();
            definition.levelId = 1;
            definition.timeLimitSeconds = 30f;
            definition.gridWidth = 5;
            definition.gridHeight = 5;
            definition.walls.Add(new CellDefinition { x = 0, y = 0 });
            definition.exits.Add(new ExitDefinition { x = 4, y = 2, color = "Red" });

            var gecko = new GeckoDefinition { color = "Red" };
            gecko.cells.Add(new CellDefinition { x = 1, y = 1 });
            gecko.cells.Add(new CellDefinition { x = 1, y = 2 });
            definition.geckos.Add(gecko);

            return definition;
        }

        [Test]
        public void IsValid_CleanDefinition_ReturnsTrueWithNoErrors()
        {
            bool valid = _validator.IsValid(CreateValidDefinition(), out List<string> errors);

            Assert.That(valid, Is.True);
            Assert.That(errors, Is.Empty);
        }

        [Test]
        public void IsValid_ZeroGridWidth_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.gridWidth = 0;

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_NegativeTimeLimit_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.timeLimitSeconds = -5f;

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_NoGeckos_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.geckos.Clear();

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_UnknownGeckoColor_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.geckos[0].color = "Pink";

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_WallOutsideGrid_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.walls.Add(new CellDefinition { x = 9, y = 9 });

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_BrokenGeckoChain_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.geckos[0].cells.Add(new CellDefinition { x = 4, y = 4 });

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_GeckoOverlappingWall_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.walls.Add(new CellDefinition { x = 1, y = 1 });

            Assert.That(_validator.IsValid(definition, out List<string> errors), Is.False);
        }

        [Test]
        public void IsValid_GeckoWithoutMatchingExit_Fails()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.geckos[0].color = "Green";

            bool valid = _validator.IsValid(definition, out List<string> errors);

            Assert.That(valid, Is.False);
            Assert.That(errors.Count, Is.EqualTo(1));
        }

        [Test]
        public void IsValid_MultipleProblems_ReportsAllOfThem()
        {
            LevelDefinition definition = CreateValidDefinition();
            definition.timeLimitSeconds = 0f;
            definition.geckos[0].color = "Pink";

            _validator.IsValid(definition, out List<string> errors);

            Assert.That(errors.Count, Is.GreaterThanOrEqualTo(2));
        }
    }
}