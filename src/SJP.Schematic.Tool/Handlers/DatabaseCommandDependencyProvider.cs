using System;
using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers;

public class DatabaseCommandDependencyProvider : IDatabaseCommandDependencyProvider
{
    public DatabaseCommandDependencyProvider(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    private DialectDescriptor GetDialectDescriptor()
    {
        var dialect = Configuration.GetValue<string>("Dialect");
        if (dialect.IsNullOrWhiteSpace())
            throw new InvalidOperationException(nameof(dialect));

        return DialectRegistry.Get(dialect);
    }

    public IDbConnectionFactory GetConnectionFactory()
    {
        var descriptor = GetDialectDescriptor();
        var connectionString = GetConnectionString();

        return descriptor.CreateConnectionFactory(connectionString);
    }

    public ISchematicConnection GetSchematicConnection()
    {
        var descriptor = GetDialectDescriptor();
        var connectionFactory = descriptor.CreateConnectionFactory(GetConnectionString());

        return new SchematicConnection(connectionFactory, descriptor.CreateDialect());
    }

    public IRelationalDatabaseProvider GetRelationalDatabaseProvider(ISchematicConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        return GetDialectDescriptor().CreateDatabaseProvider(connection);
    }

    public string GetConnectionString()
    {
        var connectionString = Configuration.GetConnectionString("Schematic");
        if (connectionString.IsNullOrWhiteSpace())
            throw new InvalidOperationException(nameof(connectionString));

        return connectionString;
    }

    public INameTranslator GetNameTranslator(NamingConvention convention)
    {
        return convention switch
        {
            NamingConvention.Verbatim => new VerbatimNameTranslator(),
            NamingConvention.Pascal => new PascalCaseNameTranslator(),
            NamingConvention.Camel => new CamelCaseNameTranslator(),
            NamingConvention.Snake => new SnakeCaseNameTranslator(),
            _ => throw new NotSupportedException($"The given naming convention is not supported {convention}, expected one of: ..."),
        };
    }
}
