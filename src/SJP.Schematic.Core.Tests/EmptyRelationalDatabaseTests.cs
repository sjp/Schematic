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
    public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetSynonym(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(() => Database.GetRoutine(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetTable_GivenValidSequenceName_ReturnsNone()
    {
        var tableName = new Identifier("test");
        var tableIsNone = await Database.GetTable(tableName).IsNone.ConfigureAwait(false);

        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllTables_WhenEnumerated_ContainsNoValues()
    {
        var hasTables = await Database.GetAllTables()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasTables, Is.False);
    }

    [Test]
    public static async Task GetAllTables2_WhenRetrieved_ContainsNoValues()
    {
        var tables = await Database.GetAllTables2().ConfigureAwait(false);

        Assert.That(tables, Is.Empty);
    }

    [Test]
    public static async Task GetView_GivenValidViewName_ReturnsNone()
    {
        var viewName = new Identifier("test");
        var viewIsNone = await Database.GetView(viewName).IsNone.ConfigureAwait(false);

        Assert.That(viewIsNone, Is.True);
    }

    [Test]
    public static async Task GetAllViews_WhenEnumerated_ContainsNoValues()
    {
        var hasViews = await Database.GetAllViews()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasViews, Is.False);
    }

    [Test]
    public static async Task GetAllViews2_WhenRetrieved_ContainsNoValues()
    {
        var views = await Database.GetAllViews2().ConfigureAwait(false);

        Assert.That(views, Is.Empty);
    }

    [Test]
    public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
    {
        var sequenceName = new Identifier("test");
        var sequenceIsNone = await Database.GetSequence(sequenceName).IsNone.ConfigureAwait(false);

        Assert.That(sequenceIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSequences_WhenEnumerated_ContainsNoValues()
    {
        var hasSequences = await Database.EnumerateAllSequences()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasSequences, Is.False);
    }

    [Test]
    public static async Task GetAllSequences2_WhenRetrieved_ContainsNoValues()
    {
        var sequences = await Database.GetAllSequences2().ConfigureAwait(false);

        Assert.That(sequences, Is.Empty);
    }

    [Test]
    public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
    {
        var synonymName = new Identifier("test");
        var synonymIsNone = await Database.GetSynonym(synonymName).IsNone.ConfigureAwait(false);

        Assert.That(synonymIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSynonyms_WhenEnumerated_ContainsNoValues()
    {
        var synonyms = await Database.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);

        Assert.That(synonyms, Is.Empty);
    }

    [Test]
    public static async Task GetAllSynonyms2_WhenRetrieved_ContainsNoValues()
    {
        var synonyms = await Database.GetAllSynonyms2().ConfigureAwait(false);

        Assert.That(synonyms, Is.Empty);
    }

    [Test]
    public static async Task GetRoutine_GivenValidRoutineName_ReturnsNone()
    {
        var routineName = new Identifier("test");
        var routineIsNone = await Database.GetRoutine(routineName).IsNone.ConfigureAwait(false);

        Assert.That(routineIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllRoutines_WhenEnumerated_ContainsNoValues()
    {
        var hasRoutines = await Database.EnumerateAllRoutines()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasRoutines, Is.False);
    }

    [Test]
    public static async Task GetAllRoutines2_WhenRetrieved_ContainsNoValues()
    {
        var routines = await Database.GetAllRoutines2().ConfigureAwait(false);

        Assert.That(routines, Is.Empty);
    }
}