using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class MainModelMapper
    {
        public MainModelMapper(IDbConnection connection, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

        public Task<Main.Table> MapAsync(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return MapAsyncCore(table, cancellationToken);
        }

        private async Task<Main.Table> MapAsyncCore(IRelationalDatabaseTable table, CancellationToken cancellationToken)
        {
            var parentKeyCount = table.ParentKeys.UCount();
            var childKeyCount = table.ChildKeys.UCount();
            var columnCount = table.Columns.UCount();

            var rowCount = await Connection.GetRowCountAsync(Database.Dialect, table.Name, cancellationToken).ConfigureAwait(false);

            return new Main.Table(
                table.Name,
                parentKeyCount,
                childKeyCount,
                columnCount,
                rowCount
            );
        }

        public Task<Main.View> MapAsync(IDatabaseView view, CancellationToken cancellationToken)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return MapAsyncCore(view, cancellationToken);
        }

        private async Task<Main.View> MapAsyncCore(IDatabaseView view, CancellationToken cancellationToken)
        {
            var columnCount = view.Columns.UCount();
            var rowCount = await Connection.GetRowCountAsync(Database.Dialect, view.Name, cancellationToken).ConfigureAwait(false);

            return new Main.View(view.Name, columnCount, rowCount);
        }

        public Main.Sequence Map(IDatabaseSequence sequence)
        {
            if (sequence == null)
                throw new ArgumentNullException(nameof(sequence));

            return new Main.Sequence(
                sequence.Name,
                sequence.Start,
                sequence.Increment,
                sequence.MinValue,
                sequence.MaxValue,
                sequence.Cache,
                sequence.Cycle
            );
        }

        public Task<Main.Synonym> MapAsync(IDatabaseSynonym synonym, CancellationToken cancellationToken)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            return MapAsyncCore(synonym, cancellationToken);
        }

        private async Task<Main.Synonym> MapAsyncCore(IDatabaseSynonym synonym, CancellationToken cancellationToken)
        {
            var targetUrl = await GetSynonymTargetUrlAsync(synonym.Target, cancellationToken).ConfigureAwait(false);
            return new Main.Synonym(synonym.Name, synonym.Target, targetUrl);
        }

        private async Task<Option<Uri>> GetSynonymTargetUrlAsync(Identifier identifier, CancellationToken cancellationToken)
        {
            var tableOption = Database.GetTable(identifier, cancellationToken);
            var tableUri = tableOption.Map(t => new Uri("tables/" + t.Name.ToSafeKey() + ".html", UriKind.Relative));
            var tableUriIsSome = await tableUri.IsSome.ConfigureAwait(false);
            if (tableUriIsSome)
                return await tableUri.ToOption().ConfigureAwait(false);

            var viewOption = Database.GetView(identifier, cancellationToken);
            var viewUri = viewOption.Map(v => new Uri("views/" + v.Name.ToSafeKey() + ".html", UriKind.Relative));
            var viewUriIsSome = await viewUri.IsSome.ConfigureAwait(false);
            if (viewUriIsSome)
                return await viewUri.ToOption().ConfigureAwait(false);

            var routineOption = Database.GetRoutine(identifier, cancellationToken);
            var routineUri = routineOption.Map(r => new Uri("routines/" + r.Name.ToSafeKey() + ".html", UriKind.Relative));
            var routineIsSome = await routineUri.IsSome.ConfigureAwait(false);
            if (routineIsSome)
                return await routineUri.ToOption().ConfigureAwait(false);

            return Option<Uri>.None;
        }

        public Main.Routine Map(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            return new Main.Routine(routine.Name);
        }
    }
}
