using System;
using System.Data;
using System.Linq;
using System.Threading;
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

        public Task<View> MapAsync(IDatabaseView view, CancellationToken cancellationToken)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return MapAsyncCore(view, cancellationToken);
        }

        public async Task<View> MapAsyncCore(IDatabaseView view, CancellationToken cancellationToken)
        {
            var rowCount = await Connection.GetRowCountAsync(Dialect, view.Name, cancellationToken).ConfigureAwait(false);
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
    }
}
