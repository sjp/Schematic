using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    internal static class IdentifierResolvingDictionaryTests
    {
        [Test]
        public static void Ctor_GivenNullDictionary_ThrowsArgumentNullException()
        {
            var resolver = Mock.Of<IIdentifierResolutionStrategy>();
            Assert.Throws<ArgumentNullException>(() => new IdentifierResolvingDictionary<string>(null, resolver));
        }

        [Test]
        public static void Ctor_GivenNullResolver_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            Assert.Throws<ArgumentNullException>(() => new IdentifierResolvingDictionary<string>(dictionary, null));
        }

        [Test]
        public static void Keys_PropertyGet_MatchesKeysInCtorArg()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keysEqual = dictionary.Keys.SequenceEqual(resolvingDictionary.Keys);
            Assert.IsTrue(keysEqual);
        }

        [Test]
        public static void Values_PropertyGet_MatchesValuesInCtorArg()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var valuesEqual = dictionary.Values.SequenceEqual(resolvingDictionary.Values);
            Assert.IsTrue(valuesEqual);
        }

        [Test]
        public static void Count_PropertyGet_MatchesCountInCtorArg()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.AreEqual(dictionary.Count, resolvingDictionary.Count);
        }

        [Test]
        public static void Enumerator_WhenInvoked_EnumeratesSameValuesAsCtorArg()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var equalEnumeration = dictionary.SequenceEqual(resolvingDictionary);
            Assert.IsTrue(equalEnumeration);
        }

        [Test]
        public static void ContainsKey_WhenGivenNullKey_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.Throws<ArgumentNullException>(() => resolvingDictionary.ContainsKey(null));
        }

        [Test]
        public static void ContainsKey_WhenGivenKeyNotInDictionaryButResolvesToOne_ReturnsTrue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keyPresent = resolvingDictionary.ContainsKey("Z");
            Assert.IsTrue(keyPresent);
        }

        [Test]
        public static void ContainsKey_WhenGivenKeyInDictionary_ReturnsTrue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keyPresent = resolvingDictionary.ContainsKey("B");
            Assert.IsTrue(keyPresent);
        }

        [Test]
        public static void ContainsKey_WhenGivenKeyInNotDictionaryAndDoesNotResolveToOne_ReturnsFalse()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["X"] = "x",
                ["Y"] = "y",
                ["Z"] = "z"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keyPresent = resolvingDictionary.ContainsKey("B");
            Assert.IsFalse(keyPresent);
        }

        [Test]
        public static void TryGetValue_WhenGivenNullKey_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.Throws<ArgumentNullException>(() => resolvingDictionary.TryGetValue(null, out var value));
        }

        [Test]
        public static void TryGetValue_WhenGivenKeyNotInDictionaryButResolvesToOne_ReturnsTrueAndCorrectValue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keyPresent = resolvingDictionary.TryGetValue("Z", out var value);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(keyPresent);
                Assert.AreEqual("b", value);
            });
        }

        [Test]
        public static void TryGetValue_WhenGivenKeyInDictionary_ReturnsTrueAndCorrectValue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keyPresent = resolvingDictionary.TryGetValue("B", out var value);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(keyPresent);
                Assert.AreEqual("b", value);
            });
        }

        [Test]
        public static void TryGetValue_WhenGivenKeyInNotDictionaryAndDoesNotResolveToOne_ReturnsFalseAndNullValue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["X"] = "x",
                ["Y"] = "y",
                ["Z"] = "z"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var keyPresent = resolvingDictionary.TryGetValue("B", out var value);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(keyPresent);
                Assert.IsNull(value);
            });
        }

        [Test]
        public static void Indexer_WhenGivenNullKey_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.Throws<ArgumentNullException>(() => { _ = resolvingDictionary[null]; });
        }

        [Test]
        public static void Indexer_WhenGivenKeyNotInDictionaryButResolvesToOne_ReturnsCorrectValue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var result = resolvingDictionary["Z"];
            Assert.AreEqual("b", result);
        }

        [Test]
        public static void Indexer_WhenGivenKeyInDictionary_ReturnsCorrectValue()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["B"] = "b",
                ["C"] = "c"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            var result = resolvingDictionary["B"];
            Assert.AreEqual("b", result);
        }

        [Test]
        public static void Indexer_WhenGivenKeyInNotDictionaryAndDoesNotResolveToOne_ThrowsKeyNotFoundException()
        {
            var dictionary = new Dictionary<Identifier, string>
            {
                ["X"] = "x",
                ["Y"] = "y",
                ["Z"] = "z"
            };
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.Throws<KeyNotFoundException>(() => { _ = resolvingDictionary["B"]; });
        }

        private sealed class FakeIdentifierResolver : IIdentifierResolutionStrategy
        {
            public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier) => _identifiers;

            private static readonly IEnumerable<Identifier> _identifiers = new[]
            {
                new Identifier("A"),
                new Identifier("B"),
                new Identifier("C"),
                new Identifier("D"),
                new Identifier("E")
            };
        }
    }
}
