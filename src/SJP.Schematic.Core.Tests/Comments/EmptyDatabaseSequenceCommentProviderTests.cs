using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyDatabaseSequenceCommentProviderTests
{
    [Test]
    public static void GetSequenceComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyDatabaseSequenceCommentProvider();
        Assert.That(() => provider.GetSequenceComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSequenceComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyDatabaseSequenceCommentProvider();
        var comment = provider.GetSequenceComments("test_sequence");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task GetAllSequenceComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyDatabaseSequenceCommentProvider();
        var hasComments = await provider.GetAllSequenceComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }
}
