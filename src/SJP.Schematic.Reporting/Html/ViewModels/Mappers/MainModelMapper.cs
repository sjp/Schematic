using System;
using System.Collections.Generic;
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

        public Main.Synonym Map(IDatabaseSynonym synonym, SynonymTargets targets)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            var targetUrl = GetSynonymTargetUrl(synonym.Target, targets);
            return new Main.Synonym(synonym.Name, synonym.Target, targetUrl);
        }

        private Option<Uri> GetSynonymTargetUrl(Identifier identifier, SynonymTargets targets)
        {
            if (targets.Tables.ContainsKey(identifier))
                return new Uri("tables/" + identifier.ToSafeKey() + ".html", UriKind.Relative);

            if (targets.Views.ContainsKey(identifier))
                return new Uri("views/" + identifier.ToSafeKey() + ".html", UriKind.Relative);

            if (targets.Routines.ContainsKey(identifier))
                return new Uri("routines/" + identifier.ToSafeKey() + ".html", UriKind.Relative);

            return Option<Uri>.None;
        }

        public Main.Routine Map(IDatabaseRoutine routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));

            return new Main.Routine(routine.Name);
        }

        public class SynonymTargets
        {
            public SynonymTargets(
                IReadOnlyCollection<IRelationalDatabaseTable> tables,
                IReadOnlyCollection<IDatabaseView> views,
                IReadOnlyCollection<IDatabaseRoutine> routines
            )
            {
                if (tables == null)
                    throw new ArgumentNullException(nameof(tables));
                if (views == null)
                    throw new ArgumentNullException(nameof(views));
                if (routines == null)
                    throw new ArgumentNullException(nameof(routines));

                var tableLookup = new Dictionary<Identifier, IRelationalDatabaseTable>();
                foreach (var table in tables)
                    tableLookup[table.Name] = table;

                var viewLookup = new Dictionary<Identifier, IDatabaseView>();
                foreach (var view in views)
                    viewLookup[view.Name] = view;

                var routineLookup = new Dictionary<Identifier, IDatabaseRoutine>();
                foreach (var routine in routines)
                    routineLookup[routine.Name] = routine;

                Tables = tableLookup;
                Views = viewLookup;
                Routines = routineLookup;
            }

            public IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Tables { get; }

            public IReadOnlyDictionary<Identifier, IDatabaseView> Views { get; }

            public IReadOnlyDictionary<Identifier, IDatabaseRoutine> Routines { get; }
        }
    }
}
