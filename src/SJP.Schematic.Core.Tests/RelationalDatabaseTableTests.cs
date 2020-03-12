using System;
using LanguageExt;
using Moq;
using NUnit.Framework;

namespace SJP.Schematic.Core.Tests
{
    [TestFixture]
    internal static class RelationalDatabaseTableTests
    {
        [Test]
        public static void Ctor_GivenNullTableName_ThrowsArgumentNullException()
        {
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(null, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullColumns_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, null, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenColumnsWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new IDatabaseColumn[] { null };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullUniqueKeys_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, null, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenUniqueKeysWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = new IDatabaseKey[] { null };
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullParentKeys_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, null, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenParentKeysWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = new IDatabaseRelationalKey[] { null };
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullChildKeys_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, null, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenChildKeysWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = new IDatabaseRelationalKey[] { null };
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullIndexes_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, null, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenIndexesWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = new IDatabaseIndex[] { null };
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullChecks_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, null, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenChecksWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = new IDatabaseCheckConstraint[] { null };
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullTriggers_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, null), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenTriggersWithNullValue_ThrowsArgumentNullException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = new IDatabaseTrigger[] { null };

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenPrimaryKeyWithNonPrimaryKeyType_ThrowsArgumentException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var key = new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Foreign, columns, true);
            var primaryKey = Option<IDatabaseKey>.Some(key);
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentException);
        }

        [Test]
        public static void Ctor_GivenUniqueKeyWithNonUniqueKeyType_ThrowsArgumentException()
        {
            Identifier tableName = "test_table";
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var key = new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Foreign, columns, true);
            var uniqueKeys = new[] { key };
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            Assert.That(() => new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers), Throws.ArgumentException);
        }

        [TestCase("", "test_table", "Table: test_table")]
        [TestCase("test_schema", "test_table", "Table: test_schema.test_table")]
        public static void ToString_WhenInvoked_ReturnsExpectedString(string schema, string localName, string expectedOutput)
        {
            var tableName = Identifier.CreateQualifiedIdentifier(schema, localName);
            var columns = new[] { Mock.Of<IDatabaseColumn>() };
            var primaryKey = Option<IDatabaseKey>.None;
            var uniqueKeys = Array.Empty<IDatabaseKey>();
            var parentKeys = Array.Empty<IDatabaseRelationalKey>();
            var childKeys = Array.Empty<IDatabaseRelationalKey>();
            var indexes = Array.Empty<IDatabaseIndex>();
            var checks = Array.Empty<IDatabaseCheckConstraint>();
            var triggers = Array.Empty<IDatabaseTrigger>();

            var table = new RelationalDatabaseTable(tableName, columns, primaryKey, uniqueKeys, parentKeys, childKeys, indexes, checks, triggers);

            var result = table.ToString();

            Assert.That(result, Is.EqualTo(expectedOutput));
        }
    }
}
