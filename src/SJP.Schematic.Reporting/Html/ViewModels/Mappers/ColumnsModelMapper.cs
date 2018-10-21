using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ColumnsModelMapper
    {
        public ColumnsModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

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
                var isPrimaryKeyColumn = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == column.Name.LocalName);
                var isUniqueKeyColumn = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == column.Name.LocalName));
                var isForeignKeyColumn = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == column.Name.LocalName));

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

        public Task<IEnumerable<Columns.TableColumn>> MapAsync(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return MapAsyncCore(table);
        }

        private static async Task<IEnumerable<Columns.TableColumn>> MapAsyncCore(IRelationalDatabaseTable table)
        {
            var primaryKey = await table.PrimaryKeyAsync().ConfigureAwait(false);
            var uniqueKeys = await table.UniqueKeysAsync().ConfigureAwait(false);
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);

            var columns = await table.ColumnsAsync().ConfigureAwait(false);

            return columns.Select((column, i) =>
            {
                var isPrimaryKeyColumn = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == column.Name.LocalName);
                var isUniqueKeyColumn = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == column.Name.LocalName));
                var isForeignKeyColumn = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == column.Name.LocalName));

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

        public IEnumerable<Columns.ViewColumn> Map(IRelationalDatabaseView view)
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

        public Task<IEnumerable<Columns.ViewColumn>> MapAsync(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return MapAsyncCore(view);
        }

        private static async Task<IEnumerable<Columns.ViewColumn>> MapAsyncCore(IRelationalDatabaseView view)
        {
            var columns = await view.ColumnsAsync().ConfigureAwait(false);

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
