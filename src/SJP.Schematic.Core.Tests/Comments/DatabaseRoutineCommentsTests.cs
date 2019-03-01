using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

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
    }
}
