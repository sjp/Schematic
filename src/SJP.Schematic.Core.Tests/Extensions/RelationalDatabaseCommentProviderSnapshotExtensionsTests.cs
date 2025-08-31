using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class RelationalDatabaseCommentProviderSnapshotExtensionsTests
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
    public static void SnapshotAsync_GivenNullDatabaseCommentProvider_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithSnapshotOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(null, new RelationalDatabaseCommentProviderSnapshotOptions()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenDatabaseCommentProviderWithNullSnapshotOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(EmptyCommentProvider, (RelationalDatabaseCommentProviderSnapshotOptions)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenDatabaseCommentProviderWithNullResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(EmptyCommentProvider, (IIdentifierResolutionStrategy)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithOptionsAndResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(null, new RelationalDatabaseCommentProviderSnapshotOptions(), new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenDatabaseCommentProviderWithResolverWithNullSnapshotOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(EmptyCommentProvider, null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithNullResolverForOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(EmptyCommentProvider, new RelationalDatabaseCommentProviderSnapshotOptions(), null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseCommentProviderWithCommentsAndDefaultResolver_ReturnsDatabaseCommentProviderWithMatchingCommentsInGetAllObjectComments()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

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

        var snapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider).ConfigureAwait(false);
        var snapshotTableComments = await snapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var snapshotViewComments = await snapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var snapshotSequenceComments = await snapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var snapshotSynonymComments = await snapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var snapshotRoutineComments = await snapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTableComments, Is.EqualTo(tableComments));
            Assert.That(snapshotViewComments, Is.EqualTo(viewComments));
            Assert.That(snapshotSequenceComments, Is.EqualTo(sequenceComments));
            Assert.That(snapshotSynonymComments, Is.EqualTo(synonymComments));
            Assert.That(snapshotRoutineComments, Is.EqualTo(routineComments));
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseCommentProviderWithCommentsAndDefaultResolver_ReturnsDatabaseCommentProviderWithMatchingCommentsInGetObjectMethods()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

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

        var snapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider).ConfigureAwait(false);
        var snapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption().ConfigureAwait(false);
        var snapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption().ConfigureAwait(false);
        var snapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var snapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var snapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTableComment, OptionIs.Some);
            Assert.That(snapshotTableComment.UnwrapSome().TableName.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(snapshotViewComment, OptionIs.Some);
            Assert.That(snapshotViewComment.UnwrapSome().ViewName.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(snapshotSequenceComment, OptionIs.Some);
            Assert.That(snapshotSequenceComment.UnwrapSome().SequenceName.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(snapshotSynonymComment, OptionIs.Some);
            Assert.That(snapshotSynonymComment.UnwrapSome().SynonymName.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(snapshotRoutineComment, OptionIs.Some);
            Assert.That(snapshotRoutineComment.UnwrapSome().RoutineName.LocalName, Is.EqualTo("test_routine_name"));
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseCommentProviderWithComments_ReturnsDatabaseCommentProviderWithMatchingCommentsInGetObjectMethods()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

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

        var snapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, identifierResolver).ConfigureAwait(false);
        var snapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption().ConfigureAwait(false);
        var snapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption().ConfigureAwait(false);
        var snapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var snapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var snapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTableComment, OptionIs.Some);
            Assert.That(snapshotTableComment.UnwrapSome().TableName.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(snapshotViewComment, OptionIs.Some);
            Assert.That(snapshotViewComment.UnwrapSome().ViewName.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(snapshotSequenceComment, OptionIs.Some);
            Assert.That(snapshotSequenceComment.UnwrapSome().SequenceName.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(snapshotSynonymComment, OptionIs.Some);
            Assert.That(snapshotSynonymComment.UnwrapSome().SynonymName.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(snapshotRoutineComment, OptionIs.Some);
            Assert.That(snapshotRoutineComment.UnwrapSome().RoutineName.LocalName, Is.EqualTo("test_routine_name"));
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseCommentProviderWithComments_ReturnsDatabaseCommentProviderWithMatchingCommentsInGetAllObjectComments()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

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

        var snapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, identifierResolver).ConfigureAwait(false);
        var snapshotTableComments = await snapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var snapshotViewComments = await snapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var snapshotSequenceComments = await snapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var snapshotSynonymComments = await snapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var snapshotRoutineComments = await snapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTableComments, Is.EqualTo(tableComments));
            Assert.That(snapshotViewComments, Is.EqualTo(viewComments));
            Assert.That(snapshotSequenceComments, Is.EqualTo(sequenceComments));
            Assert.That(snapshotSynonymComments, Is.EqualTo(synonymComments));
            Assert.That(snapshotRoutineComments, Is.EqualTo(routineComments));
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseCommentProviderWithOptions_ReturnsDatabaseCommentProviderWithMatchingCommentsInGetObjectMethodsWhenConfigured()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

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

        var snapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, new RelationalDatabaseCommentProviderSnapshotOptions()).ConfigureAwait(false);
        var snapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption().ConfigureAwait(false);
        var snapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption().ConfigureAwait(false);
        var snapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var snapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var snapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTableComment, OptionIs.Some);
            Assert.That(snapshotTableComment.UnwrapSome().TableName.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(snapshotViewComment, OptionIs.Some);
            Assert.That(snapshotViewComment.UnwrapSome().ViewName.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(snapshotSequenceComment, OptionIs.Some);
            Assert.That(snapshotSequenceComment.UnwrapSome().SequenceName.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(snapshotSynonymComment, OptionIs.Some);
            Assert.That(snapshotSynonymComment.UnwrapSome().SynonymName.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(snapshotRoutineComment, OptionIs.Some);
            Assert.That(snapshotRoutineComment.UnwrapSome().RoutineName.LocalName, Is.EqualTo("test_routine_name"));
        }

        // toggle tables
        var tableOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeTableComments = true };
        var tableSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, tableOpts).ConfigureAwait(false);
        var tableSnapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption().ConfigureAwait(false);
        var tableSnapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption().ConfigureAwait(false);
        var tableSnapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var tableSnapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var tableSnapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableSnapshotTableComment, OptionIs.Some);
            Assert.That(tableSnapshotTableComment.UnwrapSome().TableName.LocalName, Is.EqualTo("test_table_name"));

            Assert.That(tableSnapshotViewComment, OptionIs.None);
            Assert.That(tableSnapshotSequenceComment, OptionIs.None);
            Assert.That(tableSnapshotSynonymComment, OptionIs.None);
            Assert.That(tableSnapshotRoutineComment, OptionIs.None);
        }

        // toggle views
        var viewOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeTableComments = true };
        var viewSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, viewOpts).ConfigureAwait(false);
        var viewSnapshotTableComment = await snapshot.GetTableComments("test_view_name").ToOption().ConfigureAwait(false);
        var viewSnapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption().ConfigureAwait(false);
        var viewSnapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var viewSnapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var viewSnapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewSnapshotTableComment, OptionIs.None);

            Assert.That(viewSnapshotViewComment, OptionIs.Some);
            Assert.That(viewSnapshotViewComment.UnwrapSome().ViewName.LocalName, Is.EqualTo("test_view_name"));

            Assert.That(viewSnapshotSequenceComment, OptionIs.None);
            Assert.That(viewSnapshotSynonymComment, OptionIs.None);
            Assert.That(viewSnapshotRoutineComment, OptionIs.None);
        }

        // toggle sequence
        var sequenceOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeViewComments = true };
        var sequenceSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, sequenceOpts).ConfigureAwait(false);
        var sequenceSnapshotTableComment = await snapshot.GetTableComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotViewComment = await snapshot.GetViewComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var sequenceSnapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequenceSnapshotTableComment, OptionIs.None);
            Assert.That(sequenceSnapshotViewComment, OptionIs.None);

            Assert.That(sequenceSnapshotSequenceComment, OptionIs.Some);
            Assert.That(sequenceSnapshotSequenceComment.UnwrapSome().SequenceName.LocalName, Is.EqualTo("test_sequence_name"));

            Assert.That(sequenceSnapshotSynonymComment, OptionIs.None);
            Assert.That(sequenceSnapshotRoutineComment, OptionIs.None);
        }

        // toggle synonyms
        var synonymOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeSynonymComments = true };
        var synonymSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, synonymOpts).ConfigureAwait(false);
        var synonymSnapshotTableComment = await snapshot.GetTableComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotViewComment = await snapshot.GetViewComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotSequenceComment = await snapshot.GetSequenceComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption().ConfigureAwait(false);
        var synonymSnapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(synonymSnapshotTableComment, OptionIs.None);
            Assert.That(synonymSnapshotViewComment, OptionIs.None);
            Assert.That(synonymSnapshotSequenceComment, OptionIs.None);

            Assert.That(synonymSnapshotSynonymComment, OptionIs.Some);
            Assert.That(synonymSnapshotSynonymComment.UnwrapSome().SynonymName.LocalName, Is.EqualTo("test_synonym_name"));

            Assert.That(synonymSnapshotRoutineComment, OptionIs.None);
        }

        // toggle routines
        var routineOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeRoutineComments = true };
        var routineSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, routineOpts).ConfigureAwait(false);
        var routineSnapshotTableComment = await snapshot.GetTableComments("test_routine_name").ToOption().ConfigureAwait(false);
        var routineSnapshotViewComment = await snapshot.GetViewComments("test_routine_name").ToOption().ConfigureAwait(false);
        var routineSnapshotSequenceComment = await snapshot.GetSequenceComments("test_routine_name").ToOption().ConfigureAwait(false);
        var routineSnapshotSynonymComment = await snapshot.GetSynonymComments("test_routine_name").ToOption().ConfigureAwait(false);
        var routineSnapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routineSnapshotTableComment, OptionIs.None);
            Assert.That(routineSnapshotViewComment, OptionIs.None);
            Assert.That(routineSnapshotSequenceComment, OptionIs.None);
            Assert.That(routineSnapshotSynonymComment, OptionIs.None);

            Assert.That(routineSnapshotRoutineComment, OptionIs.Some);
            Assert.That(routineSnapshotRoutineComment.UnwrapSome().RoutineName.LocalName, Is.EqualTo("test_routine_name"));
        }
    }

    [Test]
    public static async Task SnapshotAsync_WhenGivenRelationalDatabaseCommentProviderWithOptions_ReturnsDatabaseCommentProviderWithMatchingCommentsInGetAllObjectCommentsWhenConfigured()
    {
        var identifierDefaults = new IdentifierDefaults("test_server", "test_database", "test_schema");
        var identifierResolver = new VerbatimIdentifierResolutionStrategy();

        var testTableName = Identifier.CreateQualifiedIdentifier("test_table_name");
        var tableComment = new Mock<IRelationalDatabaseTableComments>(MockBehavior.Strict);
        tableComment.Setup(t => t.TableName).Returns(testTableName);
        var tableComments = new[] { tableComment.Object };

        var testViewName = Identifier.CreateQualifiedIdentifier("test_view_name");
        var viewComment = new Mock<IDatabaseViewComments>(MockBehavior.Strict);
        viewComment.Setup(v => v.ViewName).Returns(testViewName);
        var viewComments = new[] { viewComment.Object };

        var testSequenceName = Identifier.CreateQualifiedIdentifier("test_sequence_name");
        var sequenceComment = new Mock<IDatabaseSequenceComments>(MockBehavior.Strict);
        sequenceComment.Setup(s => s.SequenceName).Returns(testSequenceName);
        var sequenceComments = new[] { sequenceComment.Object };

        var testSynonymName = Identifier.CreateQualifiedIdentifier("test_synonym_name");
        var synonymComment = new Mock<IDatabaseSynonymComments>(MockBehavior.Strict);
        synonymComment.Setup(s => s.SynonymName).Returns(testSynonymName);
        var synonymComments = new[] { synonymComment.Object };

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

        var snapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, new RelationalDatabaseCommentProviderSnapshotOptions()).ConfigureAwait(false);
        var snapshotTableComments = await snapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var snapshotViewComments = await snapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var snapshotSequenceComments = await snapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var snapshotSynonymComments = await snapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var snapshotRoutineComments = await snapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(snapshotTableComments, Is.EqualTo(tableComments));
            Assert.That(snapshotViewComments, Is.EqualTo(viewComments));
            Assert.That(snapshotSequenceComments, Is.EqualTo(sequenceComments));
            Assert.That(snapshotSynonymComments, Is.EqualTo(synonymComments));
            Assert.That(snapshotRoutineComments, Is.EqualTo(routineComments));
        }

        // toggle tables
        var tableOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeTableComments = true };
        var tableSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, tableOpts).ConfigureAwait(false);
        var tableSnapshotTableComments = await tableSnapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var tableSnapshotViewComments = await tableSnapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var tableSnapshotSequenceComments = await tableSnapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var tableSnapshotSynonymComments = await tableSnapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var tableSnapshotRoutineComments = await tableSnapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(tableSnapshotTableComments, Is.EqualTo(tableComments));
            Assert.That(tableSnapshotViewComments, Is.Empty);
            Assert.That(tableSnapshotSequenceComments, Is.Empty);
            Assert.That(tableSnapshotSynonymComments, Is.Empty);
            Assert.That(tableSnapshotRoutineComments, Is.Empty);
        }

        // toggle views
        var viewOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeViewComments = true };
        var viewSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, viewOpts).ConfigureAwait(false);
        var viewSnapshotTableComments = await viewSnapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var viewSnapshotViewComments = await viewSnapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var viewSnapshotSequenceComments = await viewSnapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var viewSnapshotSynonymComments = await viewSnapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var viewSnapshotRoutineComments = await viewSnapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(viewSnapshotTableComments, Is.Empty);
            Assert.That(viewSnapshotViewComments, Is.EqualTo(viewComments));
            Assert.That(viewSnapshotSequenceComments, Is.Empty);
            Assert.That(viewSnapshotSynonymComments, Is.Empty);
            Assert.That(viewSnapshotRoutineComments, Is.Empty);
        }

        // toggle sequence
        var sequenceOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeSequenceComments = true };
        var sequenceSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, sequenceOpts).ConfigureAwait(false);
        var sequenceSnapshotTableComments = await sequenceSnapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotViewComments = await sequenceSnapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotSequenceComments = await sequenceSnapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotSynonymComments = await sequenceSnapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var sequenceSnapshotRoutineComments = await sequenceSnapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sequenceSnapshotTableComments, Is.Empty);
            Assert.That(sequenceSnapshotViewComments, Is.Empty);
            Assert.That(sequenceSnapshotSequenceComments, Is.EqualTo(sequenceComments));
            Assert.That(sequenceSnapshotSynonymComments, Is.Empty);
            Assert.That(sequenceSnapshotRoutineComments, Is.Empty);
        }

        // toggle synonyms
        var synonymOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeSynonymComments = true };
        var synonymSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, synonymOpts).ConfigureAwait(false);
        var synonymSnapshotTableComments = await synonymSnapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotViewComments = await synonymSnapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotSequenceComments = await synonymSnapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotSynonymComments = await synonymSnapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var synonymSnapshotRoutineComments = await synonymSnapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(synonymSnapshotTableComments, Is.Empty);
            Assert.That(synonymSnapshotViewComments, Is.Empty);
            Assert.That(synonymSnapshotSequenceComments, Is.Empty);
            Assert.That(synonymSnapshotSynonymComments, Is.EqualTo(synonymComments));
            Assert.That(synonymSnapshotRoutineComments, Is.Empty);
        }

        // toggle routines
        var routineOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeRoutineComments = true };
        var routineSnapshot = await RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(commentProvider, routineOpts).ConfigureAwait(false);
        var routineSnapshotTableComments = await routineSnapshot.GetAllTableComments().ToListAsync().ConfigureAwait(false);
        var routineSnapshotViewComments = await routineSnapshot.GetAllViewComments().ToListAsync().ConfigureAwait(false);
        var routineSnapshotSequenceComments = await routineSnapshot.GetAllSequenceComments().ToListAsync().ConfigureAwait(false);
        var routineSnapshotSynonymComments = await routineSnapshot.GetAllSynonymComments().ToListAsync().ConfigureAwait(false);
        var routineSnapshotRoutineComments = await routineSnapshot.GetAllRoutineComments().ToListAsync().ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(routineSnapshotTableComments, Is.Empty);
            Assert.That(routineSnapshotViewComments, Is.Empty);
            Assert.That(routineSnapshotSequenceComments, Is.Empty);
            Assert.That(routineSnapshotSynonymComments, Is.Empty);
            Assert.That(routineSnapshotRoutineComments, Is.EqualTo(routineComments));
        }
    }
}