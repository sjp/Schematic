using System;
using System.Text.Json.Serialization;
using SJP.Schematic.Core;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-routine detail payload (<c>data/routines/&lt;safeKey&gt;.json</c>): the routine's
/// name and definition.
/// </summary>
public sealed class Routine : ITemplateParameter
{
    public Routine(
        Identifier routine,
        string definition
    )
    {
        ArgumentNullException.ThrowIfNull(routine);

        Name = routine.ToVisibleName();
        RoutineUrl = UrlRouter.GetRoutineUrl(routine);
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    [JsonIgnore]
    public ReportTemplate Template { get; } = ReportTemplate.Routine;

    public string Name { get; }

    public string RoutineUrl { get; }

    public string Definition { get; }
}
