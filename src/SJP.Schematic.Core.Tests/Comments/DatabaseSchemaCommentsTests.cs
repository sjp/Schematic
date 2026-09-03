using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments;

[TestFixture]
internal static class DatabaseSchemaCommentsTests
{
    [Test]
    public static void Ctor_GivenNullName_ThrowsArgumentNullException()
    {
        Assert.That(() => new DatabaseSchemaComments(null, Option<string>.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void SchemaName_PropertyGet_EqualsCtorArg()
    {
        Identifier schemaName = "test_schema";
        var comments = new DatabaseSchemaComments(schemaName, Option<string>.None);

        Assert.That(comments.SchemaName, Is.EqualTo(schemaName));
    }

    [Test]
    public static void Comment_PropertyGetGivenNoneCtorArg_IsNone()
    {
        var comments = new DatabaseSchemaComments("test_schema", Option<string>.None);

        Assert.That(comments.Comment, OptionIs.None);
    }

    [Test]
    public static void Comment_PropertyGetGivenValidCommentArg_MatchesCommentArg()
    {
        const string commentText = "this is a test comment";
        var comments = new DatabaseSchemaComments("test_schema", Option<string>.Some(commentText));

        Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
    }
}
