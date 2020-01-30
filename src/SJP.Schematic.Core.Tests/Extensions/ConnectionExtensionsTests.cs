using System.Data;
using System.Threading;
using Moq;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Core.Tests.Extensions
{
    [TestFixture]
    internal static class ConnectionExtensionsTests
    {
        [Test]
        public static void QueryAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => ConnectionExtensions.QueryAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryAsync<string>(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryAsync<string>(string.Empty, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryAsync<string>("   ", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.That(() => ConnectionExtensions.QueryAsync<string>(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QueryAsync<string>(null, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QueryAsync<string>(string.Empty, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QueryAsync<string>("   ", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => ConnectionExtensions.ExecuteScalarAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteScalarAsync<string>(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteScalarAsync<string>(string.Empty, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteScalarAsync<string>("   ", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.That(() => ConnectionExtensions.ExecuteScalarAsync<string>(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.ExecuteScalarAsync<string>(null, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.ExecuteScalarAsync<string>(string.Empty, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.ExecuteScalarAsync<string>("   ", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteScalarAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => ConnectionExtensions.ExecuteAsync(null, "test", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteAsync(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteAsync(string.Empty, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteAsync("   ", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.That(() => ConnectionExtensions.ExecuteAsync(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.ExecuteAsync(null, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.ExecuteAsync(string.Empty, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.ExecuteAsync("   ", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.ExecuteAsync("test", null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => ConnectionExtensions.QueryFirstOrNone<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryFirstOrNone<string>(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryFirstOrNone<string>(string.Empty, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryFirstOrNone<string>("   ", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.That(() => ConnectionExtensions.QueryFirstOrNone<string>(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QueryFirstOrNone<string>(null, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QueryFirstOrNone<string>(string.Empty, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QueryFirstOrNone<string>("   ", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QueryFirstOrNone<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => ConnectionExtensions.QuerySingleAsync<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleAsync<string>(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleAsync<string>(string.Empty, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleAsync<string>("   ", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.That(() => ConnectionExtensions.QuerySingleAsync<string>(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QuerySingleAsync<string>(null, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QuerySingleAsync<string>(string.Empty, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QuerySingleAsync<string>("   ", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleAsync<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.That(() => ConnectionExtensions.QuerySingleOrNone<string>(null, "test", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleOrNone<string>(null, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleOrNone<string>(string.Empty, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleOrNone<string>("   ", CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.That(() => ConnectionExtensions.QuerySingleOrNone<string>(null, "test", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QuerySingleOrNone<string>(null, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QuerySingleOrNone<string>(string.Empty, param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.That(() => connection.QuerySingleOrNone<string>("   ", param, CancellationToken.None), Throws.ArgumentNullException);
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.That(() => connection.QuerySingleOrNone<string>("test", null, CancellationToken.None), Throws.ArgumentNullException);
        }
    }
}
