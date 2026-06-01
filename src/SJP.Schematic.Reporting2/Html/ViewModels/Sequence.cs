using System;
using System.Text.Json.Serialization;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-sequence detail payload (<c>data/sequences/&lt;safeKey&gt;.json</c>): the sequence's
/// generation parameters.
/// </summary>
public sealed class Sequence : ITemplateParameter
{
    public Sequence(
        Identifier sequenceName,
        decimal start,
        decimal increment,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        int cache,
        bool cycle
    )
    {
        ArgumentNullException.ThrowIfNull(sequenceName);

        Name = sequenceName.ToVisibleName();
        SequenceUrl = UrlRouter.GetSequenceUrl(sequenceName);

        Start = start;
        Increment = increment;
        MinValue = minValue.MatchUnsafe(static mv => mv, static () => (decimal?)null);
        MaxValue = maxValue.MatchUnsafe(static mv => mv, static () => (decimal?)null);
        Cache = cache;
        Cycle = cycle;
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Sequence;

    public string Name { get; }

    public string SequenceUrl { get; }

    public decimal Start { get; }

    public decimal Increment { get; }

    public decimal? MinValue { get; }

    public decimal? MaxValue { get; }

    public int Cache { get; }

    public bool Cycle { get; }
}
