using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ConstraintsModelMapper
    {
        public Constraints.PrimaryKeyConstraint MapPrimaryKey(Identifier tableName, IDatabaseKey primaryKey)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (primaryKey == null)
                throw new ArgumentNullException(nameof(primaryKey));

            var pkConstraintName = primaryKey.Name.Match(static pkName => pkName.LocalName, static () => string.Empty);
            var columnNames = primaryKey.Columns.Select(static c => c.Name.LocalName).ToList();

            return new Constraints.PrimaryKeyConstraint(
                tableName,
                pkConstraintName,
                columnNames
            );
        }

        public Constraints.UniqueKey MapUniqueKey(Identifier tableName, IDatabaseKey uniqueKey)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            var ukConstraintName = uniqueKey.Name.Match(static ukName => ukName.LocalName, static () => string.Empty);
            var columnNames = uniqueKey.Columns.Select(static c => c.Name.LocalName).ToList();

            return new Constraints.UniqueKey(
                tableName,
                ukConstraintName,
                columnNames
            );
        }

        public Constraints.ForeignKey MapForeignKey(IDatabaseRelationalKey foreignKey)
        {
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            var childKeyName = foreignKey.ChildKey.Name.Match(static fkName => fkName.LocalName, static () => string.Empty);
            var childColumnNames = foreignKey.ChildKey.Columns.Select(static c => c.Name.LocalName).ToList();
            var parentKeyName = foreignKey.ParentKey.Name.Match(static pkName => pkName.LocalName, static () => string.Empty);
            var parentColumnNames = foreignKey.ParentKey.Columns.Select(static c => c.Name.LocalName).ToList();

            return new Constraints.ForeignKey(
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

        public Constraints.CheckConstraint MapCheckConstraint(Identifier tableName, IDatabaseCheckConstraint check)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            var constraintName = check.Name.Match(static name => name.LocalName, static () => string.Empty);

            return new Constraints.CheckConstraint(
                tableName,
                constraintName,
                check.Definition
            );
        }
    }
}
