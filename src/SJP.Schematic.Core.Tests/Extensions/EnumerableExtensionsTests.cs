using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class EnumerableExtensionsTests
    {
        [Test]
        public static void Empty_GivenNullCollection_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = null;
            Assert.Throws<ArgumentNullException>(() => input.Empty());
        }

        [Test]
        public static void Empty_GivenEmptyCollection_ReturnsTrue()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Assert.IsTrue(input.Empty());
        }

        [Test]
        public static void Empty_GivenNonEmptyCollection_ReturnsFalse()
        {
            IEnumerable<string> input = new[] { "A" };
            Assert.IsFalse(input.Empty());
        }

        [Test]
        public static void Empty_GivenNullPredicate_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Func<string, bool> predicate = null;

            Assert.Throws<ArgumentNullException>(() => input.Empty(predicate));
        }

        [Test]
        public static void Empty_GivenEmptyCollectionWithPredicate_ReturnsTrue()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Assert.IsTrue(input.Empty(x => x == "A"));
        }

        [Test]
        public static void Empty_GivenNonEmptyCollectionWithNonMatchingPredicate_ReturnsTrue()
        {
            IEnumerable<string> input = new[] { "B" };
            Assert.IsTrue(input.Empty(x => x == "A"));
        }

        [Test]
        public static void Empty_GivenNonEmptyCollectionWithMatchingPredicate_ReturnsFalse()
        {
            IEnumerable<string> input = new[] { "A" };
            Assert.IsFalse(input.Empty(x => x == "A"));
        }

        [Test]
        public static void AnyNull_GivenNullCollection_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = null;
            Assert.Throws<ArgumentNullException>(() => input.AnyNull());
        }

        [Test]
        public static void AnyNull_GivenEmptyCollection_ReturnsFalse()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Assert.IsFalse(input.AnyNull());
        }

        [Test]
        public static void AnyNull_GivenNonEmptyCollectionWithNoNullValues_ReturnsFalse()
        {
            IEnumerable<string> input = new[] { "A" };
            Assert.IsFalse(input.AnyNull());
        }

        [Test]
        public static void AnyNull_GivenNonEmptyCollectionWithNullValues_ReturnsTrue()
        {
            IEnumerable<string> input = new[] { "A", null, "C" };
            Assert.IsTrue(input.AnyNull());
        }

        [Test]
        public static void DistinctBy_GivenNullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EnumerableExtensions.DistinctBy<string, string>(null, x => x));
        }

        [Test]
        public static void DistinctBy_GivenNullSelector_ThrowsArgumentNullException()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };

            Assert.Throws<ArgumentNullException>(() => source.DistinctBy<string, string>(null));
        }

        [Test]
        public static void DistinctBy_ForComparerOverloadGivenNullCollection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EnumerableExtensions.DistinctBy<string, string>(null, x => x, EqualityComparer<string>.Default));
        }

        [Test]
        public static void DistinctBy_ForComparerOverloadGivenNullSelector_ThrowsArgumentNullException()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };

            Assert.Throws<ArgumentNullException>(() => source.DistinctBy(null, EqualityComparer<string>.Default));
        }

        [Test]
        public static void DistinctBy_GivenValidSelector_ReturnsExpectedResult()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };
            var expected = new[] { "first", "second" };

            var distinct = source.DistinctBy(word => word.Length);

            var areEqual = expected.SequenceEqual(distinct);

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void DistinctBy_GivenCustomComparer_ReturnsExpectedResult()
        {
            var source = new[] { "first", "FIRST", "second", "second", "third" };
            var expected = new[] { "first", "second", "third" };

            var distinct = source.DistinctBy(word => word, StringComparer.OrdinalIgnoreCase);

            var areEqual = expected.SequenceEqual(distinct);

            Assert.IsTrue(areEqual);
        }

        [Test]
        public static void DistinctBy_GivenNullComparer_ReturnsSameResultsAsNoParam()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };
            var expected = new[] { "first", "second" };

            var distinct = source.DistinctBy(word => word.Length, null);

            var areEqual = expected.SequenceEqual(distinct);

            Assert.IsTrue(areEqual);
        }
    }
}
