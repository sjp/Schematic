using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class RelationalDatabaseSnapshotExtensionsTests
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
        Assert.That(() => RelationalDatabaseSnapshotExtensions.SnapshotAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithSnapshotOptionsGivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseSnapshotExtensions.SnapshotAsync(null, new RelationalDatabaseSnapshotOptions()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithSnapshotOptionsGivenNullOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.SnapshotAsync((RelationalDatabaseSnapshotOptions)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithIdentifierResolverGivenNullDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseSnapshotExtensions.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithNullIdentifierResolverGivenDatabase_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.SnapshotAsync((IIdentifierResolutionStrategy)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithNullDatabaseForOptionsAndResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseSnapshotExtensions.SnapshotAsync(null, new RelationalDatabaseSnapshotOptions(), new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithNullOptionsForOptionsAndResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_WithNullResolversForOptionsAndResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyDatabase.SnapshotAsync(new RelationalDatabaseSnapshotOptions(), null), Throws.ArgumentNullException);
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshot.IdentifierDefaults.Server, Is.EqualTo(identifierDefaults.Server));
            Assert.That(snapshot.IdentifierDefaults.Database, Is.EqualTo(identifierDefaults.Database));
            Assert.That(snapshot.IdentifierDefaults.Schema, Is.EqualTo(identifierDefaults.Schema));
        }
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshot.IdentifierDefaults.Server, Is.EqualTo(identifierDefaults.Server));
            Assert.That(snapshot.IdentifierDefaults.Database, Is.EqualTo(identifierDefaults.Database));
            Assert.That(snapshot.IdentifierDefaults.Schema, Is.EqualTo(identifierDefaults.Schema));
        }
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
        var snapshotTables = await snapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var snapshotViews = await snapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var snapshotSequences = await snapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var snapshotSynonyms = await snapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var snapshotRoutines = await snapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTables, Is.EqualTo(tables));
            Assert.That(snapshotViews, Is.EqualTo(views));
            Assert.That(snapshotSequences, Is.EqualTo(sequences));
            Assert.That(snapshotSynonyms, Is.EqualTo(synonyms));
            Assert.That(snapshotRoutines, Is.EqualTo(routines));
        }
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

        using (Assert.EnterMultipleScope())
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
        }
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
        var snapshotTables = await snapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var snapshotViews = await snapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var snapshotSequences = await snapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var snapshotSynonyms = await snapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var snapshotRoutines = await snapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTables, Is.EqualTo(tables));
            Assert.That(snapshotViews, Is.EqualTo(views));
            Assert.That(snapshotSequences, Is.EqualTo(sequences));
            Assert.That(snapshotSynonyms, Is.EqualTo(synonyms));
            Assert.That(snapshotRoutines, Is.EqualTo(routines));
        }
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

        using (Assert.EnterMultipleScope())
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
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseWithObjectsWithOptions_ReturnsDatabaseWithMatchingObjectsInGetObjectMethodsWhenConfigured()
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

        var snapshot = await database.SnapshotAsync(new RelationalDatabaseSnapshotOptions()).ConfigureAwait(false);
        var snapshotTable = await snapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var snapshotView = await snapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var snapshotSequence = await snapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var snapshotSynonym = await snapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var snapshotRoutine = await snapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
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
        }

        // toggle tables
        var tableOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeTables = true };
        var tableSnapshot = await database.SnapshotAsync(tableOpts).ConfigureAwait(false);
        var tableSnapshotTable = await tableSnapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var tableSnapshotView = await tableSnapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var tableSnapshotSequence = await tableSnapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var tableSnapshotSynonym = await tableSnapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var tableSnapshotRoutine = await tableSnapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableSnapshotTable, OptionIs.Some);
            Assert.That(tableSnapshotTable.UnwrapSome().Name.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(tableSnapshotView, OptionIs.None);
            Assert.That(tableSnapshotSequence, OptionIs.None);
            Assert.That(tableSnapshotSynonym, OptionIs.None);
            Assert.That(tableSnapshotRoutine, OptionIs.None);
        }

        // toggle views
        var viewOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeViews = true };
        var viewSnapshot = await database.SnapshotAsync(viewOpts).ConfigureAwait(false);
        var viewSnapshotTable = await viewSnapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var viewSnapshotView = await viewSnapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var viewSnapshotSequence = await viewSnapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var viewSnapshotSynonym = await viewSnapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var viewSnapshotRoutine = await viewSnapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewSnapshotTable, OptionIs.None);

            Assert.That(viewSnapshotView, OptionIs.Some);
            Assert.That(viewSnapshotView.UnwrapSome().Name.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(viewSnapshotSequence, OptionIs.None);
            Assert.That(viewSnapshotSynonym, OptionIs.None);
            Assert.That(viewSnapshotRoutine, OptionIs.None);
        }

        // toggle sequences
        var sequenceOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeSequences = true };
        var sequenceSnapshot = await database.SnapshotAsync(sequenceOpts).ConfigureAwait(false);
        var sequenceSnapshotTable = await sequenceSnapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotView = await sequenceSnapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotSequence = await sequenceSnapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotSynonym = await sequenceSnapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotRoutine = await sequenceSnapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequenceSnapshotTable, OptionIs.None);
            Assert.That(sequenceSnapshotView, OptionIs.None);

            Assert.That(sequenceSnapshotSequence, OptionIs.Some);
            Assert.That(sequenceSnapshotSequence.UnwrapSome().Name.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(sequenceSnapshotSynonym, OptionIs.None);
            Assert.That(sequenceSnapshotRoutine, OptionIs.None);
        }

        // toggle synonyms
        var synonymOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeSynonyms = true };
        var synonymSnapshot = await database.SnapshotAsync(synonymOpts).ConfigureAwait(false);
        var synonymSnapshotTable = await synonymSnapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotView = await synonymSnapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotSequence = await synonymSnapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotSynonym = await synonymSnapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotRoutine = await synonymSnapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(synonymSnapshotTable, OptionIs.None);
            Assert.That(synonymSnapshotView, OptionIs.None);
            Assert.That(synonymSnapshotSequence, OptionIs.None);

            Assert.That(synonymSnapshotSynonym, OptionIs.Some);
            Assert.That(synonymSnapshotSynonym.UnwrapSome().Name.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(synonymSnapshotRoutine, OptionIs.None);
        }

        // toggle routines
        var routineOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeRoutines = true };
        var routineSnapshot = await database.SnapshotAsync(routineOpts).ConfigureAwait(false);
        var routineSnapshotTable = await routineSnapshot.GetTable("test_table_name").ToOption().ConfigureAwait(false);
        var routineSnapshotView = await routineSnapshot.GetView("test_view_name").ToOption().ConfigureAwait(false);
        var routineSnapshotSequence = await routineSnapshot.GetSequence("test_sequence_name").ToOption().ConfigureAwait(false);
        var routineSnapshotSynonym = await routineSnapshot.GetSynonym("test_synonym_name").ToOption().ConfigureAwait(false);
        var routineSnapshotRoutine = await routineSnapshot.GetRoutine("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routineSnapshotTable, OptionIs.None);
            Assert.That(routineSnapshotView, OptionIs.None);
            Assert.That(routineSnapshotSequence, OptionIs.None);
            Assert.That(routineSnapshotSynonym, OptionIs.None);

            Assert.That(routineSnapshotRoutine, OptionIs.Some);
            Assert.That(routineSnapshotRoutine.UnwrapSome().Name.LocalName, Is.EqualTo("test_routine_name"));
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseWithObjectsWithOptions_ReturnsDatabaseWithMatchingObjectsInGetAllObjectsWithConfigured()
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

        var snapshot = await database.SnapshotAsync(new RelationalDatabaseSnapshotOptions()).ConfigureAwait(false);
        var snapshotTables = await snapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var snapshotViews = await snapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var snapshotSequences = await snapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var snapshotSynonyms = await snapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var snapshotRoutines = await snapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTables, Is.EqualTo(tables));
            Assert.That(snapshotViews, Is.EqualTo(views));
            Assert.That(snapshotSequences, Is.EqualTo(sequences));
            Assert.That(snapshotSynonyms, Is.EqualTo(synonyms));
            Assert.That(snapshotRoutines, Is.EqualTo(routines));
        }

        // toggle tables
        var tableOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeTables = true };
        var tableSnapshot = await database.SnapshotAsync(tableOpts).ConfigureAwait(false);
        var tableSnapshotTables = await tableSnapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var tableSnapshotViews = await tableSnapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var tableSnapshotSequences = await tableSnapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var tableSnapshotSynonyms = await tableSnapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var tableSnapshotRoutines = await tableSnapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableSnapshotTables, Is.EqualTo(tables));
            Assert.That(tableSnapshotViews, Is.Empty);
            Assert.That(tableSnapshotSequences, Is.Empty);
            Assert.That(tableSnapshotSynonyms, Is.Empty);
            Assert.That(tableSnapshotRoutines, Is.Empty);
        }

        // toggle views
        var viewOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeViews = true };
        var viewSnapshot = await database.SnapshotAsync(viewOpts).ConfigureAwait(false);
        var viewSnapshotTables = await viewSnapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var viewSnapshotViews = await viewSnapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var viewSnapshotSequences = await viewSnapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var viewSnapshotSynonyms = await viewSnapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var viewSnapshotRoutines = await viewSnapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewSnapshotTables, Is.Empty);
            Assert.That(viewSnapshotViews, Is.EqualTo(views));
            Assert.That(viewSnapshotSequences, Is.Empty);
            Assert.That(viewSnapshotSynonyms, Is.Empty);
            Assert.That(viewSnapshotRoutines, Is.Empty);
        }

        // toggle sequences
        var sequenceOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeSequences = true };
        var sequenceSnapshot = await database.SnapshotAsync(sequenceOpts).ConfigureAwait(false);
        var sequenceSnapshotTables = await sequenceSnapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotViews = await sequenceSnapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotSequences = await sequenceSnapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotSynonyms = await sequenceSnapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotRoutines = await sequenceSnapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequenceSnapshotTables, Is.Empty);
            Assert.That(sequenceSnapshotViews, Is.Empty);
            Assert.That(sequenceSnapshotSequences, Is.EqualTo(sequences));
            Assert.That(sequenceSnapshotSynonyms, Is.Empty);
            Assert.That(sequenceSnapshotRoutines, Is.Empty);
        }

        // toggle synonyms
        var synonymOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeSynonyms = true };
        var synonymSnapshot = await database.SnapshotAsync(synonymOpts).ConfigureAwait(false);
        var synonymSnapshotTables = await synonymSnapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotViews = await synonymSnapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotSequences = await synonymSnapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotSynonyms = await synonymSnapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotRoutines = await synonymSnapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(synonymSnapshotTables, Is.Empty);
            Assert.That(synonymSnapshotViews, Is.Empty);
            Assert.That(synonymSnapshotSequences, Is.Empty);
            Assert.That(synonymSnapshotSynonyms, Is.EqualTo(synonyms));
            Assert.That(synonymSnapshotRoutines, Is.Empty);
        }

        // toggle routines
        var routineOpts = RelationalDatabaseSnapshotOptions.Empty with { IncludeRoutines = true };
        var routineSnapshot = await database.SnapshotAsync(routineOpts).ConfigureAwait(false);
        var routineSnapshotTables = await routineSnapshot.EnumerateAllTables().ToListAsync().ConfigureAwait(false);
        var routineSnapshotViews = await routineSnapshot.EnumerateAllViews().ToListAsync().ConfigureAwait(false);
        var routineSnapshotSequences = await routineSnapshot.EnumerateAllSequences().ToListAsync().ConfigureAwait(false);
        var routineSnapshotSynonyms = await routineSnapshot.EnumerateAllSynonyms().ToListAsync().ConfigureAwait(false);
        var routineSnapshotRoutines = await routineSnapshot.EnumerateAllRoutines().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routineSnapshotTables, Is.Empty);
            Assert.That(routineSnapshotViews, Is.Empty);
            Assert.That(routineSnapshotSequences, Is.Empty);
            Assert.That(routineSnapshotSynonyms, Is.Empty);
            Assert.That(routineSnapshotRoutines, Is.EqualTo(routines));
        }
    }
}