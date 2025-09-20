using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class RelationalDatabaseCommentProviderTests
{
    private static IRelationalDatabaseCommentProvider EmptyCommentProvider => new RelationalDatabaseCommentProvider(
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
        var tables = Array.Empty<IRelationalDatabaseTableComments>();
        var views = Array.Empty<IDatabaseViewComments>();
        var sequences = Array.Empty<IDatabaseSequenceComments>();
        var synonyms = Array.Empty<IDatabaseSynonymComments>();
        var routines = Array.Empty<IDatabaseRoutineComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
        var tables = Array.Empty<IRelationalDatabaseTableComments>();
        var views = Array.Empty<IDatabaseViewComments>();
        var sequences = Array.Empty<IDatabaseSequenceComments>();
        var synonyms = Array.Empty<IDatabaseSynonymComments>();
        var routines = Array.Empty<IDatabaseRoutineComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
    public static void Ctor_GivenNullTableComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var views = Array.Empty<IDatabaseViewComments>();
        var sequences = Array.Empty<IDatabaseSequenceComments>();
        var synonyms = Array.Empty<IDatabaseSynonymComments>();
        var routines = Array.Empty<IDatabaseRoutineComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
    public static void Ctor_GivenNullViewComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTableComments>();
        var sequences = Array.Empty<IDatabaseSequenceComments>();
        var synonyms = Array.Empty<IDatabaseSynonymComments>();
        var routines = Array.Empty<IDatabaseRoutineComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
    public static void Ctor_GivenNullSequenceComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTableComments>();
        var views = Array.Empty<IDatabaseViewComments>();
        var synonyms = Array.Empty<IDatabaseSynonymComments>();
        var routines = Array.Empty<IDatabaseRoutineComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
    public static void Ctor_GivenNullSynonymComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTableComments>();
        var views = Array.Empty<IDatabaseViewComments>();
        var sequences = Array.Empty<IDatabaseSequenceComments>();
        var routines = Array.Empty<IDatabaseRoutineComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
    public static void Ctor_GivenNullRoutineComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tables = Array.Empty<IRelationalDatabaseTableComments>();
        var views = Array.Empty<IDatabaseViewComments>();
        var sequences = Array.Empty<IDatabaseSequenceComments>();
        var synonyms = Array.Empty<IDatabaseSynonymComments>();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
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
    public static void GetTableComments_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.GetTableComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetViewComments_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.GetViewComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.GetSequenceComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetSynonymComments_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.GetSynonymComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetRoutineComments_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.GetRoutineComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task EnumerateAllTableComments_WhenInvoked_ReturnsTableCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbTableComments = await commentProvider.EnumerateAllTableComments().ToListAsync().ConfigureAwait(false);
        var tableName = dbTableComments.Select(t => t.TableName).Single();

        Assert.That(tableName, Is.EqualTo(testTableName));
    }

    [Test]
    public static async Task GetAllTableComments2_WhenInvoked_ReturnsTableCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbTableComments = await commentProvider.GetAllTableComments2().ConfigureAwait(false);
        var tableName = dbTableComments.Select(t => t.TableName).Single();

        Assert.That(tableName, Is.EqualTo(testTableName));
    }

    [Test]
    public static async Task GetTableComments_WhenGivenMatchingTableName_ReturnsTableCommentFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbTableComments = await commentProvider.GetTableComments(testTableName).ToOption().ConfigureAwait(false);
        var tableName = dbTableComments.Match(t => t.TableName.LocalName, string.Empty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbTableComments, OptionIs.Some);
            Assert.That(tableName, Is.EqualTo(testTableName.LocalName));
        }
    }

    [Test]
    public static async Task GetTableComments_WhenGivenNonMatchingTableName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbTable = await commentProvider.GetTableComments("missing_table_name").ToOption().ConfigureAwait(false);

        Assert.That(dbTable, OptionIs.None);
    }

    [Test]
    public static async Task EnumerateAllViewComments_WhenInvoked_ReturnsViewCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbViewComments = await commentProvider.EnumerateAllViewComments().ToListAsync().ConfigureAwait(false);
        var viewName = dbViewComments.Select(v => v.ViewName).Single();

        Assert.That(viewName, Is.EqualTo(testViewName));
    }

    [Test]
    public static async Task GetAllViewComments2_WhenInvoked_ReturnsViewCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbViewComments = await commentProvider.GetAllViewComments2().ConfigureAwait(false);
        var viewName = dbViewComments.Select(v => v.ViewName).Single();

        Assert.That(viewName, Is.EqualTo(testViewName));
    }

    [Test]
    public static async Task GetViewComments_WhenGivenMatchingViewName_ReturnsViewCommentFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbViewComments = await commentProvider.GetViewComments(testViewName).ToOption().ConfigureAwait(false);
        var viewName = dbViewComments.Match(v => v.ViewName.LocalName, string.Empty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbViewComments, OptionIs.Some);
            Assert.That(viewName, Is.EqualTo(testViewName.LocalName));
        }
    }

    [Test]
    public static async Task GetViewComments_WhenGivenNonMatchingViewName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tables = Array.Empty<IRelationalDatabaseTableComments>();

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tables,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbView = await commentProvider.GetViewComments("missing_view_name").ToOption().ConfigureAwait(false);

        Assert.That(dbView, OptionIs.None);
    }

    [Test]
    public static async Task EnumerateAllSequenceComments_WhenInvoked_ReturnsSequenceCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSequences = await commentProvider.EnumerateAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var sequenceName = dbSequences.Select(s => s.SequenceName).Single();

        Assert.That(sequenceName, Is.EqualTo(testSequenceName));
    }

    [Test]
    public static async Task GetAllSequenceComments_WhenInvoked_ReturnsSequenceCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSequences = await commentProvider.GetAllSequenceComments().ConfigureAwait(false);
        var sequenceName = dbSequences.Select(s => s.SequenceName).Single();

        Assert.That(sequenceName, Is.EqualTo(testSequenceName));
    }

    [Test]
    public static async Task GetSequenceComments_WhenGivenMatchingSequenceName_ReturnsSequenceCommentFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSequence = await commentProvider.GetSequenceComments(testSequenceName).ToOption().ConfigureAwait(false);
        var sequenceName = dbSequence.Match(s => s.SequenceName.LocalName, string.Empty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbSequence, OptionIs.Some);
            Assert.That(sequenceName, Is.EqualTo(testSequenceName.LocalName));
        }
    }

    [Test]
    public static async Task GetSequenceComments_WhenGivenNonMatchingSequenceName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var synonymComments = Array.Empty<IDatabaseSynonymComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSequence = await commentProvider.GetSequenceComments("missing_sequence_name").ToOption().ConfigureAwait(false);

        Assert.That(dbSequence, OptionIs.None);
    }

    [Test]
    public static async Task EnumerateAllSynonymComments_WhenInvoked_ReturnsSynonymCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSynonyms = await commentProvider.EnumerateAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var synonymName = dbSynonyms.Select(s => s.SynonymName).Single();

        Assert.That(synonymName, Is.EqualTo(testSynonymName));
    }

    [Test]
    public static async Task GetAllSynonymComments_WhenInvoked_ReturnsSynonymCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSynonyms = await commentProvider.GetAllSynonymComments().ConfigureAwait(false);
        var synonymName = dbSynonyms.Select(s => s.SynonymName).Single();

        Assert.That(synonymName, Is.EqualTo(testSynonymName));
    }

    [Test]
    public static async Task GetSynonymComments_WhenGivenMatchingSynonymName_ReturnsSynonymCommentFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSynonym = await commentProvider.GetSynonymComments(testSynonymName).ToOption().ConfigureAwait(false);
        var synonymName = dbSynonym.Match(s => s.SynonymName.LocalName, string.Empty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbSynonym, OptionIs.Some);
            Assert.That(synonymName, Is.EqualTo(testSynonymName.LocalName));
        }
    }

    [Test]
    public static async Task GetSynonymComments_WhenGivenNonMatchingSynonymName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

        var routineComments = Array.Empty<IDatabaseRoutineComments>();

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbSynonym = await commentProvider.GetSynonymComments("missing_synonym_name").ToOption().ConfigureAwait(false);

        Assert.That(dbSynonym, OptionIs.None);
    }

    [Test]
    public static async Task EnumerateAllRoutineComments_WhenInvoked_ReturnsRoutineCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routineComment = new Mock<IDatabaseRoutineComments>(MockBehavior.Strict);
        routineComment.Setup(r => r.RoutineName).Returns(testRoutineName);
        var routineComments = new[] { routineComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbRoutines = await commentProvider.EnumerateAllRoutineComments().ToListAsync().ConfigureAwait(false);
        var routineName = dbRoutines.Select(r => r.RoutineName).Single();

        Assert.That(routineName, Is.EqualTo(testRoutineName));
    }

    [Test]
    public static async Task GetAllRoutineComments_WhenInvoked_ReturnsRoutineCommentsFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();
        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routineComment = new Mock<IDatabaseRoutineComments>(MockBehavior.Strict);
        routineComment.Setup(r => r.RoutineName).Returns(testRoutineName);
        var routineComments = new[] { routineComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbRoutines = await commentProvider.GetAllRoutineComments().ConfigureAwait(false);
        var routineName = dbRoutines.Select(r => r.RoutineName).Single();

        Assert.That(routineName, Is.EqualTo(testRoutineName));
    }

    [Test]
    public static async Task GetRoutineComments_WhenGivenMatchingRoutineName_ReturnsRoutineCommentFromCtor()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routineComment = new Mock<IDatabaseRoutineComments>(MockBehavior.Strict);
        routineComment.Setup(r => r.RoutineName).Returns(testRoutineName);
        var routineComments = new[] { routineComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbRoutine = await commentProvider.GetRoutineComments(testRoutineName).ToOption().ConfigureAwait(false);
        var routineName = dbRoutine.Match(r => r.RoutineName.LocalName, string.Empty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbRoutine, OptionIs.Some);
            Assert.That(routineName, Is.EqualTo(testRoutineName.LocalName));
        }
    }

    [Test]
    public static async Task GetRoutineComments_WhenGivenNonMatchingRoutineName_ReturnsNone()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var tableComments = Array.Empty<IRelationalDatabaseTableComments>();
        var viewComments = Array.Empty<IDatabaseViewComments>();
        var sequenceComments = Array.Empty<IDatabaseSequenceComments>();
        var synonymComments = Array.Empty<IDatabaseSynonymComments>();

        var testRoutineName = Identifier.CreateQualifiedIdentifier("test_routine_name");
        var routineComment = new Mock<IDatabaseRoutineComments>(MockBehavior.Strict);
        routineComment.Setup(r => r.RoutineName).Returns(testRoutineName);
        var routineComments = new[] { routineComment.Object };

        var commentProvider = new RelationalDatabaseCommentProvider(
            identifierDefaults,
            identifierResolver,
            tableComments,
            viewComments,
            sequenceComments,
            synonymComments,
            routineComments
        );

        var dbRoutine = await commentProvider.GetRoutineComments("missing_routine_name").ToOption().ConfigureAwait(false);

        Assert.That(dbRoutine, OptionIs.None);
    }
}