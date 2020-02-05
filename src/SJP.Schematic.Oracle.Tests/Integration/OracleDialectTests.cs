using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDialectTests : OracleTest
    {
        [Test]
        public async Task GetDatabaseDisplayVersionAsync_WhenInvoked_ReturnsNonEmptyString()
        {
            var versionStr = await Dialect.GetDatabaseDisplayVersionAsync().ConfigureAwait(false);
            var validStr = !versionStr.IsNullOrWhiteSpace();

            Assert.That(validStr, Is.True);
        }

        [Test]
        public async Task GetDatabaseVersionAsync_WhenInvoked_ReturnsNonNullVersion()
        {
            var version = await Dialect.GetDatabaseVersionAsync().ConfigureAwait(false);

            Assert.That(version, Is.Not.Null);
        }
    }
}
