using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Mapping.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping.Comments;

internal static class DatabaseUserDefinedTypeCommentsMapperTests
{
    [Test]
    public static void Map_GivenCommentedType_RoundTripsToEquivalentComments()
    {
        var mapper = new DatabaseUserDefinedTypeCommentsMapper();
        var comments = new DatabaseUserDefinedTypeComments(
            Identifier.CreateQualifiedIdentifier("test_schema", "test_type"),
            Option<string>.Some("a type comment"));

        var result = mapper.Map(mapper.Map(comments));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TypeName, Is.EqualTo(comments.TypeName));
            Assert.That(result.Comment.UnwrapSome(), Is.EqualTo("a type comment"));
        }
    }

    [Test]
    public static void Map_GivenUncommentedType_RoundTripsToNoComment()
    {
        var mapper = new DatabaseUserDefinedTypeCommentsMapper();
        var comments = new DatabaseUserDefinedTypeComments("test_type", Option<string>.None);

        var result = mapper.Map(mapper.Map(comments));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.TypeName, Is.EqualTo(comments.TypeName));
            Assert.That(result.Comment, OptionIs.None);
        }
    }
}
