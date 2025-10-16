using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyRelationalDatabaseTableCommentProviderTests
{
    [Test]
    public static void GetTableComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseTableCommentProvider();
        Assert.That(() => provider.GetTableComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetTableComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseTableCommentProvider();
        var comment = provider.GetTableComments("test_table");
        var isNone = await comment.IsNone;

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllTableComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseTableCommentProvider();
        var hasComments = await provider.EnumerateAllTableComments().AnyAsync();

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllTableComments_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyRelationalDatabaseTableCommentProvider();
        var comments = await provider.GetAllTableComments();

        Assert.That(comments, Is.Empty);
    }
}