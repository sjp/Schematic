using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Invocation;

namespace SJP.Schematic.Tool.Commands
{
    internal class OrmCommand : Command
    {
        public OrmCommand()
            : base("orm", "Generate ORM projects to interact with a database.")
        {
            AddCommand(new GenerateEfCoreCommand());
            AddCommand(new GenerateOrmLiteCommand());
            AddCommand(new GeneratePocoCommand());

            Handler = CommandHandler.Create<IHelpBuilder>(help =>
            {
                help.Write(this);
                return 1;
            });
        }
    }
}
