using System;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers
{
    internal sealed class SynonymModelMapper
    {
        public Synonym Map(IDatabaseSynonym synonym, SynonymTargets targets)
        {
            if (synonym == null)
                throw new ArgumentNullException(nameof(synonym));
            if (targets == null)
                throw new ArgumentNullException(nameof(targets));

            var targetUrl = GetSynonymTargetUrl(synonym.Target, targets);
            return new Synonym(synonym.Name, synonym.Target, targetUrl, "../");
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
    }
}
