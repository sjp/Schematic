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
using SJP.Schematic.MySql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.MySql.Tests.Integration.Comments
{
    internal sealed class MySqlRoutineCommentProviderTests : MySqlTest
    {
        private IDatabaseRoutineCommentProvider RoutineCommentProvider => new MySqlRoutineCommentProvider(DbConnection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await DbConnection.ExecuteAsync(@"
CREATE FUNCTION comment_test_routine_1()
  RETURNS TEXT
  LANGUAGE SQL
  DETERMINISTIC
BEGIN
  RETURN 'test';
END", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
CREATE PROCEDURE comment_test_routine_2()
DETERMINISTIC
BEGIN
   COMMIT;
END", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
CREATE FUNCTION comment_test_routine_3()
  RETURNS TEXT
  LANGUAGE SQL
  DETERMINISTIC
  COMMENT 'This is a test function comment.'
BEGIN
  RETURN 'test';
END
", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync(@"
CREATE PROCEDURE comment_test_routine_4()
  DETERMINISTIC
  COMMENT 'This is a test stored procedure comment.'
BEGIN
   COMMIT;
END
", CancellationToken.None).ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await DbConnection.ExecuteAsync("drop function comment_test_routine_1", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop function comment_test_routine_3", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop procedure comment_test_routine_2", CancellationToken.None).ConfigureAwait(false);
            await DbConnection.ExecuteAsync("drop procedure comment_test_routine_4", CancellationToken.None).ConfigureAwait(false);
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

        private readonly AsyncLock _lock = new();
        private readonly Dictionary<Identifier, AsyncLazy<IDatabaseRoutineComments>> _commentsCache = new();

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineComment()
        {
            var routineIsSome = await RoutineCommentProvider.GetRoutineComments("comment_test_routine_1").IsSome.ConfigureAwait(false);
            Assert.That(routineIsSome, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "comment_test_routine_1";
            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName.LocalName, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(routineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "comment_test_routine_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_routine_1");

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
                .AnyAsync(t => string.Equals(t.RoutineName.LocalName, "comment_test_routine_1", StringComparison.Ordinal))
                .ConfigureAwait(false);

            Assert.That(containsTestRoutine, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("comment_test_routine_1").ConfigureAwait(false);

            Assert.That(comments.Comment.IsNone, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test function comment.";
            var comments = await GetRoutineCommentsAsync("comment_test_routine_3").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.That(routineComment, Is.EqualTo(expectedComment));
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("comment_test_routine_2").ConfigureAwait(false);

            Assert.That(comments.Comment.IsNone, Is.True);
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test stored procedure comment.";
            var comments = await GetRoutineCommentsAsync("comment_test_routine_4").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.That(routineComment, Is.EqualTo(expectedComment));
        }
    }
}
