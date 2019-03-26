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
                return new Uri(UrlRouter.GetTableUrl(identifier), UriKind.Relative);

            if (targets.Views.ContainsKey(identifier))
                return new Uri(UrlRouter.GetViewUrl(identifier), UriKind.Relative);

            if (targets.Sequences.ContainsKey(identifier))
                return new Uri(UrlRouter.GetSequenceUrl(identifier), UriKind.Relative);

            if (targets.Synonyms.ContainsKey(identifier))
                return new Uri(UrlRouter.GetSynonymUrl(identifier), UriKind.Relative);

            if (targets.Routines.ContainsKey(identifier))
                return new Uri(UrlRouter.GetRoutineUrl(identifier), UriKind.Relative);

            return Option<Uri>.None;
        }
    }
}
