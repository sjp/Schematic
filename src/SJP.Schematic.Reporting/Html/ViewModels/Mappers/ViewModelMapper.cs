using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ViewModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseView, View>
    {
        public ViewModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        public View Map(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var rowCount = Connection.GetRowCount(Dialect, dbObject.Name);
            var viewColumns = dbObject.Columns.ToList();

            var columns = viewColumns.Select((vc, i) =>
                new View.Column(
                    vc.Name?.LocalName ?? string.Empty,
                    i + 1,
                    vc.IsNullable,
                    vc.Type.Definition,
                    vc.DefaultValue
                )).ToList();

            return new View(
                dbObject.Name,
                "../",
                rowCount,
                dbObject.Definition,
                columns
            );
        }

        public Task<View> MapAsync(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<View> MapAsyncCore(IRelationalDatabaseView dbObject)
        {
            var rowCount = await Connection.GetRowCountAsync(Dialect, dbObject.Name).ConfigureAwait(false);
            var viewColumns = await dbObject.ColumnsAsync().ConfigureAwait(false);

            var columns = viewColumns.Select((vc, i) =>
                new View.Column(
                    vc.Name?.LocalName ?? string.Empty,
                    i + 1,
                    vc.IsNullable,
                    vc.Type.Definition,
                    vc.DefaultValue
                )).ToList();

            return new View(
                dbObject.Name,
                "../",
                rowCount,
                dbObject.Definition,
                columns
            );
        }
    }
}
