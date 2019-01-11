using System;
using System.Text;
using NUnit.Framework;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Tests
{
    [TestFixture]
    internal static class StringExtensionsTests
    {
        [Test]
        public static void ToStringLiteral_GivenNullString_ReturnsNull()
        {
            var result = StringExtensions.ToStringLiteral(null);

            Assert.IsNull(result);
        }

        [Test]
        public static void ToStringLiteral_GivenSimpleString_ReturnsExpectedText()
        {
            const string input = "test";
            const string expected = "\"test\"";

            var result = input.ToStringLiteral();

            Assert.AreEqual(expected, result);
        }

        [Test]
        public static void ToStringLiteral_GivenStringWithDoubleQuotes_ReturnsExpectedText()
        {
            const string input = "te\"st";
            const string expected = "\"te\\\"st\"";

            var result = input.ToStringLiteral();

            Assert.AreEqual(expected, result);
        }
    }
}
