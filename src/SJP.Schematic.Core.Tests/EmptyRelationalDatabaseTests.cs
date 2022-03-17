using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class EmptyRelationalDatabaseTests
{
    private static IRelationalDatabase Database => new EmptyRelationalDatabase(Mock.Of<IIdentifierDefaults>());

    [Test]
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        Assert.That(() => new EmptyRelationalDatabase(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetTable(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetView(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetSequence(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
    {
        var sequenceName = new Identifier("test");
        var sequenceIsNone = await Database.GetSequence(sequenceName).IsNone.ConfigureAwait(false);

        Assert.That(sequenceIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllSequences_WhenEnumerated_ContainsNoValues()
    {
        var hasSequences = await Database.GetAllSequences()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasSequences, Is.False);
    }

    [Test]
    public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetSynonym(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
    {
        var synonymName = new Identifier("test");
        var synonymIsNone = await Database.GetSynonym(synonymName).IsNone.ConfigureAwait(false);

        Assert.That(synonymIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllSynonyms_WhenEnumerated_ContainsNoValues()
    {
        var synonyms = await Database.GetAllSynonyms().ToListAsync().ConfigureAwait(false);

        Assert.That(synonyms, Is.Empty);
    }

    [Test]
    public static void GetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetRoutine(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetRoutine_GivenValidRoutineName_ReturnsNone()
    {
        var routineName = new Identifier("test");
        var routineIsNone = await Database.GetRoutine(routineName).IsNone.ConfigureAwait(false);

        Assert.That(routineIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllRoutines_WhenEnumerated_ContainsNoValues()
    {
        var hasRoutines = await Database.GetAllRoutines()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasRoutines, Is.False);
    }
}
