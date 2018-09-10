using System;
using System.Collections.Generic;
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
    }
}
