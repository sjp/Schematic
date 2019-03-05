using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class DatabaseSequenceCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSequenceComments(null, Option<string>.None));
        }

        [Test]
        public static void Ctor_GivenValidName_DoesNotThrow()
        {
            _ = new DatabaseSequenceComments("test_sequence", Option<string>.None);
            Assert.Pass();
        }

        [Test]
        public static void SequenceName_PropertyGet_EqualsCtorArg()
        {
            Identifier sequenceName = "test_sequence";
            var comments = new DatabaseSequenceComments(sequenceName, Option<string>.None);

            Assert.AreEqual(sequenceName, comments.SequenceName);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new DatabaseSequenceComments("test_sequence", Option<string>.None);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new DatabaseSequenceComments("test_sequence", commentArg);

            Assert.AreEqual(commentText, comments.Comment.UnwrapSome());
        }
    }
}
