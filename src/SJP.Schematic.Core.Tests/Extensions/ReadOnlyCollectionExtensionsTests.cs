using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class ReadOnlyCollectionExtensionsTests
    {
        [Test]
        public static void Empty_GivenNullCollection_ThrowsArgumentNullException()
        {
            IReadOnlyCollection<string> input = null;
            Assert.Throws<ArgumentNullException>(() => input.Empty());
        }

        [Test]
        public static void Empty_GivenEmptyCollection_ReturnsTrue()
        {
            IReadOnlyCollection<string> input = Array.Empty<string>();
            Assert.IsTrue(input.Empty());
        }

        [Test]
        public static void Empty_GivenNonEmptyCollection_ReturnsFalse()
        {
            IReadOnlyCollection<string> input = new[] { "A" };
            Assert.IsFalse(input.Empty());
        }

        [Test]
        public static void ToReadOnlyList_GivenNullCollection_ThrowsArgumentNullException()
        {
            IEnumerable<string> input = null;
            Assert.Throws<ArgumentNullException>(() => input.ToReadOnlyList());
        }

        [Test]
        public static void ToReadOnlyList_GivenNonEmptyCollection_ReturnsListWithSameValues()
        {
            IEnumerable<string> input = new[] { "A" };
            var result = input.ToReadOnlyList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, result.Count);
                Assert.AreEqual("A", result[0]);
            });
        }
    }
}
