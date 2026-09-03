using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyDatabaseSchemaCommentProviderTests
{
    [Test]
    public static void GetSchemaComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseSchemaCommentProvider();
        Assert.That(() => provider.GetSchemaComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSchemaComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseSchemaCommentProvider();
        var comments = provider.GetSchemaComments("schema_name");
        var commentsAreNone = await comments.IsNone;

        Assert.That(commentsAreNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSchemaComments_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSchemaCommentProvider();
        var hasComments = await provider.EnumerateAllSchemaComments().AnyAsync();

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllSchemaComments_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseSchemaCommentProvider();
        var comments = await provider.GetAllSchemaComments();

        Assert.That(comments, Is.Empty);
    }
}
