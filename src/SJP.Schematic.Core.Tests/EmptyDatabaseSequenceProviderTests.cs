using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyDatabaseSequenceProviderTests
{
    [Test]
    public static void GetSequence_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseSequenceProvider();
        Assert.That(() => provider.GetSequence(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSequence_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseSequenceProvider();
        var sequence = provider.GetSequence("sequence_name");
        var sequenceIsNone = await sequence.IsNone.ConfigureAwait(false);

        Assert.That(sequenceIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllSequences_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSequenceProvider();
        var hasSequences = await provider.GetAllSequences()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasSequences, Is.False);
    }

    [Test]
    public static async Task GetAllSequences2_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSequenceProvider();
        var sequences = await provider.GetAllSequences2().ConfigureAwait(false);

        Assert.That(sequences, Is.Empty);
    }
}