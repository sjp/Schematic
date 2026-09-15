using System.Data;
using System.Threading.Tasks;
using Npgsql;
using NUnit.Framework;

namespace SJP.Schematic.PostgreSql.Tests.Integration;

internal sealed class PostgreSqlConnectionFactoryTests : PostgreSqlTest
{
    [Test]
    public void OpenConnection_WhenInvoked_ReturnsConnectionInOpenState()
    {
        var factory = Config.ConnectionFactory;
        using var connection = factory.OpenConnection();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
    }

    [Test]
    public async Task OpenConnectionAsync_WhenInvoked_ReturnsConnectionInOpenState()
    {
        var factory = Config.ConnectionFactory;
        await using var connection = await factory.OpenConnectionAsync();

        Assert.That(connection.State, Is.EqualTo(ConnectionState.Open));
    }

    [Test]
    public async Task OpenConnectionAsync_WhenStatementRepeated_PreparesStatementOnServer()
    {
        var cancellationToken = TestContext.CurrentContext.CancellationToken;
        var factory = Config.ConnectionFactory;
        await using var connection = (NpgsqlConnection)await factory.OpenConnectionAsync(cancellationToken);

        // A statement text no other test issues, so preparation cannot come from elsewhere.
        const string repeatedSql = "select 'auto prepare probe'::text as probe";
        for (var i = 0; i < 3; i++)
        {
            await using var repeated = new NpgsqlCommand(repeatedSql, connection);
            await repeated.ExecuteScalarAsync(cancellationToken);
        }

        await using var preparedLookup = new NpgsqlCommand("select count(*) from pg_catalog.pg_prepared_statements where statement = @sql", connection);
        preparedLookup.Parameters.AddWithValue("sql", repeatedSql);
        var preparedCount = (long)(await preparedLookup.ExecuteScalarAsync(cancellationToken))!;

        Assert.That(preparedCount, Is.EqualTo(1));
    }
}