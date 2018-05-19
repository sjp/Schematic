using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.SchemaSpy.Html.ViewModels.Mappers
{
    internal class MainModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseTable, Main.Table>,
        IDatabaseModelMapper<IRelationalDatabaseView, Main.View>
    {
        public MainModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public Main.Table Map(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var parentKeyLookup = dbObject.ParentKey;
            var parentKeyCount = parentKeyLookup.UCount();

            var childKeys = dbObject.ChildKeys;
            var childKeyCount = childKeys.UCount();

            var columnLookup = dbObject.Column;
            var columnCount = columnLookup.UCount();

            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);

            return new Main.Table(dbObject.Name)
            {
                ParentsCount = parentKeyCount,
                ChildrenCount = childKeyCount,
                ColumnCount = columnCount,
                RowCount = rowCount
            };
        }

        public async Task<Main.Table> MapAsync(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var parentKeyLookup = await dbObject.ParentKeyAsync().ConfigureAwait(false);
            var parentKeyCount = parentKeyLookup.UCount();

            var childKeys = await dbObject.ChildKeysAsync().ConfigureAwait(false);
            var childKeyCount = childKeys.UCount();

            var columnLookup = await dbObject.ColumnAsync().ConfigureAwait(false);
            var columnCount = columnLookup.UCount();

            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);

            return new Main.Table(dbObject.Name)
            {
                ParentsCount = parentKeyCount,
                ChildrenCount = childKeyCount,
                ColumnCount = columnCount,
                RowCount = rowCount
            };
        }

        public Main.View Map(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnLookup = dbObject.Column;
            var columnCount = columnLookup.UCount();

            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);

            return new Main.View(dbObject.Name)
            {
                ColumnCount = columnCount,
                RowCount = rowCount
            };
        }

        public async Task<Main.View> MapAsync(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnLookup = await dbObject.ColumnAsync().ConfigureAwait(false);
            var columnCount = columnLookup.UCount();

            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);

            return new Main.View(dbObject.Name)
            {
                ColumnCount = columnCount,
                RowCount = rowCount
            };
        }
    }
}
