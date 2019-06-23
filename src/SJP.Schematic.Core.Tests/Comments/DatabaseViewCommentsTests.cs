using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class DatabaseViewCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseViewComments(null, Option<string>.None, Empty.CommentLookup));
        }

        [Test]
        public static void Ctor_GivenNullColumnComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseViewComments("test_view", Option<string>.None, null));
        }

        [Test]
        public static void Ctor_GivenValidNameAndComments_DoesNotThrow()
        {
            _ = new DatabaseViewComments("test_view", Option<string>.None, Empty.CommentLookup);
            Assert.Pass();
        }

        [Test]
        public static void ViewName_PropertyGet_EqualsCtorArg()
        {
            Identifier viewName = "test_view";
            var comments = new DatabaseViewComments(viewName, Option<string>.None, Empty.CommentLookup);

            Assert.AreEqual(viewName, comments.ViewName);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new DatabaseViewComments("test_view", Option<string>.None, Empty.CommentLookup);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new DatabaseViewComments("test_view", commentArg, Empty.CommentLookup);

            Assert.AreEqual(commentText, comments.Comment.UnwrapSome());
        }

        [Test]
        public static void ColumnComments_PropertyGetWhenCtorGivenEmptyDictionary_IsEmpty()
        {
            var comments = new DatabaseViewComments("test_view", Option<string>.None, Empty.CommentLookup);

            var count = comments.ColumnComments.Count;

            Assert.Zero(count);
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

            Assert.Multiple(() =>
            {
                Assert.AreEqual(columnComments.Count, propColumnComments.Count);

                var seqEqual = columnComments.Keys.SequenceEqual(propColumnComments.Keys);
                Assert.IsTrue(seqEqual);
            });
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
                Assert.AreEqual(columnComments.Count, propColumnComments.Count);

                Assert.AreEqual(columnComments["test_column_1"].IsNone, propColumnComments["test_column_1"].IsNone);
                Assert.AreEqual(columnComments["test_column_2"].IsNone, propColumnComments["test_column_2"].IsNone);
                Assert.AreEqual(columnComments["test_column_3"].IsNone, propColumnComments["test_column_3"].IsNone);

                Assert.AreEqual(columnComments["test_column_2"].UnwrapSome(), propColumnComments["test_column_2"].UnwrapSome());
                Assert.AreEqual(columnComments["test_column_3"].UnwrapSome(), propColumnComments["test_column_3"].UnwrapSome());
            });
        }
    }
}
