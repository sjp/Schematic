using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class RelationalDatabaseCommentProviderExtensionsTests
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
        Assert.That(() => RelationalDatabaseCommentProviderExtensions.SnapshotAsync(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullDatabaseCommentProviderWithResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderExtensions.SnapshotAsync(null, new VerbatimIdentifierResolutionStrategy()), Throws.ArgumentNullException);
    }

    [Test]
    public static void SnapshotAsync_GivenNullIdentifierResolver_ThrowsArgumentNullException()
    {
        Assert.That(() => RelationalDatabaseCommentProviderExtensions.SnapshotAsync(EmptyCommentProvider, null), Throws.ArgumentNullException);
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

        var snapshot = await RelationalDatabaseCommentProviderExtensions.SnapshotAsync(commentProvider).ConfigureAwait(false);
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

        var snapshot = await RelationalDatabaseCommentProviderExtensions.SnapshotAsync(commentProvider, identifierResolver).ConfigureAwait(false);
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

        var snapshot = await RelationalDatabaseCommentProviderExtensions.SnapshotAsync(commentProvider).ConfigureAwait(false);
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

        var snapshot = await RelationalDatabaseCommentProviderExtensions.SnapshotAsync(commentProvider, identifierResolver).ConfigureAwait(false);
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
}