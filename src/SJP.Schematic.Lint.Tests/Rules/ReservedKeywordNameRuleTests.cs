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
    internal static class ReservedKeywordNameRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new ReservedKeywordNameRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var tableName = new Identifier("test");

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                database,
                tableName,
                new List<IDatabaseTableColumn>(),
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
        public static void AnalyseDatabase_GivenTableWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var tableName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                database,
                tableName,
                new List<IDatabaseTableColumn>(),
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

        [Test]
        public static void AnalyseDatabase_GivenTableWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
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
        public static void AnalyseDatabase_GivenTableWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "SELECT",
                Mock.Of<IDbType>(),
                false,
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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                database,
                viewName,
                "select 1",
                new List<IDatabaseViewColumn>(),
                Array.Empty<IDatabaseViewIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                database,
                viewName,
                "select 1",
                new List<IDatabaseViewColumn>(),
                Array.Empty<IDatabaseViewIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseViewColumn(
                Mock.Of<IRelationalDatabaseView>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                database,
                viewName,
                "select 1",
                new List<IDatabaseViewColumn> { testColumn },
                Array.Empty<IDatabaseViewIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithColumnNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseViewColumn(
                Mock.Of<IRelationalDatabaseView>(),
                "SELECT",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                database,
                viewName,
                "select 1",
                new List<IDatabaseViewColumn> { testColumn },
                Array.Empty<IDatabaseViewIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenSequenceWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("test");

            var database = CreateFakeDatabase();
            var sequence = new DatabaseSequence(
                database,
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            database.Sequences = new[] { sequence };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenSequenceWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var sequence = new DatabaseSequence(
                database,
                sequenceName,
                1,
                1,
                1,
                100,
                true,
                10
            );
            database.Sequences = new[] { sequence };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenSynonymWithRegularName_ProducesNoMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var synonymName = new Identifier("test");

            var database = CreateFakeDatabase();
            var synonym = new DatabaseSynonym(database, synonymName, "target");
            database.Synonyms = new[] { synonym };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenSynonymWithNameContainingReservedKeyword_ProducesMessages()
        {
            var rule = new ReservedKeywordNameRule(RuleLevel.Error);
            var synonymName = new Identifier("SELECT");

            var database = CreateFakeDatabase();
            var synonym = new DatabaseSynonym(database, synonymName, "target");
            database.Synonyms = new[] { synonym };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect { ReservedKeywords = new[] { "SELECT" } };

            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
