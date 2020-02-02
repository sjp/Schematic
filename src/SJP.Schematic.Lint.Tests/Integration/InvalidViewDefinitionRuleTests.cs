using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint.Rules;

namespace SJP.Schematic.Lint.Tests.Integration
{
    internal sealed class InvalidViewDefinitionRuleTests : SqliteTest
    {
        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync("create view valid_view_1 as select 1 as dummy").ConfigureAwait(false);
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
            var dialect = Mock.Of<IDatabaseDialect>();
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new InvalidViewDefinitionRule(connection, dialect, level), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenNullDialect_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            IDatabaseDialect dialect = null;
            const RuleLevel level = RuleLevel.Error;
            Assert.That(() => new InvalidViewDefinitionRule(connection, dialect, level), Throws.ArgumentNullException);
        }

        [Test]
        public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
        {
            var connection = Mock.Of<IDbConnection>();
            var dialect = Mock.Of<IDatabaseDialect>();
            const RuleLevel level = (RuleLevel)999;
            Assert.That(() => new InvalidViewDefinitionRule(connection, dialect, level), Throws.ArgumentException);
        }

        [Test]
        public void AnalyseViews_GivenNullViews_ThrowsArgumentNullException()
        {
            var rule = new InvalidViewDefinitionRule(Connection, Dialect, RuleLevel.Error);
            Assert.That(() => rule.AnalyseViews(null), Throws.ArgumentNullException);
        }

        [Test]
        public async Task AnalyseViews_GivenDatabaseWithOnlyValidViews_ProducesNoMessages()
        {
            var rule = new InvalidViewDefinitionRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var views = new[]
            {
                await database.GetView("valid_view_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.False);
        }

        [Test]
        public async Task AnalyseViews_GivenViewsWithOnlyInvalidViews_ProducesMessages()
        {
            var rule = new InvalidViewDefinitionRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var views = new[]
            {
                await database.GetView("invalid_view_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }

        [Test]
        public async Task AnalyseViews_GivenViewsWithValidAndInvalidViews_ProducesMessages()
        {
            var rule = new InvalidViewDefinitionRule(Connection, Dialect, RuleLevel.Error);
            var database = GetSqliteDatabase();

            var views = new[]
            {
                await database.GetView("valid_view_1").UnwrapSomeAsync().ConfigureAwait(false),
                await database.GetView("invalid_view_1").UnwrapSomeAsync().ConfigureAwait(false)
            };

            var hasMessages = await rule.AnalyseViews(views).AnyAsync().ConfigureAwait(false);

            Assert.That(hasMessages, Is.True);
        }
    }
}
