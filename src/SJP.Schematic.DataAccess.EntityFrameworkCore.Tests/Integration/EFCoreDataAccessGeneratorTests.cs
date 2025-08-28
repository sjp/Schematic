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

namespace SJP.Schematic.DataAccess.EntityFrameworkCore.Tests.Integration;

internal sealed class EFCoreDataAccessGeneratorTests : SqliteTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    [OneTimeSetUp]
    public async Task Init()
    {
        await DbConnection.ExecuteAsync(@"create table dal_test_table_1 (
    testint integer not null primary key autoincrement,
    testdecimal numeric default 2.45,
    testblob blob default X'DEADBEEF',
    testdatetime datetime default CURRENT_TIMESTAMP,
    teststring text default 'test'
)", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("create view dal_test_view_1 as select * from dal_test_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    [OneTimeTearDown]
    public async Task CleanUp()
    {
        await DbConnection.ExecuteAsync("drop view dal_test_view_1", CancellationToken.None).ConfigureAwait(false);
        await DbConnection.ExecuteAsync("drop table dal_test_table_1", CancellationToken.None).ConfigureAwait(false);
    }

    [Test]
    public async Task GenerateAsync_GivenDatabaseWithTablesAndViews_GeneratesFilesInExpectedLocations()
    {
        using var tempDir = new TemporaryDirectory();
        var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);
        var tablesDir = Path.Combine(tempDir.DirectoryPath, "Tables");
        var viewsDir = Path.Combine(tempDir.DirectoryPath, "Views");

        var expectedAppContextPath = Path.Combine(tempDir.DirectoryPath, "AppContext.cs");
        var expectedTable1Path = Path.Combine(tablesDir, "Main", "DalTestTable1.cs");
        var expectedView1Path = Path.Combine(viewsDir, "Main", "DalTestView1.cs");

        var mockFs = new MockFileSystem(new Dictionary<string, MockFileData>(StringComparer.Ordinal)
        {
            [tempDir.DirectoryPath + Path.PathSeparator] = new MockDirectoryData(),
            [expectedAppContextPath] = new MockFileData(Array.Empty<byte>()),
            [expectedTable1Path] = new MockFileData(Array.Empty<byte>()),
            [expectedView1Path] = new MockFileData(Array.Empty<byte>())
        });

        var nameTranslator = new PascalCaseNameTranslator();
        var generator = new EFCoreDataAccessGenerator(mockFs, Database, new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults), nameTranslator);
        await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(mockFs.FileExists(projectPath), Is.True);
            Assert.That(mockFs.FileExists(expectedAppContextPath), Is.True);
            Assert.That(mockFs.Directory.Exists(tablesDir), Is.True);
            Assert.That(mockFs.Directory.Exists(viewsDir), Is.True);
            Assert.That(mockFs.FileExists(expectedTable1Path), Is.True);
            Assert.That(mockFs.FileExists(expectedView1Path), Is.True);
        }
    }

    [Test, Ignore("Skipping to improve unit test perf")]
    public async Task GenerateAsync_GivenDatabaseWithoutTablesOrViews_BuildsProjectSuccessfully()
    {
        using var tempDir = new TemporaryDirectory();
        var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

        var database = new EmptyRelationalDatabase(Database.IdentifierDefaults);

        var fileSystem = new FileSystem();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new PascalCaseNameTranslator();
        var generator = new EFCoreDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);
        await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

        var buildsSuccessfully = await ProjectBuildsSuccessfullyAsync(projectPath).ConfigureAwait(false);
        Assert.That(buildsSuccessfully, Is.True);
    }

    [Test, Ignore("Skipping to improve unit test perf")]
    public async Task GenerateAsync_GivenDatabaseWithTablesAndViews_BuildsProjectSuccessfully()
    {
        using var tempDir = new TemporaryDirectory();
        var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

        var fileSystem = new FileSystem();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new PascalCaseNameTranslator();
        var generator = new EFCoreDataAccessGenerator(fileSystem, Database, commentProvider, nameTranslator);
        await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

        var buildsSuccessfully = await ProjectBuildsSuccessfullyAsync(projectPath).ConfigureAwait(false);
        Assert.That(buildsSuccessfully, Is.True);
    }

    private static Task<bool> ProjectBuildsSuccessfullyAsync(string projectPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectPath);
        if (!File.Exists(projectPath))
            throw new FileNotFoundException("Expected to find a csproj at: " + projectPath, projectPath);

        return ProjectBuildsSuccessfullyAsyncCore(projectPath);
    }

    private static async Task<bool> ProjectBuildsSuccessfullyAsyncCore(string projectPath)
    {
        var projectDir = Path.GetDirectoryName(projectPath);
        var escapedProjectPath = projectPath.Replace("\"", "\\\"", StringComparison.Ordinal);

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
        await process.WaitForExitAsync().ConfigureAwait(false);

        return process.ExitCode == ExitSuccess;
    }

    private const string TestNamespace = "EFCoreTestNamespace";
    private const string TestCsprojFilename = "DataAccessGeneratorTest.csproj";
    private const int ExitSuccess = 0;
}