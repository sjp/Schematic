using System.Threading;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions;

[TestFixture]
internal static class ConnectionExtensionsTests
{
    [Test]
    public static void QueryAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.QueryAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QueryAsync_WithoutParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QueryAsync<string>(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new TestQuery { Test = "test" };

        Assert.That(() => ConnectionExtensions.QueryAsync(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QueryAsync_WithParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new TestQuery { Test = "test" };

        Assert.That(() => connection.QueryAsync(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QueryAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryEnumerableAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.QueryEnumerableAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QueryEnumerableAsync_WithoutParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QueryEnumerableAsync<string>(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryEnumerableAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new TestQuery { Test = "test" };

        Assert.That(() => ConnectionExtensions.QueryEnumerableAsync(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QueryEnumerableAsync_WithParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new TestQuery { Test = "test" };

        Assert.That(() => connection.QueryEnumerableAsync(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryEnumerableAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QueryEnumerableAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void ExecuteScalarAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.ExecuteScalarAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ExecuteScalarAsync_WithoutParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.ExecuteScalarAsync<string>(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void ExecuteScalarAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new TestQuery { Test = "test" };

        Assert.That(() => ConnectionExtensions.ExecuteScalarAsync(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ExecuteScalarAsync_WithParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new TestQuery { Test = "test" };

        Assert.That(() => connection.ExecuteScalarAsync(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void ExecuteScalarAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.ExecuteScalarAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void ExecuteAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.ExecuteAsync(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ExecuteAsync_WithoutParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.ExecuteAsync(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void ExecuteAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new { Test = "test" };

        Assert.That(() => ConnectionExtensions.ExecuteAsync(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void ExecuteAsync_WithParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new { Test = "test" };

        Assert.That(() => connection.ExecuteAsync(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void ExecuteAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.ExecuteAsync("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryFirstOrNone_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.QueryFirstOrNone<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QueryFirstOrNone_WithoutParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QueryFirstOrNone<string>(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryFirstOrNone_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new TestQuery { Test = "test" };

        Assert.That(() => ConnectionExtensions.QueryFirstOrNone(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QueryFirstOrNone_WithParamsGivenNullSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new TestQuery { Test = "test" };

        Assert.That(() => connection.QueryFirstOrNone(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QueryFirstOrNone_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QueryFirstOrNone<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuerySingleAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.QuerySingleAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuerySingleAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QuerySingleAsync<string>(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuerySingleAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new TestQuery { Test = "test" };

        Assert.That(() => ConnectionExtensions.QuerySingleAsync(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuerySingleAsync_WithParamsGivenNullSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new TestQuery { Test = "test" };

        Assert.That(() => connection.QuerySingleAsync(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuerySingleAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QuerySingleAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuerySingleOrNone_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        Assert.That(() => ConnectionExtensions.QuerySingleOrNone<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuerySingleOrNone_WithoutParamsGivenNullSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QuerySingleOrNone<string>(sql, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuerySingleOrNone_WithParamsGivenNullConnection_ThrowsArgumentNullException()
    {
        var param = new TestQuery { Test = "test" };

        Assert.That(() => ConnectionExtensions.QuerySingleOrNone(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [TestCase((string)null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void QuerySingleOrNone_WithParamsGivenNullOrWhiteSpaceSql_ThrowsArgumentNullException(string sql)
    {
        var connection = Mock.Of<IDbConnectionFactory>();
        var param = new TestQuery { Test = "test" };

        Assert.That(() => connection.QuerySingleOrNone(sql, param, CancellationToken.None), Throws.ArgumentNullException);
    }

    [Test]
    public static void QuerySingleOrNone_WithParamsGivenNullParam_ThrowsArgumentNullException()
    {
        var connection = Mock.Of<IDbConnectionFactory>();

        Assert.That(() => connection.QuerySingleOrNone<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
    }

    private sealed record TestQuery : ISqlQuery<string>
    {
        public required string Test { get; init; }
    }
}