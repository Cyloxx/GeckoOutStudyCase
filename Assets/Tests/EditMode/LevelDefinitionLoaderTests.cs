using GeckoOut.Data;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class LevelDefinitionLoaderTests
    {
        private LevelDefinitionLoader _loader;

        [SetUp]
        public void SetUp()
        {
            _loader = new LevelDefinitionLoader();
        }

        [Test]
        public void Load_FullLevelJson_FillsAllFields()
        {
            string json = @"{
                ""levelId"": 7,
                ""timeLimitSeconds"": 45,
                ""gridWidth"": 6,
                ""gridHeight"": 5,
                ""walls"": [ { ""x"": 3, ""y"": 1 } ],
                ""exits"": [ { ""x"": 5, ""y"": 2, ""color"": ""Red"" } ],
                ""geckos"": [
                    { ""color"": ""Red"",
                      ""cells"": [ { ""x"": 1, ""y"": 1 }, { ""x"": 1, ""y"": 2 } ] }
                ]
            }";

            LevelDefinition definition = _loader.Load(json);

            Assert.That(definition.levelId, Is.EqualTo(7));
            Assert.That(definition.timeLimitSeconds, Is.EqualTo(45f));
            Assert.That(definition.gridWidth, Is.EqualTo(6));
            Assert.That(definition.walls.Count, Is.EqualTo(1));
            Assert.That(definition.exits[0].color, Is.EqualTo("Red"));
            Assert.That(definition.geckos[0].cells.Count, Is.EqualTo(2));
        }

        [Test]
        public void Load_MissingOptionalArrays_ProducesEmptyLists()
        {
            string json = @"{ ""levelId"": 1, ""timeLimitSeconds"": 30,
                              ""gridWidth"": 4, ""gridHeight"": 4 }";

            LevelDefinition definition = _loader.Load(json);

            Assert.That(definition.walls, Is.Empty);
            Assert.That(definition.exits, Is.Empty);
            Assert.That(definition.geckos, Is.Empty);
        }

        [Test]
        public void Load_EmptyString_Throws()
        {
            Assert.Throws<System.ArgumentException>(delegate
            {
                _loader.Load("   ");
            });
        }
    }
}