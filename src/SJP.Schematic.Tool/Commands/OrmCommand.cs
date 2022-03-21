using System.CommandLine;

namespace SJP.Schematic.Tool.Commands;

internal sealed class OrmCommand : Command
{
    public OrmCommand()
        : base("orm", "Generate ORM projects to interact with a database.")
    {
        AddCommand(new GenerateEfCoreCommand());
        AddCommand(new GenerateOrmLiteCommand());
        AddCommand(new GeneratePocoCommand());
    }
}