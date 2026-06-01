using System;
using System.Text.Json.Serialization;
using LanguageExt;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-synonym detail payload (<c>data/synonyms/&lt;safeKey&gt;.json</c>): the synonym and the
/// object it aliases. <see cref="TargetUrl"/> is the target's hash route, omitted from the JSON
/// when the target cannot be resolved to a known object.
/// </summary>
public sealed class Synonym : ITemplateParameter
{
    public Synonym(
        Identifier synonymName,
        Identifier targetName,
        Option<Uri> targetUrl
    )
    {
        ArgumentNullException.ThrowIfNull(synonymName);
        ArgumentNullException.ThrowIfNull(targetName);

        Name = synonymName.ToVisibleName();
        SynonymUrl = UrlRouter.GetSynonymUrl(synonymName);
        TargetName = targetName.ToVisibleName();
        TargetUrl = targetUrl.MatchUnsafe(static uri => uri.ToString(), static () => (string?)null);
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Synonym;

    public string Name { get; }

    public string SynonymUrl { get; }

    public string TargetName { get; }

    public string? TargetUrl { get; }
}
