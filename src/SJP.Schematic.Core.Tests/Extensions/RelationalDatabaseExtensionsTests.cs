using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class RelationalDatabaseExtensionsTests
{
    private static IRelationalDatabase EmptyDatabase => new RelationalDatabase(
        new IdentifierDefaults("test_server", "test_database", "test_schema"),
        new VerbatimIdentifierResolutionStrategy(),
        [],
        [],
        [],
        [],
        []
    );

    [Test]
    public static void SnapshotAsync_GivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseExtensions.SnapshotAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithIdentifierResolverGivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseExtensions.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.SnapshotAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenEmptyRelationalDatabaseWithDefaultResolver_ReturnsDatabaseWithMatchingIdentifierDefaults()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            [],
            [],
            [],
            [],
            []
        );

        var snapshot = await database.SnapshotAsync().ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.IdentifierDefaults.Server, Is.EqualTo(identifierDefaults.Server));
            Assert.That(snapshot.IdentifierDefaults.Database, Is.EqualTo(identifierDefaults.Database));
            Assert.That(snapshot.IdentifierDefaults.Schema, Is.EqualTo(identifierDefaults.Schema));
        });
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenEmptyRelationalDatabase_ReturnsDatabaseWithMatchingIdentifierDefaults()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            [],
            [],
            [],
            [],
            []
        );

        var snapshot = await database.SnapshotAsync(identifierResolver).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.IdentifierDefaults.Server, Is.EqualTo(identifierDefaults.Server));
            Assert.That(snapshot.IdentifierDefaults.Database, Is.EqualTo(identifierDefaults.Database));
            Assert.That(snapshot.IdentifierDefaults.Schema, Is.EqualTo(identifierDefaults.Schema));
        });
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseWithObjectsAndDefaultResolver_ReturnsDatabaseWithMatchingObjectsInGetAllObjects()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routine = new Mock<IDatabaseRoutine>(MockBehavior.Strict);
        routine.Setup(t => t.Name).Returns(testRoutineName);
        var routines = new[] { routine.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var snapshot = await database.SnapshotAsync(identifierResolver).ConfigureAwait(false);
        var snapshotTables = await snapshot.GetAllTables().ToListAsync().ConfigureAwait(false);
        var snapshotViews = await snapshot.GetAllViews().ToListAsync().ConfigureAwait(false);
        var snapshotSequences = await snapshot.GetAllSequences().ToListAsync().ConfigureAwait(false);
        var snapshotSynonyms = await snapshot.GetAllSynonyms().ToListAsync().ConfigureAwait(false);
        var snapshotRoutines = await snapshot.GetAllRoutines().ToListAsync().ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshotTables, Is.EqualTo(tables));
            Assert.That(snapshotViews, Is.EqualTo(views));
            Assert.That(snapshotSequences, Is.EqualTo(sequences));
            Assert.That(snapshotSynonyms, Is.EqualTo(synonyms));
            Assert.That(snapshotRoutines, Is.EqualTo(routines));
        });
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseWithObjects_ReturnsDatabaseWithMatchingObjectsInGetAllObjects()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routine = new Mock<IDatabaseRoutine>(MockBehavior.Strict);
        routine.Setup(t => t.Name).Returns(testRoutineName);
        var routines = new[] { routine.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var snapshot = await database.SnapshotAsync(identifierResolver).ConfigureAwait(false);
        var snapshotTables = await snapshot.GetAllTables().ToListAsync().ConfigureAwait(false);
        var snapshotViews = await snapshot.GetAllViews().ToListAsync().ConfigureAwait(false);
        var snapshotSequences = await snapshot.GetAllSequences().ToListAsync().ConfigureAwait(false);
        var snapshotSynonyms = await snapshot.GetAllSynonyms().ToListAsync().ConfigureAwait(false);
        var snapshotRoutines = await snapshot.GetAllRoutines().ToListAsync().ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshotTables, Is.EqualTo(tables));
            Assert.That(snapshotViews, Is.EqualTo(views));
            Assert.That(snapshotSequences, Is.EqualTo(sequences));
            Assert.That(snapshotSynonyms, Is.EqualTo(synonyms));
            Assert.That(snapshotRoutines, Is.EqualTo(routines));
        });
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseWithObjectsAndDefaultResolver_ReturnsDatabaseWithMatchingObjectsInGetObjectMethods()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routine = new Mock<IDatabaseRoutine>(MockBehavior.Strict);
        routine.Setup(t => t.Name).Returns(testRoutineName);
        var routines = new[] { routine.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var snapshot = await database.SnapshotAsync().ConfigureAwait(false);
        var snapshotTable = await snapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var snapshotView = await snapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var snapshotSequence = await snapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var snapshotSynonym = await snapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var snapshotRoutine = await snapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshotTable, OptionIs.Some);
            Assert.That(snapshotTable.UnwrapSome().Name.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(snapshotView, OptionIs.Some);
            Assert.That(snapshotView.UnwrapSome().Name.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(snapshotSequence, OptionIs.Some);
            Assert.That(snapshotSequence.UnwrapSome().Name.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(snapshotSynonym, OptionIs.Some);
            Assert.That(snapshotSynonym.UnwrapSome().Name.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(snapshotRoutine, OptionIs.Some);
            Assert.That(snapshotRoutine.UnwrapSome().Name.LocalName, Is.EqualTo("test_routine_name"));
        });
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseWithObjects_ReturnsDatabaseWithMatchingObjectsInGetObjectMethods()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routine = new Mock<IDatabaseRoutine>(MockBehavior.Strict);
        routine.Setup(t => t.Name).Returns(testRoutineName);
        var routines = new[] { routine.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var snapshot = await database.SnapshotAsync(identifierResolver).ConfigureAwait(false);
        var snapshotTable = await snapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var snapshotView = await snapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var snapshotSequence = await snapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var snapshotSynonym = await snapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var snapshotRoutine = await snapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshotTable, OptionIs.Some);
            Assert.That(snapshotTable.UnwrapSome().Name.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(snapshotView, OptionIs.Some);
            Assert.That(snapshotView.UnwrapSome().Name.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(snapshotSequence, OptionIs.Some);
            Assert.That(snapshotSequence.UnwrapSome().Name.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(snapshotSynonym, OptionIs.Some);
            Assert.That(snapshotSynonym.UnwrapSome().Name.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(snapshotRoutine, OptionIs.Some);
            Assert.That(snapshotRoutine.UnwrapSome().Name.LocalName, Is.EqualTo("test_routine_name"));
        });
    }
}