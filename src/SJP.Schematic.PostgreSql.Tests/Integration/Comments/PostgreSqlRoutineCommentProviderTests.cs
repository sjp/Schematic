﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Comments;

internal sealed class PostgreSqlRoutineCommentProviderTests : PostgreSqlTest
{
    private IDatabaseRoutineCommentProvider RoutineCommentProvider => new PostgreSqlRoutineCommentProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public async Task Init()
    {
        // func
        await DbConnection.ExecuteAsync(@"CREATE FUNCTION db_comment_test_routine_1(val integer)
RETURNS integer AS $$
BEGIN
    RETURN val + 1;
END; $$
LANGUAGE PLPGSQL", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop function db_comment_test_routine_1(integer)", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineComment()
    {
        var routineIsSome = await RoutineCommentProvider.GetRoutineComments("db_comment_test_routine_1").IsSome.ConfigureAwait(false);
        Assert.That(routineIsSome, Is.True);
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresent_ReturnsRoutineWithCorrectName()
    {
        const string routineName = "db_comment_test_routine_1";
        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName.LocalName, Is.EqualTo(routineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("db_comment_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Schema, "db_comment_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName, Is.EqualTo(routineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
    {
        var routineName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_comment_test_routine_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

        var routineComments = await RoutineCommentProvider.GetRoutineComments(routineName).UnwrapSomeAsync().ConfigureAwait(false);

        Assert.That(routineComments.RoutineName, Is.EqualTo(expectedRoutineName));
    }

    [Test]
    public async Task GetRoutineComments_WhenRoutinePresentGivenDifferentCasedName_ShouldBeResolvedCorrectly()
    {
        var routineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_COMMENT_TEST_ROUTINE_1");
        var expectedRoutineName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_comment_test_routine_1");

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
    public async Task EnumerateAllRoutineComments_WhenEnumerated_ContainsRoutineComments()
    {
        var hasRoutineComments = await RoutineCommentProvider.EnumerateAllRoutineComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasRoutineComments, Is.True);
    }

    [Test]
    public async Task EnumerateAllRoutineComments_WhenEnumerated_ContainsTestRoutineComment()
    {
        var containsTestRoutine = await RoutineCommentProvider.EnumerateAllRoutineComments()
            .AnyAsync(r => string.Equals(r.RoutineName.LocalName, "db_comment_test_routine_1", StringComparison.Ordinal))
            .ConfigureAwait(false);

        Assert.That(containsTestRoutine, Is.True);
    }

    [Test]
    public async Task GetAllRoutineComments_WhenRetrieved_ContainsRoutineComments()
    {
        var routineComments = await RoutineCommentProvider.GetAllRoutineComments().ConfigureAwait(false);

        Assert.That(routineComments, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllRoutineComments_WhenRetrieved_ContainsTestRoutineComment()
    {
        var routineComments = await RoutineCommentProvider.GetAllRoutineComments().ConfigureAwait(false);
        var containsTestRoutine = routineComments.Any(r => string.Equals(r.RoutineName.LocalName, "db_comment_test_routine_1", StringComparison.Ordinal));

        Assert.That(containsTestRoutine, Is.True);
    }
}