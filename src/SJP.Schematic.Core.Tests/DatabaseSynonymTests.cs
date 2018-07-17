using System;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseSynonymTests
    {
        [Test]
        public static void Ctor_GivenNullDatabase_ThrowsArgNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseSynonym(null, "test", "test"));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseSynonym(database, null, "test"));
        }

        [Test]
        public static void Ctor_GivenNullTarget_ThrowsArgNullException()
        {
            var database = Mock.Of<IRelationalDatabase>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseSynonym(database, "test", null));
        }

        [Test]
        public static void Database_PropertyGet_ShouldMatchCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var synonym = new DatabaseSynonym(database, "test", "test");

            Assert.AreSame(database, synonym.Database);
        }

        [Test]
        public static void Name_PropertyGet_ShouldEqualCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new DatabaseSynonym(database, synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Name.LocalName);
        }

        [Test]
        public static void Target_PropertyGetGivenNotNullCtorArg_ShouldEqualCtorArg()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string synonymName = "synonym_test_synonym_1";
            var synonym = new DatabaseSynonym(database, synonymName, synonymName);

            Assert.AreEqual(synonymName, synonym.Target.LocalName);
        }

        [Test]
        public static void Name_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            var synonymName = new Identifier("synonym_test_synonym_1");
            var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public static void Name_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            var synonymName = new Identifier("asd", "synonym_test_synonym_1");
            var expectedSynonymName = new Identifier(database.ServerName, database.DatabaseName, "asd", "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public static void Name_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            var synonymName = new Identifier("qwe", "asd", "synonym_test_synonym_1");
            var expectedSynonymName = new Identifier(database.ServerName, "qwe", "asd", "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public static void Name_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Mock.Of<IRelationalDatabase>();
            var synonymName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");
            var expectedSynonymName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");
            const string targetName = "synonym_test_synonym_1";

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedSynonymName, synonym.Name);
        }

        [Test]
        public static void Target_GivenLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("synonym_test_synonym_1");
            var expectedTargetName = new Identifier(database.ServerName, database.DatabaseName, database.DefaultSchema, "synonym_test_synonym_1");

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }

        [Test]
        public static void Target_GivenSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("asd", "synonym_test_synonym_1");
            var expectedTargetName = new Identifier(database.ServerName, database.DatabaseName, "asd", "synonym_test_synonym_1");

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }

        [Test]
        public static void Target_GivenDatabaseAndSchemaAndLocalNameOnlyInCtor_ShouldBeQualifiedCorrectly()
        {
            var databaseMock = new Mock<IRelationalDatabase>();
            databaseMock.Setup(d => d.ServerName).Returns("a");
            databaseMock.Setup(d => d.DatabaseName).Returns("b");
            databaseMock.Setup(d => d.DefaultSchema).Returns("c");
            var database = databaseMock.Object;

            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("qwe", "asd", "synonym_test_synonym_1");
            var expectedTargetName = new Identifier(database.ServerName, "qwe", "asd", "synonym_test_synonym_1");

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }

        [Test]
        public static void Target_GivenFullyQualifiedNameInCtor_ShouldBeQualifiedCorrectly()
        {
            var database = Mock.Of<IRelationalDatabase>();
            const string synonymName = "synonym_test_synonym_1";
            var targetName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");
            var expectedTargetName = new Identifier("qwe", "asd", "zxc", "synonym_test_synonym_1");

            var synonym = new DatabaseSynonym(database, synonymName, targetName);

            Assert.AreEqual(expectedTargetName, synonym.Target);
        }
    }
}
