using System;

namespace SJP.Schematic.Reporting;

/// <summary>
/// Describes the failure of a single report-rendering operation — one table, view, diagram, or
/// renderer. These are collected and surfaced together inside the <see cref="AggregateException"/>
/// thrown by <see cref="ReportGenerator.GenerateAsync"/>, so that every failure is reported, not
/// just the first, each labelled with the object or section that produced it.
/// </summary>
internal sealed class RenderException : Exception
{
    public RenderException()
    {
    }

    public RenderException(string? message)
        : base(message)
    {
    }

    public RenderException(string target, Exception innerException)
        : base($"Failed to render {target}.", innerException)
    {
        Target = target;
    }

    /// <summary>
    /// A human-readable label for the operation that failed, e.g. <c>table 'public.actor'</c>.
    /// </summary>
    public string Target { get; } = string.Empty;
}
