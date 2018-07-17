using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.PostgreSql.Tests
{
    [TestFixture]
    internal static class PostgreSqlDatabaseKeyTests
    {
        [Test]
        public static void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseKey(null, keyName, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseKey(table, null, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentException>(() => new PostgreSqlDatabaseKey(table, keyName, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseKey(table, keyName, keyType, null));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Array.Empty<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseKey(table, keyName, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new PostgreSqlDatabaseKey(table, keyName, keyType, columns));
        }

        [Test]
        public static void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new PostgreSqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(table, key.Table);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new PostgreSqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(keyName, key.Name);
        }

        [Test]
        public static void KeyType_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new PostgreSqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(keyType, key.KeyType);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new PostgreSqlDatabaseKey(table, keyName, keyType, columns);

            Assert.AreEqual(columns, key.Columns);
        }

        [Test]
        public static void IsEnabled_GivenRegularObject_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new PostgreSqlDatabaseKey(table, keyName, keyType, columns);

            Assert.IsTrue(key.IsEnabled);
        }
    }
}
