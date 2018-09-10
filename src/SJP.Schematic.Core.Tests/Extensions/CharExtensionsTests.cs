using System.Globalization;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class CharExtensionsTests
    {
        [Test]
        public static void GetUnicodeCategory_GivenSpaceChar_ReturnsSpaceSeparatorCategory()
        {
            Assert.AreEqual(UnicodeCategory.SpaceSeparator, ' '.GetUnicodeCategory());
        }

        [Test]
        public static void GetUnicodeCategory_GivenLowercaseChar_ReturnsLowercaseCategory()
        {
            Assert.AreEqual(UnicodeCategory.LowercaseLetter, 'a'.GetUnicodeCategory());
        }

        [Test]
        public static void GetUnicodeCategory_GivenUppercaseChar_ReturnsUppercaseCategory()
        {
            Assert.AreEqual(UnicodeCategory.UppercaseLetter, 'A'.GetUnicodeCategory());
        }

        [Test]
        public static void IsDigit_GivenDigit_ReturnsTrue()
        {
            Assert.IsTrue('1'.IsDigit());
        }

        [Test]
        public static void IsDigit_GivenNonDigit_ReturnsFalse()
        {
            Assert.IsFalse('a'.IsDigit());
        }

        [Test]
        public static void IsLetter_GivenLetter_ReturnsTrue()
        {
            Assert.IsTrue('a'.IsLetter());
        }

        [Test]
        public static void IsLetter_GivenNonLetter_ReturnsFalse()
        {
            Assert.IsFalse('1'.IsLetter());
        }

        [Test]
        public static void IsLetterOrDigit_GivenLetter_ReturnsTrue()
        {
            Assert.IsTrue('a'.IsLetterOrDigit());
        }

        [Test]
        public static void IsLetterOrDigit_GivenDigit_ReturnsTrue()
        {
            Assert.IsTrue('1'.IsLetterOrDigit());
        }

        [Test]
        public static void IsLetterOrDigit_GivenNonDigitOrLetter_ReturnsFalse()
        {
            Assert.IsFalse('_'.IsLetterOrDigit());
        }

        [Test]
        public static void IsPunctuation_GivenPunctuation_ReturnsTrue()
        {
            Assert.IsTrue(','.IsPunctuation());
        }

        [Test]
        public static void IsLetter_GivenNonPunctuation_ReturnsFalse()
        {
            Assert.IsFalse('1'.IsPunctuation());
        }

        [Test]
        public static void IsWhiteSpace_GivenWhiteSpace_ReturnsTrue()
        {
            Assert.IsTrue(' '.IsWhiteSpace());
        }

        [Test]
        public static void IsWhiteSpace_GivenNonWhiteSpace_ReturnsFalse()
        {
            Assert.IsFalse('1'.IsWhiteSpace());
        }

        [Test]
        public static void ToLowerInvariant_GivenLowercaseChar_ReturnsLowercaseChar()
        {
            Assert.AreEqual('a', 'a'.ToLowerInvariant());
        }

        [Test]
        public static void ToLowerInvariant_GivenUppercaseChar_ReturnsLowercaseChar()
        {
            Assert.AreEqual('a', 'A'.ToLowerInvariant());
        }

        [Test]
        public static void ToUpperInvariant_GivenLowercaseChar_ReturnsUppercaseChar()
        {
            Assert.AreEqual('A', 'a'.ToUpperInvariant());
        }

        [Test]
        public static void ToUpperInvariant_GivenUppercaseChar_ReturnsUppercaseChar()
        {
            Assert.AreEqual('A', 'A'.ToUpperInvariant());
        }
    }
}
