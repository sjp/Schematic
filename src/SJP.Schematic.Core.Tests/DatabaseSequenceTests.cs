using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseSequenceTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            Assert.That(() => new DatabaseSequence(null, 1, 1, Option<decimal>.None, Option<decimal>.None, true, 0), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenZeroIncrement_ThrowsArgException()
        {
            Assert.That(() => new DatabaseSequence("test", 1, 0, Option<decimal>.None, Option<decimal>.None, true, 0), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenPositiveIncrementAndMinValueLargerThanStart_ThrowsArgumentException()
        {
            Assert.That(() => new DatabaseSequence("test", 1, 1, Option<decimal>.Some(2), Option<decimal>.None, true, 0), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenPositiveIncrementAndMaxValueLessThanStart_ThrowsArgumentException()
        {
            Assert.That(() => new DatabaseSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.Some(-1), true, 0), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNegativeIncrementAndMinValueLessThanStart_ThrowsArgumentException()
        {
            Assert.That(() => new DatabaseSequence("test", 1, -1, Option<decimal>.Some(0), Option<decimal>.None, true, 0), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNegativeIncrementAndMaxValueGreaterThanStart_ThrowsArgumentException()
        {
            Assert.That(() => new DatabaseSequence("test", 1, -1, Option<decimal>.None, Option<decimal>.Some(2), true, 0), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNegativeCacheSize_SetsCacheSizeToUnknownValue()
        {
            var sequence = new DatabaseSequence("test", 1, -1, Option<decimal>.None, Option<decimal>.None, true, -3);

            Assert.That(sequence.Cache, Is.EqualTo(DatabaseSequence.UnknownCacheSize));
        }

        [Test]
        public static void Name_PropertyGet_MatchesCtorArg()
        {
            var sequenceName = new Identifier("test");
            var sequence = new DatabaseSequence(sequenceName, 1, 1, Option<decimal>.None, Option<decimal>.None, true, 0);

            Assert.That(sequence.Name, Is.EqualTo(sequenceName));
        }

        [Test]
        public static void Cache_PropertyGet_MatchesCtorArg()
        {
            const int cacheSize = 20;
            var sequence = new DatabaseSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, true, cacheSize);

            Assert.That(sequence.Cache, Is.EqualTo(cacheSize));
        }

        [Test]
        public static void Cycle_PropertyGet_MatchesCtorArg()
        {
            var sequence = new DatabaseSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.None, true, 1);

            Assert.That(sequence.Cycle, Is.True);
        }

        [Test]
        public static void Increment_PropertyGet_MatchesCtorArg()
        {
            const int increment = 100;
            var sequence = new DatabaseSequence("test", 1, increment, Option<decimal>.None, Option<decimal>.None, true, 1);

            Assert.That(sequence.Increment, Is.EqualTo(increment));
        }

        [Test]
        public static void MaxValue_PropertyGet_MatchesCtorArg()
        {
            const int maxValue = 100;
            var sequence = new DatabaseSequence("test", 1, 1, Option<decimal>.None, Option<decimal>.Some(maxValue), true, 1);

            Assert.That(sequence.MaxValue.UnwrapSome(), Is.EqualTo(maxValue));
        }

        [Test]
        public static void MinValue_PropertyGet_MatchesCtorArg()
        {
            const int minValue = 100;
            var sequence = new DatabaseSequence("test", minValue, 1, Option<decimal>.Some(minValue), Option<decimal>.None, true, 1);

            Assert.That(sequence.MinValue.UnwrapSome(), Is.EqualTo(minValue));
        }

        [Test]
        public static void Start_PropertyGet_MatchesCtorArg()
        {
            const int start = 100;
            var sequence = new DatabaseSequence("test", start, 1, Option<decimal>.None, Option<decimal>.None, true, 1);

            Assert.That(sequence.Start, Is.EqualTo(start));
        }

        [TestCase("", "test_sequence", "Sequence: test_sequence")]
        [TestCase("test_schema", "test_sequence", "Sequence: test_schema.test_sequence")]
        public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
        {
            var sequenceName = Identifier.CreateQualifiedIdentifier(schema, localName);
            var sequence = new DatabaseSequence(sequenceName, 1, 1, Option<decimal>.None, Option<decimal>.None, true, 1);

            var result = sequence.ToString();

            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
