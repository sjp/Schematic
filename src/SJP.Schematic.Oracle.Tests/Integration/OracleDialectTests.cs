using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDialectTests : OracleTest
    {
        [Test]
        public void GetDatabaseDisplayVersion_GivenValidConnection_ReturnsNonEmptyString()
        {
            var versionStr = Dialect.GetDatabaseDisplayVersion(Connection);
            var validStr = !versionStr.IsNullOrWhiteSpace();

            Assert.IsTrue(validStr);
        }

        [Test]
        public async Task GetDatabaseDisplayVersionAsync_GivenValidConnection_ReturnsNonEmptyString()
        {
            var versionStr = await Dialect.GetDatabaseDisplayVersionAsync(Connection).ConfigureAwait(false);
            var validStr = !versionStr.IsNullOrWhiteSpace();

            Assert.IsTrue(validStr);
        }

        [Test]
        public void GetDatabaseVersion_GivenValidConnection_ReturnsNonNullVersion()
        {
            var version = Dialect.GetDatabaseVersion(Connection);

            Assert.IsNotNull(version);
        }

        [Test]
        public async Task GetDatabaseVersionAsync_GivenValidConnection_ReturnsNonNullVersion()
        {
            var version = await Dialect.GetDatabaseVersionAsync(Connection).ConfigureAwait(false);

            Assert.IsNotNull(version);
        }
    }
}
