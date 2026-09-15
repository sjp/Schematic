using System;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierDefaults")
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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("identifierResolver")
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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableComments")
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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewComments")
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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceComments")
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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymComments")
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
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineComments")
        );
    }

    [Test]
    public static void GetTableComments_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetTableComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("tableName")
        );
    }

    [Test]
    public static void GetViewComments_GivenNullIdentifier_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetViewComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("viewName")
        );
    }

    [Test]
    public static void GetSequenceComments_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetSequenceComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }

    [Test]
    public static void GetSynonymComments_GivenNullSynonymName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetSynonymComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("synonymName")
        );
    }

    [Test]
    public static void GetRoutineComments_GivenNullRoutineName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetRoutineComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("routineName")
        );
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

        var dbTableComments = await commentProvider.EnumerateAllTableComments().ToListAsync();
        var tableName = dbTableComments.Select(t => t.TableName).Single();

        Assert.That(tableName, Is.EqualTo(testTableName));
    }

    [Test]
    public static async Task GetAllTableComments_WhenInvoked_ReturnsTableCommentsFromCtor()
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

        var dbTableComments = await commentProvider.GetAllTableComments();
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

        var dbTableComments = await commentProvider.GetTableComments(testTableName).ToOption();
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

        var dbTable = await commentProvider.GetTableComments("missing_table_name").ToOption();

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

        var dbViewComments = await commentProvider.EnumerateAllViewComments().ToListAsync();
        var viewName = dbViewComments.Select(v => v.ViewName).Single();

        Assert.That(viewName, Is.EqualTo(testViewName));
    }

    [Test]
    public static async Task GetAllViewComments_WhenInvoked_ReturnsViewCommentsFromCtor()
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

        var dbViewComments = await commentProvider.GetAllViewComments();
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

        var dbViewComments = await commentProvider.GetViewComments(testViewName).ToOption();
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

        var dbView = await commentProvider.GetViewComments("missing_view_name").ToOption();

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

        var dbSequences = await commentProvider.EnumerateAllSequenceComments().ToListAsync();
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

        var dbSequences = await commentProvider.GetAllSequenceComments();
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

        var dbSequence = await commentProvider.GetSequenceComments(testSequenceName).ToOption();
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

        var dbSequence = await commentProvider.GetSequenceComments("missing_sequence_name").ToOption();

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

        var dbSynonyms = await commentProvider.EnumerateAllSynonymComments().ToListAsync();
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

        var dbSynonyms = await commentProvider.GetAllSynonymComments();
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

        var dbSynonym = await commentProvider.GetSynonymComments(testSynonymName).ToOption();
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

        var dbSynonym = await commentProvider.GetSynonymComments("missing_synonym_name").ToOption();

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

        var dbRoutines = await commentProvider.EnumerateAllRoutineComments().ToListAsync();
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

        var dbRoutines = await commentProvider.GetAllRoutineComments();
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

        var dbRoutine = await commentProvider.GetRoutineComments(testRoutineName).ToOption();
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

        var dbRoutine = await commentProvider.GetRoutineComments("missing_routine_name").ToOption();

        Assert.That(dbRoutine, OptionIs.None);
    }
    [Test]
    public static async Task GetTableComments_WhenGivenTableNameQualifiedByDefaultDatabase_ReturnsTableCommentFromCtor()
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

        var qualifiedTableName = Identifier.CreateQualifiedIdentifier("test_database", "test_schema", "test_table_name");
        var dbTableComments = await commentProvider.GetTableComments(qualifiedTableName).ToOption();
        var tableName = dbTableComments.Match(t => t.TableName.LocalName, string.Empty);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(dbTableComments, OptionIs.Some);
            Assert.That(tableName, Is.EqualTo(testTableName.LocalName));
        }
    }

    [Test]
    public static async Task GetTableComments_WhenGivenTableNameQualifiedByDifferentDatabase_ReturnsNone()
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

        var otherDatabaseTableName = Identifier.CreateQualifiedIdentifier("other_database", "test_schema", "test_table_name");
        var dbTableComments = await commentProvider.GetTableComments(otherDatabaseTableName).ToOption();

        Assert.That(dbTableComments, OptionIs.None);
    }

    [Test]
    public static void Ctor_GivenNullUserDefinedTypeComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
                identifierDefaults,
                identifierResolver,
                [],
                [],
                [],
                [],
                [],
                null
            ),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("userDefinedTypeComments")
        );
    }

    [Test]
    public static void GetUserDefinedTypeComments_GivenNullTypeName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetUserDefinedTypeComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("typeName")
        );
    }

    [Test]
    public static async Task GetUserDefinedTypeComments_WhenGivenMatchingTypeName_ReturnsCommentsFromCtor()
    {
        Identifier typeName = "test_type";
        var provider = CreateProviderWithUserDefinedTypeComments(new DatabaseUserDefinedTypeComments(typeName, Option<string>.None));

        var comments = await provider.GetUserDefinedTypeComments(typeName).UnwrapSomeAsync();

        Assert.That(comments.TypeName.LocalName, Is.EqualTo(typeName.LocalName));
    }

    [Test]
    public static async Task GetUserDefinedTypeComments_WhenGivenNonMatchingTypeName_ReturnsNone()
    {
        var provider = CreateProviderWithUserDefinedTypeComments(new DatabaseUserDefinedTypeComments("test_type", Option<string>.None));

        var comments = await provider.GetUserDefinedTypeComments("other_type").ToOption();

        Assert.That(comments, OptionIs.None);
    }

    [Test]
    public static async Task EnumerateAllUserDefinedTypeComments_WhenInvoked_ReturnsCommentsFromCtor()
    {
        Identifier typeName = "test_type";
        var provider = CreateProviderWithUserDefinedTypeComments(new DatabaseUserDefinedTypeComments(typeName, Option<string>.None));

        var comments = await provider.EnumerateAllUserDefinedTypeComments().ToListAsync();

        Assert.That(comments.Select(c => c.TypeName).Single(), Is.EqualTo(typeName));
    }

    [Test]
    public static async Task GetAllUserDefinedTypeComments_WhenInvoked_ReturnsCommentsFromCtor()
    {
        Identifier typeName = "test_type";
        var provider = CreateProviderWithUserDefinedTypeComments(new DatabaseUserDefinedTypeComments(typeName, Option<string>.None));

        var comments = await provider.GetAllUserDefinedTypeComments();

        Assert.That(comments.Select(c => c.TypeName).Single(), Is.EqualTo(typeName));
    }

    [Test]
    public static void Ctor_GivenNullSchemaComments_ThrowsArgumentNullException()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        Assert.That(
            () => new RelationalDatabaseCommentProvider(
                identifierDefaults,
                identifierResolver,
                [],
                [],
                [],
                [],
                [],
                [],
                null
            ),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("schemaComments")
        );
    }

    [Test]
    public static void GetSchemaComments_GivenNullSchemaName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => EmptyCommentProvider.GetSchemaComments(null),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("schemaName")
        );
    }

    [Test]
    public static async Task GetSchemaComments_WhenGivenMatchingSchemaName_ReturnsCommentsFromCtor()
    {
        Identifier schemaName = "test_schema";
        var provider = CreateProviderWithSchemaComments(new DatabaseSchemaComments(schemaName, Option<string>.None));

        var comments = await provider.GetSchemaComments(schemaName).UnwrapSomeAsync();

        Assert.That(comments.SchemaName.LocalName, Is.EqualTo(schemaName.LocalName));
    }

    [Test]
    public static async Task GetSchemaComments_WhenGivenNonMatchingSchemaName_ReturnsNone()
    {
        var provider = CreateProviderWithSchemaComments(new DatabaseSchemaComments("test_schema", Option<string>.None));

        var comments = await provider.GetSchemaComments("other_schema").ToOption();

        Assert.That(comments, OptionIs.None);
    }

    [Test]
    public static async Task GetSchemaComments_WhenGivenQualifiedSchemaName_ReturnsCommentsMatchingLocalName()
    {
        var provider = CreateProviderWithSchemaComments(new DatabaseSchemaComments(new Identifier("test_database", "test_schema"), Option<string>.None));

        var comments = await provider.GetSchemaComments(new Identifier("other_database", "test_schema")).UnwrapSomeAsync();

        Assert.That(comments.SchemaName.LocalName, Is.EqualTo("test_schema"));
    }

    [Test]
    public static async Task GetTableComments_WhenResolverYieldsSeveralCandidates_ReturnsFirstMatchingCandidate()
    {
        var identifierDefaults = new IdentifierDefaults(null, null, "test_schema");
        var identifierResolver = new Mock<IIdentifierResolutionStrategy>(MockBehavior.Strict);
        identifierResolver
            .Setup(r => r.GetResolutionOrder(It.IsAny<Identifier>()))
            .Returns(new[] { new Identifier("missing_table_name"), new Identifier("second_table_name"), new Identifier("first_table_name") });

        var tableComments = new[] { CreateTableComments("first_table_name"), CreateTableComments("second_table_name") };
        var commentProvider = new RelationalDatabaseCommentProvider(identifierDefaults, identifierResolver.Object, tableComments, [], [], [], []);

        var comments = await commentProvider.GetTableComments("requested_table_name").UnwrapSomeAsync();

        Assert.That(comments.TableName, Is.EqualTo(new Identifier("second_table_name")));
    }

    [Test]
    public static void QualifyObjectName_GivenNameWithEveryComponent_ReturnsSameInstance()
    {
        var commentProvider = new QualifyingCommentProvider(new IdentifierDefaults("test_server", "test_database", "test_schema"));
        var objectName = new Identifier("other_server", "other_database", "other_schema", "test_table_name");

        var result = commentProvider.Qualify(objectName);

        Assert.That(result, Is.SameAs(objectName));
    }

    [Test]
    public static void QualifyObjectName_WhenDefaultsFillMissingComponents_ReturnsQualifiedName()
    {
        var commentProvider = new QualifyingCommentProvider(new IdentifierDefaults("test_server", "test_database", "test_schema"));
        var objectName = new Identifier("other_schema", "test_table_name");

        var result = commentProvider.Qualify(objectName);

        Assert.That(result, Is.EqualTo(new Identifier("test_server", "test_database", "other_schema", "test_table_name")));
    }

    private static IRelationalDatabaseTableComments CreateTableComments(Identifier tableName)
    {
        var tableComments = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComments.Setup(t => t.TableName).Returns(tableName);
        return tableComments.Object;
    }

    private sealed class QualifyingCommentProvider : RelationalDatabaseCommentProvider
    {
        public QualifyingCommentProvider(IIdentifierDefaults identifierDefaults)
            : base(identifierDefaults, new VerbatimIdentifierResolutionStrategy(), [], [], [], [], [])
        {
        }

        public Identifier Qualify(Identifier objectName) => QualifyObjectName(objectName);
    }

    [Test]
    public static async Task EnumerateAllSchemaComments_WhenInvoked_ReturnsCommentsFromCtor()
    {
        Identifier schemaName = "test_schema";
        var provider = CreateProviderWithSchemaComments(new DatabaseSchemaComments(schemaName, Option<string>.None));

        var comments = await provider.EnumerateAllSchemaComments().ToListAsync();

        Assert.That(comments.Select(c => c.SchemaName).Single(), Is.EqualTo(schemaName));
    }

    [Test]
    public static async Task GetAllSchemaComments_WhenInvoked_ReturnsCommentsFromCtor()
    {
        Identifier schemaName = "test_schema";
        var provider = CreateProviderWithSchemaComments(new DatabaseSchemaComments(schemaName, Option<string>.None));

        var comments = await provider.GetAllSchemaComments();

        Assert.That(comments.Select(c => c.SchemaName).Single(), Is.EqualTo(schemaName));
    }

    private static IRelationalDatabaseCommentProvider CreateProviderWithSchemaComments(params IDatabaseSchemaComments[] schemaComments)
    {
        return new RelationalDatabaseCommentProvider(
            new IdentifierDefaults("test_server", "test_database", "test_schema"),
            new VerbatimIdentifierResolutionStrategy(),
            [],
            [],
            [],
            [],
            [],
            [],
            schemaComments
        );
    }

    private static IRelationalDatabaseCommentProvider CreateProviderWithUserDefinedTypeComments(params IDatabaseUserDefinedTypeComments[] typeComments)
    {
        return new RelationalDatabaseCommentProvider(
            new IdentifierDefaults("test_server", "test_database", "test_schema"),
            new VerbatimIdentifierResolutionStrategy(),
            [],
            [],
            [],
            [],
            [],
            typeComments
        );
    }
}
