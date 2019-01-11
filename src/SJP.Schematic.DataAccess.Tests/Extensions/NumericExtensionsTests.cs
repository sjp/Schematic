using NUnit.Framework;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class NumericExtensionsTests
    {
        [Test]
        public static void ToNumericLiteral_GivenInt_ReturnsExpectedText()
        {
            const int input = 1;
            const string expected = "1";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToNumericLiteral_GivenUnsignedInt_ReturnsExpectedText()
        {
            const uint input = 1;
            const string expected = "1U";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToNumericLiteral_GivenLong_ReturnsExpectedText()
        {
            const long input = 1;
            const string expected = "1L";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToNumericLiteral_GivenUnsignedLong_ReturnsExpectedText()
        {
            const ulong input = 1;
            const string expected = "1UL";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToNumericLiteral_GivenFloat_ReturnsExpectedText()
        {
            const float input = 1.1F;
            const string expected = "1.1F";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToNumericLiteral_GivenDouble_ReturnsExpectedText()
        {
            const double input = 1.1;
            const string expected = "1.1";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToNumericLiteral_GivenDecimal_ReturnsExpectedText()
        {
            const decimal input = 1.1M;
            const string expected = "1.1M";
            var result = input.ToNumericLiteral();

            Assert.AreEqual(expected, result);
        }
    }
}
