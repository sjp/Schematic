using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.SqlServer.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.SqlServer.Tests.Integration.Comments;

internal sealed class SqlServerUserDefinedTypeCommentProviderTests : SqlServerTest
{
    private IDatabaseUserDefinedTypeCommentProvider TypeCommentProvider => new SqlServerUserDefinedTypeCommentProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await ExecuteBatchAsync(
            "create type comment_test_udt_1 from varchar(50)",
            "create type comment_test_udt_2 from int",
            "create type comment_test_udt_3 as table (test_column int)"
        );

        await AddCommentForType("This is a test alias type.", "dbo", "comment_test_udt_2");
        await AddCommentForType("This is a test table type.", "dbo", "comment_test_udt_3");
    }

    [OneTimeTearDown]
    public Task CleanUp() => ExecuteBatchAsync(
        "drop type comment_test_udt_1",
        "drop type comment_test_udt_2",
        "drop type comment_test_udt_3");

    private Task AddCommentForType(string comment, string schemaName, string typeName)
    {
        const string querySql = @"
EXEC sys.sp_addextendedproperty @name = N'MS_Description',
  @value = @Comment,
  @level0type = N'SCHEMA',
  @level0name = @SchemaName,
  @level1type = N'TYPE',
  @level1name = @TypeName";
        return DbConnection.ExecuteAsync(querySql, new { Comment = comment, SchemaName = schemaName, TypeName = typeName }, TestContext.CurrentContext.CancellationToken);
    }

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

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypeMissing_ReturnsNone()
    {
        var typeIsNone = await TypeCommentProvider.GetUserDefinedTypeComments("type_that_doesnt_exist", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    // sys.types holds the built-in types too, but name resolution keeps to is_user_defined = 1.
    [Test]
    public async Task GetUserDefinedTypeComments_GivenBuiltInTypeName_ReturnsNone()
    {
        var typeName = new Identifier("sys", "varchar");

        var typeIsNone = await TypeCommentProvider.GetUserDefinedTypeComments(typeName, TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(typeIsNone, Is.True);
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypeMissingComment_ReturnsNoneComment()
    {
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenAliasTypeContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test alias type.";
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetUserDefinedTypeComments_WhenTableTypeContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test table type.";
        var comments = await TypeCommentProvider.GetUserDefinedTypeComments("comment_test_udt_3", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(expectedComment));
    }

    // A name is resolved before its comment is read, so a hit costs both queries and a miss stops
    // after the name query.
    [Test]
    public async Task GetUserDefinedTypeComments_WhenTypePresent_IssuesResolutionAndCommentQueries()
    {
        var countingConnectionFactory = new CountingDbConnectionFactory(DbConnection);
        var commentProvider = new SqlServerUserDefinedTypeCommentProvider(countingConnectionFactory, IdentifierDefaults);

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
        var commentProvider = new SqlServerUserDefinedTypeCommentProvider(countingConnectionFactory, IdentifierDefaults);

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
        }
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
