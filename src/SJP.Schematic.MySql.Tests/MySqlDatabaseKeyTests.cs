using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Linq;

namespace SJP.Schematic.MySql.Tests
{
    [TestFixture]
    internal class MySqlDatabaseKeyTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseKey(null, keyName, keyType, columns));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseKey(table, null, keyType, columns));
        }

        [Test]
        public void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentException>(() => new MySqlDatabaseKey(table, keyName, keyType, columns));
        }

        [Test]
        public void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseKey(table, keyName, keyType, null));
        }

        [Test]
        public void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Enumerable.Empty<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseKey(table, keyName, keyType, columns));
        }

        [Test]
        public void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new MySqlDatabaseKey(table, keyName, keyType, columns));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new MySqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(table, key.Table);
        }

        [Test]
        public void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new MySqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(keyName, key.Name);
        }

        [Test]
        public void KeyType_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new MySqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(keyType, key.KeyType);
        }

        [Test]
        public void Columns_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new MySqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(columns, key.Columns);
        }
    }
}
