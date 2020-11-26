﻿using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Commands
{
    internal sealed class GenerateEfCoreCommand : Command
    {
        public GenerateEfCoreCommand()
            : base("efcore", "Generate a C# project for use with EF Core.")
        {
            var namingConventionArg = new Argument { Arity = ArgumentArity.ExactlyOne }
                .FromAmong("verbatim", "pascal", "camel", "snake");
            namingConventionArg.SetDefaultValue("pascal");
            var namingOption = new Option("--convention", "The naming convention to use. Defaults to 'pascal'.")
            {
                Argument = namingConventionArg
            };
            AddOption(namingOption);

            var projectPathOption = new Option<FileInfo>(
                "--project-path",
                description: "The file path used to save the generated .csproj, e.g. 'C:\\tmp\\Example.DataAccess.EFCore.csproj'. Related files will use the same directory."
            )
            {
                IsRequired = true
            };
            AddOption(projectPathOption);

            var baseNamespaceOption = new Option<string>(
                "--base-namespace",
                description: "A namespace to use that generated classes will belong in. e.g. 'Example.DataAccess.EFCore'."
            )
            {
                IsRequired = true
            };
            AddOption(baseNamespaceOption);

            Handler = CommandHandler.Create<IConsole, FileInfo, string, FileInfo, string, CancellationToken>(static (console, config, convention, projectPath, baseNamespace, cancellationToken) =>
            {
                var handler = new GenerateEfCoreCommandHandler(config);
                return handler.HandleCommandAsync(console, projectPath, baseNamespace, convention, cancellationToken);
            });
        }
    }
}
