using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal class SqliteCheckConstraintTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new SqliteCheckConstraint(null, "test_check", "test_check"));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new SqliteCheckConstraint(table, null, "test_check"));
        }

        [Test]
        public void Ctor_GivenNullLocalName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new SqliteCheckConstraint(table, new SchemaIdentifier("test_schema"), "test_check"));
        }

        [Test]
        public void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new SqliteCheckConstraint(table, "test_check", null));
        }

        [Test]
        public void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new SqliteCheckConstraint(table, "test_check", string.Empty));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new SqliteCheckConstraint(table, "test_check", "      "));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var table = new Mock<IRelationalDatabaseTable>();
            table.Setup(t => t.Name).Returns(tableName);
            var tableArg = table.Object;

            var check = new SqliteCheckConstraint(tableArg, "test_check", "test_check");

            Assert.Multiple(() =>
            {
                Assert.AreEqual(tableName, check.Table.Name);
                Assert.AreSame(tableArg, check.Table);
            });
        }

        [Test]
        public void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier checkName = "test_check";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new SqliteCheckConstraint(table, checkName, "test_check");

            Assert.AreEqual(checkName, check.Name);
        }

        [Test]
        public void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new SqliteCheckConstraint(table, "test_check", checkDefinition);

            Assert.AreEqual(checkDefinition, check.Definition);
        }

        [Test]
        public void IsEnabled_PropertyGet_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new SqliteCheckConstraint(table, "test_check", "test_check_definition");

            Assert.IsTrue(check.IsEnabled);
        }
    }
}
