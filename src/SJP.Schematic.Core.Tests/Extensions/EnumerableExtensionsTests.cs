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
            Assert.That(() => input.Empty(), Throws.ArgumentNullException);
        }

        [Test]
        public static void Empty_GivenEmptyCollection_ReturnsTrue()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Assert.That(input.Empty(), Is.True);
        }

        [Test]
        public static void Empty_GivenNonEmptyCollection_ReturnsFalse()
        {
            IEnumerable<string> input = new[] { "A" };
            Assert.That(input.Empty(), Is.False);
        }

        [Test]
        public static void Empty_GivenNullCollectionWithValidPredicate_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = null;
            bool predicate(string _) => true;

            Assert.That(() => input.Empty(predicate), Throws.ArgumentNullException);
        }

        [Test]
        public static void Empty_GivenNullPredicate_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Func<string, bool> predicate = null;

            Assert.That(() => input.Empty(predicate), Throws.ArgumentNullException);
        }

        [Test]
        public static void Empty_GivenEmptyCollectionWithPredicate_ReturnsTrue()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Assert.That(input.Empty(x => x == "A"), Is.True);
        }

        [Test]
        public static void Empty_GivenNonEmptyCollectionWithNonMatchingPredicate_ReturnsTrue()
        {
            IEnumerable<string> input = new[] { "B" };
            Assert.That(input.Empty(x => x == "A"), Is.True);
        }

        [Test]
        public static void Empty_GivenNonEmptyCollectionWithMatchingPredicate_ReturnsFalse()
        {
            IEnumerable<string> input = new[] { "A" };
            Assert.That(input.Empty(x => x == "A"), Is.False);
        }

        [Test]
        public static void AnyNull_GivenNullCollection_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = null;
            Assert.That(() => input.AnyNull(), Throws.ArgumentNullException);
        }

        [Test]
        public static void AnyNull_GivenEmptyCollection_ReturnsFalse()
        {
            IEnumerable<string> input = Array.Empty<string>();
            Assert.That(input.AnyNull(), Is.False);
        }

        [Test]
        public static void AnyNull_GivenNonEmptyCollectionWithNoNullValues_ReturnsFalse()
        {
            IEnumerable<string> input = new[] { "A" };
            Assert.That(input.AnyNull(), Is.False);
        }

        [Test]
        public static void AnyNull_GivenNonEmptyCollectionWithNullValues_ReturnsTrue()
        {
            IEnumerable<string> input = new[] { "A", null, "C" };
            Assert.That(input.AnyNull(), Is.True);
        }

        [Test]
        public static void DistinctBy_GivenNullCollection_ThrowsArgumentNullException()
        {
            Assert.That(() => EnumerableExtensions.DistinctBy<string, string>(null, x => x), Throws.ArgumentNullException);
        }

        [Test]
        public static void DistinctBy_GivenNullSelector_ThrowsArgumentNullException()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };

            Assert.That(() => source.DistinctBy<string, string>(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void DistinctBy_ForComparerOverloadGivenNullCollection_ThrowsArgumentNullException()
        {
            Assert.That(() => EnumerableExtensions.DistinctBy<string, string>(null, x => x, EqualityComparer<string>.Default), Throws.ArgumentNullException);
        }

        [Test]
        public static void DistinctBy_ForComparerOverloadGivenNullSelector_ThrowsArgumentNullException()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };

            Assert.That(() => source.DistinctBy(null, EqualityComparer<string>.Default), Throws.ArgumentNullException);
        }

        [Test]
        public static void DistinctBy_GivenValidSelector_ReturnsExpectedResult()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };
            var expected = new[] { "first", "second" };

            var distinct = source.DistinctBy(word => word.Length);

            Assert.That(distinct, Is.EqualTo(expected));
        }

        [Test]
        public static void DistinctBy_GivenCustomComparer_ReturnsExpectedResult()
        {
            var source = new[] { "first", "FIRST", "second", "second", "third" };
            var expected = new[] { "first", "second", "third" };

            var distinct = source.DistinctBy(word => word, StringComparer.OrdinalIgnoreCase);

            Assert.That(distinct, Is.EqualTo(expected));
        }

        [Test]
        public static void DistinctBy_GivenNullComparer_ReturnsSameResultsAsNoParam()
        {
            var source = new[] { "first", "second", "third", "fourth", "fifth" };
            var expected = new[] { "first", "second" };

            var distinct = source.DistinctBy(word => word.Length, null);

            Assert.That(distinct, Is.EqualTo(expected));
        }
    }
}
