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
        Assert.That(() => EmptyCommentProvider.SnapshotAsync((RelationalDatabaseCommentProviderSnapshotOptions)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenDatabaseCommentProviderWithNullResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.SnapshotAsync((IIdentifierResolutionStrategy)null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithOptionsAndResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderSnapshotExtensions.SnapshotAsync(null, new RelationalDatabaseCommentProviderSnapshotOptions(), new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenDatabaseCommentProviderWithResolverWithNullSnapshotOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithNullResolverForOptions_ThrowsArgumentNullException()
    {
        Assert.That(() => EmptyCommentProvider.SnapshotAsync(new RelationalDatabaseCommentProviderSnapshotOptions(), null), Throws.ArgumentNullException);
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

        var snapshot = await commentProvider.SnapshotAsync();
        var snapshotTableComments = await snapshot.EnumerateAllTableComments().ToListAsync();
        var snapshotViewComments = await snapshot.EnumerateAllViewComments().ToListAsync();
        var snapshotSequenceComments = await snapshot.EnumerateAllSequenceComments().ToListAsync();
        var snapshotSynonymComments = await snapshot.EnumerateAllSynonymComments().ToListAsync();
        var snapshotRoutineComments = await snapshot.EnumerateAllRoutineComments().ToListAsync();

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

        var snapshot = await commentProvider.SnapshotAsync();
        var snapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption();
        var snapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption();
        var snapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption();
        var snapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption();
        var snapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption();

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

        var snapshot = await commentProvider.SnapshotAsync(identifierResolver);
        var snapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption();
        var snapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption();
        var snapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption();
        var snapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption();
        var snapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption();

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

        var snapshot = await commentProvider.SnapshotAsync(identifierResolver);
        var snapshotTableComments = await snapshot.EnumerateAllTableComments().ToListAsync();
        var snapshotViewComments = await snapshot.EnumerateAllViewComments().ToListAsync();
        var snapshotSequenceComments = await snapshot.EnumerateAllSequenceComments().ToListAsync();
        var snapshotSynonymComments = await snapshot.EnumerateAllSynonymComments().ToListAsync();
        var snapshotRoutineComments = await snapshot.EnumerateAllRoutineComments().ToListAsync();

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

        var snapshot = await commentProvider.SnapshotAsync(new RelationalDatabaseCommentProviderSnapshotOptions());
        var snapshotTableComment = await snapshot.GetTableComments("test_table_name").ToOption();
        var snapshotViewComment = await snapshot.GetViewComments("test_view_name").ToOption();
        var snapshotSequenceComment = await snapshot.GetSequenceComments("test_sequence_name").ToOption();
        var snapshotSynonymComment = await snapshot.GetSynonymComments("test_synonym_name").ToOption();
        var snapshotRoutineComment = await snapshot.GetRoutineComments("test_routine_name").ToOption();

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
        var tableSnapshot = await commentProvider.SnapshotAsync(tableOpts);
        var tableSnapshotTableComment = await tableSnapshot.GetTableComments("test_table_name").ToOption();
        var tableSnapshotViewComment = await tableSnapshot.GetViewComments("test_view_name").ToOption();
        var tableSnapshotSequenceComment = await tableSnapshot.GetSequenceComments("test_sequence_name").ToOption();
        var tableSnapshotSynonymComment = await tableSnapshot.GetSynonymComments("test_synonym_name").ToOption();
        var tableSnapshotRoutineComment = await tableSnapshot.GetRoutineComments("test_routine_name").ToOption();

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
        var viewOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeViewComments = true };
        var viewSnapshot = await commentProvider.SnapshotAsync(viewOpts);
        var viewSnapshotTableComment = await viewSnapshot.GetTableComments("test_table_name").ToOption();
        var viewSnapshotViewComment = await viewSnapshot.GetViewComments("test_view_name").ToOption();
        var viewSnapshotSequenceComment = await viewSnapshot.GetSequenceComments("test_sequence_name").ToOption();
        var viewSnapshotSynonymComment = await viewSnapshot.GetSynonymComments("test_synonym_name").ToOption();
        var viewSnapshotRoutineComment = await viewSnapshot.GetRoutineComments("test_routine_name").ToOption();

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
        var sequenceOpts = RelationalDatabaseCommentProviderSnapshotOptions.Empty with { IncludeSequenceComments = true };
        var sequenceSnapshot = await commentProvider.SnapshotAsync(sequenceOpts);
        var sequenceSnapshotTableComment = await sequenceSnapshot.GetTableComments("test_table_name").ToOption();
        var sequenceSnapshotViewComment = await sequenceSnapshot.GetViewComments("test_view_name").ToOption();
        var sequenceSnapshotSequenceComment = await sequenceSnapshot.GetSequenceComments("test_sequence_name").ToOption();
        var sequenceSnapshotSynonymComment = await sequenceSnapshot.GetSynonymComments("test_synonym_name").ToOption();
        var sequenceSnapshotRoutineComment = await sequenceSnapshot.GetRoutineComments("test_routine_name").ToOption();

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
        var synonymSnapshot = await commentProvider.SnapshotAsync(synonymOpts);
        var synonymSnapshotTableComment = await synonymSnapshot.GetTableComments("test_table_name").ToOption();
        var synonymSnapshotViewComment = await synonymSnapshot.GetViewComments("test_view_name").ToOption();
        var synonymSnapshotSequenceComment = await synonymSnapshot.GetSequenceComments("test_sequence_name").ToOption();
        var synonymSnapshotSynonymComment = await synonymSnapshot.GetSynonymComments("test_synonym_name").ToOption();
        var synonymSnapshotRoutineComment = await synonymSnapshot.GetRoutineComments("test_routine_name").ToOption();

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
        var routineSnapshot = await commentProvider.SnapshotAsync(routineOpts);
        var routineSnapshotTableComment = await routineSnapshot.GetTableComments("test_table_name").ToOption();
        var routineSnapshotViewComment = await routineSnapshot.GetViewComments("test_view_name").ToOption();
        var routineSnapshotSequenceComment = await routineSnapshot.GetSequenceComments("test_sequence_name").ToOption();
        var routineSnapshotSynonymComment = await routineSnapshot.GetSynonymComments("test_synonym_name").ToOption();
        var routineSnapshotRoutineComment = await routineSnapshot.GetRoutineComments("test_routine_name").ToOption();

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

        var snapshot = await commentProvider.SnapshotAsync(new RelationalDatabaseCommentProviderSnapshotOptions());
        var snapshotTableComments = await snapshot.EnumerateAllTableComments().ToListAsync();
        var snapshotViewComments = await snapshot.EnumerateAllViewComments().ToListAsync();
        var snapshotSequenceComments = await snapshot.EnumerateAllSequenceComments().ToListAsync();
        var snapshotSynonymComments = await snapshot.EnumerateAllSynonymComments().ToListAsync();
        var snapshotRoutineComments = await snapshot.EnumerateAllRoutineComments().ToListAsync();

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
        var tableSnapshot = await commentProvider.SnapshotAsync(tableOpts);
        var tableSnapshotTableComments = await tableSnapshot.EnumerateAllTableComments().ToListAsync();
        var tableSnapshotViewComments = await tableSnapshot.EnumerateAllViewComments().ToListAsync();
        var tableSnapshotSequenceComments = await tableSnapshot.EnumerateAllSequenceComments().ToListAsync();
        var tableSnapshotSynonymComments = await tableSnapshot.EnumerateAllSynonymComments().ToListAsync();
        var tableSnapshotRoutineComments = await tableSnapshot.EnumerateAllRoutineComments().ToListAsync();

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
        var viewSnapshot = await commentProvider.SnapshotAsync(viewOpts);
        var viewSnapshotTableComments = await viewSnapshot.EnumerateAllTableComments().ToListAsync();
        var viewSnapshotViewComments = await viewSnapshot.EnumerateAllViewComments().ToListAsync();
        var viewSnapshotSequenceComments = await viewSnapshot.EnumerateAllSequenceComments().ToListAsync();
        var viewSnapshotSynonymComments = await viewSnapshot.EnumerateAllSynonymComments().ToListAsync();
        var viewSnapshotRoutineComments = await viewSnapshot.EnumerateAllRoutineComments().ToListAsync();

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
        var sequenceSnapshot = await commentProvider.SnapshotAsync(sequenceOpts);
        var sequenceSnapshotTableComments = await sequenceSnapshot.EnumerateAllTableComments().ToListAsync();
        var sequenceSnapshotViewComments = await sequenceSnapshot.EnumerateAllViewComments().ToListAsync();
        var sequenceSnapshotSequenceComments = await sequenceSnapshot.EnumerateAllSequenceComments().ToListAsync();
        var sequenceSnapshotSynonymComments = await sequenceSnapshot.EnumerateAllSynonymComments().ToListAsync();
        var sequenceSnapshotRoutineComments = await sequenceSnapshot.EnumerateAllRoutineComments().ToListAsync();

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
        var synonymSnapshot = await commentProvider.SnapshotAsync(synonymOpts);
        var synonymSnapshotTableComments = await synonymSnapshot.EnumerateAllTableComments().ToListAsync();
        var synonymSnapshotViewComments = await synonymSnapshot.EnumerateAllViewComments().ToListAsync();
        var synonymSnapshotSequenceComments = await synonymSnapshot.EnumerateAllSequenceComments().ToListAsync();
        var synonymSnapshotSynonymComments = await synonymSnapshot.EnumerateAllSynonymComments().ToListAsync();
        var synonymSnapshotRoutineComments = await synonymSnapshot.EnumerateAllRoutineComments().ToListAsync();

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
        var routineSnapshot = await commentProvider.SnapshotAsync(routineOpts);
        var routineSnapshotTableComments = await routineSnapshot.EnumerateAllTableComments().ToListAsync();
        var routineSnapshotViewComments = await routineSnapshot.EnumerateAllViewComments().ToListAsync();
        var routineSnapshotSequenceComments = await routineSnapshot.EnumerateAllSequenceComments().ToListAsync();
        var routineSnapshotSynonymComments = await routineSnapshot.EnumerateAllSynonymComments().ToListAsync();
        var routineSnapshotRoutineComments = await routineSnapshot.EnumerateAllRoutineComments().ToListAsync();

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