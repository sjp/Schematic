using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using Humanizer;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Comments;
using SJP.Schematic.Core.Extensions;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    public class EFCoreTableGenerator : DatabaseTableGenerator
    {
        public EFCoreTableGenerator(INameTranslator nameTranslator, string baseNamespace, string indent = "    ")
            : base(nameTranslator, indent)
        {
            if (baseNamespace.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(baseNamespace));

            Namespace = baseNamespace;
        }

        protected string Namespace { get; }

        public override string Generate(IReadOnlyCollection<IRelationalDatabaseTable> tables, IRelationalDatabaseTable table, Option<IRelationalDatabaseTableComments> comment)
        {
            if (tables == null)
                throw new ArgumentNullException(nameof(tables));
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

            var tableComment = comment
                .Bind(c => c.Comment)
                .IfNone(GenerateTableComment(table.Name.LocalName));
            builder.AppendComment(Indent, tableComment);

            var className = NameTranslator.TableToClassName(table.Name);
            if (className != table.Name.LocalName)
            {
                var aliasName = table.Name.LocalName.ToStringLiteral();
                builder.Append(Indent)
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

                AppendColumn(builder, columnIndent, className, column);
                hasFirstLine = true;
            }

            foreach (var relationalKey in table.ParentKeys)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var parentTable = relationalKey.ParentTable;

                var parentSchemaName = parentTable.Schema;
                var parentClassName = NameTranslator.TableToClassName(parentTable);
                var qualifiedParentName = !parentSchemaName.IsNullOrWhiteSpace()
                    ? parentSchemaName + "." + parentClassName
                    : parentClassName;

                var foreignKeyComment = comment
                    .Bind(c =>
                        relationalKey.ChildKey.Name
                            .Bind(childKeyName => c.ForeignKeyComments.TryGetValue(childKeyName, out var fkc) ? fkc : Option<string>.None))
                    .IfNone(GenerateForeignKeyComment(relationalKey));
                builder.AppendComment(columnIndent, foreignKeyComment);

                builder.Append(columnIndent)
                    .Append("public virtual ")
                    .Append(qualifiedParentName)
                    .Append(" ")
                    .Append(parentClassName)
                    .AppendLine(" { get; set; }");
            }

            foreach (var relationalKey in table.ChildKeys)
            {
                if (hasFirstLine)
                    builder.AppendLine();

                var childTableName = relationalKey.ChildTable;

                var childSchemaName = childTableName.Schema;
                var childClassName = NameTranslator.TableToClassName(childTableName);
                var qualifiedChildName = !childSchemaName.IsNullOrWhiteSpace()
                    ? childSchemaName + "." + childClassName
                    : childClassName;

                var childSetName = childClassName.Pluralize();

                var childKeyComment = GenerateChildKeyComment(relationalKey);
                builder.AppendComment(columnIndent, childKeyComment);

                var childTable = tables.FirstOrDefault(t => t.Name == relationalKey.ChildTable);
                var childKeyIsUnique = childTable != null && IsChildKeyUnique(childTable, relationalKey.ChildKey);

                if (childKeyIsUnique)
                {
                    builder.Append(columnIndent)
                        .Append("public virtual ")
                        .Append(qualifiedChildName)
                        .Append(" ")
                        .Append(childSetName)
                        .AppendLine(" { get; set; }");
                }
                else
                {
                    builder.Append(columnIndent)
                        .Append("public virtual ICollection<")
                        .Append(qualifiedChildName)
                        .Append("> ")
                        .Append(childSetName)
                        .Append(" { get; set; } = new HashSet<")
                        .Append(qualifiedChildName)
                        .AppendLine(">();");
                }
            }

            builder.Append(Indent)
                .AppendLine("}")
                .Append("}");

            return builder.ToString();
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
            if (clrType.Namespace == "System" && TypeNameMap.ContainsKey(typeName))
                typeName = TypeNameMap[typeName];

            column.AutoIncrement.IfSome(_ =>
            {
                builder.Append(columnIndent)
                    .AppendLine("[DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
            });

            var propertyName = NameTranslator.ColumnToPropertyName(className, column.Name.LocalName);
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

            var escapedForeignKeyName = relationalKey.ChildKey.Name.Match(
                name => "<c>" + SecurityElement.Escape(name.LocalName) + "</c> ",
                () => string.Empty
            ); var escapedChildTableName = SecurityElement.Escape(relationalKey.ChildTable.LocalName);
            var escapedParentTableName = SecurityElement.Escape(relationalKey.ParentTable.LocalName);

            return "The " + escapedForeignKeyName + "foreign key. Navigates from <c>" + escapedChildTableName + "</c> to <c>" + escapedParentTableName + "</c>.";
        }

        protected virtual string GenerateChildKeyComment(IDatabaseRelationalKey relationalKey)
        {
            if (relationalKey == null)
                throw new ArgumentNullException(nameof(relationalKey));

            var escapedForeignKeyName = relationalKey.ChildKey.Name.Match(
                name => "<c>" + SecurityElement.Escape(name.LocalName) + "</c> ",
                () => string.Empty
            );
            var escapedChildTableName = SecurityElement.Escape(relationalKey.ChildTable.LocalName);
            var escapedParentTableName = SecurityElement.Escape(relationalKey.ParentTable.LocalName);

            return "The " + escapedForeignKeyName + "child key. Navigates from <c>" + escapedParentTableName + "</c> to <c>" + escapedChildTableName + "</c> entities.";
        }

        private static bool IsChildKeyUnique(IRelationalDatabaseTable table, IDatabaseKey key)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var keyColumnNames = key.Columns.Select(c => c.Name.LocalName).ToList();
            var matchesPkColumns = table.PrimaryKey.Match(pk =>
            {
                var pkColumnNames = pk.Columns.Select(c => c.Name.LocalName).ToList();
                return keyColumnNames.SequenceEqual(pkColumnNames);
            }, () => false);
            if (matchesPkColumns)
                return true;

            var matchesUkColumns = table.UniqueKeys.Any(uk =>
            {
                var ukColumnNames = uk.Columns.Select(c => c.Name.LocalName).ToList();
                return keyColumnNames.SequenceEqual(ukColumnNames);
            });
            if (matchesUkColumns)
                return true;

            var uniqueIndexes = table.Indexes.Where(i => i.IsUnique).ToList();
            if (uniqueIndexes.Count == 0)
                return false;

            return uniqueIndexes.Any(i =>
            {
                var indexColumnExpressions = i.Columns
                    .Select(ic => ic.DependentColumns.Select(dc => dc.Name.LocalName).FirstOrDefault() ?? ic.Expression)
                    .ToList();
                return keyColumnNames.SequenceEqual(indexColumnExpressions);
            });
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
