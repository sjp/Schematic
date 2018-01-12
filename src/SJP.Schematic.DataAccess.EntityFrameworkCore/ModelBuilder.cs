using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Humanizer;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{

    public class ModelBuilder
    {
        public ModelBuilder(INameProvider nameProvider, string lineIndent, string indentLevel)
        {
            NameProvider = nameProvider ?? throw new ArgumentNullException(nameof(nameProvider));
            LineIndent = lineIndent ?? throw new ArgumentNullException(nameof(lineIndent));
            IndentLevel = indentLevel ?? throw new ArgumentNullException(nameof(indentLevel));
        }

        protected INameProvider NameProvider { get; }

        protected string LineIndent { get; }

        protected string IndentLevel { get; }

        public ModelBuilder AddTable(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var schemaNamespace = NameProvider.SchemaToNamespace(table.Name);
            var className = NameProvider.TableToClassName(table.Name);
            var qualifiedClassName = !schemaNamespace.IsNullOrWhiteSpace()
                ? schemaNamespace + "." + className
                : className;
            var childSetName = className.Pluralize();

            var chainIndent = LineIndent + IndentLevel;

            foreach (var column in table.Columns)
            {
                var requiresBuilder = !column.DefaultValue.IsNullOrWhiteSpace() || column.IsComputed;
                if (!requiresBuilder)
                    continue;

                _builder.Append(LineIndent)
                    .Append("modelBuilder.Entity<")
                    .Append(qualifiedClassName)
                    .AppendLine(">()");

                var columnName = NameProvider.ColumnToPropertyName(className, column.Name.LocalName);
                _builder.Append(chainIndent)
                    .Append(".Property(t => t.")
                    .Append(columnName)
                    .AppendLine(")");

                if (!column.DefaultValue.IsNullOrWhiteSpace())
                {
                    var defaultLiteral = column.DefaultValue.ToStringLiteral();
                    _builder.Append(chainIndent)
                        .Append(".HasDefaultValue(")
                        .Append(defaultLiteral)
                        .Append(")");
                }

                if (column.IsComputed && column is IDatabaseComputedColumn computedColumn)
                {
                    var computedDefinition = computedColumn.Definition.ToStringLiteral();
                    _builder.Append(chainIndent)
                        .Append(".HasComputedColumnSql(")
                        .Append(computedDefinition)
                        .Append(")");
                }

                _builder.AppendLine(";");
            }

            var primaryKey = table.PrimaryKey;
            if (primaryKey != null)
            {
                var keyColumnSet = GenerateColumnSet(className, "t", primaryKey.Columns);
                var keyNameLiteral = primaryKey.Name.LocalName.ToStringLiteral();

                _builder.Append(LineIndent)
                    .Append("modelBuilder.Entity<")
                    .Append(qualifiedClassName)
                    .AppendLine(">()")
                    .Append(chainIndent)
                    .Append(".HasKey(t => ")
                    .Append(keyColumnSet)
                    .Append(")");

                if (!keyNameLiteral.IsNullOrWhiteSpace())
                {
                    _builder.AppendLine()
                        .Append(chainIndent)
                        .Append(".HasName(")
                        .Append(keyNameLiteral)
                        .Append(")");
                }

                _builder.AppendLine(";");
            }

            foreach (var uniqueKey in table.UniqueKeys)
            {
                var keyColumnSet = GenerateColumnSet(className, "t", uniqueKey.Columns);
                var keyNameLiteral = uniqueKey.Name.LocalName.ToStringLiteral();

                _builder.Append(LineIndent)
                    .Append("modelBuilder.Entity<")
                    .Append(qualifiedClassName)
                    .AppendLine(">()")
                    .Append(chainIndent)
                    .Append(".HasAlternateKey(t => ")
                    .Append(keyColumnSet)
                    .Append(")");

                if (!keyNameLiteral.IsNullOrWhiteSpace())
                {
                    _builder.AppendLine()
                        .Append(chainIndent)
                        .Append(".HasName(")
                        .Append(keyNameLiteral)
                        .AppendLine(")");
                }

                _builder.AppendLine(";");
            }

            foreach (var index in table.Indexes)
            {
                var columns = index.Columns.SelectMany(c => c.DependentColumns).ToList();
                var columnSet = GenerateColumnSet(className, "t", columns);
                var indexNameLiteral = index.Name.LocalName.ToStringLiteral();

                _builder.Append(LineIndent)
                    .Append("modelBuilder.Entity<")
                    .Append(qualifiedClassName)
                    .AppendLine(">()")
                    .Append(chainIndent)
                    .Append(".HasIndex(t => ")
                    .Append(columnSet)
                    .Append(")");

                if (index.IsUnique)
                {
                    _builder.AppendLine()
                        .Append(chainIndent)
                        .Append(".IsUnique()");
                }

                if (!indexNameLiteral.IsNullOrWhiteSpace())
                {
                    _builder.AppendLine()
                        .Append(chainIndent)
                        .Append(".HasName(")
                        .Append(indexNameLiteral)
                        .Append(")");
                }

                _builder.AppendLine(";");
            }

            foreach (var relationalKey in table.ParentKeys)
            {
                var childKey = relationalKey.ChildKey;
                var parentKey = relationalKey.ParentKey;
                var parentPropertyName = NameProvider.TableToClassName(parentKey.Table.Name);
                var childColumnSet = GenerateColumnSet(className, "t", childKey.Columns);
                var parentColumnSet = GenerateColumnSet(className, "t", parentKey.Columns);

                _builder.Append(LineIndent)
                    .Append("modelBuilder.Entity<")
                    .Append(qualifiedClassName)
                    .AppendLine(">()")
                    .Append(chainIndent)
                    .Append(".HasOne(t => t.")
                    .Append(parentPropertyName)
                    .AppendLine(")")
                    .Append(chainIndent)
                    .Append(".WithMany(t => t.")
                    .Append(childSetName)
                    .AppendLine(")")
                    .Append(chainIndent)
                    .Append(".HasForeignKey(t => ")
                    .Append(childColumnSet)
                    .AppendLine(")")
                    .Append(chainIndent)
                    .Append(".HasPrincipalKey(t => ")
                    .Append(parentColumnSet)
                    .Append(")");

                var keyNameLiteral = relationalKey.ChildKey.Name.LocalName.ToStringLiteral();
                if (!keyNameLiteral.IsNullOrWhiteSpace())
                {
                    _builder.AppendLine()
                        .Append(chainIndent)
                        .Append(".HasConstraintName(")
                        .Append(keyNameLiteral)
                        .Append(")");
                }

                _builder.AppendLine(";");
            }

            return this;
        }

        public ModelBuilder AddSequence(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            var schemaName = sequence.Name.Schema.ToStringLiteral();
            var sequenceName = sequence.Name.LocalName.ToStringLiteral();

            _builder.Append(LineIndent)
                .Append("modelBuilder.HasSequence<decimal>(")
                .Append(sequenceName);

            if (!schemaName.IsNullOrWhiteSpace())
            {
                _builder.Append(", schema: ")
                    .Append(schemaName);
            }

            var chainIndent = LineIndent + IndentLevel;
            _builder.AppendLine(")")
                .Append(chainIndent)
                .Append(".StartsAt(")
                .Append(sequence.Start.ToNumericLiteral())
                .AppendLine(")")
                .Append(chainIndent)
                .Append(".IncrementsBy(")
                .Append(sequence.Increment.ToNumericLiteral())
                .AppendLine(");");

            return this;
        }

        protected string GenerateColumnSet(string className, string classPrefix, IEnumerable<IDatabaseColumn> columns)
        {
            if (className == null)
                throw new ArgumentNullException(nameof(className));
            if (classPrefix == null)
                throw new ArgumentNullException(nameof(classPrefix));
            if (columns == null)
                throw new ArgumentNullException(nameof(columns));

            var columnNames = columns
                .Select(c => classPrefix + "." + NameProvider.ColumnToPropertyName(className, c.Name.LocalName))
                .ToList();

            return columnNames.Count > 1
                ? "new { " + columnNames.Join(", ") + " }"
                : columnNames[0];
        }

        public bool HasRecords => _builder.Capacity > 0;

        public override string ToString() => _builder.ToString();

        private readonly StringBuilder _builder = new StringBuilder();
    }
}
