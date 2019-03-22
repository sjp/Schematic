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
    teststring text default 'test'
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
            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "EFCoreMockTest");

            try
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);

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

                var nameTranslator = new PascalCaseNameTranslator();
                var generator = new EFCoreDataAccessGenerator(mockFs, Database, new EmptyRelationalDatabaseCommentProvider(), nameTranslator);
                generator.Generate(projectPath, TestNamespace);

                Assert.Multiple(() =>
                {
                    Assert.IsTrue(mockFs.FileExists(projectPath));
                    Assert.IsTrue(mockFs.FileExists(expectedAppContextPath));
                    Assert.IsTrue(mockFs.Directory.Exists(tablesDir));
                    Assert.IsTrue(mockFs.FileExists(expectedTable1Path));
                });
            }
            finally
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);
            }
        }

        [Test]
        public async Task GenerateAsync_GivenDatabaseWithTables_GeneratesFilesInExpectedLocations()
        {
            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "EFCoreMockTestAsync");

            try
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);

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

                var nameTranslator = new PascalCaseNameTranslator();
                var generator = new EFCoreDataAccessGenerator(mockFs, Database, new EmptyRelationalDatabaseCommentProvider(), nameTranslator);
                await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

                Assert.Multiple(() =>
                {
                    Assert.IsTrue(mockFs.FileExists(projectPath));
                    Assert.IsTrue(mockFs.FileExists(expectedAppContextPath));
                    Assert.IsTrue(mockFs.Directory.Exists(tablesDir));
                    Assert.IsTrue(mockFs.FileExists(expectedTable1Path));
                });
            }
            finally
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);
            }
        }

        [Test]
        public void Generate_GivenDatabaseWithoutTables_BuildsProjectSuccessfully()
        {
            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "EFCoreTestEmpty");

            try
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);

                var projectPath = Path.Combine(testProjectDir, "DataAccessGeneratorTest.csproj");

                var database = new EmptyRelationalDatabase(Database.Dialect, Database.IdentifierDefaults);

                var fileSystem = new FileSystem();
                var commentProvider = new EmptyRelationalDatabaseCommentProvider();
                var nameTranslator = new PascalCaseNameTranslator();
                var generator = new EFCoreDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);
                generator.Generate(projectPath, TestNamespace);

                var buildsSuccessfully = ProjectBuildsSuccessfully(projectPath);
                Assert.IsTrue(buildsSuccessfully);
            }
            finally
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);
            }
        }

        [Test]
        public void Generate_GivenDatabaseWithTables_BuildsProjectSuccessfully()
        {
            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "EFCoreTest");

            try
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);

                var projectPath = Path.Combine(testProjectDir, "DataAccessGeneratorTest.csproj");

                var fileSystem = new FileSystem();
                var commentProvider = new EmptyRelationalDatabaseCommentProvider();
                var nameTranslator = new PascalCaseNameTranslator();
                var generator = new EFCoreDataAccessGenerator(fileSystem, Database, commentProvider, nameTranslator);
                generator.Generate(projectPath, TestNamespace);

                var buildsSuccessfully = ProjectBuildsSuccessfully(projectPath);
                Assert.IsTrue(buildsSuccessfully);
            }
            finally
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);
            }
        }

        [Test]
        public async Task GenerateAsync_GivenDatabaseWithoutTables_BuildsProjectSuccessfully()
        {
            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "EFCoreTestEmptyAsync");

            try
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);

                var projectPath = Path.Combine(testProjectDir, "DataAccessGeneratorTest.csproj");

                var database = new EmptyRelationalDatabase(Database.Dialect, Database.IdentifierDefaults);

                var fileSystem = new FileSystem();
                var commentProvider = new EmptyRelationalDatabaseCommentProvider();
                var nameTranslator = new PascalCaseNameTranslator();
                var generator = new EFCoreDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);
                await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

                var buildsSuccessfully = ProjectBuildsSuccessfully(projectPath);
                Assert.IsTrue(buildsSuccessfully);
            }
            finally
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);
            }
        }

        [Test]
        public async Task GenerateAsync_GivenDatabaseWithTables_BuildsProjectSuccessfully()
        {
            var testProjectDir = Path.Combine(Environment.CurrentDirectory, "EFCoreTestAsync");

            try
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);

                var projectPath = Path.Combine(testProjectDir, "DataAccessGeneratorTest.csproj");

                var fileSystem = new FileSystem();
                var commentProvider = new EmptyRelationalDatabaseCommentProvider();
                var nameTranslator = new PascalCaseNameTranslator();
                var generator = new EFCoreDataAccessGenerator(fileSystem, Database, commentProvider, nameTranslator);
                await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

                var buildsSuccessfully = ProjectBuildsSuccessfully(projectPath);
                Assert.IsTrue(buildsSuccessfully);
            }
            finally
            {
                if (Directory.Exists(testProjectDir))
                    Directory.Delete(testProjectDir, true);
            }
        }

        private static bool ProjectBuildsSuccessfully(string projectPath)
        {
            if (projectPath.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(projectPath));
            if (!File.Exists(projectPath))
                throw new FileNotFoundException("Expected to find a csproj at: " + projectPath, projectPath);

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
            using (var process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();
                process.WaitForExit();

                return process.ExitCode == ExitSuccess;
            }
        }

        private const string TestNamespace = "EFCoreTestNamespace";
        private const int ExitSuccess = 0;
    }
}
