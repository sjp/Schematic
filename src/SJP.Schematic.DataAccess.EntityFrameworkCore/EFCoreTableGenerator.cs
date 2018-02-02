using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using Humanizer;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class EFCoreTableGenerator : DatabaseTableGenerator
    {
        public EFCoreTableGenerator(INameProvider nameProvider, string baseNamespace)
            : base(nameProvider)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var schemaNamespace = NameProvider.SchemaToNamespace(table.Name);
            var tableNamespace = schemaNamespace != null
                ? Namespace + "." + schemaNamespace
                : Namespace;

            var namespaces = table.Columns
                .Select(c => c.Type.ClrType.Namespace)
                .Where(ns => ns != tableNamespace)
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            namespaces.Add("System.Collections.Generic");
            namespaces.Add("System.ComponentModel.DataAnnotations");
            namespaces.Add("System.ComponentModel.DataAnnotations.Schema");

            var builder = new StringBuilder();
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

            // todo configure for tabs?
            const string tableIndent = IndentLevel;

            var tableComment = GenerateTableComment(table.Name.LocalName);
            builder.AppendComment(tableIndent, tableComment);

            var className = NameProvider.TableToClassName(table.Name);
            if (className != table.Name.LocalName)
            {
                var aliasName = table.Name.LocalName.ToStringLiteral();
                builder.Append(tableIndent)
                    .Append("[Table(")
                    .Append(aliasName);

                var schemaName = table.Name.Schema;
                if (!schemaName.IsNullOrWhiteSpace())
                {
                    var schemaNameLiteral = schemaName.ToStringLiteral();
                    builder.Append(", Schema = ")
                        .Append(schemaNameLiteral);
                }

                builder.AppendLine(")]");
            }

            builder.Append(tableIndent)
                .Append("public class ")
                .AppendLine(className)
                .Append(tableIndent)
                .AppendLine("{");

            const string columnIndent = tableIndent + IndentLevel;
            var hasFirstLine = false;
            foreach (var column in table.Columns)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var columnComment = GenerateColumnComment(column.Name.LocalName);
                builder.AppendComment(columnIndent, columnComment);

                AppendColumn(builder, columnIndent, className, column);
                hasFirstLine = true;
            }

            foreach (var relationalKey in table.ParentKeys)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var parentKey = relationalKey.ParentKey;
                var parentTable = parentKey.Table;

                var parentSchemaName = parentTable.Name.Schema;
                var parentClassName = NameProvider.TableToClassName(parentTable.Name);
                var qualifiedParentName = !parentSchemaName.IsNullOrWhiteSpace()
                    ? parentSchemaName + "." + parentClassName
                    : parentClassName;

                var parentKeyComment = GenerateForeignKeyComment(relationalKey);
                builder.AppendComment(columnIndent, parentKeyComment);

                builder.Append(columnIndent)
                    .Append("public ")
                    .Append(qualifiedParentName)
                    .Append(" ")
                    .Append(parentClassName)
                    .AppendLine(" { get; set; }");
            }

            foreach (var relationalKey in table.ChildKeys)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var childKey = relationalKey.ChildKey;
                var childTable = childKey.Table;

                var childSchemaName = childTable.Name.Schema;
                var childClassName = NameProvider.TableToClassName(childTable.Name);
                var qualifiedChildName = !childSchemaName.IsNullOrWhiteSpace()
                    ? childSchemaName + "." + childClassName
                    : childClassName;

                var childSetName = childClassName.Pluralize();

                var childKeyComment = GenerateChildKeyComment(relationalKey);
                builder.AppendComment(columnIndent, childKeyComment);

                builder.Append(columnIndent)
                    .Append("public List<")
                    .Append(qualifiedChildName)
                    .Append("> ")
                    .Append(childSetName)
                    .AppendLine(" { get; set; }");
            }

            builder.Append(tableIndent)
                .AppendLine("}")
                .Append("}");

            return builder.ToString();
        }

        private void AppendColumn(StringBuilder builder, string columnIndent, string className, IDatabaseTableColumn column)
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

            var isConstrainedType = clrType == typeof(string) || clrType == typeof(byte[]);
            if (isConstrainedType && column.Type.MaxLength > 0)
            {
                builder.Append(columnIndent)
                    .Append("[MaxLength(")
                    .Append(column.Type.MaxLength.ToString(CultureInfo.InvariantCulture))
                    .AppendLine(")]");
            }

            if (!clrType.IsValueType && !column.IsNullable)
            {
                builder.Append(columnIndent)
                    .AppendLine("[Required]");
            }

            var typeName = clrType.Name;
            if (clrType.Namespace == "System" && _typeNameMap.ContainsKey(typeName))
                typeName = _typeNameMap[typeName];

            if (column.AutoIncrement != null)
            {
                builder.Append(columnIndent)
                    .AppendLine("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            }

            var propertyName = NameProvider.ColumnToPropertyName(className, column.Name.LocalName);
            builder.Append(columnIndent)
                .Append("[Column(");

            if (propertyName != column.Name.LocalName)
            {
                var aliasName = column.Name.LocalName.ToStringLiteral();
                builder.Append(aliasName)
                    .Append(", ");
            }

            builder.Append("TypeName = ")
                .Append(column.Type.TypeName.LocalName.ToStringLiteral())
                .AppendLine(")]");

            builder.Append(columnIndent)
                .Append("public ")
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

        protected virtual string GenerateForeignKeyComment(IDatabaseRelationalKey relationalKey)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var escapedForeignKeyName = SecurityElement.Escape(relationalKey.ChildKey.Name.LocalName ?? string.Empty);
            var escapedChildTableName = SecurityElement.Escape(relationalKey.ChildKey.Table.Name.LocalName);
            var escapedParentTableName = SecurityElement.Escape(relationalKey.ParentKey.Table.Name.LocalName);

            return "The <c>" + escapedForeignKeyName + "</c> foreign key. Navigates from <c>" + escapedChildTableName + "</c> to <c>" + escapedParentTableName + "</c>.";
        }

        protected virtual string GenerateChildKeyComment(IDatabaseRelationalKey relationalKey)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var escapedForeignKeyName = SecurityElement.Escape(relationalKey.ChildKey.Name.LocalName ?? string.Empty);
            var escapedChildTableName = SecurityElement.Escape(relationalKey.ChildKey.Table.Name.LocalName);
            var escapedParentTableName = SecurityElement.Escape(relationalKey.ParentKey.Table.Name.LocalName);

            return "The <c>" + escapedForeignKeyName + "</c> child key. Navigates from <c>" + escapedParentTableName + "</c> to <c>" + escapedChildTableName + "</c> entities.";
        }

        private const string IndentLevel = "    ";

        private readonly static IReadOnlyDictionary<string, string> _typeNameMap = new Dictionary<string, string>
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
