using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Serialization.Mapping.Comments;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Serialization.Tests.Mapping.Comments;

internal static class DatabaseSchemaCommentsMapperTests
{
    [Test]
    public static void Map_GivenCommentedSchema_RoundTripsToEquivalentComments()
    {
        var mapper = new DatabaseSchemaCommentsMapper();
        var comments = new DatabaseSchemaComments("test_schema", Option<string>.Some("a schema comment"));

        var result = mapper.Map(mapper.Map(comments));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SchemaName, Is.EqualTo(comments.SchemaName));
            Assert.That(result.Comment.UnwrapSome(), Is.EqualTo("a schema comment"));
        }
    }

    [Test]
    public static void Map_GivenUncommentedSchema_RoundTripsToNoComment()
    {
        var mapper = new DatabaseSchemaCommentsMapper();
        var comments = new DatabaseSchemaComments("test_schema", Option<string>.None);

        var result = mapper.Map(mapper.Map(comments));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result.SchemaName, Is.EqualTo(comments.SchemaName));
            Assert.That(result.Comment, OptionIs.None);
        }
    }
}
