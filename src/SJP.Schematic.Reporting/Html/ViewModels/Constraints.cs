using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    internal class Constraints : ITemplateParameter
    {
        public ReportTemplate Template { get; } = ReportTemplate.Constraints;

        public IEnumerable<PrimaryKeyConstraint> PrimaryKeys
        {
            get => _primaryKeys;
            set => _primaryKeys = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<PrimaryKeyConstraint> _primaryKeys = Enumerable.Empty<PrimaryKeyConstraint>();

        public uint PrimaryKeysCount => _primaryKeys.UCount();

        public string PrimaryKeysTableClass => PrimaryKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

        public IEnumerable<UniqueKey> UniqueKeys
        {
            get => _uniqueKeys;
            set => _uniqueKeys = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<UniqueKey> _uniqueKeys = Enumerable.Empty<UniqueKey>();

        public uint UniqueKeysCount => _uniqueKeys.UCount();

        public string UniqueKeysTableClass => UniqueKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

        public IEnumerable<ForeignKey> ForeignKeys
        {
            get => _foreignKeys;
            set => _foreignKeys = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<ForeignKey> _foreignKeys = Enumerable.Empty<ForeignKey>();

        public uint ForeignKeysCount => _foreignKeys.UCount();

        public string ForeignKeysTableClass => ForeignKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

        public IEnumerable<CheckConstraint> CheckConstraints
        {
            get => _checks;
            set => _checks = value ?? throw new ArgumentNullException(nameof(value));
        }

        private IEnumerable<CheckConstraint> _checks = Enumerable.Empty<CheckConstraint>();

        public uint CheckConstraintsCount => _checks.UCount();

        public string CheckConstraintsTableClass => CheckConstraintsCount > 0 ? CssClasses.DataTableClass : string.Empty;

        internal abstract class TableConstraint
        {
            protected TableConstraint(Identifier tableName)
            {
                _tableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
            }

            public string TableName => _tableName.ToVisibleName();

            public string TableUrl => _tableName.ToSafeKey();

            public string ConstraintName
            {
                get => _constraintName;
                set => _constraintName = value ?? string.Empty;
            }

            private string _constraintName = string.Empty;

            private readonly Identifier _tableName;
        }

        internal class PrimaryKeyConstraint : TableConstraint
        {
            public PrimaryKeyConstraint(Identifier tableName)
                : base(tableName)
            {
            }

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
            public UniqueKey(Identifier tableName)
                : base(tableName)
            {
            }

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
            public ForeignKey(Identifier childTableName, Identifier parentTableName)
                : base(childTableName)
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
            public CheckConstraint(Identifier tableName)
                : base(tableName)
            {
            }

            public string Definition
            {
                get => _definition;
                set => _definition = value ?? string.Empty;
            }

            private string _definition = string.Empty;
        }
    }
}
