using System;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseSequenceTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSequence(null, 1, 1, null, null, true, 0));
        }

        [Test]
        public static void Ctor_GivenZeroIncrement_ThrowsArgException()
        {
            Assert.Throws<ArgumentException>(() => new DatabaseSequence("test", 1, 0, null, null, true, 0));
        }

        [Test]
        public static void Ctor_GivenPositiveIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DatabaseSequence("test", 1, 1, 2, null, true, 0));
        }

        [Test]
        public static void Ctor_GivenPositiveIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DatabaseSequence("test", 1, 1, null, -1, true, 0));
        }

        [Test]
        public static void Ctor_GivenNegativeIncrementAndMinValueLessThanStart_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DatabaseSequence("test", 1, -1, 0, null, true, 0));
        }

        [Test]
        public static void Ctor_GivenNegativeIncrementAndMaxValueGreaterThanStart_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new DatabaseSequence("test", 1, -1, null, 2, true, 0));
        }

        [Test]
        public static void Ctor_GivenNegativeCacheSize_SetsCacheSizeToUnknownValue()
        {
            var sequence =  new DatabaseSequence("test", 1, -1, null, null, true, -3);

            Assert.AreEqual(DatabaseSequence.UnknownCacheSize, sequence.Cache);
        }

        [Test]
        public static void Name_PropertyGet_MatchesCtorArg()
        {
            var sequenceName = new Identifier("test");
            var sequence = new DatabaseSequence(sequenceName, 1, 1, null, null, true, 0);

            Assert.AreEqual(sequenceName, sequence.Name);
        }

        [Test]
        public static void Cache_PropertyGet_MatchesCtorArg()
        {
            const int cacheSize = 20;
            var sequence = new DatabaseSequence("test", 1, 1, null, null, true, cacheSize);

            Assert.AreEqual(cacheSize, sequence.Cache);
        }

        [Test]
        public static void Cycle_PropertyGet_MatchesCtorArg()
        {
            var sequence = new DatabaseSequence("test", 1, 1, null, null, true, 1);

            Assert.IsTrue(sequence.Cycle);
        }

        [Test]
        public static void Increment_PropertyGet_MatchesCtorArg()
        {
            const int increment = 100;
            var sequence = new DatabaseSequence("test", 1, increment, null, null, true, 1);

            Assert.AreEqual(increment, sequence.Increment);
        }

        [Test]
        public static void MaxValue_PropertyGet_MatchesCtorArg()
        {
            const int maxValue = 100;
            var sequence = new DatabaseSequence("test", 1, 1, null, maxValue, true, 1);

            Assert.AreEqual(maxValue, sequence.MaxValue);
        }

        [Test]
        public static void MinValue_PropertyGet_MatchesCtorArg()
        {
            const int minValue = 100;
            var sequence = new DatabaseSequence("test", minValue, 1, minValue, null, true, 1);

            Assert.AreEqual(minValue, sequence.MinValue);
        }

        [Test]
        public static void Start_PropertyGet_MatchesCtorArg()
        {
            const int start = 100;
            var sequence = new DatabaseSequence("test", start, 1, null, null, true, 1);

            Assert.AreEqual(start, sequence.Start);
        }
    }
}
