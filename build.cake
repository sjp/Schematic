#tool nuget:?package=Codecov&version=1.13.0
#addin nuget:?package=Cake.Codecov&version=1.0.1

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

Task("Build")
    .Does(() =>
{
    DotNetCoreBuild(solutionFile.FullPath, new DotNetCoreBuildSettings
    {
        Configuration = configuration,
        Verbosity = DotNetCoreVerbosity.Minimal
    });
});

Task("Run-Unit-Tests")
    .IsDependentOn("Build")
    .DoesForEach(testProjects.Value, testProject =>
{
    var deleteDirSettings = new DeleteDirectorySettings
    {
        Force = true,
        Recursive = true
    };
    var dirs = GetDirectories("./**/TestResults");
    DeleteDirectories(dirs, deleteDirSettings);

    try
    {
        var testSettings = new DotNetCoreTestSettings
        {
            Configuration = configuration,
            ArgumentCustomization = builder =>
            {
                return builder.AppendSwitchQuoted("--collect", "XPlat Code Coverage");
            }
        };

        DotNetCoreTest(testProject, testSettings);
    }
    finally
    {
        if (AppVeyor.IsRunningOnAppVeyor)
        {
            // Upload coverage report
            if (reportCoverage)
            {
                var coverageReport = GetFiles("./**/coverage.cobertura.xml").First().FullPath;
                Codecov(coverageReport);
            }
        }

        dirs = GetDirectories("./**/TestResults");
        DeleteDirectories(dirs, deleteDirSettings);
    }
})
.DeferOnError();

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
