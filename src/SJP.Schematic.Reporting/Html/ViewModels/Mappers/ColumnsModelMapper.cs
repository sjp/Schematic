using System;
using System.Collections.Generic;
using System.Linq;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ColumnsModelMapper
    {
        public IEnumerable<Columns.TableColumn> Map(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var primaryKey = table.PrimaryKey;
            var uniqueKeys = table.UniqueKeys.ToList();
            var parentKeys = table.ParentKeys.ToList();

            var columns = table.Columns.ToList();

            return columns.Select((column, i) =>
            {
                var isPrimaryKeyColumn = primaryKey.Match(pk => pk.Columns.Any(c => string.Equals(c.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal)), static () => false);
                var isUniqueKeyColumn = uniqueKeys.Any(uk => uk.Columns.Any(ukc => string.Equals(ukc.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal)));
                var isForeignKeyColumn = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => string.Equals(fkc.Name.LocalName, column.Name.LocalName, StringComparison.Ordinal)));

                return new Columns.TableColumn(
                    table.Name,
                    i + 1,
                    column.Name.LocalName,
                    column.Type.Definition,
                    column.IsNullable,
                    column.DefaultValue,
                    isPrimaryKeyColumn,
                    isUniqueKeyColumn,
                    isForeignKeyColumn
                );
            }).ToList();
        }

        public IEnumerable<Columns.ViewColumn> Map(IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var columns = view.Columns.ToList();

            return columns.Select((c, i) =>
                new Columns.ViewColumn(
                    view.Name,
                    i + 1,
                    c.Name.LocalName,
                    c.Type.Definition,
                    c.IsNullable
                )
            ).ToList();
        }
    }
}
