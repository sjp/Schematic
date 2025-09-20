using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class EmptyRelationalDatabaseCommentProviderTests
{
    private static IIdentifierDefaults IdentifierDefaults => new IdentifierDefaults("a", "b", "c");

    [Test]
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
    public static async Task EnumerateAllTableComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.EnumerateAllTableComments().AnyAsync().ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllTableComments2_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comments = await provider.GetAllTableComments2().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
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
    public static async Task EnumerateAllViewComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.EnumerateAllViewComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllViewComments2_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comments = await provider.GetAllViewComments2().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
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
    public static async Task EnumerateAllSynonymComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.EnumerateAllSynonymComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllSynonymComments2_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comments = await provider.GetAllSynonymComments2().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
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
    public static async Task EnumerateAllSequenceComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.EnumerateAllSequenceComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllSequenceComments2_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comments = await provider.GetAllSequenceComments2().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
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
    public static async Task EnumerateAllRoutineComments_WhenInvoked_DoesNotEnumerateAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var hasComments = await provider.EnumerateAllRoutineComments()
            .AnyAsync()
            .ConfigureAwait(false);

        Assert.That(hasComments, Is.False);
    }

    [Test]
    public static async Task GetAllRoutineComments2_WhenInvoked_DoesNotContainAnyValues()
    {
        var provider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var comments = await provider.GetAllRoutineComments2().ConfigureAwait(false);

        Assert.That(comments, Is.Empty);
    }
}