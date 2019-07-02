using System;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    internal static class ObjectParameterTransformerTests
    {
        [Test]
        public static void ToDictionary_GivenNullObject_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectParameterTransformer.ToDictionary(null));
        }

        [Test]
        public static void ToDictionary_GivenEmptyObject_ReturnsEmptyDictionary()
        {
            var lookup = ObjectParameterTransformer.ToDictionary(new { });

            Assert.Zero(lookup.Count);
        }

        [Test]
        public static void ToDictionary_GivenNonEmptyObject_ReturnsDictionaryWithCorrectKeysAndValues()
        {
            const int first = 1;
            const string second = "second";
            var lookup = ObjectParameterTransformer.ToDictionary(new { FirstKey = first, SecondKey = second });

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, lookup.Count);
                Assert.AreEqual(first, lookup["FirstKey"]);
                Assert.AreEqual(second, lookup["SecondKey"]);
            });
        }
    }
}
