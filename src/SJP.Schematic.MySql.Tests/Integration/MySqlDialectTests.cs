using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.MySql.Tests.Integration
{
    internal sealed class MySqlDialectTests : MySqlTest
    {
        [Test]
        public async Task GetDatabaseDisplayVersionAsync_GivenValidConnection_ReturnsNonEmptyString()
        {
            var versionStr = await Dialect.GetDatabaseDisplayVersionAsync().ConfigureAwait(false);
            var validStr = !versionStr.IsNullOrWhiteSpace();

            Assert.IsTrue(validStr);
        }

        [Test]
        public async Task GetDatabaseVersionAsync_GivenValidConnection_ReturnsNonNullVersion()
        {
            var version = await Dialect.GetDatabaseVersionAsync().ConfigureAwait(false);

            Assert.IsNotNull(version);
        }
    }
}
