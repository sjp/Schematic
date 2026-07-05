using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Lint;
using SJP.Schematic.Reporting.Html.Lint.Rules;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Reporting.Tests.Html.Lint.Rules;

internal sealed class ForeignKeySelfReferenceRuleTests : SqliteRuleTestBase
{
    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"
create table reporting_table_with_self_referencing_columns_1 (
    column_1 integer not null primary key autoincrement,
    column_2 integer,
    constraint reporting_self_ref_fk_1 foreign key (column_2) references reporting_table_with_self_referencing_columns_1 (column_1)
)", CancellationToken.None);
        await DbConnection.ExecuteAsync("insert into reporting_table_with_self_referencing_columns_1 (column_1, column_2) values (1, 1)", CancellationToken.None);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop table reporting_table_with_self_referencing_columns_1", CancellationToken.None);
    }

    [Test]
    public static void Ctor_GivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => new ForeignKeySelfReferenceRule(null!, RuleLevel.Error), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenInvalidLevel_ThrowsArgumentException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        const RuleLevel level = (RuleLevel)999;
        Assert.That(() => new ForeignKeySelfReferenceRule(connection, level), Throws.ArgumentException);
    }

    [Test]
    public static void AnalyseTables_GivenNullTables_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<ISchematicConnection>();
        var rule = new ForeignKeySelfReferenceRule(connection, RuleLevel.Error);
        Assert.That(() => rule.AnalyseTables(null!), Throws.ArgumentNullException);
    }

    [Test]
    public async Task AnalyseTables_GivenTableWithSelfReferencingRow_ProducesMessageWithVisibleTableName()
    {
        var rule = new ForeignKeySelfReferenceRule(Connection, RuleLevel.Error);
        var database = GetSqliteDatabase();

        var tables = new[]
        {
            await database.GetTable("reporting_table_with_self_referencing_columns_1").UnwrapSomeAsync(),
        };

        var messages = await rule.AnalyseTables(tables);

        Assert.That(messages, Is.Not.Empty);
        var message = messages.Single();
        Assert.That(message.Message, Does.Contain("reporting_table_with_self_referencing_columns_1"));
        Assert.That(message.Message, Does.Not.Contain("LocalName ="));
    }
}
