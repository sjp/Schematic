using System.Threading.Tasks;
using NUnit.Framework;

namespace SJP.Schematic.SqlServer.Tests.Integration.Versions.V2019
{
    internal sealed class ServerProperties2014Tests : SqlServer2019Test
    {
        [Test]
        public void Ctor_GivenNullDto_ThrowsArgumentNullException()
        {
            Assert.That(() => new ServerProperties2014(null), Throws.ArgumentNullException);
        }

        [Test]
        public async Task BuildClrVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.BuildClrVersion, Is.Not.Null);
        }

        [Test]
        public async Task Collation_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.Collation, Is.Not.Null);
        }

        [Test]
        public async Task CollationID_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.CollationID, Is.Not.Zero);
        }

        [Test]
        public async Task ComparisonStyle_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ComparisonStyle, Is.Not.Zero);
        }

        [Test]
        public async Task ComputerNamePhysicalNetBIOS_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ComputerNamePhysicalNetBIOS, Is.Not.Null);
        }

        [Test]
        public async Task Edition_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.Edition, Is.Not.Null);
        }

        [Test]
        public async Task EditionID_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.Edition, Is.Not.Zero);
        }

        [Test]
        public async Task EngineEdition_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.EngineEdition, Is.Not.Zero);
        }

        [Test]
        public async Task HadrManagerStatus_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.HadrManagerStatus, Is.Not.Null);
        }

        [Test]
        public async Task InstanceDefaultDataPath_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.InstanceDefaultDataPath, Is.Not.Null);
        }

        [Test]
        public async Task InstanceDefaultLogPath_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.InstanceDefaultLogPath, Is.Not.Null);
        }

        [Test]
        public async Task InstanceName_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.InstanceName, Throws.Nothing);
        }

        [Test]
        public async Task IsAdvancedAnalyticsInstalled_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.IsAdvancedAnalyticsInstalled, Throws.Nothing);
        }

        [Test]
        public async Task IsClustered_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsClustered, Is.Not.Null);
        }

        [Test]
        public async Task IsFullTextInstalled_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsFullTextInstalled, Is.Not.Null);
        }

        [Test]
        public async Task IsHadrEnabled_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsHadrEnabled, Is.Not.Null);
        }

        [Test]
        public async Task IsIntegratedSecurityOnly_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsIntegratedSecurityOnly, Is.Not.Null);
        }

        [Test]
        public async Task IsLocalDB_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsLocalDB, Is.Not.Null);
        }

        [Test]
        public async Task IsSingleUser_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsSingleUser, Is.Not.Null);
        }

        [Test]
        public async Task IsXTPSupported_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.IsXTPSupported, Is.Not.Null);
        }

        [Test]
        public async Task LCID_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.LCID, Is.Not.Zero);
        }

        [Test]
        public async Task LicenseType_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.LicenseType, Is.Not.Null);
        }

        [Test]
        public async Task MachineName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.MachineName, Is.Not.Null);
        }

        [Test]
        public async Task NumLicenses_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.NumLicenses, Is.Null);
        }

        [Test]
        public async Task ProcessID_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProcessID, Is.Not.Null);
        }

        [Test]
        public async Task ProductBuild_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductBuild, Is.Not.Zero);
        }

        [Test]
        public async Task ProductBuildType_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.ProductBuildType, Throws.Nothing);
        }

        [Test]
        public async Task ProductLevel_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductLevel, Is.Not.Null);
        }

        [Test]
        public async Task ProductMajorVersion_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductMajorVersion, Is.Not.Zero);
        }

        [Test]
        public async Task ProductMinorVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductMinorVersion, Is.Not.Null);
        }

        [Test]
        public async Task ProductUpdateLevel_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.ProductUpdateLevel, Throws.Nothing);
        }

        [Test]
        public async Task ProductUpdateReference_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductUpdateReference, Is.Not.Null);
        }

        [Test]
        public async Task ProductVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ProductVersion, Is.Not.Null);
        }

        [Test]
        public async Task ResourceLastUpdateDateTime_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.ResourceLastUpdateDateTime, Throws.Nothing);
        }

        [Test]
        public async Task ResourceVersion_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ResourceVersion, Is.Not.Null);
        }

        [Test]
        public async Task ServerName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.ServerName, Is.Not.Null);
        }

        [Test]
        public async Task SqlCharSet_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlCharSet, Is.Not.Zero);
        }

        [Test]
        public async Task SqlCharSetName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlCharSetName, Is.Not.Null);
        }

        [Test]
        public async Task SqlSortOrder_PropertyGet_IsNotZero()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlSortOrder, Is.Not.Zero);
        }

        [Test]
        public async Task SqlSortOrderName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.SqlSortOrderName, Is.Not.Null);
        }

        [Test]
        public async Task FilestreamShareName_PropertyGet_IsNotNull()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(serverProps.FilestreamShareName, Is.Not.Null);
        }

        [Test]
        public async Task FilestreamConfiguredLevel_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.FilestreamConfiguredLevel, Throws.Nothing);
        }

        [Test]
        public async Task FilestreamEffectiveLevel_PropertyGet_ThrowsNothing()
        {
            var serverProps = await Dialect.GetServerProperties2014(Config.ConnectionFactory).ConfigureAwait(false);

            Assert.That(() => serverProps.FilestreamEffectiveLevel, Throws.Nothing);
        }
    }
}
