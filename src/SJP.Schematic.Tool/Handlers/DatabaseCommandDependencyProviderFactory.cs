using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers;

public class DatabaseCommandDependencyProviderFactory : IDatabaseCommandDependencyProviderFactory
{
    public IDatabaseCommandDependencyProvider GetDbDependencies(CommonSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var config = GetConfig(settings);
        return new DatabaseCommandDependencyProvider(config);
    }

    // Configuration is layered so that the most specific source wins:
    //   1. a JSON config file (explicit --config, or a conventional schematic.json), if present
    //   2. environment variables
    //   3. inline --dialect / --connection-string options, which override the above
    //
    // Values sourced from the config file, or passed inline via --dialect / --connection-string,
    // may reference `${NAME}` placeholders, which are resolved against the process environment.
    // The ambient environment (source 2) is left untouched: it isn't rescanned for placeholders,
    // so unrelated variables like PATH are never re-expanded.
    protected static IConfiguration GetConfig(CommonSettings settings)
    {
        var builder = new ConfigurationBuilder();

        var configFilePath = settings.ResolveConfigFilePath();
        if (configFilePath != null)
        {
            var fileConfig = new ConfigurationBuilder().AddJsonFile(configFilePath).Build();
            var resolvedFileValues = fileConfig.AsEnumerable()
                .Where(kvp => kvp.Value != null)
                .ToDictionary(kvp => kvp.Key, kvp => EnvironmentVariableSubstitution.Resolve(kvp.Value));
            builder.AddInMemoryCollection(resolvedFileValues);
        }

        builder.AddEnvironmentVariables();

        var overrides = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(settings.Dialect))
            overrides["Dialect"] = EnvironmentVariableSubstitution.Resolve(settings.Dialect);
        if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
            overrides["ConnectionStrings:Schematic"] = EnvironmentVariableSubstitution.Resolve(settings.ConnectionString);

        if (overrides.Count > 0)
            builder.AddInMemoryCollection(overrides);

        return builder.Build();
    }
}
