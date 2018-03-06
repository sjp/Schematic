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
    internal class OrphanedTableRuleTests
    {
        [Test]
        public void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new OrphanedTableRule(level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithParentKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var parentKey = new DatabaseRelationalKey(
                new DatabaseKey(
                    Mock.Of<IRelationalDatabaseTable>(),
                    "child_key",
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseTableColumn>() },
                    true
                ),
                new DatabaseKey(
                    Mock.Of<IRelationalDatabaseTable>(),
                    "parent_key",
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseTableColumn>() },
                    true
                ),
                System.Data.Rule.None,
                System.Data.Rule.None
            );
            var childTable = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                new[] { parentKey },
                Enumerable.Empty<IDatabaseRelationalKey>(),
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { childTable };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithChildKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var childKey = new DatabaseRelationalKey(
                new DatabaseKey(
                    Mock.Of<IRelationalDatabaseTable>(),
                    "child_key",
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseTableColumn>() },
                    true
                ),
                new DatabaseKey(
                    Mock.Of<IRelationalDatabaseTable>(),
                    "parent_key",
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseTableColumn>() },
                    true
                ),
                System.Data.Rule.None,
                System.Data.Rule.None
            );
            var parentTable = new RelationalDatabaseTable(
                database,
                "test",
                new List<IDatabaseTableColumn>(),
                null,
                Enumerable.Empty<IDatabaseKey>(),
                Enumerable.Empty<IDatabaseRelationalKey>(),
                new[] { childKey },
                Enumerable.Empty<IDatabaseTableIndex>(),
                Enumerable.Empty<IDatabaseCheckConstraint>(),
                Enumerable.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { parentTable };

            var messages = rule.AnalyseDatabase(database);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenTableWithNoRelations_ProducesMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);
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

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();

            return new FakeRelationalDatabase(dialect, connection);
        }
    }
}
