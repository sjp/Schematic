using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels.Mappers
{
    internal class ConstraintsModelMapper :
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
            return new Constraints.PrimaryKeyConstraint(dbObject.Table.Name)
            {
                Columns = columnNames,
                ConstraintName = dbObject.Name?.LocalName
            };
        }

        Constraints.UniqueKey IDatabaseModelMapper<IDatabaseKey, Constraints.UniqueKey>.Map(IDatabaseKey dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnNames = dbObject.Columns.Select(c => c.Name.LocalName).ToList();
            return new Constraints.UniqueKey(dbObject.Table.Name)
            {
                Columns = columnNames,
                ConstraintName = dbObject.Name?.LocalName
            };
        }

        public Constraints.ForeignKey Map(IDatabaseRelationalKey dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var childColumnNames = dbObject.ChildKey.Columns.Select(c => c.Name.LocalName).ToList();
            var parentColumnNames = dbObject.ParentKey.Columns.Select(c => c.Name.LocalName).ToList();

            return new Constraints.ForeignKey(dbObject.ChildKey.Table.Name, dbObject.ParentKey.Table.Name)
            {
                ChildColumns = childColumnNames,
                ConstraintName = dbObject.ChildKey.Name?.LocalName,
                ParentColumns = parentColumnNames,
                ParentConstraintName = dbObject.ParentKey.Name?.LocalName,
                DeleteRule = dbObject.DeleteRule,
                UpdateRule = dbObject.UpdateRule
            };
        }

        public Constraints.CheckConstraint Map(IDatabaseCheckConstraint dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return new Constraints.CheckConstraint(dbObject.Table.Name)
            {
                ConstraintName = dbObject.Name?.LocalName,
                Definition = dbObject.Definition
            };
        }
    }
}
