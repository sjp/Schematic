using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class UniqueIndexWithNullableColumnsRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new UniqueIndexWithNullableColumnsRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNoIndexes_ProducesNoMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNoUniqueIndexes_ProducesNoMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var index = new DatabaseIndex(
                "test_index_name",
                false,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Array.Empty<IDatabaseColumn>(),
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { index },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNoNullableColumnsInUniqueIndex_ProducesNoMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var uniqueIndex = new DatabaseIndex(
                "test_index_name",
                true,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Array.Empty<IDatabaseColumn>(),
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { uniqueIndex },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithNullableColumnsInUniqueIndex_ProducesMessages()
        {
            var rule = new UniqueIndexWithNullableColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var uniqueIndex = new DatabaseIndex(
                "test_index_name",
                true,
                new[] { new DatabaseIndexColumn(testColumn, IndexColumnOrder.Ascending) },
                Array.Empty<IDatabaseColumn>(),
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { uniqueIndex },
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
