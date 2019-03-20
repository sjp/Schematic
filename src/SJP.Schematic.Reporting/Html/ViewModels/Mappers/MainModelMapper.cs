using System;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class MainModelMapper
    {
        public Main.Table Map(IRelationalDatabaseTable table, ulong rowCount)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var parentKeyCount = table.ParentKeys.UCount();
            var childKeyCount = table.ChildKeys.UCount();
            var columnCount = table.Columns.UCount();

            return new Main.Table(
                table.Name,
                parentKeyCount,
                childKeyCount,
                columnCount,
                rowCount
            );
        }

        public Main.View Map(IDatabaseView view, ulong rowCount)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var columnCount = view.Columns.UCount();
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

            if (targets.Sequences.ContainsKey(identifier))
                return new Uri("sequences/" + identifier.ToSafeKey() + ".html", UriKind.Relative);

            if (targets.Synonyms.ContainsKey(identifier))
                return new Uri("synonyms/" + identifier.ToSafeKey() + ".html", UriKind.Relative);

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
    }
}
