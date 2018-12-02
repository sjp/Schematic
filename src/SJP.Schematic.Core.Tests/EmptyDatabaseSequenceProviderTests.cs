using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class EmptyDatabaseSequenceProviderTests
    {
        [Test]
        public static void GetSequence_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSequence(null));
        }

        [Test]
        public static void GetSequenceAsync_GivenNullName_ThrowsArgumentNullException()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            Assert.Throws<ArgumentNullException>(() => provider.GetSequenceAsync(null));
        }

        [Test]
        public static void GetSequence_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequence = provider.GetSequence("sequence_name");

            Assert.IsTrue(sequence.IsNone);
        }

        [Test]
        public static async Task GetSequenceAsync_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequence = provider.GetSequenceAsync("sequence_name");
            var sequenceIsNone = await sequence.IsNone.ConfigureAwait(false);

            Assert.IsTrue(sequenceIsNone);
        }

        [Test]
        public static void Sequences_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequences = provider.Sequences;

            Assert.Zero(sequences.Count);
        }

        [Test]
        public static void Sequences_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequences = provider.Sequences.ToList();

            Assert.Zero(sequences.Count);
        }

        [Test]
        public static async Task SequencesAsync_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequences = await provider.SequencesAsync().ConfigureAwait(false);

            Assert.Zero(sequences.Count);
        }

        [Test]
        public static async Task SequencesAsync_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequences = await provider.SequencesAsync().ConfigureAwait(false);
            var sequenceList = sequences.ToList();

            Assert.Zero(sequenceList.Count);
        }
    }
}
