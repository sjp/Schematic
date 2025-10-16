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
        var tableIsNone = await Database.GetTable(tableName).IsNone;

        Assert.That(tableIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllTables_WhenEnumerated_ContainsNoValues()
    {
        var hasTables = await Database.EnumerateAllTables().AnyAsync();

        Assert.That(hasTables, Is.False);
    }

    [Test]
    public static async Task GetAllTables_WhenRetrieved_ContainsNoValues()
    {
        var tables = await Database.GetAllTables();

        Assert.That(tables, Is.Empty);
    }

    [Test]
    public static async Task GetView_GivenValidViewName_ReturnsNone()
    {
        var viewName = new Identifier("test");
        var viewIsNone = await Database.GetView(viewName).IsNone;

        Assert.That(viewIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllViews_WhenEnumerated_ContainsNoValues()
    {
        var hasViews = await Database.EnumerateAllViews().AnyAsync();

        Assert.That(hasViews, Is.False);
    }

    [Test]
    public static async Task GetAllViews_WhenRetrieved_ContainsNoValues()
    {
        var views = await Database.GetAllViews();

        Assert.That(views, Is.Empty);
    }

    [Test]
    public static async Task GetSequence_GivenValidSequenceName_ReturnsNone()
    {
        var sequenceName = new Identifier("test");
        var sequenceIsNone = await Database.GetSequence(sequenceName).IsNone;

        Assert.That(sequenceIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSequences_WhenEnumerated_ContainsNoValues()
    {
        var hasSequences = await Database.EnumerateAllSequences().AnyAsync();

        Assert.That(hasSequences, Is.False);
    }

    [Test]
    public static async Task GetAllSequences_WhenRetrieved_ContainsNoValues()
    {
        var sequences = await Database.GetAllSequences();

        Assert.That(sequences, Is.Empty);
    }

    [Test]
    public static async Task GetSynonym_GivenValidSynonymName_ReturnsNone()
    {
        var synonymName = new Identifier("test");
        var synonymIsNone = await Database.GetSynonym(synonymName).IsNone;

        Assert.That(synonymIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSynonyms_WhenEnumerated_ContainsNoValues()
    {
        var synonyms = await Database.EnumerateAllSynonyms().ToListAsync();

        Assert.That(synonyms, Is.Empty);
    }

    [Test]
    public static async Task GetAllSynonyms_WhenRetrieved_ContainsNoValues()
    {
        var synonyms = await Database.GetAllSynonyms();

        Assert.That(synonyms, Is.Empty);
    }

    [Test]
    public static async Task GetRoutine_GivenValidRoutineName_ReturnsNone()
    {
        var routineName = new Identifier("test");
        var routineIsNone = await Database.GetRoutine(routineName).IsNone;

        Assert.That(routineIsNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllRoutines_WhenEnumerated_ContainsNoValues()
    {
        var hasRoutines = await Database.EnumerateAllRoutines().AnyAsync();

        Assert.That(hasRoutines, Is.False);
    }

    [Test]
    public static async Task GetAllRoutines_WhenRetrieved_ContainsNoValues()
    {
        var routines = await Database.GetAllRoutines();

        Assert.That(routines, Is.Empty);
    }
}