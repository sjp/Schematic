using System;
using System.Data;
using System.Linq;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class RelationalDatabaseTableExtensionsTests
    {
        private static IRelationalDatabaseTable GetMockTable(Identifier tableName)
        {
            var checks = new[]
            {
                new DatabaseCheckConstraint(Option<Identifier>.Some("test_check"), "test_definition_1", true),
                new DatabaseCheckConstraint(Option<Identifier>.None, "test_definition_2", true)
            };

            var testColumnType = new ColumnDataType(
                "integer",
                DataType.Integer,
                "INTEGER",
                typeof(int),
                false,
                10,
                Option<INumericPrecision>.None,
                Option<Identifier>.None
            );
            var columns = new[]
            {
                new DatabaseColumn("test_column_1", testColumnType, false, Option<string>.None, Option<IAutoIncrement>.None),
                new DatabaseColumn("test_column_2", testColumnType, false, Option<string>.None, Option<IAutoIncrement>.None)
            };

            var indexColumns = columns.Select(c => new DatabaseIndexColumn("test", c, IndexColumnOrder.Ascending)).ToList();
            var indexes = new[]
            {
                new DatabaseIndex("test_index_1", true, indexColumns, Array.Empty<IDatabaseColumn>(), false),
                new DatabaseIndex("test_index_2", true, indexColumns, Array.Empty<IDatabaseColumn>(), false)
            };

            var childKeys = new[]
            {
                new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Foreign, columns, true),
                new DatabaseKey(Option<Identifier>.Some("test_fk_2"), DatabaseKeyType.Foreign, columns, true)
            };
            var uniqueKeys = new[]
            {
                new DatabaseKey(Option<Identifier>.None, DatabaseKeyType.Unique, columns, true),
                new DatabaseKey(Option<Identifier>.Some("test_uk_2"), DatabaseKeyType.Unique, columns, true)
            };
            var parentKeys = new[]
            {
                new DatabaseRelationalKey("child_table", childKeys[0], "parent_table", uniqueKeys[0], Rule.Cascade, Rule.Cascade),
                new DatabaseRelationalKey("child_table", childKeys[1], "parent_table", uniqueKeys[1], Rule.Cascade, Rule.Cascade)
            };
            var triggers = new[]
            {
                new DatabaseTrigger("test_trigger_1", "test_definition", TriggerQueryTiming.After, TriggerEvent.Delete, true),
                new DatabaseTrigger("test_trigger_2", "test_definition", TriggerQueryTiming.After, TriggerEvent.Delete, true)
            };

            var tableMock = new Mock<IRelationalDatabaseTable>();
            tableMock.SetupGet(t => t.Name).Returns(tableName);
            tableMock.Setup(t => t.Checks).Returns(checks);
            tableMock.Setup(t => t.Columns).Returns(columns);
            tableMock.Setup(t => t.Indexes).Returns(indexes);
            tableMock.Setup(t => t.ParentKeys).Returns(parentKeys);
            tableMock.Setup(t => t.Triggers).Returns(triggers);
            tableMock.Setup(t => t.UniqueKeys).Returns(uniqueKeys);

            return tableMock.Object;
        }

        [Test]
        public static void GetCheckLookup_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetCheckLookup(null));
        }

        [Test]
        public static void GetCheckLookup_ForIdentifierResolvingOverloadGivenNullTable_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetCheckLookup(null, resolver));
        }

        [Test]
        public static void GetCheckLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
        {
            var table = GetMockTable("test");

            Assert.Throws<ArgumentNullException>(() => table.GetCheckLookup(null));
        }

        [Test]
        public static void GetCheckLookup_GivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_check" };
            var table = GetMockTable("test");

            var checkLookup = table.GetCheckLookup();
            var lookupKeys = checkLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetCheckLookup_ForIdentifierResolvingOverloadGivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_check" };
            var table = GetMockTable("test");

            var checkLookup = table.GetCheckLookup(new VerbatimIdentifierResolutionStrategy());
            var lookupKeys = checkLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetColumnLookup_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetColumnLookup(null));
        }

        [Test]
        public static void GetColumnLookup_ForIdentifierResolvingOverloadGivenNullTable_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetColumnLookup(null, resolver));
        }

        [Test]
        public static void GetColumnLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
        {
            var table = GetMockTable("test");

            Assert.Throws<ArgumentNullException>(() => table.GetColumnLookup(null));
        }

        [Test]
        public static void GetColumnLookup_GivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_column_1", "test_column_2" };
            var table = GetMockTable("test");

            var columnLookup = table.GetColumnLookup();
            var lookupKeys = columnLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetColumnLookup_ForIdentifierResolvingOverloadGivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_column_1", "test_column_2" };
            var table = GetMockTable("test");

            var columnLookup = table.GetColumnLookup(new VerbatimIdentifierResolutionStrategy());
            var lookupKeys = columnLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetIndexLookup_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetIndexLookup(null));
        }

        [Test]
        public static void GetIndexLookup_ForIdentifierResolvingOverloadGivenNullTable_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetIndexLookup(null, resolver));
        }

        [Test]
        public static void GetIndexLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
        {
            var table = GetMockTable("test");

            Assert.Throws<ArgumentNullException>(() => table.GetIndexLookup(null));
        }

        [Test]
        public static void GetIndexLookup_GivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_index_1", "test_index_2" };
            var table = GetMockTable("test");

            var indexLookup = table.GetIndexLookup();
            var lookupKeys = indexLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetIndexLookup_ForIdentifierResolvingOverloadGivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_index_1", "test_index_2" };
            var table = GetMockTable("test");

            var indexLookup = table.GetIndexLookup(new VerbatimIdentifierResolutionStrategy());
            var lookupKeys = indexLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetParentKeyLookup_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetParentKeyLookup(null));
        }

        [Test]
        public static void GetParentKeyLookup_ForIdentifierResolvingOverloadGivenNullTable_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetParentKeyLookup(null, resolver));
        }

        [Test]
        public static void GetParentKeyLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
        {
            var table = GetMockTable("test");

            Assert.Throws<ArgumentNullException>(() => table.GetParentKeyLookup(null));
        }

        [Test]
        public static void GetParentKeyLookup_GivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_fk_2" };
            var table = GetMockTable("test");

            var parentKeyLookup = table.GetParentKeyLookup();
            var lookupKeys = parentKeyLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetParentKeyLookup_ForIdentifierResolvingOverloadGivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_fk_2" };
            var table = GetMockTable("test");

            var parentKeyLookup = table.GetParentKeyLookup(new VerbatimIdentifierResolutionStrategy());
            var lookupKeys = parentKeyLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetTriggerLookup_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetTriggerLookup(null));
        }

        [Test]
        public static void GetTriggerLookup_ForIdentifierResolvingOverloadGivenNullTable_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetTriggerLookup(null, resolver));
        }

        [Test]
        public static void GetTriggerLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
        {
            var table = GetMockTable("test");

            Assert.Throws<ArgumentNullException>(() => table.GetTriggerLookup(null));
        }

        [Test]
        public static void GetTriggerLookup_GivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_trigger_1", "test_trigger_2" };
            var table = GetMockTable("test");

            var triggerLookup = table.GetTriggerLookup();
            var lookupKeys = triggerLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetTriggerLookup_ForIdentifierResolvingOverloadGivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_trigger_1", "test_trigger_2" };
            var table = GetMockTable("test");

            var triggerLookup = table.GetTriggerLookup(new VerbatimIdentifierResolutionStrategy());
            var lookupKeys = triggerLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetUniqueKeyLookup_GivenNullTable_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetUniqueKeyLookup(null));
        }

        [Test]
        public static void GetUniqueKeyLookup_ForIdentifierResolvingOverloadGivenNullTable_ThrowsArgumentNullException()
        {
            var resolver = new VerbatimIdentifierResolutionStrategy();

            Assert.Throws<ArgumentNullException>(() => RelationalDatabaseTableExtensions.GetUniqueKeyLookup(null, resolver));
        }

        [Test]
        public static void GetUniqueKeyLookup_ForIdentifierResolvingOverloadGivenNullResolver_ThrowsArgumentNullException()
        {
            var table = GetMockTable("test");

            Assert.Throws<ArgumentNullException>(() => table.GetUniqueKeyLookup(null));
        }

        [Test]
        public static void GetUniqueKeyLookup_GivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_uk_2" };
            var table = GetMockTable("test");

            var uniqueKeyLookup = table.GetUniqueKeyLookup();
            var lookupKeys = uniqueKeyLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }

        [Test]
        public static void GetUniqueKeyLookup_ForIdentifierResolvingOverloadGivenTableWithColumns_ReturnsLookupWithExpectedKeys()
        {
            var expectedKeys = new[] { "test_uk_2" };
            var table = GetMockTable("test");

            var uniqueKeyLookup = table.GetUniqueKeyLookup(new VerbatimIdentifierResolutionStrategy());
            var lookupKeys = uniqueKeyLookup.Keys.Select(c => c.LocalName);

            var equalKeys = expectedKeys.SequenceEqual(lookupKeys);

            Assert.IsTrue(equalKeys);
        }
    }
}
