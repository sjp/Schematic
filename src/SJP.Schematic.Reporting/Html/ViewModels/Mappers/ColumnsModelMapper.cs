using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal class ColumnsModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseTable, IEnumerable<Columns.TableColumn>>,
        IDatabaseModelMapper<IRelationalDatabaseView, IEnumerable<Columns.ViewColumn>>
    {
        public ColumnsModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public IEnumerable<Columns.TableColumn> Map(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var primaryKey = dbObject.PrimaryKey;
            var uniqueKeys = dbObject.UniqueKeys.ToList();
            var parentKeys = dbObject.ParentKeys.ToList();

            var columns = dbObject.Columns.ToList();

            return columns.Select((column, i) =>
            {
                var isPrimaryKeyColumn = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == column.Name.LocalName);
                var isUniqueKeyColumn = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == column.Name.LocalName));
                var isForeignKeyColumn = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == column.Name.LocalName));

                return new Columns.TableColumn(
                    dbObject.Name,
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

        public Task<IEnumerable<Columns.TableColumn>> MapAsync(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<IEnumerable<Columns.TableColumn>> MapAsyncCore(IRelationalDatabaseTable dbObject)
        {
            var primaryKey = await dbObject.PrimaryKeyAsync().ConfigureAwait(false);
            var uniqueKeys = await dbObject.UniqueKeysAsync().ConfigureAwait(false);
            var parentKeys = await dbObject.ParentKeysAsync().ConfigureAwait(false);

            var columns = await dbObject.ColumnsAsync().ConfigureAwait(false);

            return columns.Select((column, i) =>
            {
                var isPrimaryKeyColumn = primaryKey != null && primaryKey.Columns.Any(c => c.Name.LocalName == column.Name.LocalName);
                var isUniqueKeyColumn = uniqueKeys.Any(uk => uk.Columns.Any(ukc => ukc.Name.LocalName == column.Name.LocalName));
                var isForeignKeyColumn = parentKeys.Any(fk => fk.ChildKey.Columns.Any(fkc => fkc.Name.LocalName == column.Name.LocalName));

                return new Columns.TableColumn(
                    dbObject.Name,
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

        public IEnumerable<Columns.ViewColumn> Map(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columns = dbObject.Columns.ToList();

            return columns.Select((c, i) =>
                new Columns.ViewColumn(
                    dbObject.Name,
                    i + 1,
                    c.Name.LocalName,
                    c.Type.Definition,
                    c.IsNullable
                )
            ).ToList();
        }

        public Task<IEnumerable<Columns.ViewColumn>> MapAsync(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<IEnumerable<Columns.ViewColumn>> MapAsyncCore(IRelationalDatabaseView dbObject)
        {
            var columns = await dbObject.ColumnsAsync().ConfigureAwait(false);

            return columns.Select((c, i) =>
                new Columns.ViewColumn(
                    dbObject.Name,
                    i + 1,
                    c.Name.LocalName,
                    c.Type.Definition,
                    c.IsNullable
                )
            ).ToList();
        }
    }
}
