using System;
using System.Data;
using System.Threading.Tasks;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

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

        public Main.Table Map(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var parentKeys = table.ParentKeys;
            var parentKeyCount = parentKeys.UCount();

            var childKeys = table.ChildKeys;
            var childKeyCount = childKeys.UCount();

            var columns = table.Columns;
            var columnCount = columns.UCount();

            var rowCount = Connection.GetRowCount(Database.Dialect, table.Name);

            return new Main.Table(
                table.Name,
                parentKeyCount,
                childKeyCount,
                columnCount,
                rowCount
            );
        }

        public Task<Main.Table> MapAsync(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            return MapAsyncCore(table);
        }

        private async Task<Main.Table> MapAsyncCore(IRelationalDatabaseTable table)
        {
            var parentKeys = await table.ParentKeysAsync().ConfigureAwait(false);
            var parentKeyCount = parentKeys.UCount();

            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var childKeyCount = childKeys.UCount();

            var columns = await table.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.UCount();

            var rowCount = await Connection.GetRowCountAsync(Database.Dialect, table.Name).ConfigureAwait(false);

            return new Main.Table(
                table.Name,
                parentKeyCount,
                childKeyCount,
                columnCount,
                rowCount
            );
        }

        public Main.View Map(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var columns = view.Columns;
            var columnCount = columns.UCount();
            var rowCount = Connection.GetRowCount(Database.Dialect, view.Name);

            return new Main.View(view.Name, columnCount, rowCount);
        }

        public Task<Main.View> MapAsync(IRelationalDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return MapAsyncCore(view);
        }

        private async Task<Main.View> MapAsyncCore(IRelationalDatabaseView view)
        {
            var columns = await view.ColumnsAsync().ConfigureAwait(false);
            var columnCount = columns.UCount();
            var rowCount = await Connection.GetRowCountAsync(Database.Dialect, view.Name).ConfigureAwait(false);

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

        public Main.Synonym Map(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            var targetUrl = GetSynonymTargetUrl(synonym.Target);
            return new Main.Synonym(synonym.Name, synonym.Target, targetUrl);
        }

        public Task<Main.Synonym> MapAsync(IDatabaseSynonym synonym)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));

            return MapAsyncCore(synonym);
        }

        private async Task<Main.Synonym> MapAsyncCore(IDatabaseSynonym synonym)
        {
            var targetUrl = await GetSynonymTargetUrlAsync(synonym.Target).ConfigureAwait(false);
            return new Main.Synonym(synonym.Name, synonym.Target, targetUrl);
        }

        private Option<Uri> GetSynonymTargetUrl(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var table = Database.GetTable(identifier);
            var tableUri = table.Map(t => new Uri("tables/" + t.Name.ToSafeKey() + ".html", UriKind.Relative));
            if (tableUri.IsSome)
                return tableUri;

            var view = Database.GetView(identifier);
            var viewUri = view.Map(v => new Uri("views/" + v.Name.ToSafeKey() + ".html", UriKind.Relative));
            if (viewUri.IsSome)
                return viewUri;

            return Option<Uri>.None;
        }

        private async Task<Option<Uri>> GetSynonymTargetUrlAsync(Identifier identifier)
        {
            var tableOption = Database.GetTableAsync(identifier);
            var tableUri = tableOption.Map(t => new Uri("tables/" + t.Name.ToSafeKey() + ".html", UriKind.Relative));
            var tableUriIsSome = await tableUri.IsSome.ConfigureAwait(false);
            if (tableUriIsSome)
                return await tableUri.ToOption().ConfigureAwait(false);

            var viewOption = Database.GetViewAsync(identifier);
            var viewUri = viewOption.Map(v => new Uri("views/" + v.Name.ToSafeKey() + ".html", UriKind.Relative));
            var viewUriIsSome = await viewUri.IsSome.ConfigureAwait(false);
            if (viewUriIsSome)
                return await viewUri.ToOption().ConfigureAwait(false);

            return Option<Uri>.None;
        }
    }
}
