using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class DatabaseViewCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.That(() => new DatabaseViewComments(null, Option<string>.None, Empty.CommentLookup), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumnComments_ThrowsArgumentNullException()
        {
            Assert.That(() => new DatabaseViewComments("test_view", Option<string>.None, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenValidNameAndComments_DoesNotThrow()
        {
            Assert.That(() => new DatabaseViewComments("test_view", Option<string>.None, Empty.CommentLookup), Throws.Nothing);
        }

        [Test]
        public static void ViewName_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            var comments = new DatabaseViewComments(viewName, Option<string>.None, Empty.CommentLookup);

            Assert.That(comments.ViewName, Is.EqualTo(viewName));
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new DatabaseViewComments("test_view", Option<string>.None, Empty.CommentLookup);

            Assert.That(comments.Comment, OptionIs.None);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new DatabaseViewComments("test_view", commentArg, Empty.CommentLookup);

            Assert.That(comments.Comment.UnwrapSome(), Is.EqualTo(commentText));
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new DatabaseViewComments("test_view", Option<string>.None, Empty.CommentLookup);

            Assert.That(comments.ColumnComments, Is.Empty);
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenDictionaryWithValues_MatchesKeys()
        {
            var columnNames = new[]
            {
                new Identifier("test_column_1"),
                new Identifier("test_column_2"),
                new Identifier("test_column_3")
            };
            var columnComments = new Dictionary<Identifier, Option<string>>
            {
                [columnNames[0]] = Option<string>.None,
                [columnNames[1]] = Option<string>.None,
                [columnNames[2]] = Option<string>.None
            };

            var comments = new DatabaseViewComments("test_view", Option<string>.None, columnComments);
            var propColumnComments = comments.ColumnComments;

            Assert.That(columnComments.Keys, Is.EqualTo(propColumnComments.Keys));
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenDictionaryWithValues_ContainsExpectedValues()
        {
            var columnNames = new[]
            {
                new Identifier("test_column_1"),
                new Identifier("test_column_2"),
                new Identifier("test_column_3")
            };
            var columnComments = new Dictionary<Identifier, Option<string>>
            {
                [columnNames[0]] = Option<string>.None,
                [columnNames[1]] = Option<string>.Some("test comment for second column"),
                [columnNames[2]] = Option<string>.Some("test comment for third column")
            };

            var comments = new DatabaseViewComments("test_view", Option<string>.None, columnComments);

            var propColumnComments = comments.ColumnComments;

            Assert.Multiple(() =>
            {
                Assert.That(columnComments.Keys, Is.EqualTo(propColumnComments.Keys));

                Assert.That(columnComments["test_column_1"].IsNone, Is.EqualTo(propColumnComments["test_column_1"].IsNone));
                Assert.That(columnComments["test_column_2"].IsNone, Is.EqualTo(propColumnComments["test_column_2"].IsNone));
                Assert.That(columnComments["test_column_3"].IsNone, Is.EqualTo(propColumnComments["test_column_3"].IsNone));

                Assert.That(columnComments["test_column_2"].UnwrapSome(), Is.EqualTo(propColumnComments["test_column_2"].UnwrapSome()));
                Assert.That(columnComments["test_column_3"].UnwrapSome(), Is.EqualTo(propColumnComments["test_column_3"].UnwrapSome()));
            });
        }
    }
}
