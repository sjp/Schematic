using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration
{
    internal sealed class EFCoreDataAccessGeneratorTests : SqliteTest
    {
        private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults);

        [OneTimeSetUp]
        public Task Init()
        {
            return DbConnection.ExecuteAsync(@"create table dal_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)", CancellationToken.None);
        }

        [OneTimeTearDown]
        public Task CleanUp()
        {
            return DbConnection.ExecuteAsync("drop table dal_test_table_1", CancellationToken.None);
        }

        [Test]
        public async Task Generate_GivenDatabaseWithTables_GeneratesFilesInExpectedLocations()
        {
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);
            var tablesDir = Path.Combine(tempDir.DirectoryPath, "Tables");

            var expectedAppContextPath = Path.Combine(tempDir.DirectoryPath, "AppContext.cs");
            var expectedTable1Path = Path.Combine(tablesDir, "Main", "DalTestTable1.cs");

            var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                [tempDir.DirectoryPath + Path.PathSeparator] = new MockDirectoryData(),
                [expectedAppContextPath] = MockFileData.NullObject,
                [expectedTable1Path] = MockFileData.NullObject
            });

            var nameTranslator = new PascalCaseNameTranslator();
            var generator = new EFCoreDataAccessGenerator(mockFs, Database, new EmptyRelationalDatabaseCommentProvider(), nameTranslator);
            await generator.Generate(projectPath, TestNamespace).ConfigureAwait(false);

            Assert.Multiple(() =>
            {
                Assert.That(mockFs.FileExists(projectPath), Is.True);
                Assert.That(mockFs.FileExists(expectedAppContextPath), Is.True);
                Assert.That(mockFs.Directory.Exists(tablesDir), Is.True);
                Assert.That(mockFs.FileExists(expectedTable1Path), Is.True);
            });
        }

        [Test]
        public async Task Generate_GivenDatabaseWithoutTables_BuildsProjectSuccessfully()
        {
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

            var database = new EmptyRelationalDatabase(Database.IdentifierDefaults);

            var fileSystem = new FileSystem();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new PascalCaseNameTranslator();
            var generator = new EFCoreDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);
            await generator.Generate(projectPath, TestNamespace).ConfigureAwait(false);

            var buildsSuccessfully = await ProjectBuildsSuccessfullyAsync(projectPath).ConfigureAwait(false);
            Assert.That(buildsSuccessfully, Is.True);
        }

        [Test]
        public async Task Generate_GivenDatabaseWithTables_BuildsProjectSuccessfully()
        {
            using var tempDir = new TemporaryDirectory();
            var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

            var fileSystem = new FileSystem();
            var commentProvider = new EmptyRelationalDatabaseCommentProvider();
            var nameTranslator = new PascalCaseNameTranslator();
            var generator = new EFCoreDataAccessGenerator(fileSystem, Database, commentProvider, nameTranslator);
            await generator.Generate(projectPath, TestNamespace).ConfigureAwait(false);

            var buildsSuccessfully = await ProjectBuildsSuccessfullyAsync(projectPath).ConfigureAwait(false);
            Assert.That(buildsSuccessfully, Is.True);
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

        private const string TestNamespace = "EFCoreTestNamespace";
        private const string TestCsprojFilename = "DataAccessGeneratorTest.csproj";
        private const int ExitSuccess = 0;
    }
}
