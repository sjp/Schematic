using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class DatabaseUserDefinedTypeCommentsTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseUserDefinedTypeComments(null, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void TypeName_PropertyGet_EqualsCtorArg()
    {
        Identifier typeName = "test_type";
        var comments = new DatabaseUserDefinedTypeComments(typeName, Option<string>.None);

        Assert.That(comments.TypeName, Is.EqualTo(typeName));
    }

    [Test]
    public static void Comment_PropertyGetGivenNoneCtorArg_IsNone()
    {
        var comments = new DatabaseUserDefinedTypeComments("test_type", Option<string>.None);

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public static void Comment_PropertyGetGivenValidCommentArg_MatchesCommentArg()
    {
        const string commentText = "this is a test comment";
        var comments = new DatabaseUserDefinedTypeComments("test_type", Option<string>.Some(commentText));

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
    }
}
