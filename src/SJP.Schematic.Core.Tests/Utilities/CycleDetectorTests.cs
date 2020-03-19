using System;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    internal static class CycleDetectorTests
    {
        [Test]
        public static void GetCyclePaths_GivenNullTables_ThrowsArgumentNullException()
        {
            var cycleDetector = new CycleDetector();

            Assert.That(() => cycleDetector.GetCyclePaths(null), Throws.ArgumentNullException);
        }

        [Test]
        public static void GetCyclePaths_GivenEmptyTables_ReturnsEmptyCollection()
        {
            var cycleDetector = new CycleDetector();
            var result = cycleDetector.GetCyclePaths(Array.Empty<IRelationalDatabaseTable>());

            Assert.That(result, Is.Empty);
        }
    }
}
