using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyDatabaseViewCommentProviderTests
{
    [Test]
    public static void GetViewComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseViewCommentProvider();
        Assert.That(() => provider.GetViewComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetViewComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseViewCommentProvider();
        var comment = provider.GetViewComments("test_view");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllViewComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyDatabaseViewCommentProvider();
        var hasComments = await provider.EnumerateAllViewComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllViewComments2_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyDatabaseViewCommentProvider();
        var comments = await provider.GetAllViewComments2().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
    }
}