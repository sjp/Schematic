using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Tool
{
    [VersionOptionFromMember(MemberName = nameof(Version))]
    internal class Program
    {
        public Program(CommandLineApplication application, IConsole console)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));

            _console = console ?? throw new ArgumentNullException(nameof(console));

            _application.ExtendedHelpText = @"
Commands:
  test          Tests a connection for availability
  lint          Provides a lint report for potential schema issues
  report        Creates a graphical report on database schema and relationships

For more information, add the 'help' argument for a command.
e.g. schematic test help";

        }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Argument(0, Name = "command", Description = "The command to run")]
        [AllowedCommands(StringComparison.OrdinalIgnoreCase, "test", "lint", "report")]
        public string Command { get; set; }

        public string Version { get; } = "Schematic Command Line Tool 1.0.0.0";

        //[Option(Description = "A connection string", LongName = "connectionString", ShortName = "cs")]
        //public string ConnectionString { get; set; }
        //
        //[Option(Description = "The database dialect", LongName = "databaseDialect", ShortName = "dd")]
        //public string DatabaseDialect { get; set; }

        public string[] RemainingArgs { get; set; }

        private int OnExecute()
        {
            if (string.IsNullOrWhiteSpace(Command))
            {
                _application.ShowHelp();
                return 1;
            }

            switch (Command)
            {
                //case 1:
                //    return CommandLineApplication.Execute<Git>(RemainingArgs);
                case "2":
                    return CommandLineApplication.Execute<Docker>(RemainingArgs);
                //case 3:
                //    return Npm.Main(RemainingArgs);
                default:
                    Console.Error.WriteLine("Unknown option");
                    return 1;
            }
        }

        private readonly CommandLineApplication _application;
        private readonly IConsole _console;
    }

    internal class TestCommand
    {

    }

    // modified version of AllowedValues from base (altered error message)
    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class AllowedCommandsAttribute : ValidationAttribute
    {
        public AllowedCommandsAttribute(params string[] allowedValues)
            : this(StringComparison.CurrentCulture, allowedValues)
        {
        }

        public AllowedCommandsAttribute(StringComparison comparer, params string[] allowedValues)
            : base(GetDefaultError(allowedValues))
        {
            _allowedValues = allowedValues ?? Array.Empty<string>();
            Comparer = comparer;
        }

        private static string GetDefaultError(string[] allowedValues)
        {
            const string message = @"Invalid command '{0}'.

Allowed commands are:
";
            var formattedValues = allowedValues.Select(v => "  " + v).Join(Environment.NewLine);
            return message + formattedValues + Environment.NewLine;
        }

        public StringComparison Comparer { get; set; }

        public bool IgnoreCase
        {
            get
            {
                return Comparer == StringComparison.CurrentCultureIgnoreCase
                    || Comparer == StringComparison.InvariantCultureIgnoreCase
                    || Comparer == StringComparison.OrdinalIgnoreCase;
            }
            set
            {
                Comparer = value
                    ? StringComparison.CurrentCultureIgnoreCase
                    : StringComparison.CurrentCulture;
            }
        }

        /// <inheritdoc />
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is string str)
            {
                for (var i = 0; i < _allowedValues.Length; i++)
                {
                    if (str.Equals(_allowedValues[i], Comparer))
                    {
                        return ValidationResult.Success;
                    }
                }
            }

            return new ValidationResult(FormatErrorMessage(value as string));
        }

        private readonly string[] _allowedValues;
    }

    /// <summary>
    /// In this example, each command a nested class type.
    /// This isn't required. See the <see cref="Git"/> example for a sample on how to use inheritance.
    /// </summary>
    [Command(Name = "fake-docker", Description = "A self-sufficient runtime for containers"),
        Subcommand("container", typeof(Containers)),
        Subcommand("image", typeof(Images))]
    class Docker
    {
        private int OnExecute(CommandLineApplication app, IConsole console)
        {
            console.WriteLine("You must specify at a subcommand.");
            app.ShowHelp();
            return 1;
        }

        /// <summary>
        /// <see cref="HelpOptionAttribute"/> must be declared on each type that supports '--help'.
        /// Compare to the inheritance example, in which <see cref="GitCommandBase"/> delcares it
        /// once so that all subcommand types automatically support '--help'.
        /// </summary>
        [Command(Description = "Manage containers"),
            Subcommand("ls", typeof(List)),
            Subcommand("run", typeof(Run))]
        private class Containers
        {
            private int OnExecute(IConsole console)
            {
                console.Error.WriteLine("You must specify an action. See --help for more details.");
                return 1;
            }

            [Command(Description = "List containers"), HelpOption]
            private class List
            {
                [Option(Description = "Show all containers (default shows just running)")]
                public bool All { get; }

                private void OnExecute(IConsole console)
                {
                    console.WriteLine(string.Join("\n",
                        "CONTAINERS",
                        "----------------",
                        "jubilant_jackson",
                        "lucid_torvalds"));
                }
            }

            [Command(Description = "Run a command in a new container",
                AllowArgumentSeparator = true,
                ThrowOnUnexpectedArgument = false)]
            private class Run
            {
                [Required(ErrorMessage = "You must specify the image name")]
                [Argument(0, Description = "The image for the new container")]
                public string Image { get; }

                [Option("--name", Description = "Assign a name to the container")]
                public string Name { get; }

                /// <summary>
                /// When ThrowOnUnexpectedArgument is valids, any unrecognized arguments
                /// will be collected and set in this property, when set.
                /// </summary>
                public string[] RemainingArguments { get; }

                private void OnExecute(IConsole console)
                {
                    console.WriteLine($"Would have run {Image} (name = {Name}) with args => {ArgumentEscaper.EscapeAndConcatenate(RemainingArguments)}");
                }
            }
        }

        [Command(Description = "Manage images"),
            Subcommand("ls", typeof(List))]
        private class Images
        {
            private int OnExecute(IConsole console)
            {
                console.Error.WriteLine("You must specify an action. See --help for more details.");
                return 1;
            }


            [Command(Description = "List images",
                ThrowOnUnexpectedArgument = false)]
            private class List
            {
                [Option(Description = "Show all containers (default shows just running)")]
                public bool All { get; }

                private IReadOnlyList<string> RemainingArguments { get; }

                private void OnExecute(IConsole console)
                {
                    console.WriteLine(string.Join("\n",
                        "IMAGES",
                        "--------------------",
                        "microsoft/dotnet:2.0"));
                }
            }
        }
    }
}
