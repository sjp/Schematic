using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.PostgreSql.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.PostgreSql.Tests.Integration.Comments;

internal sealed class PostgreSqlSchemaCommentProviderTests : PostgreSqlTest
{
    private IDatabaseSchemaCommentProvider SchemaCommentProvider => new PostgreSqlSchemaCommentProvider(DbConnection, IdentifierDefaults);

    [OneTimeSetUp]
    public async Task Init()
    {
        await ExecuteBatchAsync(
            "create schema comment_test_schema_1",
            "create schema comment_test_schema_2",
            "comment on schema comment_test_schema_2 is 'This is a test schema.'"
        );
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await ExecuteBatchAsync(
            "drop schema comment_test_schema_1",
            "drop schema comment_test_schema_2"
        );
    }

    [Test]
    public async Task GetSchemaComments_WhenSchemaPresent_ReturnsSchemaComment()
    {
        var schemaIsSome = await SchemaCommentProvider.GetSchemaComments("comment_test_schema_1", TestContext.CurrentContext.CancellationToken).IsSome;

        Assert.That(schemaIsSome, Is.True);
    }

    [Test]
    public async Task GetSchemaComments_WhenSchemaPresent_ReturnsSchemaWithCorrectName()
    {
        const string schemaName = "comment_test_schema_1";
        var comments = await SchemaCommentProvider.GetSchemaComments(schemaName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.SchemaName.LocalName, Is.EqualTo(schemaName));
    }

    // A schema does not sit inside another schema, so -- unlike every other comment provider --
    // the name is never qualified with a server, database or parent schema.
    [Test]
    public async Task GetSchemaComments_WhenSchemaPresent_ReturnsLocalNameOnly()
    {
        var comments = await SchemaCommentProvider.GetSchemaComments("comment_test_schema_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(comments.SchemaName.Server, Is.Null);
            Assert.That(comments.SchemaName.Database, Is.Null);
            Assert.That(comments.SchemaName.Schema, Is.Null);
        }
    }

    // Only the local name is read from the given identifier, so the leading parts are ignored
    // rather than treated as a schema that must also match.
    [Test]
    public async Task GetSchemaComments_GivenQualifiedName_ResolvesOnLocalNameOnly()
    {
        var schemaName = new Identifier("A", "B", "some_other_schema", "comment_test_schema_1");
        var expectedSchemaName = new Identifier("comment_test_schema_1");

        var comments = await SchemaCommentProvider.GetSchemaComments(schemaName, TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.SchemaName, Is.EqualTo(expectedSchemaName));
    }

    // PostgreSQL folds an unquoted identifier to lower case and this provider has no identifier
    // resolver, so a name given in the wrong case is not the name of an existing schema.
    [Test]
    public async Task GetSchemaComments_GivenNameInWrongCase_ReturnsNone()
    {
        var schemaIsNone = await SchemaCommentProvider.GetSchemaComments("COMMENT_TEST_SCHEMA_1", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(schemaIsNone, Is.True);
    }

    [Test]
    public async Task GetSchemaComments_WhenSchemaMissing_ReturnsNone()
    {
        var schemaIsNone = await SchemaCommentProvider.GetSchemaComments("schema_that_doesnt_exist", TestContext.CurrentContext.CancellationToken).IsNone;

        Assert.That(schemaIsNone, Is.True);
    }

    // The schema exists, so a result is returned -- the comment within it is what is absent.
    [Test]
    public async Task GetSchemaComments_WhenSchemaMissingComment_ReturnsNoneComment()
    {
        var comments = await SchemaCommentProvider.GetSchemaComments("comment_test_schema_1", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public async Task GetSchemaComments_WhenSchemaContainsComment_ReturnsExpectedValue()
    {
        const string expectedComment = "This is a test schema.";
        var comments = await SchemaCommentProvider.GetSchemaComments("comment_test_schema_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(expectedComment));
    }

    [Test]
    public async Task GetSchemaComments_WhenSchemaIsSystemSchema_ReturnsComments()
    {
        var schemaIsSome = await SchemaCommentProvider.GetSchemaComments("pg_catalog", TestContext.CurrentContext.CancellationToken).IsSome;

        Assert.That(schemaIsSome, Is.True);
    }

    [Test]
    public async Task EnumerateAllSchemaComments_WhenEnumerated_ContainsSchemaComments()
    {
        var hasSchemaComments = await SchemaCommentProvider.EnumerateAllSchemaComments(TestContext.CurrentContext.CancellationToken).AnyAsync();

        Assert.That(hasSchemaComments, Is.True);
    }

    [Test]
    public async Task EnumerateAllSchemaComments_WhenEnumerated_ContainsTestSchemaComment()
    {
        var containsTestSchema = await SchemaCommentProvider.EnumerateAllSchemaComments(TestContext.CurrentContext.CancellationToken)
            .AnyAsync(c => string.Equals(c.SchemaName.LocalName, "comment_test_schema_2", StringComparison.Ordinal));

        Assert.That(containsTestSchema, Is.True);
    }

    [Test]
    public async Task GetAllSchemaComments_WhenRetrieved_ContainsSchemaComments()
    {
        var schemaComments = await SchemaCommentProvider.GetAllSchemaComments(TestContext.CurrentContext.CancellationToken);

        Assert.That(schemaComments, Is.Not.Empty);
    }

    [Test]
    public async Task GetAllSchemaComments_WhenRetrieved_ContainsTestSchemaComments()
    {
        var schemaComments = await SchemaCommentProvider.GetAllSchemaComments(TestContext.CurrentContext.CancellationToken);
        var schemaNames = schemaComments.Select(static c => c.SchemaName.LocalName).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(schemaNames, Does.Contain("comment_test_schema_1"));
            Assert.That(schemaNames, Does.Contain("comment_test_schema_2"));
        }
    }

    // Matching the schema provider, which reports system schemas and flags them rather than
    // filtering them out.
    [Test]
    public async Task GetAllSchemaComments_WhenRetrieved_ContainsSystemSchemas()
    {
        var schemaComments = await SchemaCommentProvider.GetAllSchemaComments(TestContext.CurrentContext.CancellationToken);
        var schemaNames = schemaComments.Select(static c => c.SchemaName.LocalName).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(schemaNames, Does.Contain("pg_catalog"));
            Assert.That(schemaNames, Does.Contain("information_schema"));
        }
    }

    [Test]
    public async Task GetAllSchemaComments_WhenRetrieved_IsOrderedByName()
    {
        var schemaComments = await SchemaCommentProvider.GetAllSchemaComments(TestContext.CurrentContext.CancellationToken);
        var schemaNames = schemaComments.Select(static c => c.SchemaName.LocalName).ToList();

        Assert.That(schemaNames, Is.Ordered.Using<string>(StringComparer.Ordinal));
    }

    [Test]
    public async Task GetAllSchemaComments_WhenRetrieved_MatchesSingleLoadedComment()
    {
        var schemaComments = await SchemaCommentProvider.GetAllSchemaComments(TestContext.CurrentContext.CancellationToken);
        var bulkComments = schemaComments.First(static c => string.Equals(c.SchemaName.LocalName, "comment_test_schema_2", StringComparison.Ordinal));
        var singleComments = await SchemaCommentProvider.GetSchemaComments("comment_test_schema_2", TestContext.CurrentContext.CancellationToken).UnwrapSomeAsync();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(bulkComments.SchemaName, Is.EqualTo(singleComments.SchemaName));
            Assert.That(bulkComments.Comment.UnwrapSome(), Is.EqualTo(singleComments.Comment.UnwrapSome()));
        }
    }

    [Test]
    public async Task EnumerateAllSchemaComments_WhenEnumerated_MatchesGetAllSchemaComments()
    {
        var enumeratedNames = await SchemaCommentProvider.EnumerateAllSchemaComments(TestContext.CurrentContext.CancellationToken)
            .Select(static c => c.SchemaName.LocalName)
            .ToListAsync();
        var retrievedComments = await SchemaCommentProvider.GetAllSchemaComments(TestContext.CurrentContext.CancellationToken);

        Assert.That(enumeratedNames, Is.EqualTo(retrievedComments.Select(static c => c.SchemaName.LocalName).ToList()));
    }
}
