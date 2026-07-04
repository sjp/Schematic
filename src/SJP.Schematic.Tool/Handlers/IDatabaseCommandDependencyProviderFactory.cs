using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool.Handlers;

public interface IDatabaseCommandDependencyProviderFactory
{
    IDatabaseCommandDependencyProvider GetDbDependencies(CommonSettings settings);
}
