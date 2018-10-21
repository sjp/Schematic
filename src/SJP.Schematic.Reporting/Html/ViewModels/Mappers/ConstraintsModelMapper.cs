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

            var columnNames = primaryKey.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.PrimaryKeyConstraint(
                tableName,
                primaryKey.Name?.LocalName,
                columnNames
            );
        }

        public Constraints.UniqueKey MapUniqueKey(Identifier tableName, IDatabaseKey uniqueKey)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (uniqueKey == null)
                throw new ArgumentNullException(nameof(uniqueKey));

            var columnNames = uniqueKey.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.UniqueKey(
                tableName,
                uniqueKey.Name?.LocalName,
                columnNames
            );
        }

        public Constraints.ForeignKey MapForeignKey(IDatabaseRelationalKey foreignKey)
        {
            if (foreignKey == null)
                throw new ArgumentNullException(nameof(foreignKey));

            var childColumnNames = foreignKey.ChildKey.Columns.Select(c => c.Name.LocalName).ToList();
            var parentColumnNames = foreignKey.ParentKey.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.ForeignKey(
                foreignKey.ChildTable,
                foreignKey.ChildKey.Name?.LocalName,
                childColumnNames,
                foreignKey.ParentTable,
                foreignKey.ParentKey.Name?.LocalName,
                parentColumnNames,
                foreignKey.DeleteRule,
                foreignKey.UpdateRule
            );
        }

        public Constraints.CheckConstraint MapCheckConstraint(Identifier tableName, IDatabaseCheckConstraint check)
        {
            if (tableName == null)
                throw new ArgumentNullException(nameof(tableName));
            if (check == null)
                throw new ArgumentNullException(nameof(check));

            return new Constraints.CheckConstraint(
                tableName,
                check.Name?.LocalName,
                check.Definition
            );
        }
    }
}
