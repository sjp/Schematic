using System;
using System.Globalization;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// Internal. Not intended to be used outside of this assembly. Only required for templating.
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
        bool cycle,
        string rootPath
    )
    {
        if (sequenceName == null)
            throw new ArgumentNullException(nameof(sequenceName));

        Name = sequenceName.ToVisibleName();
        RootPath = rootPath ?? throw new ArgumentNullException(nameof(rootPath));
        Start = start;
        Increment = increment;
        MinValueText = minValue.Match(static mv => mv.ToString(CultureInfo.InvariantCulture), static () => string.Empty);
        MaxValueText = maxValue.Match(static mv => mv.ToString(CultureInfo.InvariantCulture), static () => string.Empty);
        Cache = cache;
        CycleText = cycle ? "✓" : "✗";
    }

    public ReportTemplate Template { get; } = ReportTemplate.Sequence;

    public string RootPath { get; }

    public string Name { get; }

    public decimal Start { get; }

    public decimal Increment { get; }

    public string MinValueText { get; }

    public string MaxValueText { get; }

    public int Cache { get; }

    public string CycleText { get; }
}
