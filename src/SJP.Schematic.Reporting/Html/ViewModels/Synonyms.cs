using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Synonyms : ITemplateParameter
{
    public Synonyms(IEnumerable<Main.Synonym> synonyms)
    {
        if (synonyms.NullOrAnyNull())
            throw new ArgumentNullException(nameof(synonyms));

        SynonymsCount = synonyms.UCount();
        SynonymsTableClass = SynonymsCount > 0 ? CssClasses.DataTableClass : string.Empty;
        AllSynonyms = synonyms;
    }

    public ReportTemplate Template { get; } = ReportTemplate.Synonyms;

    public uint SynonymsCount { get; }

    public HtmlString SynonymsTableClass { get; }

    public IEnumerable<Main.Synonym> AllSynonyms { get; }
}