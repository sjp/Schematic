using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Dapper;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.Poco.Tests.Integration
{
    internal sealed class PocoDataAccessGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Dialect, Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public async Task Init()
        {
            await Connection.ExecuteAsync(@"
create view test_view_1 as
select
    1 as testint,
    2.45 as testdouble,
    X'DEADBEEF' as testblob,
    CURRENT_TIMESTAMP as testdatetime,
    'test' as teststring
").ConfigureAwait(false);
            await Connection.ExecuteAsync(@"create table view_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)").ConfigureAwait(false);
            await Connection.ExecuteAsync("create view test_view_2 as select * from view_test_table_1").ConfigureAwait(false);
        }

        [OneTimeTearDown]
        public async Task CleanUp()
        {
            await Connection.ExecuteAsync("drop view test_view_1").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop view test_view_2").ConfigureAwait(false);
            await Connection.ExecuteAsync("drop table view_test_table_1").ConfigureAwait(false);
        }

        [Test]
        public async Task Generate_GivenDatabaseWithTablesAndViews_GeneratesFilesInExpectedLocations()
        {
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);
            var tablesDir = Path.Combine(tempDir.DirectoryPath, "Tables");
            var viewsDir = Path.Combine(tempDir.DirectoryPath, "Views");

            var expectedTable1Path = Path.Combine(tablesDir, "Main", "ViewTestTable1.cs");
            var expectedView1Path = Path.Combine(viewsDir, "Main", "TestView1.cs");
            var expectedView2Path = Path.Combine(viewsDir, "Main", "TestView2.cs");

            var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [tempDir.DirectoryPath + Path.PathSeparator] = new MockDirectoryData(),
                [expectedTable1Path] = MockFileData.NullObject,
                [expectedView1Path] = MockFileData.NullObject,
                [expectedView2Path] = MockFileData.NullObject
            });

            var nameTranslator = new PascalCaseNameTranslator();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var generator = new PocoDataAccessGenerator(mockFs, Database, commentProvider, nameTranslator);
            await generator.Generate(projectPath, TestNamespace).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.IsTrue(mockFs.FileExists(projectPath));
                Assert.IsTrue(mockFs.Directory.Exists(tablesDir));
                Assert.IsTrue(mockFs.Directory.Exists(viewsDir));
                Assert.IsTrue(mockFs.FileExists(expectedTable1Path));
                Assert.IsTrue(mockFs.FileExists(expectedView1Path));
                Assert.IsTrue(mockFs.FileExists(expectedView2Path));
            });
        }

        [Test]
        public async Task Generate_GivenDatabaseWithoutTables_BuildsProjectSuccessfully()
        {
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

            var database = new EmptyRelationalDatabase(Database.Dialect, Database.IdentifierDefaults);

            var fileSystem = new FileSystem();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new PascalCaseNameTranslator();
            var generator = new PocoDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);
            await generator.Generate(projectPath, TestNamespace).ConfigureAwait(false);

            var buildsSuccessfully = await ProjectBuildsSuccessfullyAsync(projectPath).ConfigureAwait(false);
            Assert.IsTrue(buildsSuccessfully);
        }

        [Test]
        public async Task Generate_GivenDatabaseWithTables_BuildsProjectSuccessfully()
        {
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

            var fileSystem = new FileSystem();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new PascalCaseNameTranslator();
            var generator = new PocoDataAccessGenerator(fileSystem, Database, commentProvider, nameTranslator);
            await generator.Generate(projectPath, TestNamespace).ConfigureAwait(false);

            var buildsSuccessfully = await ProjectBuildsSuccessfullyAsync(projectPath).ConfigureAwait(false);
            Assert.IsTrue(buildsSuccessfully);
        }

        private static Task<bool> ProjectBuildsSuccessfullyAsync(string projectPath)
        {
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (!File.Exists(projectPath))
                throw new FileNotFoundException("Expected to find a csproj at: " + projectPath, projectPath);

            return ProjectBuildsSuccessfullyAsyncCore(projectPath);
        }

        private static async Task<bool> ProjectBuildsSuccessfullyAsyncCore(string projectPath)
        {
            var projectDir = Path.GetDirectoryName(projectPath);
            var escapedProjectPath = projectPath.Replace("\"", "\\\"");

            var startInfo = new ProcessStartInfo
            {
                ArgumentList = { "build", escapedProjectPath },
                CreateNoWindow = true,
                FileName = "dotnet",
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = projectDir
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var exitCode = await process.WaitForExitAsync().ConfigureAwait(false);

            return exitCode == ExitSuccess;
        }

        private const string TestNamespace = "PocoTestNamespace";
        private const string TestCsprojFilename = "DataAccessGeneratorTest.csproj";
        private const int ExitSuccess = 0;
    }
}
