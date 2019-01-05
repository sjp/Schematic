using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities
{
    [TestFixture]
    internal static class VersionResolvingDictionaryTests
    {
        [Test]
        public static void Ctor_GivenNullDictionary_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new VersionResolvingDictionary<int>(null));
        }

        [Test]
        public static void Ctor_GivenEmptyDictionary_ThrowsArgumentException()
        {
            var dictionary = new Dictionary<Version, int>();
            Assert.Throws<ArgumentException>(() => new VersionResolvingDictionary<int>(dictionary));
        }

        [Test]
        public static void GetValue_GivenNullVersion_ThrowsArgmentNullException()
        {
            var dictionary = new Dictionary<Version, int>
            {
                [new Version(1, 0)] = 1,
                [new Version(1, 1)] = 2
            };
            var versionDictionary = new VersionResolvingDictionary<int>(dictionary);

            Assert.Throws<ArgumentNullException>(() => versionDictionary.GetValue(null));
        }

        [Test]
        public static void GetValue_GivenVersionLessThanLowestInCtor_ReturnsLowestInCtor()
        {
            var dictionary = new Dictionary<Version, int>
            {
                [new Version(1, 0)] = 1,
                [new Version(1, 1)] = 2
            };
            var versionDictionary = new VersionResolvingDictionary<int>(dictionary);

            var value = versionDictionary.GetValue(new Version(0, 1));

            Assert.AreEqual(1, value);
        }

        [Test]
        public static void GetValue_GivenVersionMoreThanHighestInCtor_ReturnsHighestInCtor()
        {
            var dictionary = new Dictionary<Version, int>
            {
                [new Version(1, 0)] = 1,
                [new Version(1, 1)] = 2
            };
            var versionDictionary = new VersionResolvingDictionary<int>(dictionary);

            var value = versionDictionary.GetValue(new Version(2, 0));

            Assert.AreEqual(2, value);
        }

        [Test]
        public static void GetValue_GivenVersionEqualToMiddle_ReturnsValueFromMiddleVersion()
        {
            var dictionary = new Dictionary<Version, int>
            {
                [new Version(1, 0)] = 1,
                [new Version(1, 1)] = 2,
                [new Version(2, 0)] = 3
            };
            var versionDictionary = new VersionResolvingDictionary<int>(dictionary);

            var value = versionDictionary.GetValue(new Version(1, 1));

            Assert.AreEqual(2, value);
        }

        [Test]
        public static void GetValue_GivenVersionAboveMiddle_ReturnsValueFromMiddleVersion()
        {
            var dictionary = new Dictionary<Version, int>
            {
                [new Version(1, 0)] = 1,
                [new Version(1, 1)] = 2,
                [new Version(2, 0)] = 3
            };
            var versionDictionary = new VersionResolvingDictionary<int>(dictionary);

            var value = versionDictionary.GetValue(new Version(1, 5));

            Assert.AreEqual(2, value);
        }
    }
}
