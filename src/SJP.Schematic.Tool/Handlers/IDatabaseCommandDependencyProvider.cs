using Microsoft.Extensions.Configuration;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers;

public interface IDatabaseCommandDependencyProvider
{
    IDbConnectionFactory GetConnectionFactory();
    string GetConnectionString();
    INameTranslator GetNameTranslator(NamingConvention convention);
    ISchematicConnection GetSchematicConnection();
    IConfiguration Configuration { get; }
}