using System;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ConstraintsModelMapper :
        IDatabaseModelMapper<IDatabaseKey, Constraints.PrimaryKeyConstraint>,
        IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>,
        IDatabaseModelMapper<IDatabaseRelationalKey, Constraints.ForeignKey>,
        IDatabaseModelMapper<IDatabaseCheckConstraint, Constraints.CheckConstraint>
    {
        Constraints.PrimaryKeyConstraint IDatabaseModelMapper<IDatabaseKey, Constraints.PrimaryKeyConstraint>.Map(IDatabaseKey dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnNames = dbObject.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.PrimaryKeyConstraint(
                dbObject.Table.Name,
                dbObject.Name?.LocalName,
                columnNames
            );
        }

        Constraints.UniqueKey IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>.Map(IDatabaseKey dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnNames = dbObject.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.UniqueKey(
                dbObject.Table.Name,
                dbObject.Name?.LocalName,
                columnNames
            );
        }

        public Constraints.ForeignKey Map(IDatabaseRelationalKey dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var childColumnNames = dbObject.ChildKey.Columns.Select(c => c.Name.LocalName).ToList();
            var parentColumnNames = dbObject.ParentKey.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.ForeignKey(
                dbObject.ChildKey.Table.Name,
                dbObject.ChildKey.Name?.LocalName,
                childColumnNames,
                dbObject.ParentKey.Table.Name,
                dbObject.ParentKey.Name?.LocalName,
                parentColumnNames,
                dbObject.DeleteRule,
                dbObject.UpdateRule
            );
        }

        public Constraints.CheckConstraint Map(IDatabaseCheckConstraint dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return new Constraints.CheckConstraint(
                dbObject.Table.Name,
                dbObject.Name?.LocalName,
                dbObject.Definition
            );
        }
    }
}
