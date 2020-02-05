using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Nito.AsyncEx;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.Oracle.Tests.Integration
{
    internal sealed class OracleDatabasePackageProviderTests : OracleTest
    {
        private IOracleDatabasePackageProvider PackageProvider => new OracleDatabasePackageProvider(Connection, IdentifierDefaults, IdentifierResolver);
        private AsyncLazy<List<IOracleDatabasePackage>> _packages;
        private Task<List<IOracleDatabasePackage>> GetAllPackages() => _packages.Task;

        [OneTimeSetUp]
        public async Task Init()
        {
            _packages = new AsyncLazy<List<IOracleDatabasePackage>>(() => PackageProvider.GetAllPackages().ToListAsync().AsTask());

            await Connection.ExecuteAsync(@"CREATE PACKAGE db_test_package_1 AS
    PROCEDURE test_proc();
END db_test_package_1").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"CREATE PACKAGE BODY db_test_package_1 AS
    PROCEDURE test_proc() AS
    BEGIN
        SELECT 1 AS TEST_COL FROM DUAL;
    END test_proc;
END db_test_package_1").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"CREATE PACKAGE db_test_package_2 AS
    PROCEDURE test_proc();
END db_test_package_2").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop package db_test_package_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop package db_test_package_2").ConfigureAwait(false);
        }

        private Task<IOracleDatabasePackage> GetPackageAsync(Identifier packageName)
        {
            if (packageName == null)
                throw new ArgumentNullException(nameof(packageName));

            return GetPackageAsyncCore(packageName);
        }

        private async Task<IOracleDatabasePackage> GetPackageAsyncCore(Identifier packageName)
        {
            using (await _lock.LockAsync().ConfigureAwait(false))
            {
                if (!_packagesCache.TryGetValue(packageName, out var lazyPackage))
                {
                    lazyPackage = new AsyncLazy<IOracleDatabasePackage>(() => PackageProvider.GetPackage(packageName).UnwrapSomeAsync());
                    _packagesCache[packageName] = lazyPackage;
                }

                return await lazyPackage;
            }
        }

        private readonly AsyncLock _lock = new AsyncLock();
        private readonly Dictionary<Identifier, AsyncLazy<IOracleDatabasePackage>> _packagesCache = new Dictionary<Identifier, AsyncLazy<IOracleDatabasePackage>>();

        [Test]
        public async Task GetPackage_WhenPackagePresent_ReturnsPackage()
        {
            var packageIsSome = await PackageProvider.GetPackage("db_test_package_1").IsSome.ConfigureAwait(false);
            Assert.That(packageIsSome, Is.True);
        }

        [Test]
        public async Task GetPackage_WhenPackagePresent_ReturnsPackageWithCorrectName()
        {
            const string packageName = "db_test_package_1";
            const string expectedPackageName = "DB_TEST_PACKAGE_1";

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name.LocalName, Is.EqualTo(expectedPackageName));
        }

        [Test]
        public async Task GetPackage_WhenPackagePresentGivenLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var packageName = new Identifier("DB_TEST_PACKAGE_1");
            var expectedPackageName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name, Is.EqualTo(expectedPackageName));
        }

        [Test]
        public async Task GetPackage_WhenPackagePresentGivenSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var packageName = new Identifier(IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");
            var expectedPackageName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name, Is.EqualTo(expectedPackageName));
        }

        [Test]
        public async Task GetPackage_WhenPackagePresentGivenDatabaseAndSchemaAndLocalNameOnly_ShouldBeQualifiedCorrectly()
        {
            var packageName = new Identifier(IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");
            var expectedPackageName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name, Is.EqualTo(expectedPackageName));
        }

        [Test]
        public async Task GetPackage_WhenPackagePresentGivenFullyQualifiedName_ShouldBeQualifiedCorrectly()
        {
            var packageName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name, Is.EqualTo(packageName));
        }

        [Test]
        public async Task GetPackage_WhenPackagePresentGivenFullyQualifiedNameWithDifferentServer_ShouldBeQualifiedCorrectly()
        {
            var packageName = new Identifier("A", IdentifierDefaults.Database, IdentifierDefaults.Schema, "db_test_package_1");
            var expectedPackageName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name, Is.EqualTo(expectedPackageName));
        }

        [Test]
        public async Task GetPackage_WhenPackagePresentGivenFullyQualifiedNameWithDifferentServerAndDatabase_ShouldBeQualifiedCorrectly()
        {
            var packageName = new Identifier("A", "B", IdentifierDefaults.Schema, "db_test_package_1");
            var expectedPackageName = new Identifier(IdentifierDefaults.Server, IdentifierDefaults.Database, IdentifierDefaults.Schema, "DB_TEST_PACKAGE_1");

            var package = await PackageProvider.GetPackage(packageName).UnwrapSomeAsync().ConfigureAwait(false);

            Assert.That(package.Name, Is.EqualTo(expectedPackageName));
        }

        [Test]
        public async Task GetPackage_WhenPackageMissing_ReturnsNone()
        {
            var packageIsNone = await PackageProvider.GetPackage("package_that_doesnt_exist").IsNone.ConfigureAwait(false);
            Assert.That(packageIsNone, Is.True);
        }

        [Test]
        public async Task GetAllPackages_WhenEnumerated_ContainsPackages()
        {
            var packages = await GetAllPackages().ConfigureAwait(false);

            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public async Task GetAllPackages_WhenEnumerated_ContainsTestPackage()
        {
            const string expectedPackageName = "DB_TEST_PACKAGE_1";

            var packages = await GetAllPackages().ConfigureAwait(false);
            var containsTestPackage = packages.Any(s => s.Name.LocalName == expectedPackageName);

            Assert.That(containsTestPackage, Is.True);
        }

        [Test]
        public async Task Specification_GivenPackageWithBody_ReturnsCorrectValue()
        {
            var package = await GetPackageAsync("DB_TEST_PACKAGE_1").ConfigureAwait(false);

            const string expectedSpecification = @"PACKAGE db_test_package_1 AS
    PROCEDURE test_proc();
END db_test_package_1";

            Assert.That(package.Specification, Is.EqualTo(expectedSpecification));
        }

        [Test]
        public async Task Specification_GivenPackageWithoutBody_ReturnsCorrectValue()
        {
            var package = await GetPackageAsync("DB_TEST_PACKAGE_2").ConfigureAwait(false);

            const string expectedSpecification = @"PACKAGE db_test_package_2 AS
    PROCEDURE test_proc();
END db_test_package_2";

            Assert.That(package.Specification, Is.EqualTo(expectedSpecification));
        }

        [Test]
        public async Task Body_GivenPackageWithBody_ReturnsCorrectValue()
        {
            var package = await GetPackageAsync("DB_TEST_PACKAGE_1").ConfigureAwait(false);

            const string expectedBody = @"PACKAGE BODY db_test_package_1 AS
    PROCEDURE test_proc() AS
    BEGIN
        SELECT 1 AS TEST_COL FROM DUAL;
    END test_proc;
END db_test_package_1";
            var packageBody = package.Body.UnwrapSome();

            Assert.That(packageBody, Is.EqualTo(expectedBody));
        }

        [Test]
        public async Task Body_GivenPackageWithoutBody_ReturnsNone()
        {
            var package = await GetPackageAsync("DB_TEST_PACKAGE_2").ConfigureAwait(false);

            Assert.That(package.Body, OptionIs.None);
        }
    }
}
