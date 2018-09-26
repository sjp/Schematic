using System;
using System.Linq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class VerbatimIdentifierResolutionStrategyTests
    {
        [Test]
        public static void GetResolutionOrder_GivenNullIdentifier_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();
            Assert.Throws<ArgumentNullException>(() => resolver.GetResolutionOrder(null));
        }

        [Test]
        public static void GetResolutionOrder_GivenIdentifier_ReturnsIdenticalIdentifier()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();
            var input = new Identifier("A", "B", "C", "D");

            var results = resolver.GetResolutionOrder(input).ToList();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, results.Count);
                Assert.AreEqual(input, results[0]);
            });
        }
    }
}
