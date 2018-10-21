using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class ViewModelMapper
    {
        public ViewModelMapper(IDbConnection connection, IDatabaseDialect dialect)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Dialect = dialect ?? throw new ArgumentNullException(nameof(dialect));
        }

        private IDbConnection Connection { get; }

        private IDatabaseDialect Dialect { get; }

        public View Map(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var rowCount = Connection.GetRowCount(Dialect, view.Name);
            var viewColumns = view.Columns.ToList();

            var columns = viewColumns.Select((vc, i) =>
                new View.Column(
                    vc.Name?.LocalName ?? string.Empty,
                    i + 1,
                    vc.IsNullable,
                    vc.Type.Definition,
                    vc.DefaultValue
                )).ToList();

            return new View(
                view.Name,
                "../",
                rowCount,
                view.Definition,
                columns
            );
        }

        public Task<View> MapAsync(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return MapAsyncCore(view);
        }

        private async Task<View> MapAsyncCore(IRelationalDatabaseView view)
        {
            var rowCount = await Connection.GetRowCountAsync(Dialect, view.Name).ConfigureAwait(false);
            var viewColumns = await view.ColumnsAsync().ConfigureAwait(false);

            var columns = viewColumns.Select((vc, i) =>
                new View.Column(
                    vc.Name?.LocalName ?? string.Empty,
                    i + 1,
                    vc.IsNullable,
                    vc.Type.Definition,
                    vc.DefaultValue
                )).ToList();

            return new View(
                view.Name,
                "../",
                rowCount,
                view.Definition,
                columns
            );
        }
    }
}
