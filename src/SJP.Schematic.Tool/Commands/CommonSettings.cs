using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;

namespace SJP.Schematic.Tool.Commands;

public abstract class CommonSettings : CommandSettings
{
    [CommandOption("-c|--config <FILE>")]
    [Description("A path to a configuration file used to retrieve options such as connection strings.")]
    public FileInfo? ConfigFile { get; init; }
}
