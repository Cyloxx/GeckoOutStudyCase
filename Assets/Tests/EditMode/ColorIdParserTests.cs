using GeckoOut.Core.Board;
using GeckoOut.Data;
using NUnit.Framework;

namespace GeckoOut.Tests
{
    public class ColorIdParserTests
    {
        [TestCase("Red", ColorId.Red)]
        [TestCase("green", ColorId.Green)]
        [TestCase("BLUE", ColorId.Blue)]
        public void TryParse_KnownColorAnyCase_ReturnsTrue(string text, ColorId expected)
        {
            bool parsed = ColorIdParser.TryParse(text, out ColorId color);

            Assert.That(parsed, Is.True);
            Assert.That(color, Is.EqualTo(expected));
        }

        [TestCase("Pink")]
        [TestCase("42")]
        [TestCase("")]
        [TestCase(null)]
        public void TryParse_InvalidText_ReturnsFalse(string text)
        {
            bool parsed = ColorIdParser.TryParse(text, out ColorId color);

            Assert.That(parsed, Is.False);
        }
    }
}