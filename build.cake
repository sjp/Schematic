#tool nuget:?package=Codecov&version=1.10.0
#addin nuget:?package=Cake.Codecov&version=0.8.0
#addin nuget:?package=Cake.Coverlet&version=2.4.2
#tool nuget:?package=docfx.console&version=2.51.0
#addin nuget:?package=Cake.DocFx&version=0.13.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
    target = "Default";

var configuration = Argument("configuration", "Release");
if (string.IsNullOrWhiteSpace(configuration))
    configuration = "Release";

const bool reportCoverage = true;

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionFile = GetFiles("./src/*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));
var testProjects = new Lazy<IEnumerable<string>>(() => solution.Value
    .Projects
    .Select(p => p.Path.FullPath)
    .Where(p => p.EndsWith(".Tests.csproj")));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    DotNetCoreClean(solutionFile.FullPath, new DotNetCoreCleanSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal
    });
});

Task("Build")
    .Does(() =>
{
    DotNetCoreBuild(solutionFile.FullPath, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal
    });
});

Task("Pack")
    .Does(() =>
{
    var packageDirectory = "./artifacts/packages";
    EnsureDirectoryExists(packageDirectory);
    DotNetCorePack(solutionFile.FullPath, new DotNetCorePackSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal,
        OutputDirectory = packageDirectory
    });
    Zip(packageDirectory, "./artifacts/packages.zip");
    DeleteDirectory(packageDirectory, new DeleteDirectorySettings { Recursive = true });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .DoesForEach(testProjects.Value, testProject =>
{
    var tempDirectory = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetRandomFileName());
    CreateDirectory(tempDirectory);
    try
    {
        try
        {
            var testSettings = new DotNetCoreTestSettings
            {
                Configuration = configuration
            };

            var coverletSettings = new CoverletSettings
            {
                CollectCoverage = reportCoverage,
                CoverletOutputFormat = CoverletOutputFormat.opencover,
                CoverletOutputDirectory = Directory(tempDirectory),
                CoverletOutputName = "coverage.xml"
            };

            DotNetCoreTest(testProject, testSettings, coverletSettings);
        }
        finally
        {
            if (AppVeyor.IsRunningOnAppVeyor)
            {
                // Upload coverage report
                if (reportCoverage)
                {
                    var coverageReport = GetFiles(tempDirectory + "/**/coverage.xml").First().FullPath;
                    Codecov(coverageReport);
                }
            }
        }
    }
    finally
    {
        DeleteDirectory(tempDirectory, new DeleteDirectorySettings { Recursive = true });
    }
})
.DeferOnError();

Task("Build-Docs")
    .Does(() =>
{
    var docConfigPath = "./docs/docfx.json";
    DocFxMetadata(docConfigPath);
    DocFxBuild(docConfigPath);
});

Task("Docs")
    .IsDependentOn("Build-Docs")
    .Does(() =>
{
    EnsureDirectoryExists("./artifacts");
    Zip("./docs/_site", "./artifacts/docs.zip");
});

Task("Publish-Artifacts")
    .IsDependentOn("Pack")
    .IsDependentOn("Docs")
    .Does(() =>
{
    if (AppVeyor.IsRunningOnAppVeyor)
    {
        var artifactFiles = GetFiles("./artifacts/*");
        foreach (var artifactFile in artifactFiles)
        {
            AppVeyor.UploadArtifact(artifactFile);
        }
    }
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
