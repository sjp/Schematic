﻿using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Threading;
using SJP.Schematic.Tool.Handlers;

namespace SJP.Schematic.Tool.Commands;

internal sealed class GenerateEfCoreCommand : Command
{
    public GenerateEfCoreCommand()
        : base("efcore", "Generate a C# project for use with EF Core.")
    {
        var namingOption = new Option<NamingConvention>(
            "--convention",
            () => NamingConvention.Pascal,
            "The naming convention to use."
        );
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

        Handler = CommandHandler.Create<IConsole, FileInfo, NamingConvention, FileInfo, string, CancellationToken>(static (console, config, convention, projectPath, baseNamespace, cancellationToken) =>
        {
            var handler = new GenerateEfCoreCommandHandler(console, config);
            return handler.HandleCommandAsync(projectPath, baseNamespace, convention, cancellationToken);
        });
    }
}