using System;
using System.Data;
using System.Threading.Tasks;
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

        public Main.Table Map(IRelationalDatabaseTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var parentKeyLookup = table.ParentKey;
            var parentKeyCount = parentKeyLookup.UCount();

            var childKeys = table.ChildKeys;
            var childKeyCount = childKeys.UCount();

            var columnLookup = table.Column;
            var columnCount = columnLookup.UCount();

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
            var parentKeyLookup = await table.ParentKeyAsync().ConfigureAwait(false);
            var parentKeyCount = parentKeyLookup.UCount();

            var childKeys = await table.ChildKeysAsync().ConfigureAwait(false);
            var childKeyCount = childKeys.UCount();

            var columnLookup = await table.ColumnAsync().ConfigureAwait(false);
            var columnCount = columnLookup.UCount();

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

            var columnLookup = view.Column;
            var columnCount = columnLookup.UCount();
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
            var columnLookup = await view.ColumnAsync().ConfigureAwait(false);
            var columnCount = columnLookup.UCount();
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

        private Uri GetSynonymTargetUrl(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var table = Database.GetTable(identifier);
            if (table != null)
            {
                return new Uri("tables/" + table.Name.ToSafeKey() + ".html", UriKind.Relative);
            }
            else
            {
                var view = Database.GetView(identifier);
                if (view != null)
                    return new Uri("views/" + view.Name.ToSafeKey() + ".html", UriKind.Relative);
            }

            return null;
        }

        private async Task<Uri> GetSynonymTargetUrlAsync(Identifier identifier)
        {
            var table = await Database.GetTableAsync(identifier).ConfigureAwait(false);
            if (table != null)
            {
                return new Uri("tables/" + table.Name.ToSafeKey() + ".html", UriKind.Relative);
            }
            else
            {
                var view = await Database.GetViewAsync(identifier).ConfigureAwait(false);
                if (view != null)
                    return new Uri("views/" + view.Name.ToSafeKey() + ".html", UriKind.Relative);
            }

            return null;
        }
    }
}
