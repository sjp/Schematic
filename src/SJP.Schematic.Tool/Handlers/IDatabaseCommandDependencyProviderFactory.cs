namespace SJP.Schematic.Tool.Handlers;

public interface IDatabaseCommandDependencyProviderFactory
{
    IDatabaseCommandDependencyProvider GetDbDependencies(string filePath);
}