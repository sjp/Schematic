using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels.Mappers;

internal sealed class SynonymModelMapper
{
    public Synonym Map(IDatabaseSynonym synonym, SynonymTargets targets)
    {
        ArgumentNullException.ThrowIfNull(synonym);
        ArgumentNullException.ThrowIfNull(targets);

        var targetUrl = targets.GetTargetUrl(synonym.Target);
        return new Synonym(synonym.Name, synonym.Target, targetUrl);
    }
}