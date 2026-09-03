using System;
using System.Collections.Generic;
using EnumsNET;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The constraints summary payload (<c>data/constraints.json</c>): primary keys, unique keys,
/// foreign keys, and check constraints across the schema. Each row links to its owning table (and,
/// for foreign keys, the referenced table) via hash routes.
/// </summary>
public sealed class Constraints
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
        protected TableConstraint(Identifier tableName, string constraintName, bool isValidated, ConstraintDeferrability deferrability)
        {
            ArgumentNullException.ThrowIfNull(tableName);
            if (!deferrability.IsValid())
                throw new ArgumentException($"The {nameof(ConstraintDeferrability)} provided must be a valid enum.", nameof(deferrability));

            TableName = tableName.ToVisibleName();
            TableUrl = UrlRouter.GetTableUrl(tableName);
            ConstraintName = constraintName ?? string.Empty;
            IsValidated = isValidated;
            DeferrabilityDescription = ConstraintStateNames.GetDeferrabilityName(deferrability);
        }

        public string TableName { get; }

        public string TableUrl { get; }

        public string ConstraintName { get; }

        public bool IsValidated { get; }

        public string DeferrabilityDescription { get; }
    }

    public sealed class PrimaryKeyConstraintRow : TableConstraint
    {
        public PrimaryKeyConstraintRow(Identifier tableName, string constraintName, IEnumerable<string> columnNames, bool isValidated, ConstraintDeferrability deferrability)
            : base(tableName, constraintName, isValidated, deferrability)
        {
            ArgumentNullException.ThrowIfNull(columnNames);
            if (columnNames.Empty())
                throw new ArgumentException("A key must have at least one column.", nameof(columnNames));

            ColumnNames = columnNames.Join(", ");
        }

        public string ColumnNames { get; }
    }

    public sealed class UniqueKeyRow : TableConstraint
    {
        public UniqueKeyRow(Identifier tableName, string constraintName, IEnumerable<string> columnNames, bool isValidated, ConstraintDeferrability deferrability)
            : base(tableName, constraintName, isValidated, deferrability)
        {
            ArgumentNullException.ThrowIfNull(columnNames);
            if (columnNames.Empty())
                throw new ArgumentException("A key must have at least one column.", nameof(columnNames));

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
            ReferentialAction updateAction,
            bool isValidated,
            ConstraintDeferrability deferrability,
            ForeignKeyMatchType matchType
        )
            : base(childTableName, childConstraintName, isValidated, deferrability)
        {
            ArgumentNullException.ThrowIfNull(parentTableName);
            ArgumentNullException.ThrowIfNull(childColumnNames);
            if (childColumnNames.Empty())
                throw new ArgumentException("A foreign key must have at least one column.", nameof(childColumnNames));
            ArgumentNullException.ThrowIfNull(parentColumnNames);
            if (parentColumnNames.Empty())
                throw new ArgumentException("A foreign key must refer to at least one parent column.", nameof(parentColumnNames));
            if (!deleteAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(deleteAction));
            if (!updateAction.IsValid())
                throw new ArgumentException($"The {nameof(ReferentialAction)} provided must be a valid enum.", nameof(updateAction));
            if (!matchType.IsValid())
                throw new ArgumentException($"The {nameof(ForeignKeyMatchType)} provided must be a valid enum.", nameof(matchType));

            ParentTableName = parentTableName.ToVisibleName();
            ParentTableUrl = UrlRouter.GetTableUrl(parentTableName);
            ParentConstraintName = parentConstraintName ?? string.Empty;

            ChildColumnNames = childColumnNames.Join(", ");
            ParentColumnNames = parentColumnNames.Join(", ");

            DeleteActionDescription = GetActionDescription(deleteAction);
            UpdateActionDescription = GetActionDescription(updateAction);
            MatchTypeDescription = ConstraintStateNames.GetMatchTypeName(matchType);
        }

        public string ParentConstraintName { get; }

        public string ChildColumnNames { get; }

        public string ParentTableName { get; }

        public string ParentTableUrl { get; }

        public string ParentColumnNames { get; }

        public string DeleteActionDescription { get; }

        public string UpdateActionDescription { get; }

        public string MatchTypeDescription { get; }

        private static string GetActionDescription(ReferentialAction action) => action switch
        {
            ReferentialAction.NoAction => "NO ACTION",
            ReferentialAction.Restrict => "RESTRICT",
            ReferentialAction.Cascade => "CASCADE",
            ReferentialAction.SetDefault => "SET DEFAULT",
            ReferentialAction.SetNull => "SET NULL",
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
    }

    /// <summary>
    /// A check-constraint row. Named distinctly from <see cref="Table.CheckConstraint"/> so the JSON
    /// source generator emits non-colliding metadata.
    /// </summary>
    public sealed class CheckConstraintRow : TableConstraint
    {
        public CheckConstraintRow(Identifier tableName, string constraintName, string definition, bool isValidated, ConstraintDeferrability deferrability)
            : base(tableName, constraintName, isValidated, deferrability)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(definition);

            Definition = definition;
        }

        public string Definition { get; }
    }
}
