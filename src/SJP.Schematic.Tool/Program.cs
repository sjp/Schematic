using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using SJP.Schematic.Tool.Commands;

namespace SJP.Schematic.Tool
{
    internal static class Program
    {
        public static Task<int> Main(string[] args)
        {
            var root = new RootCommand();
            root.Handler = CommandHandler.Create<IConsole, IHelpBuilder>((console, help) =>
            {
                // looks a bit gross but avoids a newline
                console.Out.Write(@"   _____      __                         __  _
  / ___/_____/ /_  ___  ____ ___  ____ _/ /_(_)____
  \__ \/ ___/ __ \/ _ \/ __ `__ \/ __ `/ __/ / ___/
 ___/ / /__/ / / /  __/ / / / / / /_/ / /_/ / /__
/____/\___/_/ /_/\___/_/ /_/ /_/\__,_/\__/_/\___/

The helpful database schema querying tool.

");

                help.Write(root);
                return 1;
            });

            var configFileOption = new Option<FileInfo>(
                "--config",
                getDefaultValue: () => new FileInfo(
                    Path.Combine(Directory.GetCurrentDirectory(),
                    "schematic.config.json"
                )),
                description: "A path to a configuration file used to retrieve options such as connection strings."
            ).ExistingOnly();
            root.AddGlobalOption(configFileOption);

            root.AddCommand(new OrmCommand());
            root.AddCommand(new LintCommand());
            root.AddCommand(new ReportCommand());
            root.AddCommand(new TestCommand());

            var builder = new CommandLineBuilder(root);
            builder.UseHelp();
            builder.UseVersionOption();
            builder.UseDebugDirective();
            builder.UseParseErrorReporting();
            builder.ParseResponseFileAs(ResponseFileHandling.ParseArgsAsSpaceSeparated);
            builder.CancelOnProcessTermination();
            builder.UseExceptionHandler(HandleException);

            var parser = builder.Build();
            return parser.InvokeAsync(args);
        }

        private static void HandleException(Exception exception, InvocationContext context)
        {
            context.Console.ResetTerminalForegroundColor();
            context.Console.SetTerminalForegroundRed();

            if (exception is TargetInvocationException tie &&
                tie.InnerException is object)
            {
                exception = tie.InnerException;
            }

            if (exception is OperationCanceledException)
            {
                context.Console.Error.WriteLine("...canceled.");
            }
            // don't know why, but oracle wraps cancellation in its own exception
            else if (exception is OracleException oex && oex.Number == 1013)
            {
                context.Console.Error.WriteLine("...canceled.");
            }
            else if (exception is CommandException command)
            {
                context.Console.Error.WriteLine($"The '{context.ParseResult.CommandResult.Command.Name}' command failed:");
                context.Console.Error.WriteLine($"    {command.Message}");

                if (command.InnerException != null)
                {
                    context.Console.Error.WriteLine();
                    context.Console.Error.WriteLine(command.InnerException.ToString());
                }
            }
            else
            {
                context.Console.Error.WriteLine("An unhandled exception has occurred: ");
                context.Console.Error.WriteLine(exception.ToString());
            }

            context.Console.ResetTerminalForegroundColor();

            context.ResultCode = 1;
        }
    }
}
