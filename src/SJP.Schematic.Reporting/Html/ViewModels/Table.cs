using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal sealed class Table : ITemplateParameter
    {
        public Table(
            Identifier tableName,
            IEnumerable<Column> columns,
            PrimaryKeyConstraint primaryKey,
            IEnumerable<UniqueKey> uniqueKeys,
            IEnumerable<ForeignKey> foreignKeys,
            IEnumerable<CheckConstraint> checks,
            IEnumerable<Index> indexes,
            IEnumerable<Diagram> diagrams,
            string rootPath,
            ulong rowCount
        )
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));

            Name = tableName.ToVisibleName();
            TableUrl = tableName.ToSafeKey();
            RowCount = rowCount;

            Columns = columns ?? throw new ArgumentNullException(nameof(columns));
            ColumnsCount = columns.UCount();
            ColumnsTableClass = ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;

            PrimaryKey = primaryKey;
            PrimaryKeyExists = primaryKey != null;
            PrimaryKeyTableClass = PrimaryKeyExists ? CssClasses.DataTableClass : string.Empty;

            UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
            UniqueKeysCount = uniqueKeys.UCount();
            UniqueKeysTableClass = UniqueKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

            ForeignKeys = foreignKeys ?? throw new ArgumentNullException(nameof(foreignKeys));
            ForeignKeysCount = foreignKeys.UCount();
            ForeignKeysTableClass = ForeignKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

            CheckConstraints = checks ?? throw new ArgumentNullException(nameof(checks));
            CheckConstraintsCount = checks.UCount();
            CheckConstraintsTableClass = CheckConstraintsCount > 0 ? CssClasses.DataTableClass : string.Empty;

            Indexes = indexes ?? throw new ArgumentNullException(nameof(checks));
            IndexesCount = indexes.UCount();
            IndexesTableClass = IndexesCount > 0 ? CssClasses.DataTableClass : string.Empty;

            Diagrams = diagrams ?? throw new ArgumentNullException(nameof(diagrams));

            RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        }

        public ReportTemplate Template { get; } = ReportTemplate.Table;

        public string RootPath { get; }

        public string Name { get; }

        public string TableUrl { get; }

        public ulong RowCount { get; }

        public IEnumerable<Column> Columns { get; }

        public uint ColumnsCount { get; }

        public string ColumnsTableClass { get; }

        public PrimaryKeyConstraint PrimaryKey { get; }

        public bool PrimaryKeyExists { get; }

        public string PrimaryKeyTableClass { get; }

        public IEnumerable<UniqueKey> UniqueKeys { get; }

        public uint UniqueKeysCount { get; }

        public string UniqueKeysTableClass { get; }

        public IEnumerable<ForeignKey> ForeignKeys { get; }

        public uint ForeignKeysCount { get; }

        public string ForeignKeysTableClass { get; }

        public IEnumerable<CheckConstraint> CheckConstraints { get; }

        public uint CheckConstraintsCount { get; }

        public string CheckConstraintsTableClass { get; }

        public IEnumerable<Index> Indexes { get; }

        public uint IndexesCount { get; }

        public string IndexesTableClass { get; }

        public IEnumerable<Diagram> Diagrams { get; }

        internal sealed class Column
        {
            public Column(
                string columnName,
                int ordinal,
                bool isNullable,
                string typeDefinition,
                string defaultValue,
                bool isPrimaryKeyColumn,
                bool isUniqueKeyColumn,
                bool isForeignKeyColumn,
                IEnumerable<ChildKey> childKeys,
                IEnumerable<ParentKey> parentKeys
            )
            {
                ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
                Ordinal = ordinal;
                TitleNullable = isNullable ? "Nullable" : string.Empty;
                NullableText = isNullable ? "✓" : string.Empty;
                Type = typeDefinition ?? string.Empty;
                DefaultValue = defaultValue ?? string.Empty;

                ColumnClass = BuildColumnClass(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);
                ColumnIcon = BuildColumnIcon(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);
                ColumnTitle = BuildColumnTitle(isPrimaryKeyColumn, isUniqueKeyColumn, isForeignKeyColumn);

                ChildKeys = childKeys ?? throw new ArgumentNullException(nameof(childKeys));
                ChildKeysCount = childKeys.UCount();

                ParentKeys = parentKeys ?? throw new ArgumentNullException(nameof(parentKeys));
                ParentKeysCount = parentKeys.UCount();
            }

            public int Ordinal { get; }

            public string ColumnName { get; }

            public string TitleNullable { get; }

            public string NullableText { get; }

            public string Type { get; }

            public string DefaultValue { get; }

            public string ColumnClass { get; }

            public string ColumnIcon { get; }

            public string ColumnTitle { get; }

            public IEnumerable<ParentKey> ParentKeys { get; }

            public uint ParentKeysCount { get; }

            public IEnumerable<ChildKey> ChildKeys { get; }

            public uint ChildKeysCount { get; }

            private static string BuildColumnClass(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
            {
                var isKey = isPrimaryKeyColumn || isUniqueKeyColumn || isForeignKeyColumn;
                return isKey ? @"class=""detail keyColumn""" : string.Empty;
            }

            private static string BuildColumnIcon(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
            {
                var iconPieces = new List<string>();

                if (isPrimaryKeyColumn)
                {
                    const string iconText = @"<i title=""Primary Key"" class=""fa fa-key primaryKeyIcon"" style=""padding-left: 5px; padding-right: 5px;""></i>";
                    iconPieces.Add(iconText);
                }

                if (isUniqueKeyColumn)
                {
                    const string iconText = @"<i title=""Unique Key"" class=""fa fa-key uniqueKeyIcon"" style=""padding-left: 5px; padding-right: 5px;""></i>";
                    iconPieces.Add(iconText);
                }

                if (isForeignKeyColumn)
                {
                    const string iconText = @"<i title=""Foreign Key"" class=""fa fa-key foreignKeyIcon"" style=""padding-left: 5px; padding-right: 5px;""></i>";
                    iconPieces.Add(iconText);
                }

                return string.Concat(iconPieces);
            }

            private static string BuildColumnTitle(bool isPrimaryKeyColumn, bool isUniqueKeyColumn, bool isForeignKeyColumn)
            {
                var titlePieces = new List<string>();

                if (isPrimaryKeyColumn)
                    titlePieces.Add("Primary Key");
                if (isUniqueKeyColumn)
                    titlePieces.Add("Unique Key");
                if (isForeignKeyColumn)
                    titlePieces.Add("Foreign Key");

                return titlePieces.Join(", ");
            }
        }

        internal abstract class TableConstraint
        {
            protected TableConstraint(string constraintName)
            {
                ConstraintName = constraintName;
            }

            public string ConstraintName { get; }
        }

        internal sealed class PrimaryKeyConstraint : TableConstraint
        {
            public PrimaryKeyConstraint(string constraintName, IEnumerable<string> columns)
                : base(constraintName)
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));

                ColumnNames = columns.Join(", ");
            }

            public string ColumnNames { get; }
        }

        internal sealed class UniqueKey : TableConstraint
        {
            public UniqueKey(string constraintName, IEnumerable<string> columns)
                : base(constraintName)
            {
                if (columns == null || columns.Empty())
                    throw new ArgumentNullException(nameof(columns));

                ColumnNames = columns.Join(", ");
            }

            public string ColumnNames { get; }
        }

        internal sealed class ForeignKey : TableConstraint
        {
            public ForeignKey(
                string constraintName,
                IEnumerable<string> columnNames,
                Identifier parentTableName,
                string parentConstraintName,
                IEnumerable<string> parentColumnNames,
                Rule deleteRule,
                Rule updateRule
            ) : base(constraintName)
            {
                if (columnNames == null || columnNames.Empty())
                    throw new ArgumentNullException(nameof(columnNames));
                if (parentTableName == null)
                    throw new ArgumentNullException(nameof(parentTableName));
                if (parentColumnNames == null || parentColumnNames.Empty())
                    throw new ArgumentNullException(nameof(parentColumnNames));
                if (!deleteRule.IsValid())
                    throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(deleteRule));
                if (!updateRule.IsValid())
                    throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(updateRule));

                ChildColumnNames = columnNames.Join(", ");
                ParentConstraintName = parentConstraintName;
                ParentTableName = parentTableName.ToVisibleName();
                ParentTableUrl = parentTableName.ToSafeKey();
                ParentColumnNames = parentColumnNames.Join(", ");

                DeleteRuleDescription = _ruleDescription[deleteRule];
                UpdateRuleDescription = _ruleDescription[updateRule];
            }

            public string ParentConstraintName { get; }

            public string ChildColumnNames { get; }

            public string ParentTableName { get; }

            public string ParentTableUrl { get; }

            public string ParentColumnNames { get; }

            public string DeleteRuleDescription { get; }

            public string UpdateRuleDescription { get; }

            private readonly static IReadOnlyDictionary<Rule, string> _ruleDescription = new Dictionary<Rule, string>
            {
                [Rule.None] = "NONE",
                [Rule.Cascade] = "CASCADE",
                [Rule.SetDefault] = "SET DEFAULT",
                [Rule.SetNull] = "SET NULL"
            };
        }

        internal sealed class CheckConstraint : TableConstraint
        {
            public CheckConstraint(string constraintName, string definition)
                : base(constraintName)
            {
                if (definition.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(definition));

                Definition = definition;
            }

            public string Definition { get; }
        }

        internal sealed class Index
        {
            public Index(
                string indexName,
                bool isUnique,
                IEnumerable<string> columnNames,
                IEnumerable<IndexColumnOrder> columnSorts,
                IEnumerable<string> includedColumnNames
            )
            {
                Name = indexName ?? string.Empty;
                UniqueText = isUnique ? "✓" : string.Empty;

                ColumnsText = columnNames.Zip(
                    columnSorts.Select(SortToString),
                    (c, s) => c + " " + s
                ).Join(", ");
                IncludedColumnsText = includedColumnNames.Join(", ");
            }

            public string Name { get; }

            public string UniqueText { get; }

            public string ColumnsText { get; }

            public string IncludedColumnsText { get; }

            private static string SortToString(IndexColumnOrder order)
            {
                return order == IndexColumnOrder.Ascending
                    ? "ASC"
                    : "DESC";
            }
        }

        internal sealed class ParentKey
        {
            public ParentKey(string constraintName, Identifier parentTableName, string parentColumnName, string qualifiedChildColumnName)
            {
                if (parentTableName == null)
                    throw new ArgumentNullException(nameof(parentTableName));
                if (parentColumnName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(parentColumnName));
                if (qualifiedChildColumnName.IsNullOrWhiteSpace())
                    throw new ArgumentOutOfRangeException(nameof(qualifiedChildColumnName));

                ParentTableName = parentTableName.ToVisibleName();
                ParentTableUrl = parentTableName.ToSafeKey();
                ParentColumnName = parentColumnName;

                var qualifiedParentColumnName = ParentTableName + "." + parentColumnName;
                var description = qualifiedChildColumnName + " references " + qualifiedParentColumnName;
                if (!constraintName.IsNullOrWhiteSpace())
                    description += " via " + constraintName;
                ConstraintDescription = description;
            }

            public string ConstraintDescription { get; }

            public string ParentTableName { get; }

            public string ParentTableUrl { get; }

            public string ParentColumnName { get; }
        }

        internal sealed class ChildKey
        {
            public ChildKey(string constraintName, Identifier childTableName, string childColumnName, string qualifiedParentColumnName)
            {
                if (childTableName == null)
                    throw new ArgumentNullException(nameof(childTableName));
                if (childColumnName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(childColumnName));
                if (qualifiedParentColumnName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(qualifiedParentColumnName));

                ChildTableName = childTableName.ToVisibleName();
                ChildTableUrl = childTableName.ToSafeKey();
                ChildColumnName = childColumnName;

                var qualifiedChildColumnName = ChildTableName + "." + ChildColumnName;
                var description = qualifiedChildColumnName + " references " + qualifiedParentColumnName;
                if (!constraintName.IsNullOrWhiteSpace())
                    description += " via " + constraintName;
                ConstraintDescription = description;
            }

            public string ConstraintDescription { get; }

            public string ChildTableName { get; }

            public string ChildTableUrl { get; }

            public string ChildColumnName { get; }
        }

        internal sealed class Diagram
        {
            public Diagram(Identifier tableName, string diagramName, string dotDefinition, bool isActive)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                if (diagramName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(diagramName));
                Name = diagramName;

                if (dotDefinition.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(dotDefinition));
                Dot = dotDefinition;

                ContainerId = tableName.ToSafeKey() + "-" + Name.ToLowerInvariant() + "-chart";
                ActiveClass = isActive ? "class=\"active\"" : string.Empty;
                ActiveText = isActive ? "active" : string.Empty;
            }

            public string Name { get; }

            public string ContainerId { get; }

            public string ActiveClass { get; }

            public string ActiveText { get; }

            public string Dot { get; }
        }
    }
}
