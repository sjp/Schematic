﻿using System;
using System.Data;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal class OrphansModelMapper : IDatabaseModelMapper<IRelationalDatabaseTable, Orphans.Table>
    {
        public OrphansModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public Orphans.Table Map(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnCount = dbObject.Column.UCount();
            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);

            return new Orphans.Table(dbObject.Name, columnCount, rowCount);
        }

        public async Task<Orphans.Table> MapAsync(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnLookup = await dbObject.ColumnAsync().ConfigureAwait(false);
            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);

            return new Orphans.Table(dbObject.Name, columnLookup.UCount(), rowCount);
        }
    }
}
