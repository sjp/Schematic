using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Poco
{
    public class PocoTableGenerator : DatabaseTableGenerator
    {
        public PocoTableGenerator(INameTranslator nameTranslator, string baseNamespace, string indent = "    ")
            : base(nameTranslator, indent)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IRelationalDatabase database, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var schemaNamespace = NameTranslator.SchemaToNamespace(table.Name);
            var tableNamespace = schemaNamespace != null
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = table.Columns
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != tableNamespace)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var builder = StringBuilderCache.Acquire();
            foreach (var ns in namespaces)
            {
                builder.Append("using ")
                    .Append(ns)
                    .AppendLine(";");
            }

            if (namespaces.Count > 0)
                builder.AppendLine();

            builder.Append("namespace ")
                .AppendLine(tableNamespace)
                .AppendLine("{");

            var tableComment = comment
                .Bind(c => c.Comment)
                .IfNone(GenerateTableComment(table.Name.LocalName));
            builder.AppendComment(Indent, tableComment);

            var className = NameTranslator.TableToClassName(table.Name);
            builder.Append(Indent)
                .Append("public class ")
                .AppendLine(className)
                .Append(Indent)
                .AppendLine("{");

            var columnIndent = Indent + Indent;
            var hasFirstLine = false;
            foreach (var column in table.Columns)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var columnComment = comment
                    .Bind(c => c.ColumnComments.TryGetValue(column.Name, out var cc) ? cc : Option<string>.None)
                    .IfNone(GenerateColumnComment(column.Name.LocalName));
                builder.AppendComment(columnIndent, columnComment);

                builder.Append(columnIndent);
                AppendColumn(builder, columnIndent, className, column);
                hasFirstLine = true;
            }

            builder.Append(Indent)
                .AppendLine("}")
                .Append("}");

            return builder.GetStringAndRelease();
        }

        private void AppendColumn(StringBuilder builder, string columnIndent, string className, IDatabaseColumn column)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (columnIndent == null)
                throw new ArgumentNullException(nameof(columnIndent));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var clrType = column.Type.ClrType;
            var nullableSuffix = clrType.IsValueType && column.IsNullable ? "?" : string.Empty;

            var typeName = clrType.Name;
            if (clrType.Namespace == "System" && TypeNameMap.ContainsKey(typeName))
                typeName = TypeNameMap[typeName];

            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);

            builder.Append("public ")
                .Append(typeName)
                .Append(nullableSuffix)
                .Append(" ")
                .Append(propertyName)
                .AppendLine(" { get; set; }");
        }

        protected virtual string GenerateTableComment(string tableName)
        {
            if (tableName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(tableName));

            var escapedTableName = SecurityElement.Escape(tableName);
            return "A mapping class to query the <c>" + escapedTableName + "</c> table.";
        }

        protected virtual string GenerateColumnComment(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var escapedColumnName = SecurityElement.Escape(columnName);
            return "The <c>" + escapedColumnName + "</c> column.";
        }

        private static readonly IReadOnlyDictionary<string, string> TypeNameMap = new Dictionary<string, string>
        {
            ["Boolean"] = "bool",
            ["Byte"] = "byte",
            ["Byte[]"] = "byte[]",
            ["SByte"] = "sbyte",
            ["Char"] = "char",
            ["Decimal"] = "decimal",
            ["Double"] = "double",
            ["Single"] = "float",
            ["Int32"] = "int",
            ["UInt32"] = "uint",
            ["Int64"] = "long",
            ["UInt64"] = "ulong",
            ["Object"] = "object",
            ["Int16"] = "short",
            ["UInt16"] = "ushort",
            ["String"] = "string"
        };
    }
}
