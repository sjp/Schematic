using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Comments;

internal sealed class PostgreSqlUserDefinedTypeCommentProviderTests : PostgreSqlTest
{
    private IDatabaseUserDefinedTypeCommentProvider TypeCommentProvider => new PostgreSqlUserDefinedTypeCommentProvider(DbConnection, IdentifierDefaults, IdentifierResolver);

    [OneTimeSetUp]
    public Task Init() => ExecuteBatchAsync(
        "create domain comment_test_udt_1 as integer",
        "create domain comment_test_udt_2 as integer",
        "create type comment_test_udt_3 as enum ('first', 'second')",
        "create type comment_test_udt_4 as (first_attr integer, second_attr text)",
        "create table comment_test_udt_table_1 (test_column integer)",
        "comment on domain comment_test_udt_2 is 'This is a test domain.'",
        "comment on type comment_test_udt_3 is 'This is a test enum.'",
        "comment on type comment_test_udt_4 is 'This is a test composite type.'"
    );

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop table comment_test_udt_table_1",
        "drop type comment_test_udt_4",
        "drop type comment_test_udt_3",
        "drop domain comment_test_udt_2",
        "drop domain comment_test_udt_1"
    );

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresent_ReturnsTypeComment()
    {
        var typeIsSome = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_1", TestContext.CurrentContext.CancellationToken).IsSome;

        Assert.That(typeIsSome, Is.True);
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresent_ReturnsTypeWithCorrectName()
    {
        const string typeName = "comment_test_udt_1";
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.TypeName.LocalName, Is.EqualTo(typeName));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier("comment_test_udt_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_udt_1");

        var comments = await TypeCommentProvider.GetUserDefinedTypeComments(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.TypeName, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier(IdentifierDefaults.Schema, "comment_test_udt_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_udt_1");

        var comments = await TypeCommentProvider.GetUserDefinedTypeComments(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.TypeName, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
    {
        var typeName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_udt_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_udt_1");

        var comments = await TypeCommentProvider.GetUserDefinedTypeComments(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.TypeName, Is.EqualTo(expectedTypeName));
    }

    // Unlike the schema comment provider, this one resolves through the identifier resolver, which
    // offers the lower-cased name before the name as given.
    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresentGivenDifferentCasedName_ShouldBeResolvedCorrectly()
    {
        var typeName = new Identifier(IdentifierDefaults.Schema, "COMMENT_TEST_UDT_1");
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_udt_1");

        var comments = await TypeCommentProvider.GetUserDefinedTypeComments(typeName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.TypeName, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypeMissing_ReturnsNone()
    {
        var typeIsNone = await TypeCommentProvider.GetUserDefinedTypeComments("type_that_doesnt_exist", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    // A table's row type lives in pg_type too, but it was not declared by a user, so name
    // resolution rejects it before any comment is read.
    [Test]
    public async Task GetUserDefinedTypeComments_GivenTableRowTypeName_ReturnsNone()
    {
        var typeIsNone = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_table_1", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    [Test]
    public async Task GetUserDefinedTypeComments_GivenBuiltInTypeName_ReturnsNone()
    {
        var typeIsNone = await TypeCommentProvider.GetUserDefinedTypeComments("integer", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypeMissingComment_ReturnsNoneComment()
    {
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenDomainContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test domain.";
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenEnumContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test enum.";
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_3", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenCompositeTypeContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test composite type.";
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_4", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(expectedComment));
    }

    // A name is resolved before its comment is read, so a hit costs both queries and a miss stops
    // after the name query.
    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresent_IssuesResolutionAndCommentQueries()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var commentProvider = new PostgreSqlUserDefinedTypeCommentProvider(countingConnectionFactory, IdentifierDefaults, IdentifierResolver);

        var typeIsSome = await commentProvider.GetUserDefinedTypeComments("comment_test_udt_1", TestContext.CurrentContext.CancellationToken).IsSome;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsSome, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(2));
        }
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypeMissing_IssuesNoCommentQuery()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var commentProvider = new PostgreSqlUserDefinedTypeCommentProvider(countingConnectionFactory, IdentifierDefaults, IdentifierResolver);

        var typeIsNone = await commentProvider.GetUserDefinedTypeComments("type_that_doesnt_exist", TestContext.CurrentContext.CancellationToken).IsNone;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeIsNone, Is.True);
            Assert.That(countingConnectionFactory.QueryCount, Is.EqualTo(1));
        }
    }

    [Test]
    public async Task EnumerateAllUserDefinedTypeComments_WhenEnumerated_ContainsTypeComments()
    {
        var hasTypeComments = await TypeCommentProvider.EnumerateAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken).AnyAsync();

        Assert.That(hasTypeComments, Is.True);
    }

    [Test]
    public async Task EnumerateAllUserDefinedTypeComments_WhenEnumerated_ContainsTestTypeComment()
    {
        var containsTestType = await TypeCommentProvider.EnumerateAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken)
            .AnyAsync(c => string.Equals(c.TypeName.LocalName, "comment_test_udt_2", StringComparison.Ordinal));

        Assert.That(containsTestType, Is.True);
    }

    [Test]
    public async Task GetAllUserDefinedTypeComments_WhenRetrieved_ContainsTypeComments()
    {
        var typeComments = await TypeCommentProvider.GetAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken);

        Assert.That(typeComments, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllUserDefinedTypeComments_WhenRetrieved_ContainsTestTypeComments()
    {
        var typeComments = await TypeCommentProvider.GetAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken);
        var typeNames = typeComments.Select(static c => c.TypeName.LocalName).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(typeNames, Does.Contain("comment_test_udt_1"));
            Assert.That(typeNames, Does.Contain("comment_test_udt_2"));
            Assert.That(typeNames, Does.Contain("comment_test_udt_3"));
            Assert.That(typeNames, Does.Contain("comment_test_udt_4"));
        }
    }

    [Test]
    public async Task GetAllUserDefinedTypeComments_WhenRetrieved_ExcludesTableRowTypes()
    {
        var typeComments = await TypeCommentProvider.GetAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken);
        var containsTableRowType = typeComments.Any(static c => string.Equals(c.TypeName.LocalName, "comment_test_udt_table_1", StringComparison.Ordinal));

        Assert.That(containsTableRowType, Is.False);
    }

    [Test]
    public async Task GetAllUserDefinedTypeComments_WhenRetrieved_QualifiesTypeNames()
    {
        var expectedTypeName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "comment_test_udt_2");

        var typeComments = await TypeCommentProvider.GetAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken);
        var comments = typeComments.First(static c => string.Equals(c.TypeName.LocalName, "comment_test_udt_2", StringComparison.Ordinal));

        Assert.That(comments.TypeName, Is.EqualTo(expectedTypeName));
    }

    [Test]
    public async Task GetAllUserDefinedTypeComments_WhenRetrieved_MatchesSingleLoadedComment()
    {
        var typeComments = await TypeCommentProvider.GetAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken);
        var bulkComments = typeComments.First(static c => string.Equals(c.TypeName.LocalName, "comment_test_udt_2", StringComparison.Ordinal));
        var singleComments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bulkComments.TypeName, Is.EqualTo(singleComments.TypeName));
            Assert.That(bulkComments.Comment.UnwrapSome(), Is.EqualTo(singleComments.Comment.UnwrapSome()));
        }
    }

    [Test]
    public async Task EnumerateAllUserDefinedTypeComments_WhenEnumerated_MatchesGetAllUserDefinedTypeComments()
    {
        var enumeratedNames = await TypeCommentProvider.EnumerateAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken)
            .Select(static c => c.TypeName.LocalName)
            .ToListAsync();
        var retrievedComments = await TypeCommentProvider.GetAllUserDefinedTypeComments(TestContext.CurrentContext.CancellationToken);

        Assert.That(enumeratedNames, Is.EqualTo(retrievedComments.Select(static c => c.TypeName.LocalName).ToList()));
    }
}
