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
        public static async Task GetSequence_GivenValidName_ReturnsNone()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequence = provider.GetSequence("sequence_name");
            var sequenceIsNone = await sequence.IsNone.ConfigureAwait(false);

            Assert.IsTrue(sequenceIsNone);
        }

        [Test]
        public static async Task GetAllSequences_PropertyGet_HasCountOfZero()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequences = await provider.GetAllSequences().ConfigureAwait(false);

            Assert.Zero(sequences.Count);
        }

        [Test]
        public static async Task GetAllSequences_WhenEnumerated_ContainsNoValues()
        {
            var provider = new EmptyDatabaseSequenceProvider();
            var sequences = await provider.GetAllSequences().ConfigureAwait(false);
            var sequenceList = sequences.ToList();

            Assert.Zero(sequenceList.Count);
        }
    }
}
