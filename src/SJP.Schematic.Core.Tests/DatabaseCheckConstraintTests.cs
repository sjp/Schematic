using System;
using NUnit.Framework;
using Moq;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal class DatabaseCheckConstraintTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint(null, "test_check", "test_check", true));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint(table, null, "test_check", true));
        }

        [Test]
        public void Ctor_GivenNullDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint(table, "test_check", null, true));
        }

        [Test]
        public void Ctor_GivenEmptyDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint(table, "test_check", string.Empty, true));
        }

        [Test]
        public void Ctor_GivenWhiteSpaceDefinition_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Assert.Throws<ArgumentNullException>(() => new DatabaseCheckConstraint(table, "test_check", "      ", true));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            Identifier tableName = "test_table";
            var table = new Mock<IRelationalDatabaseTable>();
            table.Setup(t => t.Name).Returns(tableName);
            var tableArg = table.Object;

            var check = new DatabaseCheckConstraint(tableArg, "test_check", "test_check", true);

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
            var check = new DatabaseCheckConstraint(table, checkName, "test_check", true);

            Assert.AreEqual(checkName, check.Name);
        }

        [Test]
        public void Definition_PropertyGet_EqualsCtorArg()
        {
            const string checkDefinition = "test_check_definition";
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new DatabaseCheckConstraint(table, "test_check", checkDefinition, true);

            Assert.AreEqual(checkDefinition, check.Definition);
        }

        [Test]
        public void IsEnabled_WhenTrueProvidedInCtor_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new DatabaseCheckConstraint(table, "test_check", "test_check_definition", true);

            Assert.IsTrue(check.IsEnabled);
        }

        [Test]
        public void IsEnabled_WhenFalseProvidedInCtor_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            var check = new DatabaseCheckConstraint(table, "test_check", "test_check_definition", false);

            Assert.IsFalse(check.IsEnabled);
        }
    }
}
