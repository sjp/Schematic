using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;
using System.Linq;

namespace SJP.Schematic.SqlServer.Tests
{
    [TestFixture]
    internal class SqlServerDatabaseKeyTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseKey(null, keyName, keyType, columns, enabled));
        }

        [Test]
        public void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseKey(table, null, keyType, columns, enabled));
        }

        [Test]
        public void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled));
        }

        [Test]
        public void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseKey(table, keyName, keyType, null, enabled));
        }

        [Test]
        public void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Enumerable.Empty<IDatabaseColumn>();
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled));
        }

        [Test]
        public void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled);

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
            const bool enabled = true;

            var key = new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled);

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
            const bool enabled = true;

            var key = new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled);

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
            const bool enabled = true;

            var key = new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled);

            Assert.AreEqual(columns, key.Columns);
        }

        [Test]
        public void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled);

            Assert.AreEqual(enabled, key.IsEnabled);
        }

        [Test]
        public void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = false;

            var key = new SqlServerDatabaseKey(table, keyName, keyType, columns, enabled);

            Assert.AreEqual(enabled, key.IsEnabled);
        }
    }
}
