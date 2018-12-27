using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;

namespace SJP.Schematic.Lint.Tests.Rules
{
    [TestFixture]
    internal static class OrphanedTableRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new OrphanedTableRule(level));
        }

        [Test]
        public static void AnalyseDatabaseAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabaseAsync(null));
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithParentKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var parentKey = new DatabaseRelationalKey(
                "child_table",
                new DatabaseKey(
                    Option<Identifier>.Some("child_key"),
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                "parent_table",
                new DatabaseKey(
                    Option<Identifier>.Some("parent_key"),
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                System.Data.Rule.None,
                System.Data.Rule.None
            );
            var childTable = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                new[] { parentKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { childTable };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithChildKeys_ProducesNoMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);

            var database = CreateFakeDatabase();
            var childKey = new DatabaseRelationalKey(
                "child_table",
                new DatabaseKey(
                    Option<Identifier>.Some("child_key"),
                    DatabaseKeyType.Foreign,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                "parent_table",
                new DatabaseKey(
                    Option<Identifier>.Some("parent_key"),
                    DatabaseKeyType.Primary,
                    new[] { Mock.Of<IDatabaseColumn>() },
                    true
                ),
                System.Data.Rule.None,
                System.Data.Rule.None
            );
            var parentTable = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                new[] { childKey },
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { parentTable };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithNoRelations_ProducesMessages()
        {
            var rule = new OrphanedTableRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var table = new RelationalDatabaseTable(
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

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.NotZero(messages.Count());
        }

        private static FakeRelationalDatabase CreateFakeDatabase()
        {
            var dialect = new FakeDatabaseDialect();
            var connection = Mock.Of<IDbConnection>();
            var identifierDefaults = Mock.Of<IIdentifierDefaults>();

            return new FakeRelationalDatabase(dialect, connection, identifierDefaults);
        }
    }
}
