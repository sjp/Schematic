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

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteViewGenerator : DatabaseViewGenerator
    {
        public OrmLiteViewGenerator(INameTranslator nameTranslator, string baseNamespace, string indent = "    ")
            : base(nameTranslator, indent)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IDatabaseView view, Option<IDatabaseViewComments> comment)
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

            namespaces.Add("ServiceStack.DataAnnotations");

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

            var viewComment = comment
                .Bind(c => c.Comment)
                .IfNone(GenerateViewComment(view.Name.LocalName));
            builder.AppendComment(Indent, viewComment);

            var schemaName = view.Name.Schema;
            if (!schemaName.IsNullOrWhiteSpace())
            {
                var schemaNameLiteral = schemaName.ToStringLiteral();
                builder.Append(Indent)
                    .Append("[Schema(")
                    .Append(schemaNameLiteral)
                    .AppendLine(")]");
            }

            var className = NameTranslator.ViewToClassName(view.Name);
            if (className != view.Name.LocalName)
            {
                var aliasName = view.Name.LocalName.ToStringLiteral();
                builder.Append(Indent)
                    .Append("[Alias(")
                    .Append(aliasName)
                    .AppendLine(")]");
            }

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

                var columnComment = comment
                    .Bind(c => c.ColumnComments.TryGetValue(column.Name, out var cc) ? cc : Option<string>.None)
                    .IfNone(GenerateColumnComment(column.Name.LocalName));

                AppendColumn(builder, columnIndent, className, column, columnComment);
                hasFirstLine = true;
            }

            builder.Append(Indent)
                .AppendLine("}")
                .Append('}');

            return builder.GetStringAndRelease();
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

        private void AppendColumn(StringBuilder builder, string columnIndent, string className, IDatabaseColumn column, string comment)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (columnIndent == null)
                throw new ArgumentNullException(nameof(columnIndent));
            if (className.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(className));
            if (column == null)
                throw new ArgumentNullException(nameof(column));
            if (comment.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(comment));

            var clrType = column.Type.ClrType;
            var nullableSuffix = column.IsNullable ? "?" : string.Empty;

            var typeName = clrType.Name;
            if (clrType.Namespace == "System" && TypeNameMap.ContainsKey(typeName))
                typeName = TypeNameMap[typeName];

            builder.AppendComment(columnIndent, comment);

            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);
            if (propertyName != column.Name.LocalName)
            {
                var aliasName = column.Name.LocalName.ToStringLiteral();
                builder.Append(columnIndent)
                    .Append("[Alias(")
                    .Append(aliasName)
                    .AppendLine(")]");
            }

            builder.Append(columnIndent)
                .Append("public ")
                .Append(typeName)
                .Append(nullableSuffix)
                .Append(' ')
                .Append(propertyName)
                .Append(" { get; set; }");

            if (!column.IsNullable)
                builder.Append(" = default!;");

            builder.AppendLine();
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
