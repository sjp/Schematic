using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class DatabaseSynonymCommentsTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseSynonymComments(null, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenValidName_DoesNotThrow()
    {
        Assert.That(() => new DatabaseSynonymComments("test_synonym", Option<string>.None), Throws.Nothing);
    }

    [Test]
    public static void SynonymName_PropertyGet_EqualsCtorArg()
    {
        Identifier synonymName = "test_synonym";
        var comments = new DatabaseSynonymComments(synonymName, Option<string>.None);

        Assert.That(comments.SynonymName, Is.EqualTo(synonymName));
    }

    [Test]
    public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
    {
        var comments = new DatabaseSynonymComments("test_synonym", Option<string>.None);

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
    {
        const string commentText = "this is a test comment";
        var commentArg = Option<string>.Some(commentText);
        var comments = new DatabaseSynonymComments("test_synonym", commentArg);

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
    }
}