using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

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
            ArgumentNullException.ThrowIfNull(tableName);

            TableName = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);
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
            if (columnNames.NullOrEmpty())
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
            if (columnNames.NullOrEmpty())
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
            ReferentialAction deleteAction,
            ReferentialAction updateAction
        )
            : base(childTableName, childConstraintName)
        {
            ArgumentNullException.ThrowIfNull(parentTableName);
            if (childColumnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(childColumnNames));
            if (parentColumnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(parentColumnNames));
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(updateAction));

            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = UrlRouter.GetTableUrl(parentTableName);
            ParentConstraintName = parentConstraintName ?? string.Empty;

            ChildColumnNames = childColumnNames.Join(", ");
            ParentColumnNames = parentColumnNames.Join(", ");

            DeleteActionDescription = _actionDescription[deleteAction];
            UpdateActionDescription = _actionDescription[updateAction];
        }

        public string ParentConstraintName { get; }

        public string ChildColumnNames { get; }

        public string ParentTableName { get; }

        public string ParentTableUrl { get; }

        public string ParentColumnNames { get; }

        public string DeleteActionDescription { get; }

        public string UpdateActionDescription { get; }

        private static readonly IReadOnlyDictionary<ReferentialAction, string> _actionDescription = new Dictionary<ReferentialAction, string>
        {
            [ReferentialAction.NoAction] = "NO ACTION",
            [ReferentialAction.Restrict] = "RESTRICT",
            [ReferentialAction.Cascade] = "CASCADE",
            [ReferentialAction.SetDefault] = "SET DEFAULT",
            [ReferentialAction.SetNull] = "SET NULL"
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