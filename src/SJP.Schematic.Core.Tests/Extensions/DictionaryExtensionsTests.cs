using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class DictionaryExtensionsTests
    {
        [Test]
        public static void ToDictionary_GivenNullCollection_ThrowsArgumentNullException()
        {
            IEnumerable<KeyValuePair<string, string>> input = null;
            Assert.Throws<ArgumentNullException>(() => input.ToDictionary());
        }

        [Test]
        public static void ToDictionary_GivenEmptyCollection_ReturnsEmptyDictionary()
        {
            var input = new Dictionary<string, string>();
            var result = input.ToDictionary();

            Assert.Zero(result.Count);
        }

        [Test]
        public static void ToDictionary_GivenNonEmptyCollection_ReturnsDictionaryWithEqualKeysAndValues()
        {
            var input = new Dictionary<string, string>
            {
                ["a"] = "A",
                ["b"] = "B"
            };
            var result = input.ToDictionary();

            Assert.Multiple(() =>
            {
                Assert.IsTrue(result.ContainsKey("a"));
                Assert.IsTrue(result.ContainsKey("b"));
                Assert.AreEqual("A", result["a"]);
                Assert.AreEqual("B", result["b"]);
            });
        }
    }
}
