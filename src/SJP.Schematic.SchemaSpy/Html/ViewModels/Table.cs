using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    internal class Table : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.Table;

        public Identifier TableName
        {
            get => _tableName;
            set => _tableName = value ?? throw new ArgumentNullException(nameof(value));
        }

        private Identifier _tableName;

        public string RootPath
        {
            get => _rootPath;
            set => _rootPath = value ?? throw new ArgumentNullException(nameof(value));
        }

        private string _rootPath = "../";

        public string Name => _tableName.ToVisibleName();

        public string TableUrl => _tableName.ToSafeKey();

        public ulong RowCount { get; set; }

        public IEnumerable<Column> Columns
        {
            get => _columns;
            set => _columns = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Column> _columns = Enumerable.Empty<Column>();

        public uint ColumnsCount => Columns.UCount();

        public string ColumnsTableClass => ColumnsCount > 0 ? "database_objects" : string.Empty;

        public PrimaryKeyConstraint PrimaryKey { get; set; }

        public bool PrimaryKeyExists => PrimaryKey != null;

        public string PrimaryKeyTableClass => PrimaryKeyExists ? "database_objects" : string.Empty;

        public IEnumerable<UniqueKey> UniqueKeys
        {
            get => _uniqueKeys;
            set => _uniqueKeys = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<UniqueKey> _uniqueKeys = Enumerable.Empty<UniqueKey>();

        public uint UniqueKeysCount => UniqueKeys.UCount();

        public string UniqueKeysTableClass => UniqueKeysCount > 0 ? "database_objects" : string.Empty;

        public IEnumerable<ForeignKey> ForeignKeys
        {
            get => _foreignKeys;
            set => _foreignKeys = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<ForeignKey> _foreignKeys = Enumerable.Empty<ForeignKey>();

        public uint ForeignKeysCount => ForeignKeys.UCount();

        public string ForeignKeysTableClass => ForeignKeysCount > 0 ? "database_objects" : string.Empty;

        public IEnumerable<UniqueKey> CheckConstraints
        {
            get => _checkConstraints;
            set => _checkConstraints = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<UniqueKey> _checkConstraints = Enumerable.Empty<UniqueKey>();

        public uint CheckConstraintsCount => CheckConstraints.UCount();

        public string CheckConstraintsTableClass => CheckConstraintsCount > 0 ? "database_objects" : string.Empty;

        public IEnumerable<Index> Indexes
        {
            get => _indexes;
            set => _indexes = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Index> _indexes = Enumerable.Empty<Index>();

        public uint IndexesCount => Indexes.UCount();

        public string IndexesTableClass => IndexesCount > 0 ? "database_objects" : string.Empty;

        public IEnumerable<Diagram> Diagrams
        {
            get => _diagrams;
            set => _diagrams = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Diagram> _diagrams = Enumerable.Empty<Diagram>();

        internal class Column
        {
            public Column(string tableName, string columnName)
            {
                TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
                ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            }

            public int Ordinal { get; set; }

            public string ColumnName { get; }

            public string TableName { get; }

            public string TitleNullable => IsNullable ? "Nullable" : string.Empty;

            public string NullableText => IsNullable ? "✓" : string.Empty;

            public bool IsNullable { get; set; }

            public string Type
            {
                get => _type;
                set => _type = value ?? string.Empty;
            }

            private string _type = string.Empty;

            public string DefaultValue
            {
                get => _defaultValue;
                set => _defaultValue = value ?? string.Empty;
            }

            private string _defaultValue = string.Empty;

            public bool IsUniqueKeyColumn { get; set; }

            public bool IsPrimaryKeyColumn { get; set; }

            public bool IsForeignKeyColumn { get; set; }

            public string ColumnClass
            {
                get
                {
                    var isKey = IsPrimaryKeyColumn || IsUniqueKeyColumn || IsForeignKeyColumn;
                    return isKey ? @"class=""detail keyColumn""" : string.Empty;
                }
            }

            public string ColumnIcon
            {
                get
                {
                    var iconPieces = new List<string>();

                    if (IsPrimaryKeyColumn)
                    {
                        const string iconText = @"<i title=""Primary Key"" class=""fa fa-key primaryKeyIcon"" style=""padding-left: 5px; padding-right: 5px;""></i>";
                        iconPieces.Add(iconText);
                    }

                    if (IsUniqueKeyColumn)
                    {
                        const string iconText = @"<i title=""Unique Key"" class=""fa fa-key uniqueKeyIcon"" style=""padding-left: 5px; padding-right: 5px;""></i>";
                        iconPieces.Add(iconText);
                    }

                    if (IsForeignKeyColumn)
                    {
                        const string iconText = @"<i title=""Foreign Key"" class=""fa fa-key foreignKeyIcon"" style=""padding-left: 5px; padding-right: 5px;""></i>";
                        iconPieces.Add(iconText);
                    }

                    return string.Concat(iconPieces);
                }
            }

            public string ColumnTitle
            {
                get
                {
                    var titlePieces = new List<string>();

                    if (IsPrimaryKeyColumn)
                        titlePieces.Add("Primary Key");
                    if (IsUniqueKeyColumn)
                        titlePieces.Add("Unique Key");
                    if (IsForeignKeyColumn)
                        titlePieces.Add("Foreign Key");

                    return string.Join(", ", titlePieces);
                }
            }

            public string QualifiedColumnName => TableName + "." + ColumnName;

            public IEnumerable<ParentKey> ParentKeys
            {
                get => _parentKeys;
                set => _parentKeys = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<ParentKey> _parentKeys = Enumerable.Empty<ParentKey>();

            public uint ParentKeysCount => ParentKeys.UCount();

            public IEnumerable<ChildKey> ChildKeys
            {
                get => _childKeys;
                set => _childKeys = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<ChildKey> _childKeys = Enumerable.Empty<ChildKey>();

            public uint ChildKeysCount => ChildKeys.UCount();
        }

        internal abstract class TableConstraint
        {
            public string ConstraintName
            {
                get => _constraintName;
                set => _constraintName = value ?? string.Empty;
            }

            private string _constraintName = string.Empty;
        }

        internal class PrimaryKeyConstraint : TableConstraint
        {
            public IEnumerable<string> Columns
            {
                get => _columns;
                set => _columns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _columns = Enumerable.Empty<string>();

            public string ColumnNames => _columns.Join(", ");
        }

        internal class UniqueKey : TableConstraint
        {
            public IEnumerable<string> Columns
            {
                get => _columns;
                set => _columns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _columns = Enumerable.Empty<string>();

            public string ColumnNames => _columns.Join(", ");
        }

        internal class ForeignKey : TableConstraint
        {
            public ForeignKey(Identifier parentTableName)
            {
                _parentTableName = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
            }

            public string ParentConstraintName
            {
                get => _parentConstraintName;
                set => _parentConstraintName = value ?? string.Empty;
            }

            private string _parentConstraintName = string.Empty;

            public IEnumerable<string> ChildColumns
            {
                get => _childColumns;
                set => _childColumns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _childColumns = Enumerable.Empty<string>();

            public string ChildColumnNames => _childColumns.Join(", ");

            public string ParentTableName => _parentTableName.ToVisibleName();

            public string ParentTableUrl => _parentTableName.ToSafeKey();

            public IEnumerable<string> ParentColumns
            {
                get => _parentColumns;
                set => _parentColumns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _parentColumns = Enumerable.Empty<string>();

            public string ParentColumnNames => _parentColumns.Join(", ");

            public Rule DeleteRule { get; set; }

            public Rule UpdateRule { get; set; }

            public string DeleteRuleDescription => _ruleDescription[DeleteRule];

            public string UpdateRuleDescription => _ruleDescription[UpdateRule];

            private readonly Identifier _parentTableName;

            private readonly static IReadOnlyDictionary<Rule, string> _ruleDescription = new Dictionary<Rule, string>
            {
                [Rule.None] = "NONE",
                [Rule.Cascade] = "CASCADE",
                [Rule.SetDefault] = "SET DEFAULT",
                [Rule.SetNull] = "SET NULL"
            };
        }

        internal class CheckConstraint : TableConstraint
        {
            public string Definition
            {
                get => _definition;
                set => _definition = value ?? string.Empty;
            }

            private string _definition = string.Empty;
        }

        internal class Index
        {
            public string Name { get; set; }

            public bool Unique { get; set; }

            public string UniqueText => Unique ? "✓" : string.Empty;

            public IEnumerable<string> Columns
            {
                get => _columns;
                set => _columns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _columns = Enumerable.Empty<string>();

            public IEnumerable<IndexColumnOrder> ColumnSorts
            {
                get => _columnSorts;
                set => _columnSorts = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<IndexColumnOrder> _columnSorts = Enumerable.Empty<IndexColumnOrder>();

            public IEnumerable<string> IncludedColumns
            {
                get => _includedColumns;
                set => _includedColumns = value ?? throw new ArgumentNullException(nameof(value));
            }

            private IEnumerable<string> _includedColumns = Enumerable.Empty<string>();

            public string ColumnsText
            {
                get
                {
                    return Columns.Zip(
                        ColumnSorts.Select(SortToString),
                        (c, s) => c + " " + s
                    ).Join(", ");
                }
            }

            public string IncludedColumnsText => IncludedColumns.Join(", ");

            private static string SortToString(IndexColumnOrder order)
            {
                return order == IndexColumnOrder.Ascending
                    ? "ASC"
                    : "DESC";
            }
        }

        internal class ParentKey
        {
            public ParentKey(Identifier parentTableName, string parentColumnName, string qualifiedChildColumnName)
            {
                _parentTableName = parentTableName ?? throw new ArgumentNullException(nameof(parentTableName));
                ParentColumnName = parentColumnName ?? throw new ArgumentNullException(nameof(parentColumnName));
                QualifiedChildColumnName = qualifiedChildColumnName ?? throw new ArgumentNullException(nameof(qualifiedChildColumnName));
            }

            public string ConstraintDescription
            {
                get
                {
                    var message = QualifiedChildColumnName + " references " + QualifiedParentColumnName;

                    if (!ConstraintName.IsNullOrWhiteSpace())
                        message += " via " + ConstraintName;

                    return message;
                }
            }

            public string ConstraintName
            {
                get => _constraintName;
                set => _constraintName = value ?? string.Empty;
            }

            private string _constraintName = string.Empty;

            public string ParentTableName => _parentTableName.ToVisibleName();

            public string ParentTableUrl => _parentTableName.ToSafeKey();

            public string ParentColumnName { get; }

            public string QualifiedChildColumnName { get; }

            public string QualifiedParentColumnName => ParentTableName + "." + ParentColumnName;

            private readonly Identifier _parentTableName;
        }

        internal class ChildKey
        {
            public ChildKey(Identifier childTableName, string childColumnName, string qualifiedParentColumnName)
            {
                _childTableName = childTableName ?? throw new ArgumentNullException(nameof(childTableName));
                ChildColumnName = childColumnName ?? throw new ArgumentNullException(nameof(childColumnName));
                QualifiedParentColumnName = qualifiedParentColumnName ?? throw new ArgumentNullException(nameof(qualifiedParentColumnName));
            }

            public string ConstraintDescription
            {
                get
                {
                    var message = QualifiedChildColumnName + " references " + QualifiedParentColumnName;

                    if (!ConstraintName.IsNullOrWhiteSpace())
                        message += " via " + ConstraintName;

                    return message;
                }
            }

            public string ConstraintName
            {
                get => _onstraintName;
                set => _onstraintName = value ?? string.Empty;
            }

            private string _onstraintName = string.Empty;

            public string ChildTableName => _childTableName.ToVisibleName();

            public string ChildTableUrl => _childTableName.ToSafeKey();

            public string ChildColumnName { get; }

            public string QualifiedChildColumnName => ChildTableName + "." + ChildColumnName;

            public string QualifiedParentColumnName { get; }

            private readonly Identifier _childTableName;
        }

        internal class Diagram
        {
            public Diagram(Identifier tableName, string diagramName, string dotDefinition)
            {
                _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));

                if (diagramName.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(diagramName));
                Name = diagramName;

                if (dotDefinition.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(dotDefinition));
                Dot = dotDefinition;
            }

            public string Name { get; }

            public string ContainerId => _tableName.ToSafeKey() + "-" + Name.ToLowerInvariant() + "-chart";

            public bool IsActive { get; set; }

            public string ActiveClass => IsActive ? "class=\"active\"" : string.Empty;

            public string ActiveText => IsActive ? "active" : string.Empty;

            public string Dot { get; }

            private readonly Identifier _tableName;
        }
    }
}
