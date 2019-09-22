using System;
using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public static void ToParameters_GivenNullLookup_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ObjectParameterTransformer.ToParameters(null));
        }

        [Test]
        public static void ToParameters_GivenEmptyLookup_ReturnsEmptyDictionary()
        {
            var parameters = ObjectParameterTransformer.ToParameters(new Dictionary<string, object>());
            var paramCount = parameters.ParameterNames.Count();

            Assert.Zero(paramCount);
        }

        [Test]
        public static void ToParameters_GivenNonEmptyLookup_ReturnsParametersWithCorrectKeysAndValues()
        {
            const int first = 1;
            const string second = "second";

            var lookup = new Dictionary<string, object>
            {
                ["FirstKey"] = first,
                ["SecondKey"] = second
            };
            var parameters = ObjectParameterTransformer.ToParameters(lookup);
            var paramCount = parameters.ParameterNames.Count();

            Assert.Multiple(() =>
            {
                Assert.AreEqual(2, paramCount);
                Assert.AreEqual(first, parameters.Get<int>("FirstKey"));
                Assert.AreEqual(second, parameters.Get<string>("SecondKey"));
            });
        }
    }
}
