using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2017
{
    internal sealed class SqlServerDialectTests : SqlServer2017Test
    {
        [Test]
        public async Task GetDatabaseDisplayVersionAsync_WhenInvoked_ReturnsNonEmptyString()
        {
            var versionStr = await Dialect.GetDatabaseDisplayVersionAsync().ConfigureAwait(false);
            var validStr = !versionStr.IsNullOrWhiteSpace();

            Assert.IsTrue(validStr);
        }

        [Test]
        public async Task GetDatabaseVersionAsync_WhenInvoked_ReturnsNonNullVersion()
        {
            var version = await Dialect.GetDatabaseVersionAsync().ConfigureAwait(false);

            Assert.IsNotNull(version);
        }

        [Test]
        public async Task GetServerProperties2017_WhenInvoked_ReturnsNonNullObject()
        {
            var properties = await Dialect.GetServerProperties2017().ConfigureAwait(false);

            Assert.IsNotNull(properties);
        }
    }
}
