using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.Core.Utilities;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.Poco
{
    public class PocoViewGenerator : DatabaseViewGenerator
    {
        public PocoViewGenerator(INameTranslator nameTranslator, string baseNamespace, string indent = "    ")
            : base(nameTranslator, indent)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var schemaNamespace = NameTranslator.SchemaToNamespace(view.Name);
            var viewNamespace = !schemaNamespace.IsNullOrWhiteSpace()
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = view.Columns
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != viewNamespace)
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
                .AppendLine(viewNamespace)
                .AppendLine("{");

            var tableComment = GenerateViewComment(view.Name.LocalName);
            builder.AppendComment(Indent, tableComment);

            var className = NameTranslator.ViewToClassName(view.Name);

            builder.Append(Indent)
                .Append("public class ")
                .AppendLine(className)
                .Append(Indent)
                .AppendLine("{");

            var columnIndent = Indent + Indent;
            var hasFirstLine = false;
            foreach (var column in view.Columns)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var columnComment = GenerateColumnComment(column.Name.LocalName);
                builder.AppendComment(columnIndent, columnComment);

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
            if (clrType.Namespace == "System" && _typeNameMap.ContainsKey(typeName))
                typeName = _typeNameMap[typeName];

            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);

            builder.Append(columnIndent)
                .Append("public ")
                .Append(typeName)
                .Append(nullableSuffix)
                .Append(" ")
                .Append(propertyName)
                .AppendLine(" { get; set; }");
        }

        protected virtual string GenerateViewComment(string viewName)
        {
            if (viewName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(viewName));

            var escapedViewName = SecurityElement.Escape(viewName);
            return "A mapping class to query the <c>" + escapedViewName + "</c> view.";
        }

        protected virtual string GenerateColumnComment(string columnName)
        {
            if (columnName.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(columnName));

            var escapedColumnName = SecurityElement.Escape(columnName);
            return "The <c>" + escapedColumnName + "</c> column.";
        }

        private static readonly IReadOnlyDictionary<string, string> _typeNameMap = new Dictionary<string, string>
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
