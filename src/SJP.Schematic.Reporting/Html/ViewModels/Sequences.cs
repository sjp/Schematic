using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Sequences : ITemplateParameter
{
    public Sequences(IEnumerable<Main.Sequence> sequences)
    {
        if (sequences == null || sequences.AnyNull())
            throw new ArgumentNullException(nameof(sequences));

        SequencesCount = sequences.UCount();
        SequencesTableClass = SequencesCount > 0 ? CssClasses.DataTableClass : string.Empty;
        AllSequences = sequences;
    }

    public ReportTemplate Template { get; } = ReportTemplate.Sequences;

    public uint SequencesCount { get; }

    public HtmlString SequencesTableClass { get; }

    public IEnumerable<Main.Sequence> AllSequences { get; }
}
