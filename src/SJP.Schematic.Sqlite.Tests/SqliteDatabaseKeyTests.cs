using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Sqlite.Tests
{
    [TestFixture]
    internal static class SqliteDatabaseKeyTests
    {
        [Test]
        public static void Ctor_GivenNullName_DoesNotThrowArgumentNullException()
        {
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.DoesNotThrow(() => new SqliteDatabaseKey(null, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.Throws<ArgumentException>(() => new SqliteDatabaseKey(keyName, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseKey(keyName, keyType, null));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Array.Empty<IDatabaseColumn>();

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseKey(keyName, keyType, columns));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };

            Assert.Throws<ArgumentNullException>(() => new SqliteDatabaseKey(keyName, keyType, columns));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new SqliteDatabaseKey(keyName, keyType, columns);

            Assert.AreEqual(keyName, key.Name);
        }

        [Test]
        public static void KeyType_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new SqliteDatabaseKey(keyName, keyType, columns);

            Assert.AreEqual(keyType, key.KeyType);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new SqliteDatabaseKey(keyName, keyType, columns);

            Assert.AreEqual(columns, key.Columns);
        }

        [Test]
        public static void IsEnabled_PropertyGet_ReturnsTrue()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new SqliteDatabaseKey(keyName, keyType, columns);

            Assert.AreEqual(enabled, key.IsEnabled);
        }
    }
}
