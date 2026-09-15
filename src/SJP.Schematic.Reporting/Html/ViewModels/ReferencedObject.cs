using System;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// A link from an object defined by an expression (a view, a routine) to an object it references
/// (hash route into the SPA).
/// </summary>
public sealed class ReferencedObject
{
    public ReferencedObject(string name, string url)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Url = url ?? throw new ArgumentNullException(nameof(url));
    }

    public string Name { get; }

    public string Url { get; }
}
