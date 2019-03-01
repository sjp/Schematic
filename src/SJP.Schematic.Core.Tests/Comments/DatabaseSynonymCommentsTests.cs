using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

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
    }
}
