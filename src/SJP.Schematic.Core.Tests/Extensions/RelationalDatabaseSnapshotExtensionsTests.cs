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

        var snapshot = await database.SnapshotAsync();

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

        var snapshot = await database.SnapshotAsync(identifierResolver);

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

        var snapshot = await database.SnapshotAsync(identifierResolver);
        var snapshotTables = await snapshot.GetAllTables();
        var snapshotViews = await snapshot.GetAllViews();
        var snapshotSequences = await snapshot.GetAllSequences();
        var snapshotSynonyms = await snapshot.GetAllSynonyms();
        var snapshotRoutines = await snapshot.GetAllRoutines();

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

        var snapshot = await database.SnapshotAsync();
        var snapshotTable = await snapshot.GetTable("test_table_name").ToOption();
        var snapshotView = await snapshot.GetView("test_view_name").ToOption();
        var snapshotSequence = await snapshot.GetSequence("test_sequence_name").ToOption();
        var snapshotSynonym = await snapshot.GetSynonym("test_synonym_name").ToOption();
        var snapshotRoutine = await snapshot.GetRoutine("test_routine_name").ToOption();

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

        var snapshot = await database.SnapshotAsync(identifierResolver);
        var snapshotTables = await snapshot.GetAllTables();
        var snapshotViews = await snapshot.GetAllViews();
        var snapshotSequences = await snapshot.GetAllSequences();
        var snapshotSynonyms = await snapshot.GetAllSynonyms();
        var snapshotRoutines = await snapshot.GetAllRoutines();

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

        var snapshot = await database.SnapshotAsync(identifierResolver);
        var snapshotTable = await snapshot.GetTable("test_table_name").ToOption();
        var snapshotView = await snapshot.GetView("test_view_name").ToOption();
        var snapshotSequence = await snapshot.GetSequence("test_sequence_name").ToOption();
        var snapshotSynonym = await snapshot.GetSynonym("test_synonym_name").ToOption();
        var snapshotRoutine = await snapshot.GetRoutine("test_routine_name").ToOption();

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

        var snapshot = await database.SnapshotAsync(new RelationalDatabaseSnapshotOptions());
        var snapshotTable = await snapshot.GetTable("test_table_name").ToOption();
        var snapshotView = await snapshot.GetView("test_view_name").ToOption();
        var snapshotSequence = await snapshot.GetSequence("test_sequence_name").ToOption();
        var snapshotSynonym = await snapshot.GetSynonym("test_synonym_name").ToOption();
        var snapshotRoutine = await snapshot.GetRoutine("test_routine_name").ToOption();

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
        var tableSnapshot = await database.SnapshotAsync(tableOpts);
        var tableSnapshotTable = await tableSnapshot.GetTable("test_table_name").ToOption();
        var tableSnapshotView = await tableSnapshot.GetView("test_view_name").ToOption();
        var tableSnapshotSequence = await tableSnapshot.GetSequence("test_sequence_name").ToOption();
        var tableSnapshotSynonym = await tableSnapshot.GetSynonym("test_synonym_name").ToOption();
        var tableSnapshotRoutine = await tableSnapshot.GetRoutine("test_routine_name").ToOption();

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
        var viewSnapshot = await database.SnapshotAsync(viewOpts);
        var viewSnapshotTable = await viewSnapshot.GetTable("test_table_name").ToOption();
        var viewSnapshotView = await viewSnapshot.GetView("test_view_name").ToOption();
        var viewSnapshotSequence = await viewSnapshot.GetSequence("test_sequence_name").ToOption();
        var viewSnapshotSynonym = await viewSnapshot.GetSynonym("test_synonym_name").ToOption();
        var viewSnapshotRoutine = await viewSnapshot.GetRoutine("test_routine_name").ToOption();

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
        var sequenceSnapshot = await database.SnapshotAsync(sequenceOpts);
        var sequenceSnapshotTable = await sequenceSnapshot.GetTable("test_table_name").ToOption();
        var sequenceSnapshotView = await sequenceSnapshot.GetView("test_view_name").ToOption();
        var sequenceSnapshotSequence = await sequenceSnapshot.GetSequence("test_sequence_name").ToOption();
        var sequenceSnapshotSynonym = await sequenceSnapshot.GetSynonym("test_synonym_name").ToOption();
        var sequenceSnapshotRoutine = await sequenceSnapshot.GetRoutine("test_routine_name").ToOption();

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
        var synonymSnapshot = await database.SnapshotAsync(synonymOpts);
        var synonymSnapshotTable = await synonymSnapshot.GetTable("test_table_name").ToOption();
        var synonymSnapshotView = await synonymSnapshot.GetView("test_view_name").ToOption();
        var synonymSnapshotSequence = await synonymSnapshot.GetSequence("test_sequence_name").ToOption();
        var synonymSnapshotSynonym = await synonymSnapshot.GetSynonym("test_synonym_name").ToOption();
        var synonymSnapshotRoutine = await synonymSnapshot.GetRoutine("test_routine_name").ToOption();

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
        var routineSnapshot = await database.SnapshotAsync(routineOpts);
        var routineSnapshotTable = await routineSnapshot.GetTable("test_table_name").ToOption();
        var routineSnapshotView = await routineSnapshot.GetView("test_view_name").ToOption();
        var routineSnapshotSequence = await routineSnapshot.GetSequence("test_sequence_name").ToOption();
        var routineSnapshotSynonym = await routineSnapshot.GetSynonym("test_synonym_name").ToOption();
        var routineSnapshotRoutine = await routineSnapshot.GetRoutine("test_routine_name").ToOption();

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

        var snapshot = await database.SnapshotAsync(new RelationalDatabaseSnapshotOptions());
        var snapshotTables = await snapshot.GetAllTables();
        var snapshotViews = await snapshot.GetAllViews();
        var snapshotSequences = await snapshot.GetAllSequences();
        var snapshotSynonyms = await snapshot.GetAllSynonyms();
        var snapshotRoutines = await snapshot.GetAllRoutines();

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
        var tableSnapshot = await database.SnapshotAsync(tableOpts);
        var tableSnapshotTables = await tableSnapshot.GetAllTables();
        var tableSnapshotViews = await tableSnapshot.GetAllViews();
        var tableSnapshotSequences = await tableSnapshot.GetAllSequences();
        var tableSnapshotSynonyms = await tableSnapshot.GetAllSynonyms();
        var tableSnapshotRoutines = await tableSnapshot.GetAllRoutines();

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
        var viewSnapshot = await database.SnapshotAsync(viewOpts);
        var viewSnapshotTables = await viewSnapshot.GetAllTables();
        var viewSnapshotViews = await viewSnapshot.GetAllViews();
        var viewSnapshotSequences = await viewSnapshot.GetAllSequences();
        var viewSnapshotSynonyms = await viewSnapshot.GetAllSynonyms();
        var viewSnapshotRoutines = await viewSnapshot.GetAllRoutines();

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
        var sequenceSnapshot = await database.SnapshotAsync(sequenceOpts);
        var sequenceSnapshotTables = await sequenceSnapshot.GetAllTables();
        var sequenceSnapshotViews = await sequenceSnapshot.GetAllViews();
        var sequenceSnapshotSequences = await sequenceSnapshot.GetAllSequences();
        var sequenceSnapshotSynonyms = await sequenceSnapshot.GetAllSynonyms();
        var sequenceSnapshotRoutines = await sequenceSnapshot.GetAllRoutines();

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
        var synonymSnapshot = await database.SnapshotAsync(synonymOpts);
        var synonymSnapshotTables = await synonymSnapshot.GetAllTables();
        var synonymSnapshotViews = await synonymSnapshot.GetAllViews();
        var synonymSnapshotSequences = await synonymSnapshot.GetAllSequences();
        var synonymSnapshotSynonyms = await synonymSnapshot.GetAllSynonyms();
        var synonymSnapshotRoutines = await synonymSnapshot.GetAllRoutines();

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
        var routineSnapshot = await database.SnapshotAsync(routineOpts);
        var routineSnapshotTables = await routineSnapshot.GetAllTables();
        var routineSnapshotViews = await routineSnapshot.GetAllViews();
        var routineSnapshotSequences = await routineSnapshot.GetAllSequences();
        var routineSnapshotSynonyms = await routineSnapshot.GetAllSynonyms();
        var routineSnapshotRoutines = await routineSnapshot.GetAllRoutines();

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