using System;
using NUnit.Framework;
using Moq;
using System.Linq;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal class DatabaseKeyTests
    {
        [Test]
        public void Ctor_GivenNullTable_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentNullException>(() => new DatabaseKey(null, keyName, keyType, columns, true));
        }

        [Test]
        public void Ctor_GivenNullName_DoesNotThrowArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.DoesNotThrow(() => new DatabaseKey(table, null, keyType, columns, true));
        }

        [Test]
        public void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentException>(() => new DatabaseKey(table, keyName, keyType, columns, true));
        }

        [Test]
        public void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;

            Assert.Throws<ArgumentNullException>(() => new DatabaseKey(table, keyName, keyType, null, true));
        }

        [Test]
        public void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Enumerable.Empty<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new DatabaseKey(table, keyName, keyType, columns, true));
        }

        [Test]
        public void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new DatabaseKey(table, keyName, keyType, columns, true));
        }

        [Test]
        public void Table_PropertyGet_EqualsCtorArg()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new DatabaseKey(table, keyName, keyType, columns, true);

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

            var key = new DatabaseKey(table, keyName, keyType, columns, true);

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

            var key = new DatabaseKey(table, keyName, keyType, columns, true);

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

            var key = new DatabaseKey(table, keyName, keyType, columns, true);

            Assert.AreEqual(columns, key.Columns);
        }

        [Test]
        public void IsEnabled_WhenGivenTrueInCtor_ReturnsTrue()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new DatabaseKey(table, keyName, keyType, columns, enabled);

            Assert.AreEqual(enabled, key.IsEnabled);
        }

        [Test]
        public void IsEnabled_WhenGivenFalseInCtor_ReturnsFalse()
        {
            var table = Mock.Of<IRelationalDatabaseTable>();
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = false;

            var key = new DatabaseKey(table, keyName, keyType, columns, enabled);

            Assert.AreEqual(enabled, key.IsEnabled);
        }
    }
}
