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
    internal class CandidateKeyMissingRuleTests
    {
        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new CandidateKeyMissingRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithMissingPrimaryKeyAndNoUniqueKeys_ProducesMessages()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithPrimaryKey_ProducesNoMessages()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_primary_key",
                DatabaseKeyType.Primary,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                testPrimaryKey,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithMultiColumnPrimaryKey_ProducesMessages()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseTableColumn(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testUniqueKey = new DatabaseKey(
                Mock.Of<IRelationalDatabaseTable>(),
                "test_unique_key",
                DatabaseKeyType.Unique,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                new List<IDatabaseKey>() { testUniqueKey },
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
