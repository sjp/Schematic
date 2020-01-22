using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Comments;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Versions.V9_5.Comments
{
    internal sealed class PostgreSqlRoutineCommentProviderTests : PostgreSql95Test
    {
        private IDatabaseRoutineCommentProvider RoutineCommentProvider => new PostgreSqlRoutineCommentProvider(Connection, IdentifierDefaults, IdentifierResolver);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"CREATE FUNCTION v95_comment_test_routine_1(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"CREATE FUNCTION v95_comment_test_routine_2(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL").ConfigureAwait(false);

            await Connection.ExecuteAsync("comment on function v95_comment_test_routine_2 (integer) is 'This is a test function.'").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop function v95_comment_test_routine_1(integer)").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop function v95_comment_test_routine_2(integer)").ConfigureAwait(false);
        }

        private Task<IDatabaseRoutineComments> GetRoutineCommentsAsync(Identifier routineName)
        {
            if (routineName == null)
                throw new ArgumentNullException(nameof(routineName));

            lock (_lock)
            {
                if (!_commentsCache.TryGetValue(routineName, out var lazyComment))
                {
                    lazyComment = new AsyncLazy<IDatabaseRoutineComments>(() => RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync());
                    _commentsCache[routineName] = lazyComment;
                }

                return lazyComment.Task;
            }
        }

        private readonly object _lock = new object();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>> _commentsCache = new Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>>();

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineComment()
        {
            var routineIsSome = await RoutineCommentProvider.GetRoutineComments("v95_comment_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(routineIsSome);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "v95_comment_test_routine_1";
            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(routineName, routineComments.RoutineName.LocalName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("v95_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "v95_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(routineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "v95_comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDifferentCasedName_ShouldBeResolvedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "V95_COMMENT_TEST_ROUTINE_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "v95_comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutineMissing_ReturnsNone()
        {
            var routineIsNone = await RoutineCommentProvider.GetRoutineComments("routine_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.IsTrue(routineIsNone);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenEnumerated_ContainsRoutineComments()
        {
            var hasRoutineComments = await RoutineCommentProvider.GetAllRoutineComments()
                .AnyAsync()
                .ConfigureAwait(false);

            Assert.IsTrue(hasRoutineComments);
        }

        [Test]
        public async Task GetAllRoutineComments_WhenEnumerated_ContainsTestRoutineComment()
        {
            var containsTestRoutine = await RoutineCommentProvider.GetAllRoutineComments()
                .AnyAsync(r => r.RoutineName.LocalName == "v95_comment_test_routine_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestRoutine);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("v95_comment_test_routine_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test function.";
            var comments = await GetRoutineCommentsAsync("v95_comment_test_routine_2").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, routineComment);
        }
    }
}
