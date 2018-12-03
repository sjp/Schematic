using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;
using SJP.Schematic.Lint.Tests.Fakes;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class InvalidViewDefinitionRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view valid_view_1 as select 1 as asd").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view invalid_view_1 as select x from unknown_table").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view valid_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view invalid_view_1").ConfigureAwait(false);
        }

        [Test]
        public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
        {
            IDbConnection connection = null;
            const RuleLevel level = RuleLevel.Error;
            Assert.Throws<ArgumentNullException>(() => new InvalidViewDefinitionRule(connection, level));
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            var connection = Mock.Of<IDbConnection>();
            const RuleLevel level = (RuleLevel)999;
            Assert.Throws<ArgumentException>(() => new InvalidViewDefinitionRule(connection, level));
        }

        [Test]
        public void AnalyseDatabase_GivenNullDatabase_ThrowsArgumentNullException()
        {
            var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
            Assert.Throws<ArgumentNullException>(() => rule.AnalyseDatabase(null));
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyValidViews_ProducesNoMessages()
        {
            var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = GetSqliteDatabase();

            fakeDatabase.Views = new[] { database.GetView("valid_view_1").UnwrapSome() };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.Zero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithOnlyInvalidViews_ProducesMessages()
        {
            var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = GetSqliteDatabase();

            fakeDatabase.Views = new[] { database.GetView("invalid_view_1").UnwrapSome() };

            var messages = rule.AnalyseDatabase(fakeDatabase);

            Assert.NotZero(messages.Count());
        }

        [Test]
        public void AnalyseDatabase_GivenDatabaseWithValidAndInvalidViews_ProducesMessages()
        {
            var rule = new InvalidViewDefinitionRule(Connection, RuleLevel.Error);
            var fakeDatabase = CreateFakeDatabase();
            var database = GetSqliteDatabase();

            fakeDatabase.Views = new[]
            {
                database.GetView("valid_view_1").UnwrapSome(),
                database.GetView("invalid_view_1").UnwrapSome()
            };

            var messages = rule.AnalyseDatabase(fakeDatabase);

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
