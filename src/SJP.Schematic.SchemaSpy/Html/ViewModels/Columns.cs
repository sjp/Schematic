using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels
{
    public class Columns : ITemplateParameter
    {
        public SchemaSpyTemplate Template { get; } = SchemaSpyTemplate.Columns;

        public IEnumerable<Column> TableColumns { get; set; } = Enumerable.Empty<Column>();
    }

    public class Column
    {
        public string TableName { get; set; }

        public string TableType => ParentType.ToString();

        public string ColumnName { get; set; }

        public ParentObjectType ParentType
        {
            get => _parentType;
            set
            {
                if (!value.IsValid())
                    throw new ArgumentException($"The { nameof(ParentObjectType) } provided must be a valid enum.", nameof(value));

                _parentType = value;
            }
        }

        private ParentObjectType _parentType;

        public string TitleNullable => IsNullable ? "Nullable" : string.Empty;

        public string NullableText => IsNullable ? "✓" : string.Empty;

        public bool IsNullable { get; set; }

        public string Type { get; set; }

        public string DefaultValue { get; set; }

        public bool IsUniqueKeyColumn { get; set; }

        public bool IsPrimaryKeyColumn { get; set; }

        public bool IsForeignKeyColumn { get; set; }

        public string ColumnClass
        {
            get
            {
                var isKey = IsPrimaryKeyColumn || IsUniqueKeyColumn || IsForeignKeyColumn;
                return isKey ? @"class="".keyColumn""" : string.Empty;
            }
        }

        public string ColumnIcon
        {
            get
            {
                var iconPieces = new List<string>();

                if (IsForeignKeyColumn)
                {
                    const string iconText = @"<i title=""Foreign Key"" class=""foreignKeyIcon icon ion-key iconkey"" style=""padding-left: 5px;""></i>";
                    iconPieces.Add(iconText);
                }

                if (IsUniqueKeyColumn)
                {
                    const string iconText = @"<i title=""Unique Key"" class=""uniqueKeyIcon icon ion-key iconkey"" style=""padding-left: 5px;""></i>";
                    iconPieces.Add(iconText);
                }

                if (IsPrimaryKeyColumn)
                {
                    const string iconText = @"<i title=""Primary Key"" class=""primaryKeyIcon icon ion-key iconkey"" style=""padding-left: 5px;""></i>";
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

        public enum ParentObjectType
        {
            Table,
            View
        }
    }
}
