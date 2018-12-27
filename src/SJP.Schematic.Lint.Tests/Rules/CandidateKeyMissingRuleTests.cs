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
    internal static class CandidateKeyMissingRuleTests
    {
        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new CandidateKeyMissingRule(level));
        }

        [Test]
        public static void AnalyseDatabaseAsync_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabaseAsync(null));
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithMissingPrimaryKeyAndNoUniqueKeys_ProducesMessages()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);

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

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithPrimaryKey_ProducesNoMessages()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testPrimaryKey = new DatabaseKey(
                Option<Identifier>.Some("test_primary_key"),
                DatabaseKeyType.Primary,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                testPrimaryKey,
                Array.Empty<IDatabaseKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
        }

        [Test]
        public static async Task AnalyseDatabaseAsync_GivenTableWithMultiColumnPrimaryKey_ProducesMessages()
        {
            var rule = new CandidateKeyMissingRule(RuleLevel.Error);
            var database = CreateFakeDatabase();

            var testColumn = new DatabaseColumn(
                "test_column",
                Mock.Of<IDbType>(),
                false,
                null,
                null
            );
            var testUniqueKey = new DatabaseKey(
                Option<Identifier>.Some("test_unique_key"),
                DatabaseKeyType.Unique,
                new[] { testColumn },
                true
            );

            var table = new RelationalDatabaseTable(
                "test",
                new List<IDatabaseColumn>(),
                null,
                new List<IDatabaseKey>() { testUniqueKey },
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseRelationalKey>(),
                Array.Empty<IDatabaseIndex>(),
                Array.Empty<IDatabaseCheckConstraint>(),
                Array.Empty<IDatabaseTrigger>()
            );
            database.Tables = new[] { table };

            var messages = await rule.AnalyseDatabaseAsync(database).ConfigureAwait(false);

            Assert.Zero(messages.Count());
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
