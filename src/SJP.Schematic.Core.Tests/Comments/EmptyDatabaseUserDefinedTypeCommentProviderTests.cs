using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyDatabaseUserDefinedTypeCommentProviderTests
{
    [Test]
    public static void GetUserDefinedTypeComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseUserDefinedTypeCommentProvider();
        Assert.That(() => provider.GetUserDefinedTypeComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetUserDefinedTypeComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseUserDefinedTypeCommentProvider();
        var comments = provider.GetUserDefinedTypeComments("type_name");
        var commentsAreNone = await comments.IsNone;

        Assert.That(commentsAreNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllUserDefinedTypeComments_WhenEnumerated_ContainsNoValues()
    {
        var provider = new EmptyDatabaseUserDefinedTypeCommentProvider();
        var hasComments = await provider.EnumerateAllUserDefinedTypeComments().AnyAsync();

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllUserDefinedTypeComments_WhenRetrieved_ContainsNoValues()
    {
        var provider = new EmptyDatabaseUserDefinedTypeCommentProvider();
        var comments = await provider.GetAllUserDefinedTypeComments();

        Assert.That(comments, Is.Empty);
    }
}
