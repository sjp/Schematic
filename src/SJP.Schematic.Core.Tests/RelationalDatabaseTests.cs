using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests;

[TestFixture]
internal static class RelationalDatabaseTests
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
    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        Assert.That(
            () => new RelationalDatabase(
                null,
                identifierResolver,
                tables,
                views,
                sequences,
                synonyms,
                routines
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        Assert.That(
            () => new RelationalDatabase(
                identifierDefaults,
                null,
                tables,
                views,
                sequences,
                synonyms,
                routines
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenNullTables_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        Assert.That(
            () => new RelationalDatabase(
                identifierDefaults,
                identifierResolver,
                null,
                views,
                sequences,
                synonyms,
                routines
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenNullViews_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        Assert.That(
            () => new RelationalDatabase(
                identifierDefaults,
                identifierResolver,
                tables,
                null,
                sequences,
                synonyms,
                routines
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenNullSequences_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        Assert.That(
            () => new RelationalDatabase(
                identifierDefaults,
                identifierResolver,
                tables,
                views,
                null,
                synonyms,
                routines
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenNullSynonyms_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var routines = Array.Empty<IDatabaseRoutine>();

        Assert.That(
            () => new RelationalDatabase(
                identifierDefaults,
                identifierResolver,
                tables,
                views,
                sequences,
                null,
                routines
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void Ctor_GivenNullRoutines_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();

        Assert.That(
            () => new RelationalDatabase(
                identifierDefaults,
                identifierResolver,
                tables,
                views,
                sequences,
                synonyms,
                null
            ),
            Throws.ArgumentNullException
        );
    }

    [Test]
    public static void GetTable_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.GetTable(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetView_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.GetView(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSequence_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.GetSequence(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSynonym_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.GetSynonym(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutine_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.GetRoutine(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabase.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabase.SnapshotAsync(EmptyDatabase, null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetAllTables_WhenInvoked_ReturnsTablesFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbTables = await database.GetAllTables().ToListAsync().ConfigureAwait(false);
        var tableName = dbTables.Select(t => t.Name).Single();

        Assert.That(tableName, Is.EqualTo(testTableName));
    }

    [Test]
    public static async Task GetTable_WhenGivenMatchingTableName_ReturnsTableFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbTable = await database.GetTable(testTableName).ToOption().ConfigureAwait(false);
        var tableName = dbTable.Match(t => t.Name.LocalName, string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(dbTable, OptionIs.Some);
            Assert.That(tableName, Is.EqualTo(testTableName.LocalName));
        });
    }

    [Test]
    public static async Task GetTable_WhenGivenNonMatchingTableName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var table = new Mock<IRelationalDatabaseTable>(MockBehavior.Strict);
        table.Setup(t => t.Name).Returns(testTableName);
        var tables = new[] { table.Object };

        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbTable = await database.GetTable("missing_table_name").ToOption().ConfigureAwait(false);

        Assert.That(dbTable, OptionIs.None);
    }

    [Test]
    public static async Task GetAllViews_WhenInvoked_ReturnsViewsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbViews = await database.GetAllViews().ToListAsync().ConfigureAwait(false);
        var viewName = dbViews.Select(v => v.Name).Single();

        Assert.That(viewName, Is.EqualTo(testViewName));
    }

    [Test]
    public static async Task GetView_WhenGivenMatchingViewName_ReturnsViewFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbView = await database.GetView(testViewName).ToOption().ConfigureAwait(false);
        var viewName = dbView.Match(v => v.Name.LocalName, string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(dbView, OptionIs.Some);
            Assert.That(viewName, Is.EqualTo(testViewName.LocalName));
        });
    }

    [Test]
    public static async Task GetView_WhenGivenNonMatchingViewName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var view = new Mock<IDatabaseView>(MockBehavior.Strict);
        view.Setup(t => t.Name).Returns(testViewName);
        var views = new[] { view.Object };

        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbView = await database.GetView("missing_view_name").ToOption().ConfigureAwait(false);

        Assert.That(dbView, OptionIs.None);
    }

    [Test]
    public static async Task GetAllSequences_WhenInvoked_ReturnsSequencesFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbSequences = await database.GetAllSequences().ToListAsync().ConfigureAwait(false);
        var sequenceName = dbSequences.Select(s => s.Name).Single();

        Assert.That(sequenceName, Is.EqualTo(testSequenceName));
    }

    [Test]
    public static async Task GetSeqeuence_WhenGivenMatchingSequenceName_ReturnsSequenceFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbSequence = await database.GetSequence(testSequenceName).ToOption().ConfigureAwait(false);
        var sequenceName = dbSequence.Match(s => s.Name.LocalName, string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(dbSequence, OptionIs.Some);
            Assert.That(sequenceName, Is.EqualTo(testSequenceName.LocalName));
        });
    }

    [Test]
    public static async Task GetSequence_WhenGivenNonMatchingSequenceName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequence = new Mock<IDatabaseSequence>(MockBehavior.Strict);
        sequence.Setup(t => t.Name).Returns(testSequenceName);
        var sequences = new[] { sequence.Object };

        var synonyms = Array.Empty<IDatabaseSynonym>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbSequence = await database.GetSequence("missing_sequence_name").ToOption().ConfigureAwait(false);

        Assert.That(dbSequence, OptionIs.None);
    }

    [Test]
    public static async Task GetAllSynonyms_WhenInvoked_ReturnsSynonymsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var routines = Array.Empty<IDatabaseRoutine>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbSynonyms = await database.GetAllSynonyms().ToListAsync().ConfigureAwait(false);
        var synonymName = dbSynonyms.Select(s => s.Name).Single();

        Assert.That(synonymName, Is.EqualTo(testSynonymName));
    }

    [Test]
    public static async Task GetSynonym_WhenGivenMatchingSynonymName_ReturnsSynonymFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbSynonym = await database.GetSynonym(testSynonymName).ToOption().ConfigureAwait(false);
        var synonymName = dbSynonym.Match(s => s.Name.LocalName, string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(dbSynonym, OptionIs.Some);
            Assert.That(synonymName, Is.EqualTo(testSynonymName.LocalName));
        });
    }

    [Test]
    public static async Task GetSynonym_WhenGivenNonMatchingSynonymName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonym = new Mock<IDatabaseSynonym>(MockBehavior.Strict);
        synonym.Setup(t => t.Name).Returns(testSynonymName);
        var synonyms = new[] { synonym.Object };

        var routines = Array.Empty<IDatabaseRoutine>();

        var database = new RelationalDatabase(
            identifierDefaults,
            identifierResolver,
            tables,
            views,
            sequences,
            synonyms,
            routines
        );

        var dbSynonym = await database.GetSynonym("missing_synonym_name").ToOption().ConfigureAwait(false);

        Assert.That(dbSynonym, OptionIs.None);
    }

    [Test]
    public static async Task GetAllRoutines_WhenInvoked_ReturnsRoutinesFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();

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

        var dbRoutines = await database.GetAllRoutines().ToListAsync().ConfigureAwait(false);
        var routineName = dbRoutines.Select(r => r.Name).Single();

        Assert.That(routineName, Is.EqualTo(testRoutineName));
    }

    [Test]
    public static async Task GetRoutine_WhenGivenMatchingRoutineName_ReturnsRoutineFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();

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

        var dbRoutine = await database.GetRoutine(testRoutineName).ToOption().ConfigureAwait(false);
        var routineName = dbRoutine.Match(r => r.Name.LocalName, string.Empty);

        Assert.Multiple(() =>
        {
            Assert.That(dbRoutine, OptionIs.Some);
            Assert.That(routineName, Is.EqualTo(testRoutineName.LocalName));
        });
    }

    [Test]
    public static async Task GetRoutine_WhenGivenNonMatchingRoutineName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTable>();
        var views = Array.Empty<IDatabaseView>();
        var sequences = Array.Empty<IDatabaseSequence>();
        var synonyms = Array.Empty<IDatabaseSynonym>();

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

        var dbRoutine = await database.GetRoutine("missing_routine_name").ToOption().ConfigureAwait(false);

        Assert.That(dbRoutine, OptionIs.None);
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

        var snapshot = await RelationalDatabase.SnapshotAsync(database, identifierResolver).ConfigureAwait(false);

        Assert.Multiple(() =>
        {
            Assert.That(snapshot.IdentifierDefaults.Server, Is.EqualTo(identifierDefaults.Server));
            Assert.That(snapshot.IdentifierDefaults.Database, Is.EqualTo(identifierDefaults.Database));
            Assert.That(snapshot.IdentifierDefaults.Schema, Is.EqualTo(identifierDefaults.Schema));
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

        var snapshot = await RelationalDatabase.SnapshotAsync(database, identifierResolver).ConfigureAwait(false);
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

        var snapshot = await RelationalDatabase.SnapshotAsync(database, identifierResolver).ConfigureAwait(false);
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