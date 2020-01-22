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
using SJP.Schematic.SqlServer.Comments;

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments
{
    internal sealed class SqlServerRoutineCommentProviderTests : SqlServerTest
    {
        private IDatabaseRoutineCommentProvider RoutineCommentProvider => new SqlServerRoutineCommentProvider(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
CREATE FUNCTION dbo.routine_comment_tf_1()
RETURNS @ret TABLE
(
    test_col int NOT NULL
)
AS
BEGIN
   INSERT INTO @ret (test_col) VALUES (1);
   RETURN
END").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"
CREATE FUNCTION dbo.routine_comment_tf_2()
RETURNS @ret TABLE
(
    test_col int NOT NULL
)
AS
BEGIN
   INSERT INTO @ret (test_col) VALUES (1);
   RETURN
END").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"CREATE PROCEDURE routine_comment_sp_1
AS
SELECT DB_NAME() AS ThisDB").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"CREATE PROCEDURE routine_comment_sp_2
AS
SELECT DB_NAME() AS ThisDB").ConfigureAwait(false);

            await AddCommentForFunction("This is a test function comment.", "dbo", "routine_comment_tf_2").ConfigureAwait(false);
            await AddCommentForStoredProcedure("This is a test stored procedure comment.", "dbo", "routine_comment_sp_2").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop function routine_comment_tf_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop function routine_comment_tf_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop procedure routine_comment_sp_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop procedure routine_comment_sp_2").ConfigureAwait(false);
        }

        private Task AddCommentForFunction(string comment, string schemaName, string functionName)
        {
            const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'FUNCTION',
  @level1name = @FunctionName";
            return Connection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, FunctionName = functionName });
        }

        private Task AddCommentForStoredProcedure(string comment, string schemaName, string procedureName)
        {
            const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'PROCEDURE',
  @level1name = @StoredProcName";
            return Connection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, StoredProcName = procedureName });
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
            var routineIsSome = await RoutineCommentProvider.GetRoutineComments("routine_comment_tf_1").IsSome.ConfigureAwait(false);
            Assert.IsTrue(routineIsSome);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
        {
            const string routineName = "routine_comment_tf_1";
            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(routineName, routineComments.RoutineName.LocalName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("routine_comment_tf_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Schema, "routine_comment_tf_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(routineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");

            var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.AreEqual(expectedRoutineName, routineComments.RoutineName);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "routine_comment_tf_1");
            var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "routine_comment_tf_1");

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
        public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("ROUTINE_COMMENT_tf_1");
            var routineComments = await RoutineCommentProvider.GetRoutineComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName, routineComments.RoutineName.LocalName);
            Assert.IsTrue(equalNames);
        }

        [Test]
        public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameWithDifferentCase_ReturnsMatchingName()
        {
            var inputName = new Identifier("Dbo", "ROUTINE_COMMENT_tf_1");
            var routineComments = await RoutineCommentProvider.GetRoutineComments(inputName).UnwrapSomeAsync().ConfigureAwait(false);

            var equalNames = IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.Schema, routineComments.RoutineName.Schema)
                && IdentifierComparer.OrdinalIgnoreCase.Equals(inputName.LocalName, routineComments.RoutineName.LocalName);
            Assert.IsTrue(equalNames);
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
                .AnyAsync(t => t.RoutineName.LocalName == "routine_comment_tf_1")
                .ConfigureAwait(false);

            Assert.IsTrue(containsTestRoutine);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("routine_comment_tf_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetRoutineComments_WhenFunctionContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test function comment.";
            var comments = await GetRoutineCommentsAsync("routine_comment_tf_2").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, routineComment);
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureMissingComment_ReturnsNone()
        {
            var comments = await GetRoutineCommentsAsync("routine_comment_sp_1").ConfigureAwait(false);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public async Task GetRoutineComments_WhenStoredProcedureContainsComment_ReturnsExpectedValue()
        {
            const string expectedComment = "This is a test stored procedure comment.";
            var comments = await GetRoutineCommentsAsync("routine_comment_sp_2").ConfigureAwait(false);

            var routineComment = comments.Comment.UnwrapSome();

            Assert.AreEqual(expectedComment, routineComment);
        }
    }
}
