using System;
using System.Data;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class MainModelMapper :
        IDatabaseModelMapper<IRelationalDatabaseTable, Main.Table>,
        IDatabaseModelMapper<IRelationalDatabaseView, Main.View>,
        IDatabaseModelMapper<IDatabaseSynonym, Main.Synonym>,
        IDatabaseModelMapper<IDatabaseSequence, Main.Sequence>
    {
        public MainModelMapper(IDbConnection connection, IRelationalDatabase database)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
            Database = database ?? throw new ArgumentNullException(nameof(database));
        }

        private IDbConnection Connection { get; }

        private IRelationalDatabase Database { get; }

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

            var rowCount = Connection.GetRowCount(Database.Dialect, dbObject.Name);

            return new Main.Table(
                dbObject.Name,
                parentKeyCount,
                childKeyCount,
                columnCount,
                rowCount
            );
        }

        public Task<Main.Table> MapAsync(IRelationalDatabaseTable dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<Main.Table> MapAsyncCore(IRelationalDatabaseTable dbObject)
        {
            var parentKeyLookup = await dbObject.ParentKeyAsync().ConfigureAwait(false);
            var parentKeyCount = parentKeyLookup.UCount();

            var childKeys = await dbObject.ChildKeysAsync().ConfigureAwait(false);
            var childKeyCount = childKeys.UCount();

            var columnLookup = await dbObject.ColumnAsync().ConfigureAwait(false);
            var columnCount = columnLookup.UCount();

            var rowCount = await Connection.GetRowCountAsync(Database.Dialect, dbObject.Name).ConfigureAwait(false);

            return new Main.Table(
                dbObject.Name,
                parentKeyCount,
                childKeyCount,
                columnCount,
                rowCount
            );
        }

        public Main.View Map(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var columnLookup = dbObject.Column;
            var columnCount = columnLookup.UCount();
            var rowCount = Connection.GetRowCount(Database.Dialect, dbObject.Name);

            return new Main.View(dbObject.Name, columnCount, rowCount);
        }

        public Task<Main.View> MapAsync(IRelationalDatabaseView dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<Main.View> MapAsyncCore(IRelationalDatabaseView dbObject)
        {
            var columnLookup = await dbObject.ColumnAsync().ConfigureAwait(false);
            var columnCount = columnLookup.UCount();
            var rowCount = await Connection.GetRowCountAsync(Database.Dialect, dbObject.Name).ConfigureAwait(false);

            return new Main.View(dbObject.Name, columnCount, rowCount);
        }

        public Main.Sequence Map(IDatabaseSequence dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return new Main.Sequence(
                dbObject.Name,
                dbObject.Start,
                dbObject.Increment,
                dbObject.MinValue,
                dbObject.MaxValue,
                dbObject.Cache,
                dbObject.Cycle
            );
        }

        public Main.Synonym Map(IDatabaseSynonym dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            var targetUrl = GetSynonymTargetUrl(dbObject.Target);
            return new Main.Synonym(dbObject.Name, dbObject.Target, targetUrl);
        }

        public Task<Main.Synonym> MapAsync(IDatabaseSynonym dbObject)
        {
            if (dbObject == null)
                throw new ArgumentNullException(nameof(dbObject));

            return MapAsyncCore(dbObject);
        }

        private async Task<Main.Synonym> MapAsyncCore(IDatabaseSynonym dbObject)
        {
            var targetUrl = await GetSynonymTargetUrlAsync(dbObject.Target).ConfigureAwait(false);
            return new Main.Synonym(dbObject.Name, dbObject.Target, targetUrl);
        }

        private Uri GetSynonymTargetUrl(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var isTable = Database.TableExists(identifier);
            if (isTable)
            {
                return new Uri("tables/" + identifier.ToSafeKey() + ".html", UriKind.Relative);
            }
            else
            {
                var isView = Database.ViewExists(identifier);
                if (isView)
                    return new Uri("views/" + identifier.ToSafeKey() + ".html", UriKind.Relative);
            }

            return null;
        }

        private async Task<Uri> GetSynonymTargetUrlAsync(Identifier identifier)
        {
            var isTable = await Database.TableExistsAsync(identifier).ConfigureAwait(false);
            if (isTable)
            {
                return new Uri("tables/" + identifier.ToSafeKey() + ".html", UriKind.Relative);
            }
            else
            {
                var isView = await Database.ViewExistsAsync(identifier).ConfigureAwait(false);
                if (isView)
                    return new Uri("views/" + identifier.ToSafeKey() + ".html", UriKind.Relative);
            }

            return null;
        }
    }
}
