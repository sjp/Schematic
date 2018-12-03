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
    internal static class WhitespaceNameRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new WhitespaceNameRule(level));
        }

        [Test]
        public static void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var tableName = new Identifier("test");

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                tableName,
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
        public static void AnalyseDatabase_GivenTableWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var tableName = new Identifier("   test      ");

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                tableName,
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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenTableWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
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
        public static void AnalyseDatabase_GivenTableWithColumnNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "   test_column ",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn> { testColumn },
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

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn>(),
                Array.Empty<IDatabaseIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("   test   ");

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn>(),
                Array.Empty<IDatabaseIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithRegularColumnNames_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn },
                Array.Empty<IDatabaseIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenViewWithColumnNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var viewName = new Identifier("test");

            var testColumn = new DatabaseColumn(
                "   test_column   ",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );

            var database = CreateFakeDatabase();
            var view = new RelationalDatabaseView(
                viewName,
                "select 1",
                new List<IDatabaseColumn> { testColumn },
                Array.Empty<IDatabaseIndex>()
            );
            database.Views = new[] { view };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenSequenceWithRegularName_ProducesNoMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("test");

            var database = CreateFakeDatabase();
            var sequence = new DatabaseSequence(
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
        public static void AnalyseDatabase_GivenSequenceWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var sequenceName = new Identifier("   test   ");

            var database = CreateFakeDatabase();
            var sequence = new DatabaseSequence(
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
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var synonymName = new Identifier("test");

            var database = CreateFakeDatabase();
            var synonym = new DatabaseSynonym(synonymName, "target");
            database.Synonyms = new[] { synonym };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static void AnalyseDatabase_GivenSynonymWithNameContainingWhitespace_ProducesMessages()
        {
            var rule = new WhitespaceNameRule(RuleLevel.Error);
            var synonymName = new Identifier("   test   ");

            var database = CreateFakeDatabase();
            var synonym = new DatabaseSynonym(synonymName, "target");
            database.Synonyms = new[] { synonym };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IDatabaseIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }
    }
}
