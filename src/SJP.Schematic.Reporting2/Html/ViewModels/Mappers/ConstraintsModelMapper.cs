using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class ConstraintsModelMapper
{
    public Constraints.PrimaryKeyConstraintRow MapPrimaryKey(Identifier tableName, IDatabaseKey primaryKey)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(primaryKey);

        var pkConstraintName = primaryKey.Name.Match(static pkName => pkName.LocalName, static () => string.Empty);
        var columnNames = primaryKey.Columns.Select(static c => c.Name.LocalName).ToList();

        return new Constraints.PrimaryKeyConstraintRow(
            tableName,
            pkConstraintName,
            columnNames
        );
    }

    public Constraints.UniqueKeyRow MapUniqueKey(Identifier tableName, IDatabaseKey uniqueKey)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(uniqueKey);

        var ukConstraintName = uniqueKey.Name.Match(static ukName => ukName.LocalName, static () => string.Empty);
        var columnNames = uniqueKey.Columns.Select(static c => c.Name.LocalName).ToList();

        return new Constraints.UniqueKeyRow(
            tableName,
            ukConstraintName,
            columnNames
        );
    }

    public Constraints.ForeignKeyRow MapForeignKey(IDatabaseRelationalKey foreignKey)
    {
        ArgumentNullException.ThrowIfNull(foreignKey);

        var childKeyName = foreignKey.ChildKey.Name.Match(static fkName => fkName.LocalName, static () => string.Empty);
        var childColumnNames = foreignKey.ChildKey.Columns.Select(static c => c.Name.LocalName).ToList();
        var parentKeyName = foreignKey.ParentKey.Name.Match(static pkName => pkName.LocalName, static () => string.Empty);
        var parentColumnNames = foreignKey.ParentKey.Columns.Select(static c => c.Name.LocalName).ToList();

        return new Constraints.ForeignKeyRow(
            foreignKey.ChildTable,
            childKeyName,
            childColumnNames,
            foreignKey.ParentTable,
            parentKeyName,
            parentColumnNames,
            foreignKey.DeleteAction,
            foreignKey.UpdateAction
        );
    }

    public Constraints.CheckConstraintRow MapCheckConstraint(Identifier tableName, IDatabaseCheckConstraint check)
    {
        ArgumentNullException.ThrowIfNull(tableName);
        ArgumentNullException.ThrowIfNull(check);

        var constraintName = check.Name.Match(static name => name.LocalName, static () => string.Empty);

        return new Constraints.CheckConstraintRow(
            tableName,
            constraintName,
            check.Definition
        );
    }
}