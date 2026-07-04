using System;
using System.Collections.Generic;
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
    protected static IConfiguration GetConfig(CommonSettings settings)
    {
        var builder = new ConfigurationBuilder();

        var configFilePath = settings.ResolveConfigFilePath();
        if (configFilePath != null)
            builder.AddJsonFile(configFilePath);

        builder.AddEnvironmentVariables();

        var overrides = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(settings.Dialect))
            overrides["Dialect"] = settings.Dialect;
        if (!string.IsNullOrWhiteSpace(settings.ConnectionString))
            overrides["ConnectionStrings:Schematic"] = settings.ConnectionString;

        if (overrides.Count > 0)
            builder.AddInMemoryCollection(overrides);

        return builder.Build();
    }
}
