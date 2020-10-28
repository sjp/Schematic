using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V11.Comments
{
    internal sealed class PostgreSqlRoutineCommentProviderTests : PostgreSql11Test
    {
        private IDatabaseRoutineCommentProvider RoutineCommentProvider => new PostgreSqlRoutineCommentProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            // func
            await DbConnection.ExecuteAsync(@"CREATE FUNCTION v11_comment_test_routine_1(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL", CancellationToken.None).ConfigureAwait(false);
            // stored proc
            await DbConnection.ExecuteAsync(@"CREATE PROCEDURE v11_comment_test_routine_2()
LANGUAGE PLPGSQL
AS $$
BEGIN
    COMMIT;
END $$", CancellationToken.None).ConfigureAwait(false);
            // func
            await DbConnection.ExecuteAsync(@"CREATE FUNCTION v11_comment_test_routine_3(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL", CancellationToken.None).ConfigureAwait(false);
            // stored proc
            await DbConnection.ExecuteAsync(@"CREATE PROCEDURE v11_comment_test_routine_4()
LANGUAGE PLPGSQL
AS $$
BEGIN
    COMMIT;
END $$", CancellationToken.None).ConfigureAwait(false);

            await DbConnection.ExecuteAsync("comment on function v11_comment_test_routine_3 (integer) is 'This is a test function.'", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("comment on procedure v11_comment_test_routine_4 () is 'This is a test procedure.'", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop function v11_comment_test_routine_1(integer)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop procedure v11_comment_test_routine_2()", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop function v11_comment_test_routine_3(integer)", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop procedure v11_comment_test_routine_4()", CancellationToken.None).ConfigureAwait(false);
        }

        private Task<IDatabaseRoutineComments> GetRoutineCommentsAsync(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            return GetRoutineCommentsAsyncCore(routineName);
        }

        private async Task<IDatabaseRoutineComments> GetRoutineCommentsAsyncCore(Identifier routineName)
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
            {
                if (!_commentsCache.TryGetValue(routineName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseRoutineComments>(() => RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync());
                    _commentsCache[routineName] = lazyComment;
                }

                return await lazyComment.ConfigureAwait(false);
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>>();

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineComment()
        {
            var routineIsSome = await RoutineCommentProvider.GetRoutineComments("v11_comment_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.That(routineIsSome, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "v11_comment_test_routine_1";
            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName.LocalName, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("v11_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "v11_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "v11_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDifferentCasedName_ShouldBeResolvedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "V11_COMMENT_TEST_ROUTINE_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v11_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutineMissing_ReturnsNone()
        {
            var routineIsNone = await RoutineCommentProvider.GetRoutineComments("routine_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(routineIsNone, Is.True);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenEnumerated_ContainsRoutineComments()
        {
            var hasRoutineComments = await RoutineCommentProvider.GetAllRoutineComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.That(hasRoutineComments, Is.True);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenEnumerated_ContainsTestRoutineComment()
        {
            var containsTestRoutine = await RoutineCommentProvider.GetAllRoutineComments()
                .AnyAsync(t => string.Equals(t.RoutineName.LocalName, "v11_comment_test_routine_1", StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestRoutine, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("v11_comment_test_routine_1").ConfigureAwait(false);

            Assert.That(comments.Comment.IsNone, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test function.";
            var comments = await GetRoutineCommentsAsync("v11_comment_test_routine_3").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.That(routineComment, Is.EqualTo(expectedComment));
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("v11_comment_test_routine_2").ConfigureAwait(false);

            Assert.That(comments.Comment.IsNone, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test procedure.";
            var comments = await GetRoutineCommentsAsync("v11_comment_test_routine_4").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.That(routineComment, Is.EqualTo(expectedComment));
        }
    }
}
