using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Columns : ITemplateParameter
    {
        public ReportTemplate Template { get; } = ReportTemplate.Columns;

        public IEnumerable<Column> TableColumns
        {
            get => _tableColumns;
            set => _tableColumns = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<Column> _tableColumns = Enumerable.Empty<Column>();

        public uint ColumnsCount => TableColumns.UCount();

        public string ColumnsTableClass => ColumnsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        internal abstract class Column
        {
            protected Column(Identifier tableName, string columnName)
            {
                _tableName = tableName;
                _columnName = columnName;
            }

            public Identifier TableName
            {
                get => _tableName;
                set => _tableName = value ?? throw new ArgumentNullException(nameof(value));
            }

            private Identifier _tableName;

            public string Name => _tableName.ToVisibleName();

            public string TableUrl => _tableName.ToSafeKey();

            public string TableType => ParentType.ToString();

            public abstract string TableFolder { get; }

            public int Ordinal { get; set; }

            public string ColumnName
            {
                get => _columnName;
                set => _columnName = value ?? string.Empty;
            }

            private string _columnName;

            public abstract ParentObjectType ParentType { get; }

            public string TitleNullable => IsNullable ? "Nullable" : string.Empty;

            public string NullableText => IsNullable ? "✓" : string.Empty;

            public bool IsNullable { get; set; }

            public string Type
            {
                get => _type;
                set => _type = value ?? string.Empty;
            }

            private string _type = string.Empty;

            public virtual string DefaultValue
            {
                get => _defaultValue;
                set => _defaultValue = value ?? string.Empty;
            }

            private string _defaultValue = string.Empty;

            public virtual bool IsUniqueKeyColumn { get; set; }

            public virtual bool IsPrimaryKeyColumn { get; set; }

            public virtual bool IsForeignKeyColumn { get; set; }

            public virtual string ColumnClass => @"class=""detail""";

            public virtual string ColumnIcon => string.Empty;

            public virtual string ColumnTitle => string.Empty;

            public enum ParentObjectType
            {
                None,
                Table,
                View
            }
        }

        internal class TableColumn : Column
        {
            public TableColumn(Identifier tableName, string columnName)
                : base(tableName, columnName)
            {
            }

            public override string TableFolder { get; } = "tables";

            public override ParentObjectType ParentType { get; } = ParentObjectType.Table;

            public override string ColumnClass
            {
                get
                {
                    var isKey = IsPrimaryKeyColumn || IsUniqueKeyColumn || IsForeignKeyColumn;
                    return isKey ? @"class=""detail keyColumn""" : string.Empty;
                }
            }

            public override string ColumnIcon
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

            public override string ColumnTitle
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
        }

        internal class ViewColumn : Column
        {
            public ViewColumn(Identifier tableName, string columnName)
                : base(tableName, columnName)
            {
            }

            public override string TableFolder { get; } = "views";

            public override ParentObjectType ParentType { get; } = ParentObjectType.View;

            public override string DefaultValue
            {
                get => string.Empty;
                set { }
            }

            public override bool IsUniqueKeyColumn
            {
                get => false;
                set { }
            }

            public override bool IsPrimaryKeyColumn
            {
                get => false;
                set { }
            }

            public override bool IsForeignKeyColumn
            {
                get => false;
                set { }
            }
        }
    }
}
