using Microsoft.Extensions.Configuration;

namespace SJP.Schematic.Tool.Handlers;

public class DatabaseCommandDependencyProviderFactory : IDatabaseCommandDependencyProviderFactory
{
    public IDatabaseCommandDependencyProvider GetDbDependencies(string filePath)
    {
        var config = GetConfig(filePath);
        return new DatabaseCommandDependencyProvider(config);
    }

    protected static IConfiguration GetConfig(string filePath)
    {
        return new ConfigurationBuilder()
            .AddJsonFile(filePath)
            .AddEnvironmentVariables()
            .Build();
    }
}
