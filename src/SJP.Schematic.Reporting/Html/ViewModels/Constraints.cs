using System;
using System.Collections.Generic;
using System.Data;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels
{
    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Constraints : ITemplateParameter
    {
        public Constraints(
            IEnumerable<PrimaryKeyConstraint> primaryKeys,
            IEnumerable<UniqueKey> uniqueKeys,
            IEnumerable<ForeignKey> foreignKeys,
            IEnumerable<CheckConstraint> checks
        )
        {
            PrimaryKeys = primaryKeys ?? throw new ArgumentNullException(nameof(primaryKeys));
            UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
            ForeignKeys = foreignKeys ?? throw new ArgumentNullException(nameof(foreignKeys));
            CheckConstraints = checks ?? throw new ArgumentNullException(nameof(checks));

            PrimaryKeysCount = primaryKeys.UCount();
            PrimaryKeysTableClass = PrimaryKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

            UniqueKeysCount = uniqueKeys.UCount();
            UniqueKeysTableClass = UniqueKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

            ForeignKeysCount = foreignKeys.UCount();
            ForeignKeysTableClass = ForeignKeysCount > 0 ? CssClasses.DataTableClass : string.Empty;

            CheckConstraintsCount = checks.UCount();
            CheckConstraintsTableClass = CheckConstraintsCount > 0 ? CssClasses.DataTableClass : string.Empty;
        }

        public ReportTemplate Template { get; } = ReportTemplate.Constraints;

        public IEnumerable<PrimaryKeyConstraint> PrimaryKeys { get; }

        public uint PrimaryKeysCount { get; }

        public HtmlString PrimaryKeysTableClass { get; }

        public IEnumerable<UniqueKey> UniqueKeys { get; }

        public uint UniqueKeysCount { get; }

        public HtmlString UniqueKeysTableClass { get; }

        public IEnumerable<ForeignKey> ForeignKeys { get; }

        public uint ForeignKeysCount { get; }

        public HtmlString ForeignKeysTableClass { get; }

        public IEnumerable<CheckConstraint> CheckConstraints { get; }

        public uint CheckConstraintsCount { get; }

        public HtmlString CheckConstraintsTableClass { get; }

        /// <summary>
        /// Internal. Not intended to be used outside of this assembly. Only required for templating.
        /// </summary>
        public abstract class TableConstraint
        {
            protected TableConstraint(Identifier tableName, string constraintName)
            {
                if (tableName == null)
                    throw new ArgumentNullException(nameof(tableName));

                TableName = tableName.ToVisibleName();
                TableUrl = tableName.ToSafeKey();
                ConstraintName = constraintName ?? string.Empty;
            }

            public string TableName { get; }

            public string TableUrl { get; }

            public string ConstraintName { get; }
        }

        /// <summary>
        /// Internal. Not intended to be used outside of this assembly. Only required for templating.
        /// </summary>
        public sealed class PrimaryKeyConstraint : TableConstraint
        {
            public PrimaryKeyConstraint(Identifier tableName, string constraintName, IEnumerable<string> columnNames)
                : base(tableName, constraintName)
            {
                if (columnNames == null || columnNames.Empty())
                    throw new ArgumentNullException(nameof(columnNames));

                ColumnNames = columnNames.Join(", ");
            }

            public string ColumnNames { get; }
        }

        /// <summary>
        /// Internal. Not intended to be used outside of this assembly. Only required for templating.
        /// </summary>
        public sealed class UniqueKey : TableConstraint
        {
            public UniqueKey(Identifier tableName, string constraintName, IEnumerable<string> columnNames)
                : base(tableName, constraintName)
            {
                if (columnNames == null || columnNames.Empty())
                    throw new ArgumentNullException(nameof(columnNames));

                ColumnNames = columnNames.Join(", ");
            }

            public string ColumnNames { get; }
        }

        /// <summary>
        /// Internal. Not intended to be used outside of this assembly. Only required for templating.
        /// </summary>
        public sealed class ForeignKey : TableConstraint
        {
            public ForeignKey(
                Identifier childTableName,
                string childConstraintName,
                IEnumerable<string> childColumnNames,
                Identifier parentTableName,
                string parentConstraintName,
                IEnumerable<string> parentColumnNames,
                Rule deleteRule,
                Rule updateRule
            )
                : base(childTableName, childConstraintName)
            {
                if (parentTableName == null)
                    throw new ArgumentNullException(nameof(parentTableName));
                if (childColumnNames == null || childColumnNames.Empty())
                    throw new ArgumentNullException(nameof(childColumnNames));
                if (parentColumnNames == null || parentColumnNames.Empty())
                    throw new ArgumentNullException(nameof(parentColumnNames));
                if (!deleteRule.IsValid())
                    throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(deleteRule));
                if (!updateRule.IsValid())
                    throw new ArgumentException($"The { nameof(Rule) } provided must be a valid enum.", nameof(updateRule));

                ParentTableName = parentTableName.ToVisibleName();
                ParentTableUrl = parentTableName.ToSafeKey();
                ParentConstraintName = parentConstraintName ?? string.Empty;

                ChildColumnNames = childColumnNames.Join(", ");
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

            private static readonly IReadOnlyDictionary<Rule, string> _ruleDescription = new Dictionary<Rule, string>
            {
                [Rule.None] = "NONE",
                [Rule.Cascade] = "CASCADE",
                [Rule.SetDefault] = "SET DEFAULT",
                [Rule.SetNull] = "SET NULL"
            };
        }

        /// <summary>
        /// Internal. Not intended to be used outside of this assembly. Only required for templating.
        /// </summary>
        public sealed class CheckConstraint : TableConstraint
        {
            public CheckConstraint(Identifier tableName, string constraintName, string definition)
                : base(tableName, constraintName)
            {
                if (definition.IsNullOrWhiteSpace())
                    throw new ArgumentNullException(nameof(definition));

                Definition = definition;
            }

            public string Definition { get; }
        }
    }
}
