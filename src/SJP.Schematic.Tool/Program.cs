using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace SJP.Schematic.Tool
{
    [VersionOptionFromMember(MemberName = nameof(Version))]
    [Subcommand(typeof(TestCommand), typeof(LintCommand), typeof(GenerateCommand), typeof(ReportCommand))]
    internal sealed class Program
    {
        public static Task<int> Main(string[] args) => CommandLineApplication.ExecuteAsync<Program>(args);

        public string Version { get; } = "Schematic Command Line Tool 1.0.0.0";

        public string[] RemainingArgs { get; set; }

        private Task<int> OnExecuteAsync(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.ShowHelp();
            return Task.FromResult(1);
        }
    }
}
