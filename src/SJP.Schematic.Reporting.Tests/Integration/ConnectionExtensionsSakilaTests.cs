using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.Reporting.Tests.Integration;

internal sealed class ConnectionExtensionsSakilaTests : SakilaTest
{
    [Test]
    public async Task GetRowCountAsync_GivenLanguageTable_MatchesRawCountQuery()
    {
        Identifier tableName = "language";

        var rowCount = await DbConnection.GetRowCountAsync(Connection.Dialect, tableName, CancellationToken.None);
        var expectedRowCount = await DbConnection.ExecuteScalarAsync<ulong>("select count(*) from \"language\"", CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rowCount, Is.EqualTo(expectedRowCount));
            Assert.That(rowCount, Is.GreaterThan(0UL));
        }
    }

    [Test]
    public async Task GetRowCountAsync_GivenTableWithManyRows_MatchesRawCountQuery()
    {
        Identifier tableName = "actor";

        var rowCount = await DbConnection.GetRowCountAsync(Connection.Dialect, tableName, CancellationToken.None);
        var expectedRowCount = await DbConnection.ExecuteScalarAsync<ulong>("select count(*) from \"actor\"", CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(rowCount, Is.EqualTo(expectedRowCount));
            Assert.That(rowCount, Is.GreaterThan(0UL));
        }
    }

    [Test]
    public void GetRowCountAsync_GivenNullConnection_ThrowsArgumentNullException()
    {
        IDbConnectionFactory connection = null!;
        Identifier tableName = "language";

        Assert.That(() => connection!.GetRowCountAsync(Connection.Dialect, tableName, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public void GetRowCountAsync_GivenNullDialect_ThrowsArgumentNullException()
    {
        Identifier tableName = "language";
        Assert.That(() => DbConnection.GetRowCountAsync(null!, tableName, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public void GetRowCountAsync_GivenNullTableName_ThrowsArgumentNullException()
    {
        Assert.That(() => DbConnection.GetRowCountAsync(Connection.Dialect, null!, CancellationToken.None), Throws.ArgumentNullException);
    }
}
