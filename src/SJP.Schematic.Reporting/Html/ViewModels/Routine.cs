using System;
using System.Collections.Generic;
using LanguageExt;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Reporting.Html.ViewModels;

/// <summary>
/// The per-routine detail payload (<c>data/routines/&lt;safeKey&gt;.json</c>): the routine's
/// name, kind, signature and definition.
/// </summary>
public sealed class Routine
{
    public Routine(
        Identifier routine,
        string definition,
        RoutineType routineType,
        Option<string> language,
        IEnumerable<Parameter> parameters,
        Option<string> returnType,
        IEnumerable<Overload> overloads
    )
    {
        ArgumentNullException.ThrowIfNull(routine);

        Name = routine.ToVisibleName();
        RoutineUrl = UrlRouter.GetRoutineUrl(routine);
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        RoutineType = routineType.ToString();
        Language = language.MatchUnsafe(static l => l, static () => (string?)null);

        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ParametersCount = parameters.UCount();

        ReturnType = returnType.MatchUnsafe(static t => t, static () => (string?)null);

        Overloads = overloads ?? throw new ArgumentNullException(nameof(overloads));
        OverloadsCount = overloads.UCount();
    }

    public string Name { get; }

    public string RoutineUrl { get; }

    public string Definition { get; }

    public string RoutineType { get; }

    public string? Language { get; }

    public IEnumerable<Parameter> Parameters { get; }

    public uint ParametersCount { get; }

    public string? ReturnType { get; }

    public IEnumerable<Overload> Overloads { get; }

    public uint OverloadsCount { get; }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Parameter
    {
        public Parameter(Option<Identifier> parameterName, string typeDefinition, RoutineParameterDirection direction, Option<string> defaultValue, int ordinal)
        {
            ParameterName = parameterName.MatchUnsafe(static name => name.LocalName, static () => (string?)null);
            Type = typeDefinition ?? throw new ArgumentNullException(nameof(typeDefinition));
            Direction = direction.ToString();
            DefaultValue = defaultValue.MatchUnsafe(static value => value, static () => (string?)null);
            Ordinal = ordinal;
        }

        public string? ParameterName { get; }

        public string Type { get; }

        public string Direction { get; }

        public string? DefaultValue { get; }

        public int Ordinal { get; }
    }

    /// <summary>
    /// Internal. Not intended to be used outside of this assembly. Only required for templating.
    /// </summary>
    public sealed class Overload
    {
        public Overload(string definition, IEnumerable<Parameter> parameters, Option<string> returnType)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
            ReturnType = returnType.MatchUnsafe(static t => t, static () => (string?)null);
        }

        public string Definition { get; }

        public IEnumerable<Parameter> Parameters { get; }

        public string? ReturnType { get; }
    }
}
