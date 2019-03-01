using System;
using System.Collections.Generic;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Comments;

namespace SJP.Schematic.Core.Tests.Comments
{

    [TestFixture]
    internal static class RelationalDatabaseTableCommentsTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                null,
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullColumnComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullCheckComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullUniqueKeyComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullForeignKeyComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullIndexComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null,
                new Dictionary<Identifier, Option<string>>()
            ));
        }

        [Test]
        public static void Ctor_GivenNullTriggerComments_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                null
            ));
        }

        [Test]
        public static void Ctor_GivenNoNullArguments_DoesNotThrow()
        {
            _ = new RelationalDatabaseTableComments(
                "test_table",
                Option<string>.None,
                Option<string>.None,
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>(),
                new Dictionary<Identifier, Option<string>>()
            );
            Assert.Pass();
        }
    }
}
