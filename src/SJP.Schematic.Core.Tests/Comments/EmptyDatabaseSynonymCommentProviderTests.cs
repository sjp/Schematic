using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyDatabaseSynonymCommentProviderTests
{
    [Test]
    public static void GetSynonymComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseSynonymCommentProvider();
        Assert.That(() => provider.GetSynonymComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSynonymComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseSynonymCommentProvider();
        var comment = provider.GetSynonymComments("test_synonym");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task EnumerateAllSynonymComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyDatabaseSynonymCommentProvider();
        var hasComments = await provider.EnumerateAllSynonymComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllSynonymComments_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyDatabaseSynonymCommentProvider();
        var comments = await provider.GetAllSynonymComments().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
    }
}