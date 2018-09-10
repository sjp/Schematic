using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class StringExtensionsTests
    {
        [Test]
        public static void IsNullOrEmpty_GivenNullInput_ReturnsTrue()
        {
            const string input = null;
            Assert.IsTrue(input.IsNullOrEmpty());
        }

        [Test]
        public static void IsNullOrEmpty_GivenEmptyInput_ReturnsTrue()
        {
            Assert.IsTrue(string.Empty.IsNullOrEmpty());
        }

        [Test]
        public static void IsNullOrEmpty_GivenNonEmptyInput_ReturnsFalse()
        {
            Assert.IsFalse("a".IsNullOrEmpty());
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenNullInput_ReturnsTrue()
        {
            const string input = null;
            Assert.IsTrue(input.IsNullOrWhiteSpace());
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenEmptyInput_ReturnsTrue()
        {
            Assert.IsTrue(string.Empty.IsNullOrWhiteSpace());
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenWhiteSpaceInput_ReturnsTrue()
        {
            Assert.IsTrue("   ".IsNullOrWhiteSpace());
        }

        [Test]
        public static void IsNullOrWhiteSpace_GivenInputContainingNonWhiteSpace_ReturnsFalse()
        {
            Assert.IsFalse("  a ".IsNullOrWhiteSpace());
        }

        [Test]
        public static void Join_GivenNullStringCollection_ThrowsArgumentNullException()
        {
            IEnumerable<string> values = null;
            Assert.Throws<ArgumentNullException>(() => values.Join(","));
        }

        [Test]
        public static void Join_GivenNullSeparator_ThrowsArgumentNullException()
        {
            var values = Array.Empty<string>();
            Assert.Throws<ArgumentNullException>(() => values.Join(null));
        }

        [Test]
        public static void Join_GivenSingleString_ReturnsInput()
        {
            var values = new[] { "test" };
            var result = values.Join(",");

            Assert.AreEqual(values[0], result);
        }

        [Test]
        public static void Join_GivenManyStringsWithNonEmptySeparator_ReturnsStringSeparatedBySeparator()
        {
            const string expectedResult = "test1,test2,test3";
            var values = new[] { "test1", "test2", "test3" };
            var result = values.Join(",");

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public static void Join_GivenManyStringsWithEmptySeparator_ReturnsStringsConcatenated()
        {
            const string expectedResult = "test1test2test3";
            var values = new[] { "test1", "test2", "test3" };
            var result = values.Join(string.Empty);

            Assert.AreEqual(expectedResult, result);
        }

        [Test]
        public static void Contains_WhenInvokedOnNullString_ThrowsArgumentNullException()
        {
            const string input = null;
            Assert.Throws<ArgumentNullException>(() => StringExtensions.Contains(input, string.Empty, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public static void Contains_GivenNullValue_ThrowsArgumentNullException()
        {
            var input = string.Empty;
            Assert.Throws<ArgumentNullException>(() => StringExtensions.Contains(input, null, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public static void Contains_GivenInvalidStringComparison_ThrowsArgumentException()
        {
            var input = string.Empty;
            const StringComparison comparison = (StringComparison)55;
            Assert.Throws<ArgumentException>(() => StringExtensions.Contains(input, string.Empty, comparison));
        }

        [Test]
        public static void Contains_GivenEmptyStringWithNonEmptyValue_ReturnsFalse()
        {
            var input = string.Empty;
            Assert.IsFalse(StringExtensions.Contains(input, "A", StringComparison.Ordinal));
        }

        [Test]
        public static void Contains_GivenNonEmptyStringWithEmptyValue_ReturnsTrue()
        {
            const string input = "A";
            Assert.IsTrue(StringExtensions.Contains(input, string.Empty, StringComparison.Ordinal));
        }

        [Test]
        public static void Contains_GivenStringWithNonMatchingSubstring_ReturnsFalse()
        {
            const string input = "A";
            Assert.IsFalse(StringExtensions.Contains(input, "B", StringComparison.Ordinal));
        }

        [Test]
        public static void Contains_GivenStringWithMatchingSubstring_ReturnsTrue()
        {
            const string input = "test input test";
            Assert.IsTrue(StringExtensions.Contains(input, "input", StringComparison.Ordinal));
        }

        [Test]
        public static void Contains_GivenStringWithMatchingSubstringButIgnoringCase_ReturnsTrue()
        {
            const string input = "test input test";
            Assert.IsTrue(StringExtensions.Contains(input, "INPUT", StringComparison.OrdinalIgnoreCase));
        }
    }
}
