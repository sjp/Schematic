using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Oracle.Tests
{
    [TestFixture]
    internal static class OracleDatabaseKeyTests
    {
        [Test]
        public static void Ctor_GivenNullName_ThrowsArgumentNullException()
        {
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseKey(null, keyType, columns, enabled));
        }

        [Test]
        public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            Assert.Throws<ArgumentException>(() => new OracleDatabaseKey(keyName, keyType, columns, enabled));
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseKey(keyName, keyType, null, enabled));
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Array.Empty<IDatabaseColumn>();
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseKey(keyName, keyType, columns, enabled));
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };
            const bool enabled = true;

            Assert.Throws<ArgumentNullException>(() => new OracleDatabaseKey(keyName, keyType, columns, enabled));
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

            Assert.AreEqual(keyName, key.Name);
        }

        [Test]
        public static void KeyType_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

            Assert.AreEqual(keyType, key.KeyType);
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

            Assert.AreEqual(columns, key.Columns);
        }

        [Test]
        public static void IsEnabled_GivenTrueCtorArgAndPropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

            Assert.AreEqual(enabled, key.IsEnabled);
        }

        [Test]
        public static void IsEnabled_GivenFalseCtorArgAndPropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = false;

            var key = new OracleDatabaseKey(keyName, keyType, columns, enabled);

            Assert.AreEqual(enabled, key.IsEnabled);
        }
    }
}
