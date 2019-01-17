using System;
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
            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QueryAsync<string>(null, "test", CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>(null, CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>(string.Empty, CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>("   ", CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QueryAsync<string>(null, "test", param, CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>(null, param, CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>(string.Empty, param, CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>("   ", param, CancellationToken.None));
        }

        [Test]
        public static void QueryAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryAsync<string>("test", null, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.ExecuteScalarAsync<string>(null, "test", CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>(null, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>(string.Empty, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>("   ", CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.ExecuteScalarAsync<string>(null, "test", param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>(null, param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>(string.Empty, param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>("   ", param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteScalarAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteScalarAsync<string>("test", null, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.ExecuteAsync(null, "test", CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync(null, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync(string.Empty, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync("   ", CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.ExecuteAsync(null, "test", param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync(null, param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync(string.Empty, param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync("   ", param, CancellationToken.None));
        }

        [Test]
        public static void ExecuteAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.ExecuteAsync("test", null, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QueryFirstOrNone<string>(null, "test", CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>(null, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>(string.Empty, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>("   ", CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QueryFirstOrNone<string>(null, "test", param, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>(null, param, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>(string.Empty, param, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>("   ", param, CancellationToken.None));
        }

        [Test]
        public static void QueryFirstOrNone_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QueryFirstOrNone<string>("test", null, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QuerySingleAsync<string>(null, "test", CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>(null, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>(string.Empty, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>("   ", CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QuerySingleAsync<string>(null, "test", param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>(null, param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>(string.Empty, param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>("   ", param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleAsync_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleAsync<string>("test", null, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QuerySingleOrNone<string>(null, "test", CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>(null, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>(string.Empty, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithoutParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>("   ", CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenNullConnection_ThrowsArgumentNullException()
        {
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => ConnectionExtensions.QuerySingleOrNone<string>(null, "test", param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenNullSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>(null, param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenEmptySql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>(string.Empty, param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenWhiteSpaceSql_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();
            var param = new { Test = "test" };

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>("   ", param, CancellationToken.None));
        }

        [Test]
        public static void QuerySingleOrNone_WithParamsGivenNullParam_ThrowsArgumentNullException()
        {
            var connection = Mock.Of<IDbConnection>();

            Assert.Throws<ArgumentNullException>(() => connection.QuerySingleOrNone<string>("test", null, CancellationToken.None));
        }
    }
}
