using System;
using System.Linq;
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

        private static Option<Uri> GetSynonymTargetUrl(Identifier identifier, SynonymTargets targets)
        {
            if (targets.TableNames.Contains(identifier))
                return new Uri(UrlRouter.GetTableUrl(identifier), UriKind.Relative);

            if (targets.ViewNames.Contains(identifier))
                return new Uri(UrlRouter.GetViewUrl(identifier), UriKind.Relative);

            if (targets.SequenceNames.Contains(identifier))
                return new Uri(UrlRouter.GetSequenceUrl(identifier), UriKind.Relative);

            if (targets.SynonymNames.Contains(identifier))
                return new Uri(UrlRouter.GetSynonymUrl(identifier), UriKind.Relative);

            if (targets.RoutineNames.Contains(identifier))
                return new Uri(UrlRouter.GetRoutineUrl(identifier), UriKind.Relative);

            return Option<Uri>.None;
        }
    }
}
