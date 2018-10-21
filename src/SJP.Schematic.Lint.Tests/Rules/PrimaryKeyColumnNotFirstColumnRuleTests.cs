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
    internal static class PrimaryKeyColumnNotFirstColumnRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new PrimaryKeyColumnNotFirstColumnRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithMissingPrimaryKey_ProducesNoMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);

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
        public static void AnalyseDatabase_GivenTableWithPrimaryKeyWithMultipleColumns_ProducesNoMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn1, testColumn2 },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
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
        public static void AnalyseDatabase_GivenTableWithPrimaryKeyWithSingleColumnAsFirstColumn_ProducesNoMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn1 },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
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
        public static void AnalyseDatabase_GivenTableWithPrimaryKeyWithSingleColumnNotFirstColumn_ProducesMessages()
        {
            var rule = new PrimaryKeyColumnNotFirstColumnRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn1 = new DatabaseColumn(
                "test_column_1",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testColumn2 = new DatabaseColumn(
                "test_column_2",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn2 },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseColumn> { testColumn1, testColumn2 },
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
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
