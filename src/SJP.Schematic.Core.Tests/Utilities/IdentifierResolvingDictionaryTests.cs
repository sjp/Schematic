using System.Collections.Generic;
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

            Assert.That(() => new IdentifierResolvingDictionary<string>(null, resolver), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullResolver_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            Assert.That(() => new IdentifierResolvingDictionary<string>(dictionary, null), Throws.ArgumentNullException);
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

            Assert.That(dictionary.Keys, Is.EqualTo(resolvingDictionary.Keys));
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

            Assert.That(dictionary.Values, Is.EqualTo(resolvingDictionary.Values));
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

            Assert.That(dictionary.Count, Is.EqualTo(resolvingDictionary.Count));
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

            Assert.That(resolvingDictionary, Is.EqualTo(dictionary));
        }

        [Test]
        public static void ContainsKey_WhenGivenNullKey_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.That(() => resolvingDictionary.ContainsKey(null), Throws.ArgumentNullException);
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
            Assert.That(keyPresent, Is.True);
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
            Assert.That(keyPresent, Is.True);
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
            Assert.That(keyPresent, Is.False);
        }

        [Test]
        public static void TryGetValue_WhenGivenNullKey_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.That(() => resolvingDictionary.TryGetValue(null, out var value), Throws.ArgumentNullException);
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
                Assert.That(keyPresent, Is.True);
                Assert.That(value, Is.EqualTo("b"));
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
                Assert.That(keyPresent, Is.True);
                Assert.That(value, Is.EqualTo("b"));
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
                Assert.That(keyPresent, Is.False);
                Assert.That(value, Is.Null);
            });
        }

        [Test]
        public static void Indexer_WhenGivenNullKey_ThrowsArgumentNullException()
        {
            var dictionary = new Dictionary<Identifier, string>();
            var resolver = new FakeIdentifierResolver();
            var resolvingDictionary = new IdentifierResolvingDictionary<string>(dictionary, resolver);

            Assert.That(() => _ = resolvingDictionary[null], Throws.ArgumentNullException);
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
            Assert.That(result, Is.EqualTo("b"));
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
            Assert.That(result, Is.EqualTo("b"));
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

            Assert.That(() => _ = resolvingDictionary["B"], Throws.TypeOf<KeyNotFoundException>());
        }

        private sealed class FakeIdentifierResolver : IIdentifierResolutionStrategy
        {
            public IEnumerable<Identifier> GetResolutionOrder(Identifier identifier) => Identifiers;

            private static readonly IEnumerable<Identifier> Identifiers = new[]
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
