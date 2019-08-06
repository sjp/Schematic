using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using SJP.Schematic.DataAccess;

namespace SJP.Schematic.Tool
{
    [Command(Name = "generate", Description = "Generate ORM classes to interact with the database.")]
    [Subcommand(typeof(GenerateEfCommand), typeof(GenerateOrmLiteCommand), typeof(GeneratePocoCommand))]
    internal sealed class GenerateCommand
    {
        [Option(Description = "How database object names will be translated", LongName = "translator", ShortName = "t", Inherited = true)]
        [AllowedValues("camelcase", "snakecase", "pascalcase", "verbatim", IgnoreCase = true)]
        [DefaultValue("verbatim")]
        public string Translator { get; set; }

        [Option(Description = "The path to save a generated C# project to.", LongName = "project-path", ShortName = "pp", Inherited = true)]
        [LegalFilePath]
        public string ProjectPath { get; set; }

        [Option(Description = "The base namespace to use in the generated project.", LongName = "base-namespace", ShortName = "bn", Inherited = true)]
        [Required]
        public string BaseNamespace { get; set; }

        public INameTranslator GetNameTranslator() => _nameTranslators.GetValueOrDefault(Translator);

        private static readonly IReadOnlyDictionary<string, INameTranslator> _nameTranslators = new Dictionary<string, INameTranslator>
        {
            ["camelcase"] = new CamelCaseNameTranslator(),
            ["snakecase"] = new SnakeCaseNameTranslator(),
            ["pascalcase"] = new PascalCaseNameTranslator(),
            ["verbatim"] = new VerbatimNameTranslator()
        };

        private Task<int> OnExecuteAsync(CommandLineApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            return OnExecuteAsyncCore(application);
        }

        private async Task<int> OnExecuteAsyncCore(CommandLineApplication application)
        {
            await application.Error.WriteLineAsync("You must specify a subcommand.").ConfigureAwait(false);
            application.ShowHelp();
            return 1;
        }
    }
}
