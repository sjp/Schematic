using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal class ViewModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseView, View>
    {
        public ViewModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        protected IDbConnection Connection { get; }

        protected IDatabaseDialect Dialect { get; }

        public View Map(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);
            var viewColumns = dbObject.Columns.ToList();

            var columns = viewColumns.Select((vc, i) => new View.Column(vc.Name?.LocalName ?? string.Empty)
            {
                Ordinal = i + 1,
                DefaultValue = vc.DefaultValue,
                IsNullable = vc.IsNullable,
                Type = vc.Type.Definition
            }).ToList();

            return new View
            {
                ViewName = dbObject.Name,
                Definition = dbObject.Definition,
                Columns = columns,
                RowCount = rowCount
            };
        }

        public async Task<View> MapAsync(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);
            var viewColumns = await dbObject.ColumnsAsync().ConfigureAwait(false);

            var columns = viewColumns.Select((vc, i) => new View.Column(vc.Name?.LocalName ?? string.Empty)
            {
                Ordinal = i + 1,
                DefaultValue = vc.DefaultValue,
                IsNullable = vc.IsNullable,
                Type = vc.Type.Definition
            }).ToList();

            return new View
            {
                ViewName = dbObject.Name,
                Definition = dbObject.Definition,
                Columns = columns,
                RowCount = rowCount
            };
        }
    }
}
