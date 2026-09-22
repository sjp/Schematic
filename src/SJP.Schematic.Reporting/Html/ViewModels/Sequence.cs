using System;
using EnumsNET;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-sequence detail payload (<c>data/sequences/&lt;safeKey&gt;.json</c>): the sequence's
/// generation parameters.
/// </summary>
public sealed class Sequence
{
    public Sequence(
        Identifier sequenceName,
        string type,
        Option<Uri> typeUrl,
        decimal start,
        decimal increment,
        Option<decimal> minValue,
        Option<decimal> maxValue,
        SequenceCacheMode cacheMode,
        Option<int> cacheSize,
        bool cycle,
        bool isOrdered
    )
    {
        ArgumentNullException.ThrowIfNull(sequenceName);
        if (!cacheMode.IsValid())
            throw new ArgumentException($"The {nameof(SequenceCacheMode)} provided must be a valid enum.", nameof(cacheMode));

        Name = sequenceName.ToVisibleName();
        SequenceUrl = UrlRouter.GetSequenceUrl(sequenceName);

        Type = type;
        TypeUrl = typeUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);
        Start = start;
        Increment = increment;
        MinValue = minValue.MatchUnsafe(static mv => mv, static () => (decimal?)null);
        MaxValue = maxValue.MatchUnsafe(static mv => mv, static () => (decimal?)null);
        Cache = SequenceCacheNames.GetName(cacheMode, cacheSize);
        Cycle = cycle;
        IsOrdered = isOrdered;
    }

    public string Name { get; }

    public string SequenceUrl { get; }

    public string Type { get; }

    /// <summary>
    /// The hash route of the user-defined type the sequence generates values of. Omitted from the
    /// JSON when that type is not one of the report's user-defined types.
    /// </summary>
    public string? TypeUrl { get; }

    public decimal Start { get; }

    public decimal Increment { get; }

    public decimal? MinValue { get; }

    public decimal? MaxValue { get; }

    public string Cache { get; }

    public bool Cycle { get; }

    public bool IsOrdered { get; }
}
