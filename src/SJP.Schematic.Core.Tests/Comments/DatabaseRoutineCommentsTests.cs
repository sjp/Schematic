using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class DatabaseRoutineCommentsTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseRoutineComments(null, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenValidName_DoesNotThrow()
    {
        Assert.That(() => new DatabaseRoutineComments("test_routine", Option<string>.None), Throws.Nothing);
    }

    [Test]
    public static void RoutineName_PropertyGet_EqualsCtorArg()
    {
        Identifier routineName = "test_routine";
        var comments = new DatabaseRoutineComments(routineName, Option<string>.None);

        Assert.That(comments.RoutineName, Is.EqualTo(routineName));
    }

    [Test]
    public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
    {
        var comments = new DatabaseRoutineComments("test_routine", Option<string>.None);
        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
    {
        const string commentText = "this is a test comment";
        var commentArg = Option<string>.Some(commentText);
        var comments = new DatabaseRoutineComments("test_routine", commentArg);

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
    }
}
