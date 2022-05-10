using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Sqlite;
using SJP.Schematic.Tests.Utilities;
using SJP.Schematic.Tests.Utilities.Integration;

namespace SJP.Schematic.DataAccess.OrmLite.Tests.Integration;

internal sealed class OrmLiteSakilaDataAccessGeneratorTests : SakilaTest
{
    private IRelationalDatabase Database => new SqliteRelationalDatabase(Connection, IdentifierDefaults, Pragma);

    [Test, Ignore("Skipping to improve unit test perf")]
    public async Task GenerateAsync_GivenDatabaseWithoutTablesOrViews_BuildsProjectSuccessfully()
    {
        using var tempDir = new TemporaryDirectory();
        var projectPath = Path.Combine(tempDir.DirectoryPath, TestCsprojFilename);

        var database = new EmptyRelationalDatabase(Database.IdentifierDefaults);

        var fileSystem = new FileSystem();
        var commentProvider = new EmptyRelationalDatabaseCommentProvider(IdentifierDefaults);
        var nameTranslator = new PascalCaseNameTranslator();
        var generator = new OrmLiteDataAccessGenerator(fileSystem, database, commentProvider, nameTranslator);
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
        var generator = new OrmLiteDataAccessGenerator(fileSystem, Database, commentProvider, nameTranslator);
        await generator.GenerateAsync(projectPath, TestNamespace).ConfigureAwait(false);

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

    private const string TestNamespace = "OrmLiteTestNamespace";
    private const string TestCsprojFilename = "DataAccessGeneratorTest.csproj";
    private const int ExitSuccess = 0;
}