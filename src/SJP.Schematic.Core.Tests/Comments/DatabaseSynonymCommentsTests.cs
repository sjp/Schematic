using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class DatabaseSynonymCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSynonymComments(null, Option<string>.None));
        }

        [Test]
        public static void Ctor_GivenValidName_DoesNotThrow()
        {
            _ = new DatabaseSynonymComments("test_synonym", Option<string>.None);
            Assert.Pass();
        }

        [Test]
        public static void SynonymName_PropertyGet_EqualsCtorArg()
        {
            Identifier synonymName = "test_synonym";
            var comments = new DatabaseSynonymComments(synonymName, Option<string>.None);

            Assert.AreEqual(synonymName, comments.SynonymName);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new DatabaseSynonymComments("test_synonym", Option<string>.None);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new DatabaseSynonymComments("test_synonym", commentArg);

            Assert.AreEqual(commentText, comments.Comment.UnwrapSome());
        }
    }
}
