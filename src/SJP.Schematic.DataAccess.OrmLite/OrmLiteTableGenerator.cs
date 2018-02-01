using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using SJP.Schematic.Core;
using SJP.Schematic.DataAccess.Extensions;

namespace SJP.Schematic.DataAccess.OrmLite
{
    public class OrmLiteTableGenerator : DatabaseTableGenerator
    {
        public OrmLiteTableGenerator(INameProvider nameProvider, string baseNamespace)
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

            namespaces.Add("ServiceStack.DataAnnotations");

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

            var schemaName = table.Name.Schema;
            if (!schemaName.IsNullOrWhiteSpace())
            {
                var schemaNameLiteral = schemaName.ToStringLiteral();
                builder.Append(tableIndent)
                    .Append("[Schema(")
                    .Append(schemaNameLiteral)
                    .AppendLine(")]");
            }

            var className = NameProvider.TableToClassName(table.Name);
            if (className != table.Name.LocalName)
            {
                var aliasName = table.Name.LocalName.ToStringLiteral();
                builder.Append(tableIndent)
                    .Append("[Alias(")
                    .Append(aliasName)
                    .AppendLine(")]");
            }

            var multiColumnUniqueKeys = table.UniqueKeys.Where(uk => uk.Columns.Skip(1).Any()).ToList();
            foreach (var uniqueKey in multiColumnUniqueKeys)
            {
                var columnNames = uniqueKey.Columns
                    .Select(c => NameProvider.ColumnToPropertyName(className, c.Name.LocalName))
                    .Select(p => p.ToStringLiteral())
                    .ToList();
                var fieldNames = string.Join(", ", columnNames);

                builder.Append(tableIndent)
                    .Append("[CompositeKey(")
                    .Append(fieldNames)
                    .AppendLine(")]");
            }

            var multiColumnIndexes = table.Indexes.Where(ix => ix.Columns.Skip(1).Any()).ToList();
            foreach (var index in multiColumnIndexes)
            {
                var indexColumns = index.Columns.ToList();
                var dependentColumns = indexColumns.SelectMany(ic => ic.DependentColumns).ToList();
                if (dependentColumns.Count > indexColumns.Count)
                    continue;

                var columnNames = dependentColumns
                    .Select(c => NameProvider.ColumnToPropertyName(className, c.Name.LocalName))
                    .Select(p => p.ToStringLiteral())
                    .ToList();
                var fieldNames = string.Join(", ", columnNames);

                builder.Append(tableIndent)
                    .Append("[CompositeIndex(");

                if (index.IsUnique)
                    builder.Append("true, ");

                builder.Append(fieldNames)
                    .AppendLine(")]");
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

            if (clrType == typeof(string) && column.Type.MaxLength > 0)
            {
                builder.Append(columnIndent)
                    .Append("[StringLength(")
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

            var isPrimaryKey = ColumnIsPrimaryKey(column);
            if (isPrimaryKey)
            {
                builder.Append(columnIndent)
                    .AppendLine("[PrimaryKey]");
            }

            var isNonUniqueIndex = ColumnIsNonUniqueIndex(column);
            if (isNonUniqueIndex)
            {
                builder.Append(columnIndent)
                    .AppendLine("[Index]");
            }

            var isUniqueIndex = ColumnIsUniqueIndex(column);
            if (isUniqueIndex)
            {
                builder.Append(columnIndent)
                    .AppendLine("[Index(true)]");
            }

            if (column.DefaultValue != null)
            {
                var literal = column.DefaultValue.ToStringLiteral();
                builder.Append(columnIndent)
                    .Append("[Default(")
                    .Append(literal)
                    .AppendLine(")]");
            }

            var isForeignKey = ColumnIsForeignKey(column);
            if (isForeignKey)
            {
                var relationalKey = ColumnRelationalKey(column);
                if (relationalKey == null)
                    throw new InvalidOperationException("Could not find parent key for foreign key relationship. Expected to find one for " + column.Name.LocalName + "." + column.Name.LocalName);

                var parentTable = relationalKey.ParentKey.Table.Name;
                var parentClassName = NameProvider.TableToClassName(parentTable);
                // TODO check that this is not implicit -- i.e. there is a naming convention applied
                //      so explicitly declaring via [References(...)] may not be necessary

                builder.Append(columnIndent)
                    .Append("[ForeignKey(typeof(")
                    .Append(parentClassName)
                    .Append(")");

                var fkName = relationalKey.ChildKey.Name.LocalName.ToStringLiteral();
                if (!fkName.IsNullOrWhiteSpace())
                {
                    builder.Append("), ForeignKeyName = ")
                        .Append(fkName);
                }

                if (relationalKey.DeleteRule != Rule.None)
                {
                    var ruleLiteral = _foreignKeyRule[relationalKey.DeleteRule].ToStringLiteral();
                    builder.Append(", OnDelete = ")
                       .Append(ruleLiteral);
                }

                if (relationalKey.UpdateRule != Rule.None)
                {
                    var ruleLiteral = _foreignKeyRule[relationalKey.UpdateRule].ToStringLiteral();
                    builder.Append(", OnUpdate = ")
                       .Append(ruleLiteral);
                }

                builder.AppendLine(")]");
            }

            if (column.AutoIncrement != null)
            {
                builder.Append(columnIndent)
                    .AppendLine("[AutoIncrement]");
            }

            if (column.IsComputed && column is IDatabaseComputedColumn computedColumn)
            {
                var expression = computedColumn.Definition.ToStringLiteral();
                builder.Append(columnIndent)
                    .Append("[Compute(")
                    .Append(expression)
                    .AppendLine(")]");
            }

            var propertyName = NameProvider.ColumnToPropertyName(className, column.Name.LocalName);
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

        protected static bool ColumnIsPrimaryKey(IDatabaseTableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var primaryKey = column.Table?.PrimaryKey;
            if (primaryKey == null)
                return false;

            var pkColumns = primaryKey.Columns.ToList();
            if (pkColumns.Count != 1)
                return false;

            return column.Name.LocalName == pkColumns[0].Name.LocalName;
        }

        protected static bool ColumnIsForeignKey(IDatabaseTableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var foreignKeys = column.Table.ParentKeys.ToList();
            if (foreignKeys.Count == 0)
                return false;

            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey.ParentKey.KeyType != DatabaseKeyType.Primary)
                    continue; // ormlite only supports FK to primary key

                var childColumns = foreignKey.ChildKey.Columns.ToList();
                if (childColumns.Count > 1)
                    continue;

                var childColumn = childColumns[0];
                if (childColumn.Name.LocalName == column.Name.LocalName)
                    return true;
            }

            return false;
        }

        protected static IDatabaseRelationalKey ColumnRelationalKey(IDatabaseTableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var foreignKeys = column.Table.ParentKeys.ToList();
            if (foreignKeys.Count == 0)
                return null;

            foreach (var foreignKey in foreignKeys)
            {
                if (foreignKey.ParentKey.KeyType != DatabaseKeyType.Primary)
                    continue; // ormlite only supports FK to primary key

                var childColumns = foreignKey.ChildKey.Columns.ToList();
                if (childColumns.Count > 1)
                    continue;

                var childColumn = childColumns[0];
                if (childColumn.Name.LocalName == column.Name.LocalName)
                    return foreignKey;
            }

            return null;
        }

        protected static bool ColumnIsNonUniqueIndex(IDatabaseTableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var indexes = column.Table.Indexes.Where(i => !i.IsUnique).ToList();
            if (indexes.Count == 0)
                return false;

            foreach (var index in indexes)
            {
                var columns = index.Columns.ToList();
                if (columns.Count > 1)
                    continue;

                var indexColumn = columns[0];
                var dependentColumns = indexColumn.DependentColumns.ToList();
                if (dependentColumns.Count > 1)
                    continue;

                var dependentColumn = dependentColumns[0];
                if (dependentColumn.Name.LocalName == column.Name.LocalName)
                    return true;
            }

            return false;
        }

        protected static bool ColumnIsUniqueIndex(IDatabaseTableColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            var indexes = column.Table.Indexes.Where(i => i.IsUnique).ToList();
            if (indexes.Count == 0)
                return false;

            foreach (var index in indexes)
            {
                var columns = index.Columns.ToList();
                if (columns.Count > 1)
                    continue;

                var indexColumn = columns[0];
                var dependentColumns = indexColumn.DependentColumns.ToList();
                if (dependentColumns.Count > 1)
                    continue;

                var dependentColumn = dependentColumns[0];
                if (dependentColumn.Name.LocalName == column.Name.LocalName)
                    return true;
            }

            return false;
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

        private readonly static IReadOnlyDictionary<Rule, string> _foreignKeyRule = new Dictionary<Rule, string>
        {
            [Rule.None] = "NO ACTION",
            [Rule.Cascade] = "CASCADE",
            [Rule.SetDefault] = "SET DEFAULT",
            [Rule.SetNull] = "SET NULL"
        };
    }
}
