using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2008R2
{
    internal sealed class ServerProperties2008Tests : SqlServer2008R2Test
    {
        [Test]
        public void Ctor_GivenNullDto_ThrowsArgumentNullException()
        {
            Assert.That(() => new ServerProperties2008(null), Throws.ArgumentNullException);
        }

        [Test]
        public async Task BuildClrVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.BuildClrVersion, Is.Not.Null);
        }

        [Test]
        public async Task Collation_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.Collation, Is.Not.Null);
        }

        [Test]
        public async Task CollationID_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.CollationID, Is.Not.Zero);
        }

        [Test]
        public async Task ComparisonStyle_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ComparisonStyle, Is.Not.Zero);
        }

        [Test]
        public async Task ComputerNamePhysicalNetBIOS_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ComputerNamePhysicalNetBIOS, Is.Not.Null);
        }

        [Test]
        public async Task Edition_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.Edition, Is.Not.Null);
        }

        [Test]
        public async Task EditionID_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.Edition, Is.Not.Zero);
        }

        [Test]
        public async Task EngineEdition_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.EngineEdition, Is.Not.Zero);
        }

        [Test]
        public async Task InstanceName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.InstanceName, Is.Not.Null);
        }

        [Test]
        public async Task IsAdvancedAnalyticsInstalled_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.IsAdvancedAnalyticsInstalled, Throws.Nothing);
        }

        [Test]
        public async Task IsClustered_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsClustered, Is.Not.Null);
        }

        [Test]
        public async Task IsFullTextInstalled_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsFullTextInstalled, Is.Not.Null);
        }

        [Test]
        public async Task IsIntegratedSecurityOnly_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsIntegratedSecurityOnly, Is.Not.Null);
        }

        [Test]
        public async Task IsSingleUser_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsSingleUser, Is.Not.Null);
        }

        [Test]
        public async Task LCID_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.LCID, Is.Not.Zero);
        }

        [Test]
        public async Task LicenseType_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.LicenseType, Is.Not.Null);
        }

        [Test]
        public async Task MachineName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.MachineName, Is.Not.Null);
        }

        [Test]
        public async Task NumLicenses_PropertyGet_IsNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.NumLicenses, Is.Null);
        }

        [Test]
        public async Task ProcessID_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProcessID, Is.Not.Null);
        }

        [Test]
        public async Task ProductLevel_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductLevel, Is.Not.Null);
        }

        [Test]
        public async Task ProductVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductVersion, Is.Not.Null);
        }

        [Test]
        public async Task ResourceLastUpdateDateTime_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.ResourceLastUpdateDateTime, Throws.Nothing);
        }

        [Test]
        public async Task ResourceVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ResourceVersion, Is.Not.Null);
        }

        [Test]
        public async Task ServerName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ServerName, Is.Not.Null);
        }

        [Test]
        public async Task SqlCharSet_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlCharSet, Is.Not.Zero);
        }

        [Test]
        public async Task SqlCharSetName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlCharSetName, Is.Not.Null);
        }

        [Test]
        public async Task SqlSortOrder_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlSortOrder, Is.Not.Zero);
        }

        [Test]
        public async Task SqlSortOrderName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlSortOrderName, Is.Not.Null);
        }

        [Test]
        public async Task FilestreamShareName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.FilestreamShareName, Is.Not.Null);
        }

        [Test]
        public async Task FilestreamConfiguredLevel_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.FilestreamConfiguredLevel, Throws.Nothing);
        }

        [Test]
        public async Task FilestreamEffectiveLevel_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2008(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.FilestreamEffectiveLevel, Throws.Nothing);
        }
    }
}
