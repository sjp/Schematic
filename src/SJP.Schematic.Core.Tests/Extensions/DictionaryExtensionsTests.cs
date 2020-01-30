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
            Assert.That(() => input.ToDictionary(), Throws.ArgumentNullException);
        }

        [Test]
        public static void ToDictionary_GivenEmptyCollection_ReturnsEmptyDictionary()
        {
            var input = new Dictionary<string, string>();
            var result = input.ToDictionary();

            Assert.That(result, Is.Empty);
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
                Assert.That(result.ContainsKey("a"), Is.True);
                Assert.That(result.ContainsKey("b"), Is.True);
                Assert.That(result["a"], Is.EqualTo("A"));
                Assert.That(result["b"], Is.EqualTo("B"));
            });
        }
    }
}
