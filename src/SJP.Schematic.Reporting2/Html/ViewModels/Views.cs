using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The views summary payload (<c>data/views.json</c>): the list of views rendered by the
/// views listing page.
/// </summary>
public sealed class Views : ITemplateParameter
{
    public Views(IEnumerable<Main.View> views)
    {
        if (views.NullOrAnyNull())
            throw new ArgumentNullException(nameof(views));

        ViewsCount = views.UCount();
        AllViews = views;
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Views;

    public uint ViewsCount { get; }

    public IEnumerable<Main.View> AllViews { get; }
}
