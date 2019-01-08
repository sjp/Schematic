using System;
using System.Security;
using System.Text;
using System.Threading;
using Humanizer;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class EFCoreDbContextBuilder
    {
        public EFCoreDbContextBuilder(IRelationalDatabase database, INameTranslator nameTranslator, string baseNamespace, string indent = "    ")
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            NameTranslator = nameTranslator ?? throw new ArgumentNullException(nameof(nameTranslator));
            Namespace = baseNamespace;
            Indent = indent ?? throw new ArgumentNullException(nameof(indent));
        }

        protected IRelationalDatabase Database { get; }

        protected INameTranslator NameTranslator { get; }

        protected string Namespace { get; }

        protected string Indent { get; }

        public string Generate()
        {
            var builder = new StringBuilder();

            builder
                .AppendLine("using System;")
                .AppendLine("using Microsoft.EntityFrameworkCore;")
                .AppendLine()
                .Append("namespace ")
                .AppendLine(Namespace)
                .AppendLine("{")
                .Append(Indent)
                .AppendLine("public class AppContext : DbContext")
                .Append(Indent)
                .AppendLine("{");

            var tableIndent = Indent + Indent;
            var contextIndent = tableIndent + Indent;
            var modelBuilder = new EFCoreModelBuilder(NameTranslator, contextIndent, Indent);

            var missingFirstLine = true;
            var tables = Database.GetAllTables(CancellationToken.None).GetAwaiter().GetResult();
            foreach (var table in tables)
            {
                if (!missingFirstLine)
                    builder.AppendLine();
                missingFirstLine = false;

                var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
                var className = NameTranslator.TableToClassName(table.Name);
                var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                    ? schemaNamespace + "." + className
                    : className;

                var setName = className.Pluralize();

                var escapedTableName = !schemaNamespace.IsNullOrWhiteSpace()
                    ? SecurityElement.Escape(table.Name.Schema + "." + table.Name.LocalName)
                    : SecurityElement.Escape(table.Name.LocalName);
                var dbSetComment = "Accesses the <c>" + escapedTableName + "</c> table.";
                builder.AppendComment(tableIndent, dbSetComment);

                builder.Append(tableIndent)
                    .Append("public DbSet<")
                    .Append(qualifiedClassName)
                    .Append("> ")
                    .Append(setName)
                    .AppendLine(" { get; set; }");

                modelBuilder.AddTable(table);
            }

            var sequences = Database.GetAllSequences(CancellationToken.None).GetAwaiter().GetResult();
            foreach (var sequence in sequences)
            {
                modelBuilder.AddSequence(sequence);
            }

            if (modelBuilder.HasRecords)
            {
                if (!missingFirstLine)
                    builder.AppendLine();

                AppendModelBuilderComment(builder, tableIndent, ModelBuilderMethodSummaryComment, ModelBuilderMethodParamComment);

                var methodBody = modelBuilder.ToString();
                builder.Append(tableIndent)
                    .AppendLine("protected override void OnModelCreating(ModelBuilder modelBuilder)")
                    .Append(tableIndent)
                    .AppendLine("{")
                    .Append(methodBody)
                    .Append(tableIndent)
                    .AppendLine("}");
            }

            builder.Append(Indent)
                .AppendLine("}")
                .Append("}");

            return builder.ToString();
        }

        protected static StringBuilder AppendModelBuilderComment(StringBuilder builder, string indent, string summary, string modelBuilderParam)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (indent == null)
                throw new ArgumentNullException(nameof(indent));
            if (summary == null)
                throw new ArgumentNullException(nameof(summary));
            if (modelBuilderParam == null)
                throw new ArgumentNullException(nameof(modelBuilderParam));

            var escapedSummary = SecurityElement.Escape(summary);
            var escapedModelBuilderParam = SecurityElement.Escape(modelBuilderParam);

            return builder.Append(indent)
                .AppendLine("/// <summary>")
                .Append(indent)
                .Append("/// ")
                .AppendLine(escapedSummary)
                .Append(indent)
                .AppendLine("/// </summary>")
                .Append(indent)
                .Append("/// <param name=\"modelBuilder\">")
                .Append(escapedModelBuilderParam)
                .AppendLine("</param>");
        }

        private const string ModelBuilderMethodSummaryComment = "Configure the model that was discovered by convention from the defined entity types.";
        private const string ModelBuilderMethodParamComment = "The builder being used to construct the model for this context.";
    }
}
