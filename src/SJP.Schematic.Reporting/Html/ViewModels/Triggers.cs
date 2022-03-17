using System;
using System.Collections.Generic;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
/// </summary>
public sealed class Triggers : ITemplateParameter
{
    public Triggers(IEnumerable<Trigger> triggers)
    {
        if (triggers == null || triggers.AnyNull())
            throw new ArgumentNullException(nameof(triggers));

        TriggersCount = triggers.UCount();
        TriggersTableClass = TriggersCount > 0 ? CssClasses.DataTableClass : string.Empty;
        AllTriggers = triggers;
    }

    public ReportTemplate Template { get; } = ReportTemplate.Triggers;

    public uint TriggersCount { get; }

    public HtmlString TriggersTableClass { get; }

    public IEnumerable<Trigger> AllTriggers { get; }
}
