using System.ComponentModel;
using System.IO;
using Spectre.Console.Cli;

namespace SJP.Schematic.Tool.Commands;

internal sealed class OrmCommand : Command<OrmCommand.Settings>
{
    public class Settings : CommonSettings
    {
        [CommandOption("--convention <NAMING_CONVENTION>")]
        [Description("The naming convention to use.")]
        [DefaultValue(NamingConvention.Pascal)]
        public NamingConvention NamingConvention { get; init; }

        [CommandOption("--project-path <FILE>")]
        [Description("The file path used to save the generated .csproj, e.g. 'C:\\tmp\\Example.DataAccess.csproj'. Related files will use the same directory.")]
        public FileInfo? ProjectPath { get; init; }

        [CommandOption("--base-namespace <NAMESPACE>")]
        [Description("A namespace to use that generated classes will belong in. e.g. 'Example.DataAccess'.")]
        public string? BaseNamespace { get; init; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        return 0; // This command acts as a group for subcommands, so it doesn't have direct execution logic.
    }
}