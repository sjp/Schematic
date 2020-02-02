using System;
using NUnit.Framework;
using Moq;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class DatabaseKeyTests
    {
        [Test]
        public static void Ctor_GivenNullName_DoesNotThrowArgumentNullException()
        {
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.That(() => new DatabaseKey(null, keyType, columns, true), Throws.Nothing);
        }

        [Test]
        public static void Ctor_GivenInvalidDatabaseKeyType_ThrowsArgumentException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = (DatabaseKeyType)55;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            Assert.That(() => new DatabaseKey(keyName, keyType, columns, true), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenNullColumnSet_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;

            Assert.That(() => new DatabaseKey(keyName, keyType, null, true), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenEmptyColumnSet_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = Array.Empty<IDatabaseColumn>();

            Assert.That(() => new DatabaseKey(keyName, keyType, columns, true), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenColumnSetContainingNullColumn_ThrowsArgumentNullException()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var columns = new IDatabaseColumn[] { null };

            Assert.That(() => new DatabaseKey(keyName, keyType, columns, true), Throws.ArgumentNullException);
        }

        [Test]
        public static void Name_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Primary;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new DatabaseKey(keyName, keyType, columns, true);

            Assert.That(key.Name.UnwrapSome(), Is.EqualTo(keyName));
        }

        [Test]
        public static void KeyType_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new DatabaseKey(keyName, keyType, columns, true);

            Assert.That(key.KeyType, Is.EqualTo(keyType));
        }

        [Test]
        public static void Columns_PropertyGet_EqualsCtorArg()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };

            var key = new DatabaseKey(keyName, keyType, columns, true);

            Assert.That(key.Columns, Is.EqualTo(columns));
        }

        [Test]
        public static void IsEnabled_WhenGivenTrueInCtor_ReturnsTrue()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = true;

            var key = new DatabaseKey(keyName, keyType, columns, enabled);

            Assert.That(key.IsEnabled, Is.EqualTo(enabled));
        }

        [Test]
        public static void IsEnabled_WhenGivenFalseInCtor_ReturnsFalse()
        {
            Identifier keyName = "test_key";
            const DatabaseKeyType keyType = DatabaseKeyType.Foreign;
            var column = Mock.Of<IDatabaseColumn>();
            var columns = new[] { column };
            const bool enabled = false;

            var key = new DatabaseKey(keyName, keyType, columns, enabled);

            Assert.That(key.IsEnabled, Is.EqualTo(enabled));
        }
    }
}
