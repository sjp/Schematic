using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Sqlite;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration
{
    internal sealed class EFCoreDataAccessGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public Task Init()
        {
            return Connection.ExecuteAsync(@"create table dal_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'asd'
)");
        }

        [OneTimeTearDown]
        public Task CleanUp()
        {
            return Connection.ExecuteAsync("drop table dal_test_table_1");
        }

        [Test]
        public void Generate_GivenDatabaseWithTables_GeneratesFilesInExpectedLocations()
        {
            var nameProvider = new PascalCaseNameProvider();
            var generator = new EFCoreDataAccessGenerator(Database, nameProvider);

            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "efcoretest");

            var projectPath = Path.Combine(testProjectDir, "DataAccessGeneratorTest.csproj");
            var tablesDir = Path.Combine(testProjectDir, "Tables");

            var expectedAppContextPath = Path.Combine(testProjectDir, "AppContext.cs");
            var expectedTable1Path = Path.Combine(tablesDir, "Main", "DalTestTable1.cs");

            var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [testProjectDir + "\\"] = new MockDirectoryData(),
                [expectedAppContextPath] = MockFileData.NullObject,
                [expectedTable1Path] = MockFileData.NullObject
            });

            generator.Generate(mockFs, projectPath, TestNamespace);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(mockFs.FileExists(projectPath));
                Assert.IsTrue(mockFs.FileExists(expectedAppContextPath));
                Assert.IsTrue(mockFs.Directory.Exists(tablesDir));
                Assert.IsTrue(mockFs.FileExists(expectedTable1Path));
            });
        }

        private const string TestNamespace = "OrmLiteTestNamespace";
    }
}
