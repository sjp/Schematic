using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyRelationalDatabaseCommentProviderTests
{
    private static IIdentifierDefaults IdentifierDefaults => new IdentifierDefaults("a", "b", "c");

    public static void Ctor_GivenNullIdentifierDefaults_ThrowsArgumentNullException()
    {
        Assert.That(() => new EmptyRelationalDatabaseCommentProvider(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetTableComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        Assert.That(() => provider.GetTableComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetTableComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comment = provider.GetTableComments("test_table");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task GetAllTableComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.GetAllTableComments().AnyAsync().ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static void GetViewComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        Assert.That(() => provider.GetViewComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetViewComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comment = provider.GetViewComments("test_view");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task GetAllViewComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.GetAllViewComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static void GetSynonymComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        Assert.That(() => provider.GetSynonymComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSynonymComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comment = provider.GetSynonymComments("test_synonym");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task GetAllSynonymComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.GetAllSynonymComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static void GetSequenceComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        Assert.That(() => provider.GetSequenceComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetSequenceComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comment = provider.GetSequenceComments("test_sequence");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task GetAllSequenceComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.GetAllSequenceComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static void GetRoutineComments_GivenNullName_ThrowsArgumentNullException()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        Assert.That(() => provider.GetRoutineComments(null), Throws.ArgumentNullException);
    }

    [Test]
    public static async Task GetRoutineComments_GivenValidName_ReturnsNone()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comment = provider.GetRoutineComments("test_routine");
        var isNone = await comment.IsNone.ConfigureAwait(false);

        Assert.That(isNone, Is.True);
    }

    [Test]
    public static async Task GetAllRoutineComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.GetAllRoutineComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }
}