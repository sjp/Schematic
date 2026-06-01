using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The constraints summary payload (<c>data/constraints.json</c>): primary keys, unique keys,
/// foreign keys, and check constraints across the schema. Each row links to its owning table (and,
/// for foreign keys, the referenced table) via hash routes.
/// </summary>
public sealed class Constraints : ITemplateParameter
{
    public Constraints(
        IEnumerable<PrimaryKeyConstraintRow> primaryKeys,
        IEnumerable<UniqueKeyRow> uniqueKeys,
        IEnumerable<ForeignKeyRow> foreignKeys,
        IEnumerable<CheckConstraintRow> checks
    )
    {
        PrimaryKeys = primaryKeys ?? throw new ArgumentNullException(nameof(primaryKeys));
        UniqueKeys = uniqueKeys ?? throw new ArgumentNullException(nameof(uniqueKeys));
        ForeignKeys = foreignKeys ?? throw new ArgumentNullException(nameof(foreignKeys));
        CheckConstraints = checks ?? throw new ArgumentNullException(nameof(checks));

        PrimaryKeysCount = primaryKeys.UCount();
        UniqueKeysCount = uniqueKeys.UCount();
        ForeignKeysCount = foreignKeys.UCount();
        CheckConstraintsCount = checks.UCount();
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Constraints;

    public IEnumerable<PrimaryKeyConstraintRow> PrimaryKeys { get; }

    public uint PrimaryKeysCount { get; }

    public IEnumerable<UniqueKeyRow> UniqueKeys { get; }

    public uint UniqueKeysCount { get; }

    public IEnumerable<ForeignKeyRow> ForeignKeys { get; }

    public uint ForeignKeysCount { get; }

    public IEnumerable<CheckConstraintRow> CheckConstraints { get; }

    public uint CheckConstraintsCount { get; }

    /// <summary>
    /// Common fields shared by every constraint row: the owning table and a hash-route link to it.
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

    public sealed class PrimaryKeyConstraintRow : TableConstraint
    {
        public PrimaryKeyConstraintRow(Identifier tableName, string constraintName, IEnumerable<string> columnNames)
            : base(tableName, constraintName)
        {
            if (columnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(columnNames));

            ColumnNames = columnNames.Join(", ");
        }

        public string ColumnNames { get; }
    }

    public sealed class UniqueKeyRow : TableConstraint
    {
        public UniqueKeyRow(Identifier tableName, string constraintName, IEnumerable<string> columnNames)
            : base(tableName, constraintName)
        {
            if (columnNames.NullOrEmpty())
                throw new ArgumentNullException(nameof(columnNames));

            ColumnNames = columnNames.Join(", ");
        }

        public string ColumnNames { get; }
    }

    public sealed class ForeignKeyRow : TableConstraint
    {
        public ForeignKeyRow(
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
            [ReferentialAction.SetNull] = "SET NULL",
        };
    }

    /// <summary>
    /// A check-constraint row. Named distinctly from <see cref="Table.CheckConstraint"/> so the JSON
    /// source generator emits non-colliding metadata.
    /// </summary>
    public sealed class CheckConstraintRow : TableConstraint
    {
        public CheckConstraintRow(Identifier tableName, string constraintName, string definition)
            : base(tableName, constraintName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(definition);

            Definition = definition;
        }

        public string Definition { get; }
    }
}
