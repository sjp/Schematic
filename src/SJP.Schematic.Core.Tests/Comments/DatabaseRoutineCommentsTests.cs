using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Comments
{
    [TestFixture]
    internal static class DatabaseRoutineCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseRoutineComments(null, Option<string>.None));
        }

        [Test]
        public static void Ctor_GivenValidName_DoesNotThrow()
        {
            _ = new DatabaseRoutineComments("test_routine", Option<string>.None);
            Assert.Pass();
        }

        [Test]
        public static void RoutineName_PropertyGet_EqualsCtorArg()
        {
            Identifier routineName = "test_routine";
            var comments = new DatabaseRoutineComments(routineName, Option<string>.None);

            Assert.AreEqual(routineName, comments.RoutineName);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenNone_IsNone()
        {
            var comments = new DatabaseRoutineComments("test_routine", Option<string>.None);

            Assert.IsTrue(comments.Comment.IsNone);
        }

        [Test]
        public static void Comment_PropertyGetWhenCtorGivenValidCommentValue_MatchesCommentValue()
        {
            const string commentText = "this is a test comment";
            var commentArg = Option<string>.Some(commentText);
            var comments = new DatabaseRoutineComments("test_routine", commentArg);

            Assert.AreEqual(commentText, comments.Comment.UnwrapSome());
        }
    }
}
