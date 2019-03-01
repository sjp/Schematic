using System;
using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{

    [TestFixture]
    internal static class DatabaseViewCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseViewComments(null, Option<string>.None, new Dictionary<Identifier, Option<string>>()));
        }

        [Test]
        public static void Ctor_GivenNullColumnComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseViewComments("test_view", Option<string>.None, null));
        }

        [Test]
        public static void Ctor_GivenValidNameAndComments_DoesNotThrow()
        {
            _ = new DatabaseViewComments("test_view", Option<string>.None, new Dictionary<Identifier, Option<string>>());
            Assert.Pass();
        }
    }
}
