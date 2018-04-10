//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
if (string.IsNullOrWhiteSpace(target))
    target = "Default";

var configuration = Argument("configuration", "Release");
if (string.IsNullOrWhiteSpace(configuration))
    configuration = "Release";

var testFramework = "netcoreapp2.0";

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var solutionFile = GetFiles("./src/*.sln").First();
var solution = new Lazy<SolutionParserResult>(() => ParseSolution(solutionFile));
var testProjects = new Lazy<IEnumerable<string>>(() => solution.Value
    .Projects
    .Select(p => p.Path.FullPath)
    .Where(p => p.EndsWith(".Tests.csproj")));
    //.Select(p => new FilePath(p).GetDirectory().FullPath));

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
    DotNetCoreRestore(solutionFile.FullPath);
    DotNetCoreBuild(solutionFile.FullPath, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal,
        NoRestore = true
    });
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
            DotNetCoreTest(testProject, new DotNetCoreTestSettings
            {
                Configuration = configuration,
                Logger = "trx",
                ArgumentCustomization = a => a.AppendSwitchQuoted("--results-directory", tempDirectory)
            });
        }
        finally
        {
            if (AppVeyor.IsRunningOnAppVeyor)
            {
                // dotnet test cannot do more than one target framework per TRX file
                // AppVeyor seems to ignore all but the first TRX upload– perhaps because the test names are identical
                // https://github.com/Microsoft/vstest/issues/880#issuecomment-341912021
                foreach (var testResultsFile in GetFiles(tempDirectory + "/**/*.trx"))
                    AppVeyor.UploadTestResults(testResultsFile, AppVeyorTestResultsType.MSTest);
            }
        }
    }
    finally
    {
        DeleteDirectory(tempDirectory, new DeleteDirectorySettings { Recursive = true });
    }
})
.DeferOnError();

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Run-Unit-Tests");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
