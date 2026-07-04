using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.IO;

namespace SJP.Schematic.Tool.Commands;

public abstract class CommonSettings : CommandSettings
{
    internal const string DefaultConfigFileName = "schematic.json";

    [CommandOption("-c|--config <FILE>")]
    [Description("A path to a configuration file used to retrieve options such as connection strings.")]
    public FileInfo? ConfigFile { get; init; }

    [CommandOption("-d|--dialect <DIALECT>")]
    [Description("The database dialect to connect to. One of: mysql, oracle, postgresql, sqlserver, sqlite. Overrides any value in the configuration file.")]
    public string? Dialect { get; init; }

    [CommandOption("--connection-string <CONNECTION_STRING>")]
    [Description("A connection string used to connect to the database. Overrides any value in the configuration file.")]
    public string? ConnectionString { get; init; }

    /// <summary>
    /// Resolves the configuration file path to use, if any. Returns the explicitly provided
    /// <c>--config</c> file, otherwise a conventional <c>schematic.json</c> in the current
    /// directory when it exists, otherwise <see langword="null"/>.
    /// </summary>
    public string? ResolveConfigFilePath()
    {
        if (ConfigFile != null)
            return ConfigFile.FullName;

        // Resolve to an absolute path: the configuration builder resolves relative paths against
        // the application base directory, whereas discovery is relative to the working directory.
        var defaultPath = Path.GetFullPath(DefaultConfigFileName);
        return File.Exists(defaultPath) ? defaultPath : null;
    }

    public override ValidationResult Validate()
    {
        var error = ValidateConnectionOptions(
            hasConfigFile: ConfigFile != null,
            hasDialect: !string.IsNullOrWhiteSpace(Dialect),
            hasConnectionString: !string.IsNullOrWhiteSpace(ConnectionString),
            defaultConfigExists: ConfigFile == null && File.Exists(DefaultConfigFileName));

        return error == null ? ValidationResult.Success() : ValidationResult.Error(error);
    }

    // Pure decision logic, kept separate so it can be unit-tested without a file system.
    internal static string? ValidateConnectionOptions(bool hasConfigFile, bool hasDialect, bool hasConnectionString, bool defaultConfigExists)
    {
        if (hasConfigFile || defaultConfigExists)
            return null;

        if (hasDialect && hasConnectionString)
            return null;

        if (hasDialect ^ hasConnectionString)
            return "Both --dialect and --connection-string must be provided together when no configuration file is used.";

        return "No database connection was specified. Provide a configuration file with --config, "
            + "pass --dialect and --connection-string directly, or run 'schematic init' to create a schematic.json file.";
    }
}
