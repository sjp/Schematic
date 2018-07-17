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
    internal static class TooManyColumnsRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new TooManyColumnsRule(level));
        }

        [Test]
        public static void Ctor_GivenZeroColumnLimit_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TooManyColumnsRule(RuleLevel.Error, 0));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new TooManyColumnsRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithLimitedNumberOfColumns_ProducesNoMessages()
        {
            var rule = new TooManyColumnsRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn> { testColumn },
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithColumnsExceedingLimit_ProducesMessages()
        {
            var rule = new TooManyColumnsRule(RuleLevel.Error, 2);
            var database = CreateFakeDatabase();

            var testColumn1 = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_1",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var testColumn2 = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_2",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var testColumn3 = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column_3",
                Mock.Of<IDbType>(),
                true,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn> { testColumn1, testColumn2, testColumn3 },
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseTableIndex>(),
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
