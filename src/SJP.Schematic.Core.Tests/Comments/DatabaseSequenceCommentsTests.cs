using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class DatabaseSequenceCommentsTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseSequenceComments(null, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenValidName_DoesNotThrow()
    {
        Assert.That(() => new DatabaseSequenceComments("test_sequence", Option<string>.None), Throws.Nothing);
    }

    [Test]
    public static void SequenceName_PropertyGet_EqualsCtorArg()
    {
        Identifier sequenceName = "test_sequence";
        var comments = new DatabaseSequenceComments(sequenceName, Option<string>.None);

        Assert.That(comments.SequenceName, Is.EqualTo(sequenceName));
    }

    [Test]
    public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
    {
        var comments = new DatabaseSequenceComments("test_sequence", Option<string>.None);

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
    {
        const string commentText = "this is a test comment";
        var commentArg = Option<string>.Some(commentText);
        var comments = new DatabaseSequenceComments("test_sequence", commentArg);

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
    }
}
